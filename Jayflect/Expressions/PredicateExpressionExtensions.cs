namespace Jayflect.Expressions;

public static class PredicateExpressionExtensions
{
    private static Expression ReplaceExpression(this Expression expression, Expression original, Expression replacement)
    {
        return new ExpressionReplacer(original, replacement).Visit(expression);
    }

    private static Expression ReplaceParameter(this Expression expression,
        ParameterExpression original,
        ParameterExpression replacement)
    {
        return new ParameterReplacer(original, replacement).Visit(expression);
    }

    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        if (Equals(left, right)) return left;
        if (Equals(left, PredicateBuilder<T>.True)) return right;
        if (Equals(right, PredicateBuilder<T>.True)) return left;
        if (Equals(left, PredicateBuilder<T>.False) || Equals(right, PredicateBuilder<T>.False))
            return PredicateBuilder<T>.False;

        var body = Expression.AndAlso(left.Body,
            right.Body.ReplaceParameter(right.Parameters[0], left.Parameters[0]));
        return Expression.Lambda<Func<T, bool>>(body, left.Parameters);
    }

    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        if (Equals(left, right)) return left;
        if (Equals(left, PredicateBuilder<T>.False)) return right;
        if (Equals(right, PredicateBuilder<T>.False)) return left;
        if (Equals(left, PredicateBuilder<T>.True) || Equals(right, PredicateBuilder<T>.True))
            return PredicateBuilder<T>.True;

        var body = Expression.OrElse(left.Body,
            right.Body.ReplaceParameter(right.Parameters[0], left.Parameters[0]));
        return Expression.Lambda<Func<T, bool>>(body, left.Parameters);
    }
}