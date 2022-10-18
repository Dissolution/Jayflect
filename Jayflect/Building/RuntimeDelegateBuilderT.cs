namespace Jayflect.Runtime;

public class RuntimeDelegateBuilder<TDelegate> : RuntimeDelegateBuilder
    where TDelegate : Delegate
{
    public RuntimeDelegateBuilder(DynamicMethod dynamicMethod)
        : base(dynamicMethod, typeof(TDelegate))
    {
        
    }

    public new TDelegate CreateDelegate()
    {
        return _dynamicMethod.CreateDelegate<TDelegate>();
    }
}