namespace Jayflect.Building;

public class RuntimeDelegateBuilder<TDelegate> : RuntimeDelegateBuilder
    where TDelegate : Delegate
{
    public RuntimeDelegateBuilder(DynamicMethod dynamicMethod)
        : base(dynamicMethod, DelegateInfo.For<TDelegate>())
    {
        
    }

    public new TDelegate CreateDelegate()
    {
        return _dynamicMethod.CreateDelegate<TDelegate>();
    }
}