using Jay.Dumping;
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
}