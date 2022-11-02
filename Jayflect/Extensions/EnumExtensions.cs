namespace Jayflect.Extensions;


public static class EnumExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum And<TEnum>(this TEnum left, TEnum right)
        where TEnum : struct, Enum
    {
        Emit.Ldarg(nameof(left));
        Emit.Ldarg(nameof(right));
        Emit.And();
        return Return<TEnum>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum Or<TEnum>(this TEnum left, TEnum right)
        where TEnum : struct, Enum
    {
        Emit.Ldarg(nameof(left));
        Emit.Ldarg(nameof(right));
        Emit.Or();
        return Return<TEnum>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equal<TEnum>(this TEnum left, TEnum right)
        where TEnum : struct, Enum
    {
        Emit.Ldarg(nameof(left));
        Emit.Ldarg(nameof(right));
        Emit.Ceq();
        return Return<bool>();
    }
    
      
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NotEqual<TEnum>(this TEnum left, TEnum right)
        where TEnum : struct, Enum
    {
        Emit.Ldarg(nameof(left));
        Emit.Ldarg(nameof(right));
        Emit.Cgt();
        return Return<bool>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddFlag<TEnum>(this ref TEnum @enum, TEnum flag)
        where TEnum : struct, Enum
    {
        @enum = @enum.Or(flag);
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Has<TEnum>(this TEnum @enum, TEnum flag)
        where TEnum : struct, Enum
    {
        return @enum.And(flag).NotEqual(default);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAny<TEnum>(this TEnum @enum, TEnum flag)
        where TEnum : struct, Enum
    {
        return @enum.And(flag).NotEqual(default);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAny<TEnum>(this TEnum @enum, TEnum flag1, TEnum flag2)
        where TEnum : struct, Enum
    {
        return @enum.And(flag1.Or(flag2)).NotEqual(default);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAny<TEnum>(this TEnum @enum, TEnum flag1, TEnum flag2, TEnum flag3)
        where TEnum : struct, Enum
    {
        return @enum.And(flag1.Or(flag2).Or(flag3)).NotEqual(default);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAny<TEnum>(this TEnum @enum, params TEnum[] flags)
        where TEnum : struct, Enum
    {
        TEnum flag = default;
        for (int i = 0; i < flags.Length; i++)
        {
            flag.AddFlag(flags[i]);
        }
        return @enum.And(flag).NotEqual(default);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAll<TEnum>(this TEnum @enum, TEnum flag)
        where TEnum : struct, Enum
    {
        return @enum.And(flag).Equal(flag);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAll<TEnum>(this TEnum @enum, TEnum flag1, TEnum flag2)
        where TEnum : struct, Enum
    {
        var flag = flag1.Or(flag2);
        return @enum.And(flag).Equal(flag);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAll<TEnum>(this TEnum @enum, TEnum flag1, TEnum flag2, TEnum flag3)
        where TEnum : struct, Enum
    {
        var flag = flag1.Or(flag2).Or(flag3);
        return @enum.And(flag).Equal(flag);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAll<TEnum>(this TEnum @enum, params TEnum[] flags)
        where TEnum : struct, Enum
    {
        TEnum flag = default;
        for (int i = 0; i < flags.Length; i++)
        {
            flag.AddFlag(flags[i]);
        }
        return @enum.And(flag).Equal(flag);
    }
}