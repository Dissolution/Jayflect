namespace Jayflect.Caching;

public sealed record class MemberDelegate(MemberInfo Member, DelegateSignature DelegateSig)
{
    public static MemberDelegate Create(MemberInfo member, DelegateSignature delegateSig)
    {
        return new MemberDelegate(member, delegateSig);
    }
    
    public static MemberDelegate Create<TMember>(TMember member, DelegateSignature delegateSig)
        where TMember : MemberInfo
    {
        return new MemberDelegate(member, delegateSig);
    }
    
    public static MemberDelegate Create<TDelegate>(MemberInfo member)
        where TDelegate : Delegate
    {
        return new MemberDelegate(member, DelegateSignature.For<TDelegate>());
    }
    
    public static MemberDelegate Create<TMember, TDelegate>(TMember member)
        where TMember : MemberInfo
        where TDelegate : Delegate
    {
        return new MemberDelegate(member, DelegateSignature.For<TDelegate>());
    }
}