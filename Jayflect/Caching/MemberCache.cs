using System.Collections.Concurrent;

namespace Jayflect.Caching;

public static class MemberCache
{
    internal static class Constants
    {

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