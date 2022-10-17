using System.Reflection.Emit;
using Jayflect.Extensions;

namespace Jayflect.Runtime;

public record class DelegateSignature(Type ReturnType, Type[] ParameterTypes)
{
    public static DelegateSignature FromMethod(MethodBase method) => 
        new(method.ReturnType(), method.GetParameterTypes());
    public static DelegateSignature FromDelegate(Delegate @delegate) =>
        FromMethod(@delegate.Method);
    public static DelegateSignature FromDelegate<TDelegate>()
        where TDelegate : Delegate =>
        FromMethod(typeof(TDelegate).GetInvokeMethod()!);
    public static DelegateSignature FromDelegateType(Type delegateType)
    {
        if (!delegateType.Implements<Delegate>())
            throw new ArgumentException("You must pass a valid Delegate Type", nameof(delegateType));
        return FromMethod(delegateType.GetInvokeMethod()!);
    }
}

public class RuntimeMethodBuilder
{
    protected readonly DynamicMethod _dynamicMethod;

    public string Name => _dynamicMethod.Name;
    public MethodAttributes Attributes => _dynamicMethod.Attributes;
    public CallingConventions CallingConventions => _dynamicMethod.CallingConvention;
    public Type ReturnType => _dynamicMethod.ReturnType;
    public Type[] ParameterTypes => _dynamicMethod.GetParameterTypes();
    
    public RuntimeMethodBuilder(DynamicMethod dynamicMethod)
    {
        _dynamicMethod = dynamicMethod;
    }
    
    public Delegate CreateDelegate() => _dynamicMethod.CreateDelegate()
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
    
    internal static DynamicMethod CreateDynamicMethod(DelegateSignature signature, string? name = null)
    {
        return new DynamicMethod(MemberNaming.CreateMemberName(MemberTypes.Method, name),
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

