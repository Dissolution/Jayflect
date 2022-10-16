using System.Collections.Concurrent;
using System.Reflection;
using Jay.Reflection.Building;
using Jay.Reflection.Exceptions;

namespace Jay.Reflection.Caching;

public sealed class DelegateMemberCache : ConcurrentDictionary<DelegateAndMember, Delegate>
{
    internal static DelegateMemberCache Instance { get; } = new DelegateMemberCache();

    public DelegateMemberCache()
        : base(capacity: 0, concurrencyLevel: 1) { }

    public bool TryGetDelegate<TDelegate>(MemberInfo member, out TDelegate? @delegate)
        where TDelegate : Delegate
    {
        if (base.TryGetValue(DelegateAndMember.Create<TDelegate>(member), out var del))
        {
            return del.Is(out @delegate);
        }
        @delegate = default;
        return false;
    }

    public TDelegate GetOrAdd<TDelegate>(MemberInfo member, Func<DelegateAndMember, TDelegate> createDelegate)
        where TDelegate : Delegate
    {
        var key = DelegateAndMember.Create<TDelegate>(member);
        var del = base.GetOrAdd(key, createDelegate(key));
        if (del is not TDelegate typedDelegate)
        {
            throw new ReflectionException("An existing delegate with a signature was encountered");
        }
        return typedDelegate;
    }

    public TDelegate GetOrAdd<TMember, TDelegate>(TMember member, Func<TMember, TDelegate> createDelegate)
        where TMember : MemberInfo
        where TDelegate : Delegate
    {
        var key = DelegateAndMember.Create<TDelegate>(member);
        var del = base.GetOrAdd(key, createDelegate(member));
        if (del is not TDelegate typedDelegate)
        {
            throw new ReflectionException("An existing delegate with a signature was encountered");
        }
        return typedDelegate;
    }

    public TDelegate GetOrAdd<TMember, TDelegate>(TMember member,
                                                  Action<RuntimeMethod<TDelegate>> buildMethod)
        where TMember : MemberInfo
        where TDelegate : Delegate
    {
        var del = base.GetOrAdd(DelegateAndMember.Create<TDelegate>(member),
            key => RuntimeBuilder.CreateDelegate(key.ToString(), buildMethod));
        if (del is not TDelegate typedDelegate)
        {
            throw new ReflectionException("An existing delegate with a signature was encountered");
        }
        return typedDelegate;
    }
}