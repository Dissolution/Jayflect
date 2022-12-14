namespace Jayflect.Expressions;

public static class PredicateBuilder
{
    /// <summary>
    /// Creates a <see cref="Predicate{T}"/> <see cref="Expression"/> that evaluates to <c>true</c>
    /// </summary>
    public static Expression<Func<T, bool>> True<T>() => PredicateBuilder<T>.True;

    /// <summary>
    /// Creates a <see cref="Predicate{T}"/> <see cref="Expression"/> that evaluates to <c>false</c>
    /// </summary>
    public static Expression<Func<T, bool>> False<T>() => PredicateBuilder<T>.False;

    /// <summary>
    /// Creates a <see cref="Predicate{T}"/> <see cref="Expression"/> evaluating the given <paramref name="predicate"/>
    /// </summary>
    public static Expression<Func<T, bool>> Create<T>(Func<T, bool> predicate) => (t => predicate(t));
}