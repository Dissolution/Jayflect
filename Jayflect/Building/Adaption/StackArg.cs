using System.Diagnostics;
using Jayflect.Building.Emission;

namespace Jayflect.Building.Adaption;

internal sealed class StackArg : Arg
{
    private readonly Type _type;

    public StackArg(Type type)
        : base(type, type != typeof(void), true)
    {
        _type = type;
    }

    protected override void Load(IFluentILEmitter emitter)
    {
        // Do nothing (expected)
    }
    
    protected override void LoadAddress(IFluentILEmitter emitter)
    {
        // We should never get here
        Debugger.Break();
    }

    public override bool Equals(Arg? arg)
    {
        return arg is StackArg stackArg && stackArg._type == _type;
    }

    public override int GetHashCode()
    {
        return _type.GetHashCode();
    }

    public override string ToString()
    {
        return Dump(_type);
    }
}