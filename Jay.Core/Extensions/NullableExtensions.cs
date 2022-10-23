using System.Runtime.CompilerServices;

namespace Jay.Extensions;

public static class NullableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetValue<T>(this Nullable<T> nullable, out T value)
        where T : struct
    {
        value = nullable.GetValueOrDefault();
        return nullable.HasValue;
    }
}