using Jayflect.Building.Adaption;
using Jayflect.Searching;


namespace Jayflect.Building.Emission;

public static class EmitterExtensions
{
    public static IFluentILEmitter EmitCast(this IFluentILEmitter emitter, Arg source, Arg dest)
    {
        source.TryLoadAs(emitter, dest).ThrowIfFailed();
        return emitter;
    }

    public static IFluentILEmitter EmitLoadParameter(this IFluentILEmitter emitter, Arg sourceParameter, Arg destType)
    {
        sourceParameter.TryLoadAs(emitter, destType).ThrowIfFailed();
        return emitter;
    }

    public static IFluentILEmitter EmitLoadInstance(this IFluentILEmitter emitter, Arg instance, MemberInfo member)
    {
        // No instance, nothing to load
        if (!member.TryGetInstanceType(out var instanceType)) return emitter;
        instance.TryLoadAs(emitter, instanceType).ThrowIfFailed();
        return emitter;
    }
    
    public static IFluentILEmitter EmitParamsLengthCheck(this IFluentILEmitter emitter,
        ParameterInfo paramsParameter, int length)
    {
        return emitter
            .Ldarg(paramsParameter)
            .Ldlen()
            .Ldc_I4(length)
            .Beq(out var lenEqual)
            .Ldstr($"{length} parameters are required in the params array")
            .Ldstr(paramsParameter.Name)
            .Newobj(MemberSearch.FindConstructor<ArgumentException>(typeof(string), typeof(string)))
            .Throw()
            .MarkLabel(lenEqual);
    }

    public static IFluentILEmitter EmitLoadParams(this IFluentILEmitter emitter,
        ParameterInfo paramsParameter,
        ReadOnlySpan<ParameterInfo> destParameters)
    {
        int len = destParameters.Length;
        // None to load?
        if (len == 0)
            return emitter;

        // Params -> Params?
        if (len == 1 && destParameters[0].IsParams())
        {
            emitter.Ldarg(paramsParameter);
        }
        else
        {
            // extract each parameter in turn
            for (var i = 0; i < len; i++)
            {
                emitter.Ldarg(paramsParameter)
                    .Ldc_I4(i)
                    .Ldelem(destParameters[i].ParameterType);
            }
        }

        // Everything will be loaded!
        var il = emitter.ToString();
        return emitter;
    }
}