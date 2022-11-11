using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Jay.Collections.Pooling;

namespace Jay.Enums;

public sealed class EnumLikeComparers<TEnum> : IEqualityComparer<TEnum>,
                                               IComparer<TEnum>
    where TEnum : EnumLike<TEnum>
{
    public static EnumLikeComparers<TEnum> Instance { get; } = new();

    public bool Equals(TEnum? x, TEnum? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Value == y.Value;
    }
    public int GetHashCode(TEnum? enumLike)
    {
        if (enumLike is null) return 0;
        return enumLike.Value.GetHashCode();
    }
    public int Compare(TEnum? x, TEnum? y)
    {
        if (x is null) return y is null ? 0 : -1;
        if (y is null) return 1;
        return x.Value.CompareTo(y.Value);
    }
}



public abstract class FlagsEnumLike<TSelf> : EnumLike<TSelf>,
                                             IBitwiseOperators<FlagsEnumLike<TSelf>, TSelf, TSelf>  
                                             where TSelf : FlagsEnumLike<TSelf>
{
    public static TSelf None { get; } = Create("None", 0UL);
    
    public static TSelf operator &(FlagsEnumLike<TSelf> left, TSelf right)
    {
        return GetEnumLike(left.Value & right.Value);
    }
    public static TSelf operator |(FlagsEnumLike<TSelf> left, TSelf right)
    {
        return GetEnumLike(left.Value | right.Value);
    }
    public static TSelf operator ^(FlagsEnumLike<TSelf> left, TSelf right)
    {
        return GetEnumLike(left.Value ^ right.Value);
    }
    public static TSelf operator ~(FlagsEnumLike<TSelf> value)
    {
        return GetEnumLike(~value.Value);
    }

    protected static TSelf GetEnumLike(ulong value)
    {
        // Check known
        if (_members.TryGetValue(value, out var existing))
            return existing;
        // Create combination
        var sb = StringBuilderPool.Shared.Borrow();
        var flagNames = _members
            .Where(pair => BitOperations.IsPow2(pair.Key))
            .Where(pair => ((value & pair.Value.Value) != 0))
            .Select(pair => pair.Value.Name);
        sb.AppendJoin('|', flagNames);
        string name = StringBuilderPool.Shared.ReturnToString(sb);
        return Create(name, value);
    }
    
    static FlagsEnumLike()
    {
        _getNextValue = () =>
        {
            /* We'll never return 0, that is reserved
             * First member:    1   (1 << 0)
             * Second member:   2   (1 << 1)
             * Third member:    4   (1 << 2)
             * Nth member:          (1 << (n-1))
             *   or                 (1 << _members.Count)
             *   which has a limit of 1 << 63 */
            int count = _members.Count;
            if (count >= 64)
                throw new InvalidOperationException("Cannot have more than 64 defined flags members");
            return 1UL << count;
        };
    }

    protected FlagsEnumLike(string name) 
        : base(name)
    {
    }
    
    protected FlagsEnumLike(string name, ulong value) 
        : base(name, value)
    {
    }
}

public abstract class EnumLike<TSelf> :
    IEquatable<TSelf>,
    IEqualityOperators<EnumLike<TSelf>, TSelf, bool>,
    IComparable<TSelf>,
    IComparisonOperators<EnumLike<TSelf>, TSelf, bool>

    where TSelf : EnumLike<TSelf>
{
    public static bool operator ==(EnumLike<TSelf>? left, TSelf? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Value == right.Value;
    }
    public static bool operator !=(EnumLike<TSelf>? left, TSelf? right)
    {
        if (ReferenceEquals(left, right)) return false;
        if (left is null || right is null) return true;
        return left.Value != right.Value;
    }

    public static bool operator >(EnumLike<TSelf> left, TSelf right)
    {
        return left.Value > right.Value;
    }
    public static bool operator >=(EnumLike<TSelf> left, TSelf right)
    {
        return left.Value >= right.Value;
    }
    public static bool operator <(EnumLike<TSelf> left, TSelf right)
    {
        return left.Value < right.Value;
    }
    public static bool operator <=(EnumLike<TSelf> left, TSelf right)
    {
        return left.Value <= right.Value;
    }
  

    protected static readonly SortedList<ulong, TSelf> _members;
    protected static Func<ulong> _getNextValue;

    static EnumLike()
    {
         _members = new();
        // Default behavior
        _getNextValue = () =>
        {
            // Start with 0 (default behavior)
            return (ulong)_members.Count;
        };
    }

    protected static TSelf Create(string name, ulong value)
    {
        object? instance = Activator.CreateInstance(typeof(TSelf), name, value);
        if (instance is TSelf enumLike) return enumLike;
        throw new InvalidOperationException();
    }
    
    protected readonly ulong _value;
    protected readonly string _name;

    internal ulong Value => _value;

    internal string Name => _name;

    protected EnumLike(string name)
        : this(name, _getNextValue()) { }
    protected EnumLike(string name, ulong value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _name = name;
        _value = value;
        if (!_members.TryAdd<ulong, TSelf>(value, (TSelf)this))
            throw new ArgumentException($"An {typeof(TSelf).Name} already exists with value {value}", nameof(value));
    }

    public int CompareTo(TSelf? enumLike)
    {
        if (enumLike is null) return 1; // I'm after null, it sorts first
        return _value.CompareTo(enumLike._value);
    }

    public bool Equals(TSelf? enumLike)
    {
        return enumLike is not null &&
               _value == enumLike._value;
    }

    public override bool Equals(object? obj)
    {
        return obj is TSelf enumLike && Equals(enumLike);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _name;
    }
}