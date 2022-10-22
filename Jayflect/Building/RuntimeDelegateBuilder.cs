using Jayflect.Building.Emission;

namespace Jayflect.Building;

public class RuntimeDelegateBuilder
{
    protected readonly DynamicMethod _dynamicMethod;
    protected readonly Type _delegateType;
    private FluentILGenerator? _emitter = null;

    public string Name => _dynamicMethod.Name;
    public MethodAttributes Attributes => _dynamicMethod.Attributes;
    public CallingConventions CallingConventions => _dynamicMethod.CallingConvention;
    public Type ReturnType => _dynamicMethod.ReturnType;
    public ParameterInfo[] Parameters { get; }
    public Type[] ParameterTypes { get; }

    public ILGenerator IlGenerator => _dynamicMethod.GetILGenerator();

    public FluentILGenerator Emitter => _emitter ??= new(IlGenerator);

    internal RuntimeDelegateBuilder(DynamicMethod dynamicMethod, Type delegateType)
    {
        Validate.IsDelegateType(delegateType);
        _dynamicMethod = dynamicMethod;
        _delegateType = delegateType;
        this.Parameters = _dynamicMethod.GetParameters();
        this.ParameterTypes = Array.ConvertAll(this.Parameters, param => param.ParameterType);
    }

    public Delegate CreateDelegate()
    {
        return _dynamicMethod.CreateDelegate(_delegateType);
    }
}