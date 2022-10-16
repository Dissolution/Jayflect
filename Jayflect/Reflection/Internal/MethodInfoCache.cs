using System.Reflection;
using Jay.Collections;
using Jay.Reflection.Exceptions;
using Jay.Reflection.Search;

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace Jay.Reflection.Internal;

internal static class MethodInfoCache
{
    private static readonly Lazy<MethodInfo> _runtimeHelpers_GetUninitializedObject_Method;
    private static readonly Lazy<MethodInfo> _type_GetTypeFromHandle_Method;
    private static readonly Lazy<MethodInfo> _multicastDelegate_GetInvocationList_Method;
    private static readonly Lazy<MethodInfo> _delegate_Combine_Method;
    private static readonly Lazy<MethodInfo> _delegate_Remove_Method;

    public static MethodInfo RuntimeHelpers_GetUninitializedObject =>
        _runtimeHelpers_GetUninitializedObject_Method.Value;

    public static MethodInfo Type_GetTypeFromHandle =>
        _type_GetTypeFromHandle_Method.Value;

    public static MethodInfo MulticastDelegate_GetInvocationList =>
        _multicastDelegate_GetInvocationList_Method.Value;

    public static MethodInfo Delegate_Combine => _delegate_Combine_Method.Value;
    public static MethodInfo Delegate_Remove => _delegate_Remove_Method.Value;

    static MethodInfoCache()
    {
        _runtimeHelpers_GetUninitializedObject_Method = new Lazy<MethodInfo>(() =>
        {
            return MemberSearch.Find<MethodInfo>(() => RuntimeHelpers.GetUninitializedObject(default(Type)!));
        });
        _type_GetTypeFromHandle_Method = new Lazy<MethodInfo>(() =>
        {
            return MemberSearch.Find<MethodInfo>(() => Type.GetTypeFromHandle(default(RuntimeTypeHandle)!));
        });
        _multicastDelegate_GetInvocationList_Method = new Lazy<MethodInfo>(() =>
        {
            return MemberSearch.Find<MulticastDelegate, MethodInfo>(multicastDelegate => multicastDelegate!.GetInvocationList());
        });

        _delegate_Combine_Method = new Lazy<MethodInfo>(() =>
        {
            return MemberSearch.Find<MethodInfo>(() => Delegate.Combine(default(Delegate), default(Delegate)));
        });
        _delegate_Remove_Method = new Lazy<MethodInfo>(() =>
        {
            return MemberSearch.Find<MethodInfo>(() => Delegate.Remove(default(Delegate), default(Delegate)));
        });
    }
}