using System.Reflection;

namespace Jay.Reflection.Caching;

public sealed class DelegateAndMember : IEquatable<DelegateAndMember>
{
    public static DelegateAndMember Create(MethodSig sig, MemberInfo member)
    {
        ArgumentNullException.ThrowIfNull(sig);
        ArgumentNullException.ThrowIfNull(member);
        return new DelegateAndMember(sig, member);
    }

    public static DelegateAndMember Create<TDelegate>(MemberInfo member)
        where TDelegate : Delegate
    {
        ArgumentNullException.ThrowIfNull(member);
        return new DelegateAndMember(MethodSig.Of<TDelegate>(), member);
    }

    public static DelegateAndMember Create<TMember>(MethodSig sig, TMember member)
        where TMember : MemberInfo
    {
        ArgumentNullException.ThrowIfNull(sig);
        ArgumentNullException.ThrowIfNull(member);
        return new DelegateAndMember(sig, member);
    }

    public static DelegateAndMember Create<TDelegate, TMember>(TMember member)
        where TDelegate : Delegate
        where TMember : MemberInfo
    {
        ArgumentNullException.ThrowIfNull(member);
        return new DelegateAndMember(MethodSig.Of<TDelegate>(), member);
    }

    public MethodSig MethodSig { get; }
    public MemberInfo Member { get; }

    private DelegateAndMember(MethodSig sig, MemberInfo member)
    {
        this.MethodSig = sig;
        this.Member = member;
    }
    public void Deconstruct(out MethodSig methodSig, out MemberInfo member)
    {
        methodSig = MethodSig;
        member = Member;
    }
    public bool Equals(DelegateAndMember? delMem)
    {
        return delMem is not null &&
               delMem.MethodSig == this.MethodSig &&
               delMem.Member == this.Member;
    }

    public override bool Equals(object? obj)
    {
        return obj is DelegateAndMember delMem && Equals(delMem);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MethodSig, Member);
    }

    public override string ToString()
    {
        return $"({MethodSig}){Member}";
    }
}