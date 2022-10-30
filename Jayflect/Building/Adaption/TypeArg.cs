using System.Diagnostics;
using Jayflect.Building.Emission;
using Jayflect.Extensions;

namespace Jayflect.Building.Adaption;

internal sealed class TypeArg : Arg
{
    private readonly Type _type;
    
    public override bool IsOnStack { get; }
    
    public TypeArg(Type type)
        : base(type.GetAccess(out var baseType), baseType)
    {
        _type = type;
        this.IsOnStack = this.Type != typeof(void);
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

    public override Type ToType()
    {
        return _type;
    }

    public override string ToString()
    {
        return Dump(_type);
    }
}