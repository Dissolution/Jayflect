using Jayflect.Building.Emission;

namespace Jayflect.Extensions;

public static class BuilderExtensions
{
    public static IFluentILEmitter GetILEmitter(this ILGenerator ilGenerator)
    {
        return new FluentILGenerator(ilGenerator);
    }

    public static IFluentILEmitter GetILEmitter(this DynamicMethod dynamicMethod)
    {
        return new FluentILGenerator(dynamicMethod.GetILGenerator());
    }

    public static IFluentILEmitter GetILEmitter(this MethodBuilder methodBuilder)
    {
        return new FluentILGenerator(methodBuilder.GetILGenerator());
    }

    public static IFluentILEmitter GetILEmitter(this ConstructorBuilder constructorBuilder)
    {
        return new FluentILGenerator(constructorBuilder.GetILGenerator());
    }
    
    
    public static void Emit(this ILGenerator ilGenerator, Action<IFluentILEmitter> emit)
    {
        var emitter = new FluentILGenerator(ilGenerator);
        emit(emitter);
    }
    
    public static void Emit(this DynamicMethod dynamicMethod, Action<IFluentILEmitter> emit)
    {
        var emitter = new FluentILGenerator(dynamicMethod.GetILGenerator());
        emit(emitter);
    }
    
    public static MethodBuilder Emit(this MethodBuilder methodBuilder, Action<IFluentILEmitter> emit)
    {
        var emitter = new FluentILGenerator(methodBuilder.GetILGenerator());
        emit(emitter);
        return methodBuilder;
    }
    
    public static void Emit(this ConstructorBuilder constructorBuilder, Action<IFluentILEmitter> emit)
    {
        var emitter = new FluentILGenerator(constructorBuilder.GetILGenerator());
        emit(emitter);
    }
}