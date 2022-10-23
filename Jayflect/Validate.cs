using Jay.Extensions;

namespace Jayflect;

internal static class Validate
{
    public static void IsDelegateType([AllowNull, NotNull] Type? type, [CallerArgumentExpression(nameof(type))] string? typeParamName = null)
    {
        if (type is null)
        {
            throw new ArgumentNullException(typeParamName);
        }
        if (!type.Implements<Delegate>())
        {
            throw new ArgumentException(Dump($"The specified type '{type}' is not a Delegate"), typeParamName);
        }
    }
    
    public static void IsExceptionType([AllowNull, NotNull] Type? type, [CallerArgumentExpression(nameof(type))] string? typeParamName = null)
    {
        if (type is null)
        {
            throw new ArgumentNullException(typeParamName);
        }
        if (!type.Implements<Exception>())
        {
            throw new ArgumentException(Dump($"The specified type '{type}' is not an Exception"), typeParamName);
        }
    }
    
    public static void IsValueType([AllowNull, NotNull] Type? type, [CallerArgumentExpression(nameof(type))] string? typeParamName = null)
    {
        if (type is null)
        {
            throw new ArgumentNullException(typeParamName);
        }
        if (!type.IsValueType)
        {
            throw new ArgumentException(Dump($"The given type '{type}' must be a value type"), typeParamName);
        }
    }
    
    public static void IsClassOrInterfaceType([AllowNull, NotNull] Type? type, [CallerArgumentExpression(nameof(type))] string? typeParamName = null)
    {
        if (type is null)
        {
            throw new ArgumentNullException(typeParamName);
        }
        if (!type.IsClass && !type.IsInterface)
        {
            throw new ArgumentException(Dump($"The given type '{type}' must be a class or interface type"), typeParamName);
        }
    }
}