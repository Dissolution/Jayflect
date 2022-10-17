namespace Jayflect.Extensions;

public static class DelegateExtensions
{
    public static MethodInfo GetInvokeMethod(this Delegate @delegate) => @delegate.Method;
}