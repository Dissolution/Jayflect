using Jay.Extensions;

namespace Jayflect.Extensions;

public static class ExpressionExtensions
{
    private static IEnumerable<T> OfType<T>(object? obj)
    {
        return obj switch
               {
                   T value => new T[1] { value },
                   IEnumerable<T> values => values,
                   _ => Array.Empty<T>(),
               };
    }

    /// <summary>
    /// Returns all <typeparamref name="T"/> values referenced in this <paramref name="expression"/>
    /// </summary>
    public static IEnumerable<T> ExtractValues<T>(this Expression? expression)
    {
        if (expression is null) yield break;
        if (expression is T exprValue) yield return exprValue;
        
        switch (expression)
        {
            case BinaryExpression binaryExpression:
            {
                foreach (T value in binaryExpression.Conversion.ExtractValues<T>())
                {
                    yield return value;
                }
                
                foreach (T value in binaryExpression.Left.ExtractValues<T>())
                {
                    yield return value;
                }
                
                if (binaryExpression.Method is T methodValue)
                {
                    yield return methodValue;
                }

                foreach (T value in binaryExpression.Right.ExtractValues<T>())
                {
                    yield return value;
                }

                break;
            }
            case BlockExpression blockExpression:
            {
                foreach (ParameterExpression variable in blockExpression.Variables)
                foreach (T value in variable.ExtractValues<T>())
                {
                    yield return value;
                }

                foreach (Expression expr in blockExpression.Expressions)
                foreach (T value in expr.ExtractValues<T>())
                {
                    yield return value;
                }

                // blockExpression.Result is contained in .Expressions
                
                break;
            }
            case ConditionalExpression conditionalExpression:
            {
                foreach (T value in conditionalExpression.Test.ExtractValues<T>())
                {
                    yield return value;
                }
                
                foreach (T value in conditionalExpression.IfTrue.ExtractValues<T>())
                {
                    yield return value;
                }

                foreach (T value in conditionalExpression.IfFalse.ExtractValues<T>())
                {
                    yield return value;
                }

                break;
            }
            case ConstantExpression constantExpression:
            {
                foreach (T value in OfType<T>(constantExpression.Value))
                {
                    yield return value;
                }

                break;
            }
            // ReSharper disable once UnusedVariable
            case DebugInfoExpression debugInfoExpression:
            {
                break;
            }
            // ReSharper disable once UnusedVariable
            case DefaultExpression defaultExpression:
            {
                break;
            }
            case DynamicExpression dynamicExpression:
            {
                if (dynamicExpression.Binder is T returnValue)
                {
                    yield return returnValue;
                }
                
                foreach (Expression argument in dynamicExpression.Arguments)
                foreach (T value in argument.ExtractValues<T>())
                {
                    yield return value;
                }

                break;
            }
            case GotoExpression gotoExpression:
            {
                if (gotoExpression.Kind is T kindValue)
                {
                    yield return kindValue;
                }

                if (gotoExpression.Target is T targetValue)
                {
                    yield return targetValue;
                }
                
                foreach (T value in gotoExpression.Value.ExtractValues<T>())
                {
                    yield return value;
                }

                break;
            }
            case IndexExpression indexExpression:
            {
                foreach (T value in indexExpression.Object.ExtractValues<T>())
                {
                    yield return value;
                }

                if (indexExpression.Indexer is T indexerValue)
                {
                    yield return indexerValue;
                }
                
                foreach (Expression argument in indexExpression.Arguments)
                foreach (T value in argument.ExtractValues<T>())
                {
                    yield return value;
                }

                break;
            }
            case InvocationExpression invocationExpression:
            {
                foreach (T value in invocationExpression.Expression.ExtractValues<T>())
                {
                    yield return value;
                }

                foreach (Expression argument in invocationExpression.Arguments)
                foreach (T value in argument.ExtractValues<T>())
                {
                    yield return value;
                }

                break;
            }
            case LabelExpression labelExpression:
            {
                if (labelExpression.Target is T targetValue)
                {
                    yield return targetValue;
                }
                
                foreach (T value in labelExpression.DefaultValue.ExtractValues<T>())
                {
                    yield return value;
                }

                break;
            }
            case LambdaExpression lambdaExpression:
            {
                foreach (T value in lambdaExpression.Body.ExtractValues<T>())
                {
                    yield return value;
                }

                foreach (ParameterExpression parameter in lambdaExpression.Parameters)
                foreach (T value in parameter.ExtractValues<T>())
                {
                    yield return value;
                }

                break;
            }
            case ListInitExpression listInitExpression:
            {
                foreach (T value in listInitExpression.NewExpression.ExtractValues<T>())
                {
                    yield return value;
                }

                foreach (T value in OfType<T>(listInitExpression.Initializers))
                {
                    yield return value;
                }

                break;
            }
            case LoopExpression loopExpression:
            {
                foreach (T value in loopExpression.Body.ExtractValues<T>())
                {
                    yield return value;
                }

                if (loopExpression.BreakLabel is T breakLabelTarget)
                {
                    yield return breakLabelTarget;
                }

                if (loopExpression.ContinueLabel is T continueLabelTarget)
                {
                    yield return continueLabelTarget;
                }

                break;
            }
            case MemberExpression memberExpression:
            {
                if (memberExpression.Member is T returnValue)
                {
                    yield return returnValue;
                }
                
                foreach (T value in memberExpression.Expression.ExtractValues<T>())
                {
                    yield return value;
                }
                
                break;
            }
            case MemberInitExpression memberInitExpression:
            {
                foreach (T value in memberInitExpression.NewExpression.ExtractValues<T>())
                {
                    yield return value;
                }

                foreach (T value in OfType<T>(memberInitExpression.Bindings))
                {
                    yield return value;
                }
                
                break;
            }
            case MethodCallExpression methodCallExpression:
            {
                foreach (T value in methodCallExpression.Object.ExtractValues<T>())
                {
                    yield return value;
                }

                foreach (Expression argument in methodCallExpression.Arguments)
                foreach (T value in argument.ExtractValues<T>())
                {
                    yield return value;
                }

                break;
            }
            case NewArrayExpression newArrayExpression:
            {
                foreach (Expression expr in newArrayExpression.Expressions)
                foreach (T value in expr.ExtractValues<T>())
                {
                    yield return value;
                }
                
                break;
            }
            case NewExpression newExpression:
            {
                if (newExpression.Constructor is T ctorValue)
                {
                    yield return ctorValue;
                }
                
                foreach (Expression argument in newExpression.Arguments)
                foreach (T value in argument.ExtractValues<T>())    
                {
                    yield return value;
                }

                foreach (T value in OfType<T>(newExpression.Members))
                {
                    yield return value;
                }
                
                break;
            }
            // ReSharper disable once UnusedVariable
            case ParameterExpression parameterExpression:
            {
                break;
            }
            case RuntimeVariablesExpression runtimeVariablesExpression:
            {
                foreach (ParameterExpression variable in runtimeVariablesExpression.Variables)
                foreach (T value in variable.ExtractValues<T>())    
                {
                    yield return value;
                }
                
                break;
            }
            case SwitchExpression switchExpression:
            {
                if (switchExpression.Comparison is T comparisonValue)
                {
                    yield return comparisonValue;
                }

                foreach (SwitchCase switchCase in switchExpression.Cases)
                {
                    foreach (Expression testValue in switchCase.TestValues)
                    foreach (T value in testValue.ExtractValues<T>())
                    {
                        yield return value;
                    }

                    foreach (T value in switchCase.Body.ExtractValues<T>())
                    {
                        yield return value;
                    }
                }

                foreach (T value in switchExpression.DefaultBody.ExtractValues<T>())
                {
                    yield return value;
                }

                break;
            }
            case TryExpression tryExpression:
            {
                foreach (T value in tryExpression.Body.ExtractValues<T>())
                {
                    yield return value;
                }

                foreach (CatchBlock handler in tryExpression.Handlers)
                {
                    foreach (T value in handler.Filter.ExtractValues<T>())
                    {
                        yield return value;
                    }

                    foreach (T value in handler.Variable.ExtractValues<T>())
                    {
                        yield return value;
                    }

                    foreach (T value in handler.Body.ExtractValues<T>())
                    {
                        yield return value;
                    }
                }

                foreach (T value in tryExpression.Fault.ExtractValues<T>())
                {
                    yield return value;
                }

                foreach (T value in tryExpression.Finally.ExtractValues<T>())
                {
                    yield return value;
                }
                
                break;
            }
            case TypeBinaryExpression typeBinaryExpression:
            {
                foreach (T value in typeBinaryExpression.Expression.ExtractValues<T>())
                {
                    yield return value;
                }
                
                break;
            }
            case UnaryExpression unaryExpression:
            {
                if (unaryExpression.Method is T methodValue)
                {
                    yield return methodValue;
                }
                
                foreach (T value in unaryExpression.Operand.ExtractValues<T>())
                {
                    yield return value;
                }

                break;
            }
            default:
            {
                throw new NotImplementedException($"{expression.GetType()} has not yet been implemented");
            }
        }
    }

    public static IEnumerable<Expression> SelfAndDescendants(this Expression? expression)
    {
        return expression.ExtractValues<Expression>();
    }

    /// <summary>
    /// Returns all <see cref="MemberInfo"/>s referenced in this <paramref name="expression"/>
    /// </summary>
    public static IEnumerable<MemberInfo> ExtractMembers(this Expression? expression)
    {
        return expression.ExtractValues<MemberInfo>();
    }

    public static TMember? ExtractMember<TMember>(this Expression? expression)
        where TMember : MemberInfo
    {
        return expression.ExtractValues<TMember>().OneOrDefault();
    }
}

public abstract class ExpressionVisitorBase : ExpressionVisitor
{
    protected static readonly IReadOnlyDictionary<ExpressionType, string> _operators;

    static ExpressionVisitorBase()
    {
        _operators = new Dictionary<ExpressionType, string>
        {
            { ExpressionType.Not, "!" },
            { ExpressionType.Equal, "==" },
            { ExpressionType.NotEqual, "!=" },
            { ExpressionType.AndAlso, "&&" },
            { ExpressionType.OrElse, "||" },
            { ExpressionType.LessThan, "<" },
            { ExpressionType.LessThanOrEqual, "<=" },
            { ExpressionType.GreaterThan, ">" },
            { ExpressionType.GreaterThanOrEqual, ">=" }
        };
    }
}
