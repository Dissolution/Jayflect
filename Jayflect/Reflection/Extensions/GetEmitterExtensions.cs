using System.Reflection.Emit;
using Jay.Reflection.Building.Emission;

namespace Jay.Reflection;

public static class GetEmitterExtensions
{
    public static IILGeneratorEmitter GetILEmitter(this ILGenerator ilGenerator)
    {
        return new ILGeneratorEmitter(ilGenerator);
    }

    public static IILGeneratorEmitter GetILEmitter(this DynamicMethod dynamicMethod)
    {
        return new ILGeneratorEmitter(dynamicMethod.GetILGenerator());
    }

    public static IILGeneratorEmitter GetILEmitter(this MethodBuilder methodBuilder)
    {
        return new ILGeneratorEmitter(methodBuilder.GetILGenerator());
    }

    public static IILGeneratorEmitter GetILEmitter(this ConstructorBuilder constructorBuilder)
    {
        return new ILGeneratorEmitter(constructorBuilder.GetILGenerator());
    }
    
    
    public static void Emit(this ILGenerator ilGenerator, Action<IILGeneratorEmitter> emit)
    {
        var emitter = new ILGeneratorEmitter(ilGenerator);
        emit(emitter);
    }
    
    public static void Emit(this DynamicMethod dynamicMethod, Action<IILGeneratorEmitter> emit)
    {
        var emitter = new ILGeneratorEmitter(dynamicMethod.GetILGenerator());
        emit(emitter);
    }
    
    public static MethodBuilder Emit(this MethodBuilder methodBuilder, Action<IILGeneratorEmitter> emit)
    {
        var emitter = new ILGeneratorEmitter(methodBuilder.GetILGenerator());
        emit(emitter);
        return methodBuilder;
    }
    
    public static void Emit(this ConstructorBuilder constructorBuilder, Action<IILGeneratorEmitter> emit)
    {
        var emitter = new ILGeneratorEmitter(constructorBuilder.GetILGenerator());
        emit(emitter);
    }
}