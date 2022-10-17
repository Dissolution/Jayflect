using System.Reflection.Emit;

namespace Jayflect.Runtime;

public interface IDelegateSignature : IEquatable<IDelegateSignature>
{
    Type ReturnType { get; }
    Type[] ParameterTypes { get; }
}

public class RuntimeMethodBuilder : IDelegateSignature
{
    protected DynamicMethod _dynamicMethod;
    
    public string Name { get; }
    public MethodAttributes Attributes { get; }
    public CallingConventions CallingConventions { get; }
    public Type ReturnType { get; }
    public Type[] ParameterTypes { get; }
    
    public RuntimeMethodBuilder(IDelegateSignature signature, string? name = null)
    {
        this.Name = MemberNaming.CreateMemberName(name);
        this.Attributes = MethodAttributes.Public | MethodAttributes.Static;
        this.CallingConventions = CallingConventions.Standard;
        this.ReturnType = signature.ReturnType;
        this.ParameterTypes = signature.ParameterTypes;
        _dynamicMethod = new DynamicMethod(Name,
            Attributes, CallingConventions, ReturnType, ParameterTypes,
            RuntimeBuilder.ModuleBuilder,
            true);
    }
}


/// <summary>
/// A runtime builder of <see cref="DynamicMethod"/>s and <see cref="Delegate"/>s
/// </summary>
public static class RuntimeBuilder
{
    public static AssemblyBuilder AssemblyBuilder { get; }
    public static ModuleBuilder ModuleBuilder { get; }

    static RuntimeBuilder()
    {
        var assemblyName = new AssemblyName($"{nameof(RuntimeBuilder)}_Assembly");
        AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder = AssemblyBuilder.DefineDynamicModule($"{nameof(RuntimeBuilder)}_Module");
    }

    internal static DynamicMethod CreateDynamicMethod(IDelegateSignature signature, string? name = null)
    {
        return new DynamicMethod(MemberNaming.CreateMemberName(name),
            MethodAttributes.Public | MethodAttributes.Static,
            CallingConventions.Standard,
            signature.ReturnType,
            signature.ParameterTypes,
            ModuleBuilder,
            true);
        
    }
   
    /*
    public static RuntimeMethod CreateRuntimeMethod(Type delegateType, string? name = null)
    {
        return new RuntimeMethod(CreateDynamicMethod(MethodSig.Of(delegateType), name), delegateType);
    }

    public static Delegate CreateDelegate(Type delegateType, Action<RuntimeMethod> buildDelegate)
    {
        return CreateDelegate(delegateType, null, buildDelegate);
    }

    public static Delegate CreateDelegate(Type delegateType, string? name, Action<RuntimeMethod> buildDelegate)
    {
        if (!delegateType.Implements<Delegate>())
            throw new ArgumentException("Must be a delegate", nameof(delegateType));
        var runtimeMethod = CreateRuntimeMethod(delegateType, name);
        buildDelegate(runtimeMethod);
        return runtimeMethod.CreateDelegate();
    }

    public static Delegate CreateDelegate(Type delegateType, Action<IILGeneratorEmitter> emitDelegate)
    {
        return CreateDelegate(delegateType, null, emitDelegate);
    }

    public static Delegate CreateDelegate(Type delegateType, string? name, Action<IILGeneratorEmitter> emitDelegate)
    {
        if (!delegateType.Implements<Delegate>())
            throw new ArgumentException("Must be a delegate", nameof(delegateType));
        var runtimeMethod = CreateRuntimeMethod(delegateType, name);
        emitDelegate(runtimeMethod.Emitter);
        return runtimeMethod.CreateDelegate();
    }
    
    public static RuntimeMethod<TDelegate> CreateRuntimeMethod<TDelegate>(string? name = null)
        where TDelegate : Delegate
    {
        return new RuntimeMethod<TDelegate>(CreateDynamicMethod(MethodSig.Of<TDelegate>(), name));
    }
    
    public static TDelegate CreateDelegate<TDelegate>(Action<RuntimeMethod<TDelegate>> buildDelegate)
        where TDelegate : Delegate
    {
        return CreateDelegate<TDelegate>(null, buildDelegate);
    }
    
    public static TDelegate CreateDelegate<TDelegate>(Action<IILGeneratorEmitter> emitDelegate)
        where TDelegate : Delegate
    {
        return CreateDelegate<TDelegate>(null, emitDelegate);
    }

    public static TDelegate CreateDelegate<TDelegate>(string? name, Action<RuntimeMethod<TDelegate>> buildDelegate)
        where TDelegate : Delegate
    {
        var runtimeMethod = CreateRuntimeMethod<TDelegate>(name);
        buildDelegate(runtimeMethod);
        return runtimeMethod.CreateDelegate();
    }
    
    public static TDelegate CreateDelegate<TDelegate>(string? name, Action<IILGeneratorEmitter> emitDelegate)
        where TDelegate : Delegate
    {
        var runtimeMethod = CreateRuntimeMethod<TDelegate>(name);
        emitDelegate(runtimeMethod.Emitter);
        return runtimeMethod.CreateDelegate();
    }

    public static TypeBuilder DefineType(TypeAttributes typeAttributes, string? name = null)
    {
        return ModuleBuilder.DefineType(
            MemberNaming.CreateMemberName(name),
            typeAttributes, 
            typeof(RuntimeBuilder));
    }

    public static CustomAttributeBuilder GetCustomAttributeBuilder<TAttribute>()
        where TAttribute : Attribute, new()
    {
        var ctor = typeof(TAttribute).GetConstructor(Reflect.InstanceFlags, Type.EmptyTypes);
        if (ctor is null)
            Dumper.ThrowException<InvalidOperationException>($"Cannot find an empty {typeof(TAttribute)} constructor.");
        return new CustomAttributeBuilder(ctor, Array.Empty<object>());
    }

    public static CustomAttributeBuilder GetCustomAttributeBuilder<TAttribute>(params object?[] ctorArgs)
        where TAttribute : Attribute
    {
        var ctor = MemberSearch.FindBestConstructor(typeof(TAttribute), Reflect.InstanceFlags, ctorArgs);
        if (ctor is null)
            Dumper.ThrowException<InvalidOperationException>($"Cannot find a {typeof(TAttribute)} constructor that matches {ctorArgs}");
        return new CustomAttributeBuilder(ctor, ctorArgs);
    }

    public static CustomAttributeBuilder GetCustomAttributeBuilder(Type attributeType, params object?[] ctorArgs)
    {
        if (!attributeType.Implements<Attribute>())
            Dumper.ThrowException<ArgumentException>($"{attributeType} is not an Attribute");
        var ctor = MemberSearch.FindBestConstructor(attributeType, Reflect.InstanceFlags, ctorArgs);
        if (ctor is null)
            Dumper.ThrowException<InvalidOperationException>($"Cannot find a {attributeType} constructor that matches {ctorArgs}");
        return new CustomAttributeBuilder(ctor, ctorArgs);
    }
    */
}

