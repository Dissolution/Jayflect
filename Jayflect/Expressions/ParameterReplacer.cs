namespace Jayflect.Expressions;

public sealed class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _original;
    private readonly ParameterExpression _replacement;

    public ParameterReplacer(ParameterExpression original, ParameterExpression replacement)
    {
        _original = original;
        _replacement = replacement;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (Equals(_original, node))
        {
            node = _replacement;
        }
        return base.VisitParameter(node);
    }
}