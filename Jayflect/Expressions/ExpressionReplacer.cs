namespace Jayflect.Expressions;

public sealed class ExpressionReplacer : ExpressionVisitor
{
    private readonly Expression _original;
    private readonly Expression _replacement;

    public ExpressionReplacer(Expression original, Expression replacement)
    {
        _original = original;
        _replacement = replacement;
    }

    public override Expression Visit(Expression? node)
    {
        if (Equals(_original, node))
            return _replacement;
        return base.Visit(node)!;
    }
}