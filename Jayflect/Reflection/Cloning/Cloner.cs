using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using Jay.Collections;
using Jay.Dumping;
using Jay.Reflection.Building;
using Jay.Reflection.Building.Emission;
using Jay.Reflection.Caching;
using Jay.Reflection.Internal;
using Jay.Reflection.Search;
using Jay.Validation;

namespace Jay.Reflection.Cloning;

[return: NotNullIfNotNull(nameof(value))]
public delegate T? DeepClone<T>(T? value);

public abstract class Argument
{
    public int Index { get; protected set; }
    public Type Type { get; protected set; }
    public string? Name { get; protected set; }

    protected Argument(int index, Type type, string? name)
    {
        this.Index = index;
        this.Type = type;
        this.Name = name;
    }

    internal abstract void Load(IILGeneratorEmitter emitter);
    internal abstract void LoadAddress(IILGeneratorEmitter emitter);

    public override string ToString()
    {
        return Dumper.Dump($"{Index}: {Type} {Name}");
    }
}

public sealed class ParameterArgument : Argument
{
    public ParameterInfo Parameter { get; }

    public ParameterArgument(ParameterInfo parameter)
        : base(parameter.Position, parameter.ParameterType, parameter.Name)
    {
        Parameter = parameter;
    }

    internal override void Load(IILGeneratorEmitter emitter)
    {
        emitter.Ldarg(Index);
    }

    internal override void LoadAddress(IILGeneratorEmitter emitter)
    {
        emitter.Ldarga(Index);
    }
}

public sealed class StackArgument : Argument
{
    public StackArgument(Type type, string? name = null) 
        : base(-1, type, name)
    {
    }

    internal override void Load(IILGeneratorEmitter emitter)
    {
        // We're already on the stack! :-)
    }

    internal override void LoadAddress(IILGeneratorEmitter emitter)
    {
        // We have to store our value as a local so that we can get a ref to it
        emitter.DeclareLocal(Type, out var lclCopy)
               .Stloc(lclCopy)
               .Ldloca(lclCopy);
    }
}


public static class Cloner
{
    public static T[] CloneArray<T>(T[] array)
    {
        int len = array.Length;
        T[] clone = new T[len];
        for (int i = 0; i < len; i++)
        {
            clone[i] = DeepClone<T>(array[i])!;
        }

        return clone;
    }

    private static readonly ConcurrentTypeDictionary<Delegate> _deepCloneDelegateCache;

    static Cloner()
    {
        _deepCloneDelegateCache = new();
    }

    private static readonly ConcurrentTypeDictionary<DeepClone<object>> _objectCloneCache = new();

    [return: NotNullIfNotNull(nameof(obj))]
    private static object? DeepClone(object? obj)
    {
        if (obj is null) return null;
        var objType = obj.GetType();
        // Prevent recursion
        if (objType == typeof(object))
            return new object();

        var cloner = _objectCloneCache.GetOrAdd(objType, CreateObjectDeepClone);
        return cloner(obj);
    }

    private static DeepClone<object> CreateObjectDeepClone(Type objType)
    {
        return RuntimeBuilder.CreateDelegate<DeepClone<object>>(Dumper.Dump($"clone_object_{objType}"),
            emitter => emitter.Ldarg(0)
                              .Unbox_Any(objType)
                              .Call(GetDeepCloneMethodForType(objType))
                              .Box(objType)
                              .Ret());
    }

    private static Delegate CreateDeepCloneDelegate(Type type)
    {
        // Special handling for object
        if (type == typeof(object))
            return (DeepClone<object>)DeepClone;

        var runtimeMethod = RuntimeBuilder.CreateRuntimeMethod(
            typeof(DeepClone<>).MakeGenericType(type),
            Dumper.Dump($"clone_{type}"));
        var emitter = runtimeMethod.Emitter;

        // String or an unmanaged type can be duplicated
        if (type == typeof(string) || TypeCache.IsUnmanaged(type))
        {
            emitter.Ldarga(0)
                   .Ldind(type)
                   .Ret();
            return runtimeMethod.CreateDelegate();
        }

        // Start with a blank clone
        emitter.DeclareLocal(type, out var clone);

        // Arrays are special
        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            var elementCloneMethod = GetDeepCloneMethodForType(elementType);
            var rank = type.GetArrayRank();
            // 2D Array []
            if (rank == 1)
            {
                // int len = array.Length
                emitter.Ldarg(0)
                       .Ldlen()
                       .DeclareLocal<int>(out var len)
                       .Stloc(len);

                // clone = new T[len]
                emitter.Ldloc(len)
                       .Newarr(elementType)
                       .Stloc(clone);

                // int i = 0
                emitter.DeclareLocal<int>(out var i)
                       .Ldc_I4_0()
                       .Stloc(i);

                // Start our FOR loop
                emitter.DefineLabel(out var lblStart)
                       .DefineLabel(out var lblCheck);

                // Head to i < len check
                emitter.Br(lblCheck);

                // Start of loop
                emitter.MarkLabel(lblStart);
                
                // clone[i] = DeepClone<T>(array[i])
                emitter.Ldloc(clone)
                       .Ldloc(i)
                       .Ldarg(0)
                       .Ldloc(i)
                       .Ldelem(elementType)
                       .Call(elementCloneMethod)
                       .Stelem(elementType);

                // i++
                emitter.Ldloc(i)
                       .Ldc_I4_1()
                       .Add()
                       .Stloc(i);

                // while i < len
                emitter.MarkLabel(lblCheck)
                       .Ldloc(i)
                       .Ldloc(len)
                       .Clt()
                       .Brtrue(lblStart);

                // Each array value has been copied
                emitter.Ldloc(clone).Ret();
                return runtimeMethod.CreateDelegate();
            }

            // 3+ dimensional arrays
            throw new NotImplementedException();
        }


        // If we're a value, we can use initobj
        if (type.IsValueType)
        {
            emitter.Ldloca(clone)
                   .Initobj(type);
        }
        // Otherwise, we need to make an uninitialized version, as we cannot 'trust' a constructor
        else
        {
            emitter.LoadType(type)
                   .Call(MethodInfoCache.RuntimeHelpers_GetUninitializedObject)
                   .Unbox_Any(type)
                   .Stloc(clone);
        }

        // Copy each field in turn
        var fields = type.GetFields(Reflect.InstanceFlags);
        if (fields.Length == 0)
        {
            Debugger.Break();
        }

        if (type.IsValueType)
        {
            foreach (var field in fields)
            {
                // Get the clone we'll be setting this field value of
                emitter.Ldloca(clone)
                       // Load the original value's field's value
                       .Ldarga(0)
                       .Ldfld(field)
                       // Clone it using Clone<T> so it will cache
                       .Call(GetDeepCloneMethodForType(field.FieldType))
                       // Set the clone's field value to the cloned value
                       .Stfld(field);
            }
        }
        else
        {
            foreach (var field in fields)
            {
                // Get the clone we'll be setting this field value of
                emitter.Ldloc(clone)
                       // Load the original value's field's value
                       .Ldarg(0)
                       .Ldfld(field)
                       // Clone it using Clone<T> so it will cache
                       .Call(GetDeepCloneMethodForType(field.FieldType))
                       // Set the clone's field value to the cloned value
                       .Stfld(field);
            }
        }

        // Finished with all fields, return the clone
        emitter.Ldloc(clone).Ret();
        return runtimeMethod.CreateDelegate();
    }

    private static MethodInfo GetDeepCloneMethodForType(Type type)
    {
        return typeof(Cloner).GetMethod(nameof(DeepClone), BindingFlags.Public | BindingFlags.Static)
                             .ThrowIfNull()
                             .MakeGenericMethod(type);
    }

    [return: NotNullIfNotNull(nameof(value))]
    public static T? DeepClone<T>(T? value)
    {
        if (value is null) return default;
        var deepClone = _deepCloneDelegateCache.GetOrAdd<T>(type => CreateDeepCloneDelegate(type)) as DeepClone<T>;
        return deepClone!(value);
    }
}