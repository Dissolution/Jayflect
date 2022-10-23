using Jayflect.Building.Emission;

namespace Jayflect.Building;

public class RuntimeDelegateBuilder
{
    protected readonly DynamicMethod _dynamicMethod;
    protected readonly DelegateSignature _delegateSig;
    private IFluentILEmitter? _emitter;

    public string Name => _dynamicMethod.Name;
    public MethodAttributes Attributes => _dynamicMethod.Attributes;
    public CallingConventions CallingConventions => _dynamicMethod.CallingConvention;

    public Type ReturnType => _delegateSig.ReturnType;

    public ParameterInfo[] Parameters => _delegateSig.Parameters;

    public ParameterInfo? FirstParameter => Parameters.Length > 0 ? Parameters[0] : null;
    
    public Type[] ParameterTypes => _delegateSig.ParameterTypes;

    public int ParameterCount => _delegateSig.ParameterCount;

    public DelegateSignature Signature => _delegateSig;

    //public ILGenerator IlGenerator => _dynamicMethod.GetILGenerator();

    public IFluentILEmitter Emitter => _emitter ??= new FluentILGenerator(_dynamicMethod.GetILGenerator());

    internal RuntimeDelegateBuilder(DynamicMethod dynamicMethod, DelegateSignature delegateSignature)
    {
        _dynamicMethod = dynamicMethod;
        _delegateSig = delegateSignature;
    }

    public Delegate CreateDelegate()
    {
        return _dynamicMethod.CreateDelegate(_delegateSig.GetDelegateType());
    }
}