using System.Reflection;

namespace Jay.Reflection;

/// <summary>
/// https://www.codeproject.com/Articles/1241363/Expression-Tree-Traversal-Via-Visitor-Pattern-in-P
/// </summary>
/*public sealed class ArgumentValidation : ExpressionVisitorBase
{
    public void ThrowIf<T>(T value, 
                                  Expression<Func<T, bool>> predicate,
                                  [CallerArgumentExpression("value")] string argumentName = "")
    {
        Visit(predicate.Body);
        
        LambdaExpression lambda = predicate;
        foreach (var binaryExpression in SplitBinaries(lambda.Body as BinaryExpression))
        {
            if (binaryExpression.NodeType == ExpressionType.Equal)
            {
                ParameterExpression? valueParameter = null;
                Expression? otherExpr = null;
                if (binaryExpression.Right is ParameterExpression rightParameter && rightParameter.Type == typeof(T))
                {
                    valueParameter = rightParameter;
                    otherExpr = binaryExpression.Left;
                }
                else if (binaryExpression.Left is ParameterExpression leftParameter && leftParameter.Type == typeof(T))
                {
                    valueParameter = leftParameter;
                    otherExpr = binaryExpression.Right;
                }
                else
                {
                    Debugger.Break();
                }

                if (otherExpr is ConstantExpression otherConst)
                {
                    if (otherConst.Value is null)
                    {
                        if (value is null)
                        {
                            throw new ArgumentNullException(argumentName);
                        }
                        continue;
                    }
                    Debugger.Break();
                }
            }

            Debugger.Break();
        }
    }
}*/

internal static class EmitValidation
{
    public static void IsValue([AllowNull, NotNull] Type? valueType,
                               [CallerArgumentExpression("valueType")]
                               string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(valueType, paramName);
        if (!valueType.IsValueType)
        {
            throw new ArgumentException($"{paramName} is not a value type", paramName);
        }
    }

    public static void IsClass([AllowNull, NotNull] Type? classType,
                               [CallerArgumentExpression("classType")]
                               string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(classType, paramName);
        if (classType.IsValueType)
        {
            throw new ArgumentException($"{paramName} is not a class type", paramName);
        }
    }

    public static void IsStatic([AllowNull, NotNull] FieldInfo? field,
                                [CallerArgumentExpression("field")] 
                                string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(field, paramName);
        if (!field.IsStatic)
        {
            throw new ArgumentException($"{paramName} is not a static Field");
        }
    }

    public static void IsStatic([AllowNull, NotNull] MethodInfo? method,
                                [CallerArgumentExpression("method")]
                                string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(method, paramName);
        if (!method.IsStatic)
        {
            throw new ArgumentException($"{paramName} is not a static Method");
        }
    }

    public static void IsInstance([AllowNull, NotNull] MethodInfo? method,
                                  [CallerArgumentExpression("method")]
                                  string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(method, paramName);
        if (!method.IsStatic)
        {
            throw new ArgumentException($"{paramName} is not a static Method");
        }
    }

    public static void DelegateCreated<TDelegate>([AllowNull, NotNull] TDelegate @delegate,
                                                  [CallerArgumentExpression("delegate")] string? paramName = null)
    {
        if (@delegate is null)
        {
            throw new InvalidOperationException($"Unable to create a {typeof(TDelegate)}");
        }
    }
}