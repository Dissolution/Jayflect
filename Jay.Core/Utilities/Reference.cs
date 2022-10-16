using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Jay;

public static class Reference
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? CompareExchange<T>(ref T? location, T? value, T? comparand)
        where T : class
    {
        var original = location;
        if (ReferenceEquals(location, comparand))
        {
            location = value;
        }
        return original;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? Exchange<T>(ref T? location, T? value)
    {
        T? original = location;
        location = value;
        return original;
    }
}