// For some reason, HashCode does not have nullability attributes correctly applied
#pragma warning disable CS8604


namespace Jay;

public static class Hasher
{
    public static int GetHashCode<T>(
        T? value)
    {
        return HashCode.Combine<T>(value);
    }
    
    public static int GetHashCode<T1, T2>(
        T1? value1, T2? value2)
    {
        return HashCode.Combine<T1, T2>(value1, value2);
    }
    
    public static int GetHashCode<T1, T2, T3>(
        T1? value1, T2? value2, T3? value3)
    {
        return HashCode.Combine<T1, T2, T3>(value1, value2, value3);
    }
    
    public static int GetHashCode<T1, T2, T3, T4>(
        T1? value1, T2? value2, T3? value3, T4? value4)
    {
        return HashCode.Combine<T1, T2, T3, T4>(value1, value2, value3, value4);
    }
    
    public static int GetHashCode<T1, T2, T3, T4, T5>(
        T1? value1, T2? value2, T3? value3, T4? value4, T5? value5)
    {
        return HashCode.Combine<T1, T2, T3, T4, T5>(value1, value2, value3, value4, value5);
    }
    
    public static int GetHashCode<T1, T2, T3, T4, T5, T6>(
        T1? value1, T2? value2, T3? value3, T4? value4, T5? value5, T6? value6)
    {
        return HashCode.Combine<T1, T2, T3, T4, T5, T6>(value1, value2, value3, value4, value5, value6);
    }
    
    public static int GetHashCode<T1, T2, T3, T4, T5, T6, T7>(
        T1? value1, T2? value2, T3? value3, T4? value4, T5? value5, T6? value6, T7? value7)
    {
        return HashCode.Combine<T1, T2, T3, T4, T5, T6, T7>(value1, value2, value3, value4, value5, value6, value7);
    }
    
    public static int GetHashCode<T1, T2, T3, T4, T5, T6, T7, T8>(
        T1? value1, T2? value2, T3? value3, T4? value4, T5? value5, T6? value6, T7? value7, T8? value8)
    {
        return HashCode.Combine<T1, T2, T3, T4, T5, T6, T7, T8>(value1, value2, value3, value4, value5, value6, value7, value8);
    }

    public static int GetHashCode<T>(ReadOnlySpan<T> span)
    {
        switch (span.Length)
        {
            case 0: return 0;
            case 1: return HashCode.Combine<T>(span[0]);
            case 2: return HashCode.Combine<T, T>(span[0], span[1]);
            case 3: return HashCode.Combine<T, T, T>(span[0], span[1], span[2]);
            case 4: return HashCode.Combine<T, T, T, T>(span[0], span[1], span[2], span[3]);
            case 5: return HashCode.Combine<T, T, T, T, T>(span[0], span[1], span[2], span[3], span[4]);
            case 6: return HashCode.Combine<T, T, T, T, T, T>(span[0], span[1], span[2], span[3], span[4], span[5]);
            case 7: return HashCode.Combine<T, T, T, T, T, T, T>(span[0], span[1], span[2], span[3], span[4], span[5], span[6]);
            case 8: return HashCode.Combine<T, T, T, T, T, T, T, T>(span[0], span[1], span[2], span[3], span[4], span[5], span[6], span[7]);
            default:
            {
                var hasher = new HashCode();
                for (var i = 0; i < span.Length; i++)
                {
                    hasher.Add<T>(span[i]);
                }
                return hasher.ToHashCode();
            }
        }
    }
    
    public static int GetHashCode<T>(params T[]? array)
    {
        if (array is null) return 0;
        switch (array.Length)
        {
            case 0: return 0;
            case 1: return HashCode.Combine<T>(array[0]);
            case 2: return HashCode.Combine<T, T>(array[0], array[1]);
            case 3: return HashCode.Combine<T, T, T>(array[0], array[1], array[2]);
            case 4: return HashCode.Combine<T, T, T, T>(array[0], array[1], array[2], array[3]);
            case 5: return HashCode.Combine<T, T, T, T, T>(array[0], array[1], array[2], array[3], array[4]);
            case 6: return HashCode.Combine<T, T, T, T, T, T>(array[0], array[1], array[2], array[3], array[4], array[5]);
            case 7: return HashCode.Combine<T, T, T, T, T, T, T>(array[0], array[1], array[2], array[3], array[4], array[5], array[6]);
            case 8: return HashCode.Combine<T, T, T, T, T, T, T, T>(array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7]);
            default:
            {
                var hasher = new HashCode();
                for (var i = 0; i < array.Length; i++)
                {
                    hasher.Add<T>(array[i]);
                }
                return hasher.ToHashCode();
            }
        }
    }

    public static int GetHashCode<T>(IEnumerable<T> enumerable)
    {
        var hasher = new HashCode();
        foreach (T value in enumerable)
        {
            hasher.Add<T>(value);
        }
        return hasher.ToHashCode();
    }

    public static int GetHashCode(params object?[]? objects)
    {
        if (objects is null) return 0;
        switch (objects.Length)
        {
            case 0: return 0;
            case 1: return HashCode.Combine<object?>(objects[0]);
            case 2: return HashCode.Combine<object?, object?>(objects[0], objects[1]);
            case 3: return HashCode.Combine<object?, object?, object?>(objects[0], objects[1], objects[2]);
            case 4: return HashCode.Combine<object?, object?, object?, object?>(objects[0], objects[1], objects[2], objects[3]);
            case 5: return HashCode.Combine<object?, object?, object?, object?, object?>(objects[0], objects[1], objects[2], objects[3], objects[4]);
            case 6: return HashCode.Combine<object?, object?, object?, object?, object?, object?>(objects[0], objects[1], objects[2], objects[3], objects[4], objects[5]);
            case 7: return HashCode.Combine<object?, object?, object?, object?, object?, object?, object?>(objects[0], objects[1], objects[2], objects[3], objects[4], objects[5], objects[6]);
            case 8: return HashCode.Combine<object?, object?, object?, object?, object?, object?, object?, object?>(objects[0], objects[1], objects[2], objects[3], objects[4], objects[5], objects[6], objects[7]);
            default:
            {
                var hasher = new HashCode();
                for (var i = 0; i < objects.Length; i++)
                {
                    hasher.Add<object?>(objects[i]);
                }
                return hasher.ToHashCode();
            }
        }
    }


    public static void Add<T>(this ref HashCode hashCode, ReadOnlySpan<T> span)
    {
        switch (span.Length)
        {
            case 0: return;
            case 1:
                hashCode.Add<T>(span[0]);
                return;
            case 2:  
                hashCode.Add<T>(span[0]);
                hashCode.Add<T>(span[1]);
                return;
            case 3: 
                hashCode.Add<T>(span[0]);
                hashCode.Add<T>(span[1]);
                hashCode.Add<T>(span[2]);
                return;
            case 4: 
                hashCode.Add<T>(span[0]);
                hashCode.Add<T>(span[1]);
                hashCode.Add<T>(span[2]);
                hashCode.Add<T>(span[3]);
                return;
            case 5: 
                hashCode.Add<T>(span[0]);
                hashCode.Add<T>(span[1]);
                hashCode.Add<T>(span[2]);
                hashCode.Add<T>(span[3]);
                hashCode.Add<T>(span[4]);
                return;
            case 6: 
                hashCode.Add<T>(span[0]);
                hashCode.Add<T>(span[1]);
                hashCode.Add<T>(span[2]);
                hashCode.Add<T>(span[3]);
                hashCode.Add<T>(span[4]);
                hashCode.Add<T>(span[5]);
                return;
            case 7: 
                hashCode.Add<T>(span[0]);
                hashCode.Add<T>(span[1]);
                hashCode.Add<T>(span[2]);
                hashCode.Add<T>(span[3]);
                hashCode.Add<T>(span[4]);
                hashCode.Add<T>(span[5]);
                hashCode.Add<T>(span[6]);
                return;
            case 8: 
                hashCode.Add<T>(span[0]);
                hashCode.Add<T>(span[1]);
                hashCode.Add<T>(span[2]);
                hashCode.Add<T>(span[3]);
                hashCode.Add<T>(span[4]);
                hashCode.Add<T>(span[5]);
                hashCode.Add<T>(span[6]);
                hashCode.Add<T>(span[7]);
                return;
            default:
            {
                for (var i = 0; i < span.Length; i++)
                {
                    hashCode.Add<T>(span[i]);
                }
                return;
            }
        }
    }

    public static void Add<T>(this ref HashCode hashCode, params T[] values)
    {
        Add<T>(ref hashCode, (ReadOnlySpan<T>)values);
    }
    
    public static void Add<T>(this ref HashCode hashCode, IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            hashCode.Add<T>(value);
        }
    }
}