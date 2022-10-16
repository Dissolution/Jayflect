using System.Reflection;
using Jay.Reflection.Caching;

namespace Jay.Reflection;

public static class MethodInfoExtensions
{


    public static TReturn Invoke<TInstance, TReturn>(this MethodInfo method,
                                                     ref TInstance instance,
                                                     params object?[] args)
    {
        var del = DelegateMemberCache.Instance
                                     .GetOrAdd<Invoker<TInstance, TReturn>>(method, m =>
                                     {
                                         method.TryAdapt<Invoker<TInstance, TReturn>>(out var del).ThrowIfFailed();
                                         return del!;
                                     });
        return del(ref instance, args);
    }
}