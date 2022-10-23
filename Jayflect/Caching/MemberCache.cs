using System.Collections.Concurrent;
using Jay.Validation;
using Jayflect.Exceptions;

// I use _ in 'constant' member names for readability
// ReSharper disable InconsistentNaming

namespace Jayflect.Caching;

public static class MemberCache
{
    internal static class Methods
    {
        public static MethodInfo Type_GetTypeFromHandle { get; } = Reflect.FindMember<MethodInfo>(() => Type.GetTypeFromHandle(default));

        public static MethodInfo Delegate_GetInvocationList { get; } =
            typeof(Delegate).GetMethod(
                    nameof(Delegate.GetInvocationList),
                    BindingFlags.Public | BindingFlags.Instance)
                .ValidateNotNull();
    }
    
    private static readonly ConcurrentDictionary<string, MemberInfo> _stringMemberCache = new();

    public static TMember GetOrAdd<TMember>(string key, TMember newMember)
        where TMember : MemberInfo
    {
        var memberInfo = _stringMemberCache.GetOrAdd(key, newMember);
        if (memberInfo is TMember member) return member;
        throw new ReflectionException($"There is an {memberInfo.GetType()} defined for \"{key}\" when a {typeof(TMember)} was requested");
    }
    
    public static TMember GetOrAdd<TMember>(string key, Func<TMember> newMember)
        where TMember : MemberInfo
    {
        var memberInfo = _stringMemberCache.GetOrAdd(key, _ => newMember());
        if (memberInfo is TMember member) return member;
        throw new ReflectionException($"There is an {memberInfo.GetType()} defined for \"{key}\" when a {typeof(TMember)} was requested");
    }
}