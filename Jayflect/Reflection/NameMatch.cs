using Jay.Enums;
using Jay.Text;

namespace Jay.Reflection;

public readonly struct NameMatch 
{
    public static implicit operator NameMatch(string name) => new NameMatch(name, NameMatchFlags.Exact);
    public static implicit operator NameMatch((string Name, NameMatchFlags MatchType) tuple) => new NameMatch(tuple.Name, tuple.MatchType);

    public static bool operator ==(NameMatch x, NameMatch y) => x.Equals(y);
    public static bool operator !=(NameMatch x, NameMatch y) => !x.Equals(y);
        
    public static readonly NameMatch Any = new NameMatch(null, NameMatchFlags.IgnoreCase);
        
    public readonly string? Name;
    public readonly NameMatchFlags NameMatchFlags;

    public NameMatch(string? name)
    {
        this.Name = name;
        this.NameMatchFlags = NameMatchFlags.Exact;
    }
    public NameMatch(string? name, NameMatchFlags nameMatchFlags)
    {
        this.Name = name;
        this.NameMatchFlags = nameMatchFlags;
    }

    public bool Matches(string? name)
    {
        if (Name is null) return true;
        if (string.IsNullOrWhiteSpace(name)) return false;
        StringComparison comparison;
        if (NameMatchFlags.HasFlag(NameMatchFlags.IgnoreCase))
        {
            comparison = StringComparison.OrdinalIgnoreCase;
        }
        else
        {
            comparison = StringComparison.Ordinal;
        }

        if (NameMatchFlags.HasFlag(NameMatchFlags.Contains))
        {
            return name.Contains(this.Name, comparison);
        }

        if (NameMatchFlags.HasFlag(NameMatchFlags.BeginsWith))
        {
            return name.StartsWith(this.Name, comparison);
        }
        if (NameMatchFlags.HasFlag(NameMatchFlags.EndsWith))
        {
            return name.EndsWith(this.Name, comparison);
        }

        return name.Equals(this.Name, comparison);
    }
        
    public bool Equals(NameMatch nameMatch)
    {
        return NameMatchFlags == nameMatch.NameMatchFlags &&
               Matches(nameMatch.Name);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is NameMatch nameMatch) return Equals(nameMatch);
        if (obj is string name) return Matches(name);
        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hasher = new HashCode();
        if (NameMatchFlags.HasFlag(NameMatchFlags.IgnoreCase))
        {
            hasher.Add(Name, StringComparer.OrdinalIgnoreCase);
        }
        else
        {
            hasher.Add(Name, StringComparer.Ordinal);
        }
        hasher.Add(NameMatchFlags);
        return hasher.ToHashCode();
    }

    public override string ToString()
    {
        using var text = TextBuilder.Borrow();
        if (Name is null)
        {
            text.Append('*');
        }
        else
        {
            if (this.NameMatchFlags.HasFlag<NameMatchFlags>(NameMatchFlags.EndsWith))
            {
                text.Append('*');
            }

            if (this.NameMatchFlags.HasFlag<NameMatchFlags>(NameMatchFlags.IgnoreCase))
            {
                text.Append(Name!.ToUpper());
            }
            else
            {
                text.Append(this.Name!);
            }

            if (this.NameMatchFlags.HasFlag<NameMatchFlags>(NameMatchFlags.BeginsWith))
            {
                text.Append('*');
            }
        }
        return text.ToString();
    }
}