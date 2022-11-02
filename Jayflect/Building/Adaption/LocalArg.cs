using Jayflect.Building.Emission;

namespace Jayflect.Building.Adaption;

internal sealed class LocalArg : Arg
{
    private readonly EmitterLocal _local;

    public LocalArg(EmitterLocal local) 
        : base(local.Type, false, false)
    {
        _local = local;
    }

    protected override void Load(IFluentILEmitter emitter)
    {
        emitter.Ldloc(_local);
    }
    
    protected override void LoadAddress(IFluentILEmitter emitter)
    {
        emitter.Ldloca(_local);
    }

    public override bool Equals(Arg? arg)
    {
        return arg is LocalArg localArg && localArg._local == _local;
    }
    public override int GetHashCode()
    {
        return _local.GetHashCode();
    }
    public override string ToString()
    {
        return Dump(_local);
    }
}