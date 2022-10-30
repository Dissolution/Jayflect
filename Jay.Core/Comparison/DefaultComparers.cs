using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Jay.Collections;
using Jay.Extensions;
using Jay.Validation;

namespace Jay.Comparison;



public sealed class DefaultComparers : IEqualityComparer<object?>, IEqualityComparer, 
                                       IComparer<object?>, IComparer
{
    private static readonly ConcurrentTypeDictionary<IEqualityComparer> _equalityComparerCache = new();
    private static readonly ConcurrentTypeDictionary<IComparer> _comparerCache = new();

    public static DefaultComparers Instance { get; } = new();

    public static void Replace<T>(IEqualityComparer<T>? equalityComparer)
    {
        IEqualityComparer comparer = equalityComparer as IEqualityComparer ??
                                     EqualityComparer<T>.Default;
        _equalityComparerCache.AddOrUpdate<T>(comparer, _ => comparer);
    }
    public static void Replace(Type type, IEqualityComparer? equalityComparer)
    {
        ArgumentNullException.ThrowIfNull(type);
        equalityComparer ??= FindEqualityComparer(type);
        _equalityComparerCache.AddOrUpdate(type, equalityComparer, (_,_) => equalityComparer);
    }
    
    public static IEqualityComparer<T> Equality<T>()
    {
        return _equalityComparerCache.GetOrAdd<T>(_ => 
                FindEqualityComparer<T>().ValidateInstanceOf<IEqualityComparer>())
            .ValidateInstanceOf<IEqualityComparer<T>>();
    }

    public static IEqualityComparer Equality(Type? type)
    {
        if (type is null) return Instance;
        return _equalityComparerCache.GetOrAdd(type, FindEqualityComparer);
    }

    private static bool IsEnumerable(Type type, [NotNullWhen(true)] out Type? genericType)
    {
        if (type.IsArray && type.GetArrayRank() == 1)
        {
            genericType = type.GetElementType()!;
            return true;
        }
        if (type.Implements(typeof(IEnumerable<>)))
        {
            genericType = type.GenericTypeArguments[0];
            return true;
        }
        genericType = null;
        return false;
    }

    public static IEqualityComparer<T> FindEqualityComparer<T>()
    {
        if (IsEnumerable(typeof(T), out _))
        {
            return _equalityComparerCache.GetOrAdd(typeof(T), FindEqualityComparer)
                .ValidateInstanceOf<IEqualityComparer<T>>();
        }
        
        return EqualityComparer<T>.Default;
    }

    private static IEqualityComparer FindEqualityComparer(Type type)
    {
        if (type == typeof(object)) return Instance;

        // Special handler for IEnumerable<T> and T[]
        if (IsEnumerable(type, out var genericType))
        {
            return typeof(EnumerableEqualityComparer<>)
                .MakeGenericType(genericType)
                .GetProperty("Default", BindingFlags.Public | BindingFlags.Static)!
                .GetValue(null)
                .ValidateInstanceOf<IEqualityComparer>();
        }

        return typeof(EqualityComparer<>).MakeGenericType(type)
            .GetProperty("Default", BindingFlags.Public | BindingFlags.Static)!
            .GetValue(null)
            .ValidateInstanceOf<IEqualityComparer>();
    }

    
    
    
    public static IComparer<T> Comparision<T>()
    {

        return Comparer<T>.Default;
    }

    public static IComparer Comparision(Type? type)
    {
        if (type is null) return Instance;
        return _comparerCache.GetOrAdd(type,
            t => (typeof(Comparer<>)
                .MakeGenericType(t)
                .GetProperty("Default", BindingFlags.Public | BindingFlags.Static)!
                .GetValue(null) as IComparer)!);
    }

    public bool Equals<T>(T? left, T? right)
    {
        return Equality<T>().Equals(left, right);
    }
    
    public new bool Equals(object? left, object? right)
    {
        if (left is null) return right is null;
        var leftType = left.GetType();
        return Equality(leftType).Equals(left, right);
    }

    bool IEqualityComparer<object?>.Equals(object? left, object? right) => this.Equals(left, right);
    bool IEqualityComparer.Equals(object? left, object? right) => this.Equals(left, right);

    public int GetHashCode<T>(T? value)
    {
        if (value is null) return 0;
        return Equality<T>().GetHashCode(value);
    }
    
    public int GetHashCode(object? obj)
    {
        if (obj is null) return 0;
        return Equality(obj.GetType()).GetHashCode(obj);
    }
    int IEqualityComparer.GetHashCode(object? obj) => GetHashCode(obj);

    public int Compare<T>(T? left, T? right)
    {
        return Comparision<T>().Compare(left, right);
    }
    
    public int Compare(object? left, object? right)
    {
        if (left is null)
        {
            if (right is null) return 0;
            return -1;
        }
        return Comparision(left.GetType()).Compare(left, right);
    }
    int IComparer.Compare(object? left, object? right) => this.Compare(left, right);
}