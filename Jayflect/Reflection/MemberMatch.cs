using System.Reflection;
using Jay.Enums;
using Jayflect;

namespace Jay.Reflection;

public readonly struct MemberMatch
{
    public static implicit operator MemberMatch(Visibility visibility) => new MemberMatch(visibility, NameMatch.Any, MemberTypes.All);
    public static implicit operator MemberMatch(string? name) => new MemberMatch(Visibility.Any, new NameMatch(name), MemberTypes.All);
    public static implicit operator MemberMatch(MemberTypes memberTypes) => new MemberMatch(Visibility.Any, NameMatch.Any, memberTypes);
        
    public static readonly MemberMatch All = new MemberMatch(Visibility.Any, NameMatch.Any, MemberTypes.All);
        
    public readonly Visibility Visibility;
    public readonly NameMatch NameMatch;
    public readonly MemberTypes MemberTypes;

    public MemberMatch(Visibility visibility, NameMatch nameMatch, MemberTypes memberTypes)
    {
        Visibility = visibility;
        NameMatch = nameMatch;
        MemberTypes = memberTypes;
    }

    public bool Matches(MemberInfo member)
    {
        ArgumentNullException.ThrowIfNull(member);
        if (!this.Visibility.HasAnyFlags<Visibility>(member.Visibility()))
            return false;
        if (!this.NameMatch.Matches(member.Name))
            return false;
        if (!this.MemberTypes.HasFlag<MemberTypes>(member.MemberType))
            return false;
        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is MemberMatch search)
            return search.Visibility == this.Visibility &&
                   search.NameMatch == this.NameMatch &&
                   search.MemberTypes == this.MemberTypes;
        if (obj is MemberInfo member)
            return Matches(member);
        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Visibility, NameMatch, MemberTypes);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Visibility} {MemberTypes} {NameMatch}";
    }
}