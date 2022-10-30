using System.Diagnostics;
using Jay.Collections;
using Jay.Extensions;
using Jay.Validation;
using Jayflect.Building;
using Jayflect.Caching;
using Jayflect.Extensions;

namespace Jayflect.Cloning;

/// <summary>
/// Deep-clones the given <paramref name="value"/>
/// </summary>
/// <typeparam name="T">The <see cref="Type"/> of value to deep clone</typeparam>
[return: NotNullIfNotNull(nameof(value))]
public delegate T DeepClone<T>(T value);

public static class Cloner
{
    private static readonly ConcurrentTypeDictionary<Delegate> _deepCloneCache = new();
    private static readonly ConcurrentTypeDictionary<DeepClone<object>> _objectCloneCache = new();

    static Cloner()
    {
        _deepCloneCache[typeof(string)] = FastClone<string>;
        _deepCloneCache[typeof(object)] = ObjectDeepClone;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T FastClone<T>(T value) => value;

    private static DeepClone<object> CreateObjectClone(Type type)
    {
        if (type == typeof(object)) // prevent recursion
            return FastClone<object>;
        
        return RuntimeBuilder.CreateDelegate<DeepClone<object>>(
            Dump($"clone_object_{type}"),
            emitter => emitter
                .Ldarg(0)
                .Unbox_Any(type)
                .Call(GetDeepCloneMethod(type))
                .Box(type)
                .Ret());
    }

    private static Delegate CreateDeepClone(Type type)
    {
        var builder = RuntimeBuilder.CreateRuntimeDelegateBuilder(
            typeof(DeepClone<>).MakeGenericType(type),
            Dump($"clone_{type}"));
        var emitter = builder.Emitter;

        /*
        // Null check for non-value types
        if (!type.IsValueType)
        {
            emitter.Ldarg(0)
                .Ldind(type)
                .Brfalse(out var notNull)
                .Ldnull()
                .Ret()
                .MarkLabel(notNull);
        }
        */

        // An unmanaged value we return as it is by-value
        if (type.IsUnmanaged())
        {
            emitter.Ldarg(0).Ret();
        }
        // Special handling for Type
        else if (type.Implements<MemberInfo>())
        {
            emitter.Ldarg(0).Ret();
        }
        // Everything else has some sort of reference down the chain
        else
        {
            // Create a raw value
            emitter.DeclareLocal(type, out var copy);
            if (type.IsValueType)
            {
                // init the copy, we'll clone the fields
                emitter.Ldloca(copy)
                    .Initobj(type);

                // copy each instance field
                var fields = type.GetFields(Reflect.Flags.Instance);
                foreach (var field in fields)
                {
                    if (field.FieldType.Implements<FieldInfo>())
                        Debugger.Break();

                    emitter.Ldloca(copy)
                        .Ldarg(0)
                        .Ldfld(field)
                        .Call(GetDeepCloneMethod(field.FieldType))
                        .Stfld(field);
                }
            }
            else
            {
                // Uninitialized object
                // We don't want to call the constructor, that may have side effects
                emitter.LoadType(type)
                    .Call(MemberCache.Methods.RuntimeHelpers_GetUninitializedObject)
                    .Unbox_Any(type)
                    .Stloc(copy);

                // copy each instance field
                var fields = type.GetFields(Reflect.Flags.Instance);
                foreach (var field in fields)
                {
                    var fieldDeepClone = GetDeepCloneMethod(field.FieldType);
                    emitter.Ldloc(copy)
                        .Ldarg(0)
                        .Ldfld(field)
                        .Call(fieldDeepClone)
                        .Stfld(field);
                }
            }

            // Load our clone and return!
            emitter.Ldloc(copy)
                .Ret();
        }

        return builder.CreateDelegate();
    }

    private static MethodInfo GetDeepCloneMethod(Type type)
    {
        return typeof(Cloner)
            .GetMethod(nameof(DeepClone), Reflect.Flags.Static)
            .ValidateNotNull()
            .MakeGenericMethod(type);
    }

    internal static DeepClone<T> GetDeepClone<T>()
    {
        return (_deepCloneCache.GetOrAdd<T>(CreateDeepClone) as DeepClone<T>)!;
    }

    [return: NotNullIfNotNull(nameof(obj))]
    public static object? ObjectDeepClone(this object? obj)
    {
        if (obj is null) return null;
        var type = obj.GetType();
        return _objectCloneCache.GetOrAdd(type, CreateObjectClone).Invoke(obj);
    }

    [return: NotNullIfNotNull(nameof(value))]
    public static T? DeepClone<T>(this T? value)
    {
        if (value is null) return default!;
        return GetDeepClone<T>().Invoke(value)!;
    }
}