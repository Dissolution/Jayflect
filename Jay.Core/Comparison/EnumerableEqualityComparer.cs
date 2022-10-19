using System.Collections;

namespace Jay.Comparison;

public sealed class EnumerableEqualityComparer<T> : IEqualityComparer<T[]>,
                                                    IEqualityComparer<IEnumerable<T>>,
                                                    IEqualityComparer
{
    public static EnumerableEqualityComparer<T> Default { get; } = new();

    private readonly IEqualityComparer<T>? _valueComparer;

    public IEqualityComparer<T> ValueEqualityComparer
    {
        get => _valueComparer ?? EqualityComparer<T>.Default;
        init => _valueComparer = value;
    }

    public EnumerableEqualityComparer()
    {
        _valueComparer = null;
    }

    public EnumerableEqualityComparer(IEqualityComparer<T>? valueEqualityComparer)
    {
        _valueComparer = valueEqualityComparer;
    }

    public bool Equals(ReadOnlySpan<T> left, ReadOnlySpan<T> right)
    {
        return MemoryExtensions.SequenceEqual(left, right, _valueComparer);
    }
    
    public bool Equals(T[]? left, T[]? right)
    {
        return MemoryExtensions.SequenceEqual(left, right, _valueComparer);
    }

    public bool Equals(IEnumerable<T>? left, IEnumerable<T>? right)
    {
        if (left is null) return right is null;
        if (right is null) return false;
        return Enumerable.SequenceEqual(left, right, _valueComparer);
    }

    bool IEqualityComparer.Equals(object? x, object? y)
    {
        if (x is T[] xArray)
            return y is T[] yArray && Equals(xArray, yArray);
        if (x is IEnumerable<T> xEnumerable)
            return y is IEnumerable<T> yEnumerable && Equals(xEnumerable, yEnumerable);
        if (x is T xTyped)
            return y is T yTyped && ValueEqualityComparer.Equals(xTyped, yTyped);
        return false;
    }

    public int GetHashCode(ReadOnlySpan<T> span)
    {
        int len = span.Length;
        if (len == 0) return 0;
        HashCode hasher = new();
        for (var i = 0; i < len; i++)
        {
            hasher.Add<T>(span[i]);
        }
        return hasher.ToHashCode();
    }
    
    public int GetHashCode(T[]? array)
    {
        if (array is null) return 0;
        int len = array.Length;
        if (len == 0) return 0;
        HashCode hasher = new();
        for (var i = 0; i < len; i++)
        {
            hasher.Add<T>(array[i]);
        }
        return hasher.ToHashCode();
    }

    public int GetHashCode(IEnumerable<T>? enumerable)
    {
        if (enumerable is null) return 0;

        HashCode hasher = new();
        var comparer = _valueComparer;
        
        if (enumerable is ICollection<T> collection)
        {
            int count = collection.Count;
            if (count == 0) return 0;
            
            if (enumerable is T[] array)
            {
                for (var i = 0; i < count; i++)
                {
                    hasher.Add<T>(array[i], comparer);
                }
                return hasher.ToHashCode();
            }

            if (collection is IList<T> list)
            {
                for (var i = 0; i < count; i++)
                {
                    hasher.Add<T>(list[i], comparer);
                }
                return hasher.ToHashCode();
            }
        }
        
        using (var e = enumerable.GetEnumerator())
        {
            while (e.MoveNext())
            {
                hasher.Add<T>(e.Current, comparer);
            }
            return hasher.ToHashCode();
        }
    }

    int IEqualityComparer.GetHashCode(object? obj)
    {
        if (obj is T[] array) return GetHashCode(array);
        if (obj is IEnumerable<T> enumerable) return GetHashCode(enumerable);
        if (obj is T typed) return ValueEqualityComparer.GetHashCode(typed);
        return 0;
    }
}