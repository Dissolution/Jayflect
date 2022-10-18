using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Jay.Extensions;

public static class GenericExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is<TOut>(this object? obj, [NotNullWhen(true)] out TOut? output)
    {
        if (obj is TOut)
        {
            output = (TOut)obj;
            return true;
        }
        output = default;
        return false;
    }
}