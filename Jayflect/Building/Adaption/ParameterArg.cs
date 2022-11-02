using Jayflect.Building.Emission;

namespace Jayflect.Building.Adaption;

internal sealed class ParameterArg : Arg
{
    private static Type GetType(ParameterInfo parameter)
    {
        var type = parameter.ParameterType;
        if ((parameter.IsIn || parameter.IsOut) && !type.IsByRef) 
            return type.MakeByRefType();
        return type;
    }
    
    private readonly ParameterInfo _parameterInfo;
    
    public ParameterArg(ParameterInfo parameterInfo)
        : base(GetType(parameterInfo), false, false)
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

    public override bool Equals(Arg? arg)
    {
        return arg is ParameterArg parameterArg &&
               parameterArg._parameterInfo == _parameterInfo;
    }
    
    public override int GetHashCode()
    {
        return _parameterInfo.GetHashCode();
    }

    public override string ToString()
    {
        return Dump(_parameterInfo);
    }
}