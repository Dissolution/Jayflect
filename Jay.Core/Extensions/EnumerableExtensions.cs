namespace Jay.Extensions;

public static class EnumerableExtensions
{
    public static T? OneOrDefault<T>(this IEnumerable<T> source, T? defaultValue = default)
    {
        using var e = source.GetEnumerator();
        if (!e.MoveNext()) return defaultValue;
        T value = e.Current;
        if (e.MoveNext()) return defaultValue;
        return value;
    }
}