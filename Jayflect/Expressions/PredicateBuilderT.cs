namespace Jayflect.Expressions;

public static class PredicateBuilder<T>
{
    /// <summary>
    /// Creates a <see cref="Predicate{T}"/> <see cref="Expression"/> that evaluates to <c>true</c>
    /// </summary>
    public static readonly Expression<Func<T, bool>> True = (item => true);

    /// <summary>
    /// Creates a <see cref="Predicate{T}"/> <see cref="Expression"/> that evaluates to <c>false</c>
    /// </summary>
    public static readonly Expression<Func<T, bool>> False = (item => false);

    /// <summary>
    /// Creates a <see cref="Predicate{T}"/> <see cref="Expression"/> evaluating the given <paramref name="predicate"/>
    /// </summary>
    public static Expression<Func<T, bool>> Create(Func<T, bool> predicate)
        => (value => (predicate(value)));
}