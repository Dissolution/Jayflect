/*using static InlineIL.IL;

namespace Jay.Reflection;

public class Unmanaged
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SizeOf<T>()
        where T : unmanaged
    {
        Emit.Sizeof<T>();
        return Return<int>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Not<T>(T value)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(value));
        Emit.Not();
        return Return<T>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Negate<T>(T value)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(value));
        Emit.Neg();
        return Return<T>();
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(T val1, T val2)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(val1));
        Emit.Ldarg(nameof(val2));
        Emit.Cgt();
        Emit.Brtrue("lblGT");
        Emit.Ldarg(nameof(val1));
        Emit.Ret();
        MarkLabel("lblGT");
        Emit.Ldarg(nameof(val2));
        Emit.Ret();
        throw Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Min<T>(T val1, T val2, T val3)
        where T : unmanaged
    {
        return Min(Min(val1, val2), val3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals<T>(T left, T right)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(left));
        Emit.Ldarg(nameof(right));
        Emit.Ceq();
        return Return<bool>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThan<T>(T left, T right)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(left));
        Emit.Ldarg(nameof(right));
        Emit.Cgt();
        return Return<bool>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanOrEqual<T>(T left, T right)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(right));
        Emit.Ldarg(nameof(left));
        Emit.Clt();
        return Return<bool>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThan<T>(T left, T right)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(left));
        Emit.Ldarg(nameof(right));
        Emit.Clt();
        return Return<bool>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanOrEqual<T>(T left, T right)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(right));
        Emit.Ldarg(nameof(left));
        Emit.Cgt();
        return Return<bool>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Or<T>(T left, T right)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(left));
        Emit.Ldarg(nameof(right));
        Emit.Or();
        return Return<T>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T And<T>(T left, T right)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(left));
        Emit.Ldarg(nameof(right));
        Emit.And();
        return Return<T>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Xor<T>(T left, T right)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(left));
        Emit.Ldarg(nameof(right));
        Emit.Xor();
        return Return<T>();
    }
}*/