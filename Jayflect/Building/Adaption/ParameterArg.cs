using Jayflect.Building.Emission;
using Jayflect.Extensions;

namespace Jayflect.Building.Adaption;

internal sealed class ParameterArg : Arg
{
    private readonly ParameterInfo _parameterInfo;
    
    public override bool IsOnStack => false;

    public ParameterArg(ParameterInfo parameterInfo)
        : base(parameterInfo.GetAccess(out var parameterType), parameterType)
    {
        _parameterInfo = parameterInfo;
    }

    protected override void Load(IFluentILEmitter emitter)
    {
        emitter.Ldarg(_parameterInfo.Position);
    }
    protected override void LoadAddress(IFluentILEmitter emitter)
    {
        emitter.Ldarga(_parameterInfo.Position);
    }

    public override string ToString()
    {
        return Dump(_parameterInfo);
    }
}