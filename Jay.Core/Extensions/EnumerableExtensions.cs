namespace Jay.Extensions;


public delegate bool SelectWhere<in TIn, TOut>(TIn input, out TOut output);

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

    public static IEnumerable<T> Double<T>(this IEnumerable<T> source)
    {
        using var e = source.GetEnumerator();
        if (!e.MoveNext())
            throw new ArgumentException("There are no elements", nameof(source));
        var firstValue = e.Current;
        if (!e.MoveNext())
            throw new ArgumentException("There is only one element", nameof(source));
        var secondValue = e.Current;
        if (e.MoveNext())
            throw new ArgumentException("There are more than two elements", nameof(source));
        yield return firstValue;
        yield return secondValue;
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
            catch (Exception)
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
            catch (Exception)
            {
                // ignore this, but continue enumerating
                continue;
            }
            yield return current;
        }
    }


    public static IEnumerable<TOut> SelectWhere<TIn, TOut>(this IEnumerable<TIn> source, SelectWhere<TIn, TOut> selectWherePredicate)
    {
        foreach (TIn input in source)
        {
            if (selectWherePredicate(input, out var output))
            {
                yield return output;
            }
        }
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
    {
        return source.Where(value => value is not null)!;
    }
}