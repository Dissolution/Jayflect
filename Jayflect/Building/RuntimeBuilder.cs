﻿using Jay.Extensions;
using Jayflect.Building.Emission;

namespace Jayflect.Building;

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
    
    public static DynamicMethod CreateDynamicMethod(DelegateSignature signature, string? name = null)
    {
        return new DynamicMethod(MemberNaming.CreateMemberName(MemberTypes.Method, name),
            MethodAttributes.Public | MethodAttributes.Static,
            CallingConventions.Standard,
            signature.ReturnType,
            signature.ParameterTypes,
            ModuleBuilder,
            true);
    }

    public static RuntimeDelegateBuilder CreateRuntimeDelegateBuilder(Type delegateType, string? name = null)
    {
        Validate.IsDelegateType(delegateType);
        var dynamicMethod = CreateDynamicMethod(DelegateSignature.FromDelegateType(delegateType), name);
        return new RuntimeDelegateBuilder(dynamicMethod, delegateType);
    }
    
    public static RuntimeDelegateBuilder<TDelegate> CreateRuntimeDelegateBuilder<TDelegate>(string? name = null)
        where TDelegate : Delegate
    {
        var dynamicMethod = CreateDynamicMethod(DelegateSignature.FromDelegate<TDelegate>(), name);
        return new RuntimeDelegateBuilder<TDelegate>(dynamicMethod);
    }

    public static Delegate CreateDelegate(Type delegateType, string? name, Action<RuntimeDelegateBuilder> buildDelegate)
    {
        Validate.IsDelegateType(delegateType);
        var runtimeDelegateBuilder = CreateRuntimeDelegateBuilder(delegateType, name);
        buildDelegate(runtimeDelegateBuilder);
        return runtimeDelegateBuilder.CreateDelegate();
    }
    
    public static TDelegate CreateDelegate<TDelegate>(string? name, Action<RuntimeDelegateBuilder<TDelegate>> buildDelegate)
        where TDelegate : Delegate
    {
        var runtimeDelegateBuilder = CreateRuntimeDelegateBuilder<TDelegate>(name);
        buildDelegate(runtimeDelegateBuilder);
        return runtimeDelegateBuilder.CreateDelegate();
    }
    
    public static Delegate CreateDelegate(Type delegateType, Action<FluentILGenerator> emitDelegate)
    {
        return CreateDelegate(delegateType, null, emitDelegate);
    }

    public static Delegate CreateDelegate(Type delegateType, string? name, Action<FluentILGenerator> emitDelegate)
    {
        if (!delegateType.Implements<Delegate>())
            throw new ArgumentException("Must be a delegate", nameof(delegateType));
        var runtimeMethod = CreateRuntimeDelegateBuilder(delegateType, name);
        emitDelegate(runtimeMethod.Emitter);
        return runtimeMethod.CreateDelegate();
    }

    public static TDelegate CreateDelegate<TDelegate>(Action<FluentILGenerator> emitDelegate)
        where TDelegate : Delegate
    {
        return CreateDelegate<TDelegate>(null, emitDelegate);
    }

    public static TDelegate CreateDelegate<TDelegate>(string? name, Action<FluentILGenerator> emitDelegate)
        where TDelegate : Delegate
    {
        var runtimeMethod = CreateRuntimeDelegateBuilder<TDelegate>(name);
        emitDelegate(runtimeMethod.Emitter);
        return runtimeMethod.CreateDelegate();
    }

    public static TypeBuilder DefineType(TypeAttributes typeAttributes, string? name = null)
    {
        return ModuleBuilder.DefineType(
            MemberNaming.CreateMemberName(MemberTypes.TypeInfo, name),
            typeAttributes, 
            typeof(RuntimeBuilder));
    }

    public static CustomAttributeBuilder GetCustomAttributeBuilder<TAttribute>()
        where TAttribute : Attribute, new()
    {
        var ctor = typeof(TAttribute).GetConstructor(Reflect.Flags.Instance, Type.EmptyTypes);
        if (ctor is null)
            throw new InvalidOperationException(Dump($"Cannot find an empty {typeof(TAttribute)} constructor."));
        return new CustomAttributeBuilder(ctor, Array.Empty<object>());
    }

    public static CustomAttributeBuilder GetCustomAttributeBuilder<TAttribute>(params object[] ctorArgs)
        where TAttribute : Attribute
    {
        var ctor = Reflect.FindConstructor(typeof(TAttribute), ctorArgs);
        return new CustomAttributeBuilder(ctor, ctorArgs);
    }

    public static CustomAttributeBuilder GetCustomAttributeBuilder(Type attributeType, params object[] ctorArgs)
    {
        if (!attributeType.Implements<Attribute>())
            throw new ArgumentException(Dump($"{attributeType} is not an Attribute"));
        var ctor = Reflect.FindConstructor(attributeType, ctorArgs);
        return new CustomAttributeBuilder(ctor, ctorArgs);
    }
}

