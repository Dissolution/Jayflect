using System.Reflection;
using System.Reflection.Emit;
using Jay.Reflection.Building.Emission;
using Jay.Reflection.Caching;

namespace Jay.Reflection.Building;

public class RuntimeMethod
{
    public static implicit operator DynamicMethod(RuntimeMethod runtimeMethod) => runtimeMethod.DynamicMethod;

    protected readonly DynamicMethod _dynamicMethod;
    private readonly Type _delegateType;
    private ILGenerator? _ilGenerator = null;
    private ILGeneratorEmitter? _emitter = null;
    private ParameterInfo[]? _parameters = null;
    private Type[]? _parameterTypes = null;

    public DynamicMethod DynamicMethod => _dynamicMethod;

    public ILGenerator ILGenerator => _ilGenerator ??= DynamicMethod.GetILGenerator();
    public IILGeneratorEmitter Emitter => _emitter ??= new ILGeneratorEmitter(this.ILGenerator);
    public IReadOnlyList<ParameterInfo> Parameters => (_parameters ??= _dynamicMethod.GetParameters());
    public Type[] ParameterTypes => (_parameterTypes ??= _dynamicMethod.GetParameterTypes());
    public int ParameterCount => Parameters.Count;
    public Type ReturnType => _dynamicMethod.ReturnType;

    public RuntimeMethod(DynamicMethod dynamicMethod, Type delegateType)
    {
        _dynamicMethod = dynamicMethod;
        _delegateType = delegateType;
    }

    public Delegate CreateDelegate()
    {
        return _dynamicMethod.CreateDelegate(_delegateType);
    }
    
    public Delegate CreateDelegate(object? target)
    {
        return _dynamicMethod.CreateDelegate(_delegateType, target);
    }
    
    public Result TryCreateDelegate([NotNullWhen(true)] out Delegate? @delegate)
    {
        try
        {
            @delegate = _dynamicMethod.CreateDelegate(_delegateType);
            return true;
        }
        catch (Exception ex)
        {
            @delegate = null;
            return ex;
        }
    }
    
    public Result TryCreateDelegate(object? target, [NotNullWhen(true)] out Delegate? @delegate)
    {
        try
        {
            @delegate = _dynamicMethod.CreateDelegate(_delegateType, target);
            return true;
        }
        catch (Exception ex)
        {
            @delegate = null;
            return ex;
        }
    }
}

