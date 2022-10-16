using Jay.Collections;
using Jayflect;

namespace Jay.Reflection.Internal;

internal class TypeCache
{
    private static readonly ConcurrentTypeDictionary<TypeCacheInfo> _typeCaches;

    static TypeCache()
    {
        _typeCaches = new();
    }

    internal static TypeCacheInfo<T> GetCache<T>()
    {
        return (_typeCaches.GetOrAdd<T>(_ => new TypeCacheInfo<T>()) as TypeCacheInfo<T>)!;
    }

    internal static TypeCacheInfo GetCache(Type type)
    {
        return _typeCaches.GetOrAdd(type, t => new TypeCacheInfo(t));
    }

    public static bool IsUnmanaged<T>()
    {
        return GetCache<T>().IsUnmanaged;
    }

    public static bool IsUnmanaged(Type type)
    {
        return GetCache(type).IsUnmanaged;
    }

    public static bool IsReferenceOrContainsReferences<T>()
    {
        return GetCache<T>().IsReferenceOrContainsReferences;
    }

    public static bool IsReferenceOrContainsReferences(Type type)
    {
        return GetCache(type).IsReferenceOrContainsReferences;
    }

    public static object? Default(Type? type)
    {
        if (type is null) return null;
        return GetCache(type).GetDefault();
    }

    public static T? Default<T>()
    {
        return GetCache<T>().GetDefault();
    }

    [return: NotNullIfNotNull("type")]
    public static object? Uninitialized(Type? type)
    {
        if (type is null) return null;
        return RuntimeHelpers.GetUninitializedObject(type);
    }

    public static T Uninitialized<T>()
    {
        return (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
    }

    public static bool Equals<T>(T? left, T? right)
    {
        return EqualityComparer<T>.Default.Equals(left, right);
    }

    public static int? SizeOf(Type? type)
    {
        if (type is null)
            return null;
        return GetCache(type).Size;
    }

    public static int? SizeOf<T>()
    {
        return GetCache<T>().Size;
    }
}

internal static class TypeCache<T>
{
    public static int? Size { get; }

    static TypeCache()
    {
        Size = Result.InvokeOrDefault<int?>(() => Danger.SizeOf<T>(), null);
    }
}