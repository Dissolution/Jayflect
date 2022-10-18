using Jayflect.Runtime;

namespace Jayflect.Caching;

public sealed record class MemberDelegate(MemberInfo Member, DelegateSignature DelegateSignature)
{
    public static MemberDelegate Create(MemberInfo member, DelegateSignature delegateSignature)
    {
        return new MemberDelegate(member, delegateSignature);
    }
    
    public static MemberDelegate Create<TMember>(TMember member, DelegateSignature delegateSignature)
        where TMember : MemberInfo
    {
        return new MemberDelegate(member, delegateSignature);
    }
    
    public static MemberDelegate Create<TDelegate>(MemberInfo member)
        where TDelegate : Delegate
    {
        return new MemberDelegate(member, DelegateSignature.FromDelegate<TDelegate>());
    }
    
    public static MemberDelegate Create<TMember, TDelegate>(TMember member)
        where TMember : MemberInfo
        where TDelegate : Delegate
    {
        return new MemberDelegate(member, DelegateSignature.FromDelegate<TDelegate>());
    }
}