using System.Reflection.Emit;
using Jay.Reflection.Caching;

namespace Jay.Reflection.Building;

public class RuntimeMethod<TDelegate> : RuntimeMethod
    where TDelegate : Delegate
{
    public RuntimeMethod(DynamicMethod dynamicMethod)
        : base(dynamicMethod, typeof(TDelegate))
    {
    }

    public new TDelegate CreateDelegate()
    {
        return _dynamicMethod.CreateDelegate<TDelegate>();
    }
    
    public new TDelegate CreateDelegate(object? target)
    {
        return _dynamicMethod.CreateDelegate<TDelegate>(target);
    }

    public Result TryCreateDelegate([NotNullWhen(true)] out TDelegate? @delegate)
    {
        try
        {
            @delegate = _dynamicMethod.CreateDelegate<TDelegate>();
            return true;
        }
        catch (Exception ex)
        {
            @delegate = null;
            return ex;
        }
    }
    
    public Result TryCreateDelegate(object? target, [NotNullWhen(true)] out TDelegate? @delegate)
    {
        try
        {
            @delegate = _dynamicMethod.CreateDelegate<TDelegate>(target);
            return true;
        }
        catch (Exception ex)
        {
            @delegate = null;
            return ex;
        }
    }
}