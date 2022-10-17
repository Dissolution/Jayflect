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

    public static IEnumerable<T> IgnoreExceptions<T>(this IEnumerable<T> enumerable)
    {
        using var enumerator = Result.InvokeOrDefault(() => enumerable.GetEnumerator());
        if (enumerator is null) yield break;

        while (true)
        {
            // Move next
            try
            {
                if (!enumerator.MoveNext())
                    yield break;
            }
            catch (Exception ex)
            {
                // ignore this, stop enumerating
                yield break;
            }
            
            // Yield current
            T current;
            try
            {
                current = enumerator.Current;
            }
            catch (Exception ex)
            {
                // ignore this, but continue enumerating
                continue;
            }
            yield return current;
        }
    }
}