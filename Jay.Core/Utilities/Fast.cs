using System.Runtime.CompilerServices;

namespace Jay.Utilities;

public static class Fast
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equal<T>(ReadOnlySpan<T> left, ReadOnlySpan<T> right)
    {
        return MemoryExtensions.SequenceEqual(left, right);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy<T>(ReadOnlySpan<T> source, Span<T> dest)
    {
        source.CopyTo(dest);
    }
}