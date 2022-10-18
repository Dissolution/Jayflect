using Jayflect.Extensions;

namespace Jayflect.Runtime;

public record class DelegateSignature(Type ReturnType, Type[] ParameterTypes)
{
    public static DelegateSignature FromMethod(MethodBase method) => 
        new(method.ReturnType(), method.GetParameterTypes());
    
    public static DelegateSignature FromDelegate(Delegate @delegate) =>
        FromMethod(@delegate.Method);
    
    public static DelegateSignature FromDelegate<TDelegate>()
        where TDelegate : Delegate =>
        FromMethod(typeof(TDelegate).GetInvokeMethod()!);
    
    public static DelegateSignature FromDelegateType(Type delegateType)
    {
        Validate.IsDelegateType(delegateType);
        return FromMethod(delegateType.GetInvokeMethod()!);
    }
}