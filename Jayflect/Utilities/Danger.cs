namespace Jayflect;

public static unsafe class Danger
{
#region Read / Write

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(void* source)
    {
        Emit.Ldarg(nameof(source));
        Emit.Ldobj<T>();
        return Return<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadUnaligned<T>(void* source)
    {
        Emit.Ldarg(nameof(source));
        Emit.Unaligned(1);
        Emit.Ldobj<T>();
        return Return<T>();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadUnaligned<T>(in byte source)
    {
        Emit.Ldarg(nameof(source));
        Emit.Unaligned(1);
        Emit.Ldobj<T>();
        return Return<T>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadUnaligned<T>(ReadOnlySpan<byte> source)
    {
        Emit.Ldarg(nameof(source));
        Emit.Unaligned(1);
        Emit.Ldobj<T>();
        return Return<T>();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(void* destination, T value)
    {
        Emit.Ldarg(nameof(destination));
        Emit.Ldarg(nameof(value));
        Emit.Stobj<T>();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnaligned<T>(void* destination, T value)
    {
        Emit.Ldarg(nameof(destination));
        Emit.Ldarg(nameof(value));
        Emit.Unaligned(1);
        Emit.Stobj<T>();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnaligned<T>(ref byte destination, T value)
    {
        Emit.Ldarg(nameof(destination));
        Emit.Ldarg(nameof(value));
        Emit.Unaligned(1);
        Emit.Stobj<T>();
    }

#endregion

#region CopyTo

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo<T>(in T source, void* destination)
    {
        Emit.Ldarg(nameof(destination));
        Emit.Ldarg(nameof(source));
        Emit.Ldobj<T>();
        Emit.Stobj<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo<T>(void* source, ref T destination)
    {
        Emit.Ldarg(nameof(destination));
        Emit.Ldarg(nameof(source));
        Emit.Ldobj<T>();
        Emit.Stobj<T>();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo<TIn, TOut>(in TIn source, ref TOut destination)
    {
        Emit.Ldarg(nameof(destination));
        Emit.Ldarg(nameof(source));
        Emit.Ldobj<TIn>();
        Emit.Stobj<TOut>();
    }

#endregion

#region As / Cast / Box / Unbox

#region To

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* VoidPointerToPointer<T>(void* pointer)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(pointer));
        Emit.Conv_I();
        return ReturnPointer<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T VoidPointerToRef<T>(void* pointer)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(pointer));
        Emit.Conv_I();
        return ref ReturnRef<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> VoidPointerToSpan<T>(void* pointer, int length)
        where T : unmanaged
    {
        return new Span<T>(pointer, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* PointerToVoidPointer<T>(T* pointer)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(pointer));
        Emit.Conv_U();
        return ReturnPointer();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T PointerToRef<T>(T* pointer)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(pointer));
        return ref ReturnRef<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> PointerToSpan<T>(T* pointer, int length)
        where T : unmanaged
    {
        return new Span<T>(pointer, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* InToVoidPointer<T>(in T @in)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(@in));
        Emit.Conv_U();
        return ReturnPointer();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* InToPointer<T>(in T @in)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(@in));
        return ReturnPointer<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T InToRef<T>(in T @in)
    {
        Emit.Ldarg(nameof(@in));
        return ref ReturnRef<T>();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> InToSpan<T>(in T @in, int length)
    {
        return MemoryMarshal.CreateSpan(ref InToRef<T>(in @in), length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* RefToVoidPointer<T>(ref T @ref)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(@ref));
        Emit.Conv_U();
        return ReturnPointer();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* RefToPointer<T>(ref T @ref)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(@ref));
        return ReturnPointer<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> RefToSpan<T>(ref T @ref, int length)
    {
        return MemoryMarshal.CreateSpan<T>(ref @ref, length);
    }

    /// <remarks>
    /// This is stupid and dangerous
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T OutToRef<T>(out T @out)
    {
        Emit.Ldarg(nameof(@out));
        Emit.Ret();
        throw Unreachable();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* SpanToVoidPointer<T>(Span<T> span)
        where T : unmanaged
    {
        return RefToVoidPointer(ref span.GetPinnableReference());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* SpanToPointer<T>(Span<T> span)
        where T : unmanaged
    {
        return RefToPointer(ref span.GetPinnableReference());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T SpanToRef<T>(Span<T> span)
    {
        return ref span.GetPinnableReference();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> ReadOnlySpanToSpan<T>(ReadOnlySpan<T> readOnlySpan)
    {
        ref T first = ref MemoryMarshal.GetReference(readOnlySpan);
        return MemoryMarshal.CreateSpan(ref first, readOnlySpan.Length);
    }

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* ReadOnlySpanToVoidPointer<T>(ReadOnlySpan<T> span)
        where T : unmanaged
    {
        return RefToVoidPointer<T>(ref ReadOnlySpanToRef<T>(span));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* ReadOnlySpanToPointer<T>(ReadOnlySpan<T> span)
        where T : unmanaged
    {
        return RefToPointer<T>(ref ReadOnlySpanToRef(span));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T ReadOnlySpanToRef<T>(ReadOnlySpan<T> readOnlySpan)
    {
        return ref MemoryMarshal.GetReference(readOnlySpan);
    }

#endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<TOut> CastSpan<TIn, TOut>(Span<TIn> inSpan)
        where TIn : struct
        where TOut : struct
    {
        return MemoryMarshal.Cast<TIn, TOut>(inSpan);
    }
    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull("obj")]
    // ReSharper disable once ReturnTypeCanBeNotNullable
    public static T? CastClass<T>(object? obj)
        where T : class
    {
        Emit.Ldarg(nameof(obj));
        Emit.Castclass<T>();
        return Return<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TOut DirectCast<TIn, TOut>(TIn input)
    {
        Emit.Ldarg(nameof(input));
        return Return<TOut>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TOut DirectCast<TIn, TOut>(ref TIn source)
    {
        Emit.Ldarg(nameof(source));
        return ref ReturnRef<TOut>();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T UnboxToRef<T>(object box)
        where T : struct
    {
        //Push(box);
        Emit.Ldarg(nameof(box));
        Emit.Unbox<T>();
        return ref ReturnRef<T>();
    }

#endregion


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SkipInit<T>(out T value)
    {
        Emit.Ret();
        throw Unreachable();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SizeOf<T>()
    {
        Emit.Sizeof<T>();
        return Return<int>();
    }

#region Blocks (byte[])

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyBlock(in byte sourceByte, ref byte destByte, int byteCount)
    {
        Emit.Ldarg(nameof(destByte));
        Emit.Ldarg(nameof(sourceByte));
        Emit.Ldarg(nameof(byteCount));
        Emit.Cpblk();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyToBlock(void* source, void* destination, uint byteCount)
    {
        Emit.Ldarg(nameof(destination));
        Emit.Ldarg(nameof(source));
        Emit.Ldarg(nameof(byteCount));
        Emit.Cpblk();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyToBlock(in byte source, ref byte destination, uint byteCount)
    {
        Emit.Ldarg(nameof(destination));
        Emit.Ldarg(nameof(source));
        Emit.Ldarg(nameof(byteCount));
        Emit.Cpblk();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyToBlockUnaligned(void* source, void* destination, uint byteCount)
    {
        Emit.Ldarg(nameof(destination));
        Emit.Ldarg(nameof(source));
        Emit.Ldarg(nameof(byteCount));
        Emit.Unaligned(1);
        Emit.Cpblk();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyToBlockUnaligned(in byte source, ref byte destination, uint byteCount)
    {
        Emit.Ldarg(nameof(destination));
        Emit.Ldarg(nameof(source));
        Emit.Ldarg(nameof(byteCount));
        Emit.Unaligned(1);
        Emit.Cpblk();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InitBlock(void* startAddress, byte value, uint byteCount)
    {
        Emit.Ldarg(nameof(startAddress));
        Emit.Ldarg(nameof(value));
        Emit.Ldarg(nameof(byteCount));
        Emit.Initblk();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InitBlock(ref byte startAddress, byte value, uint byteCount)
    {
        Emit.Ldarg(nameof(startAddress));
        Emit.Ldarg(nameof(value));
        Emit.Ldarg(nameof(byteCount));
        Emit.Initblk();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InitBlockUnaligned(void* startAddress, byte value, uint byteCount)
    {
        Emit.Ldarg(nameof(startAddress));
        Emit.Ldarg(nameof(value));
        Emit.Ldarg(nameof(byteCount));
        Emit.Unaligned(1);
        Emit.Initblk();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount)
    {
        Emit.Ldarg(nameof(startAddress));
        Emit.Ldarg(nameof(value));
        Emit.Ldarg(nameof(byteCount));
        Emit.Unaligned(1);
        Emit.Initblk();
    }
    
    /// <summary>
    /// Makes an exact copy of the given <see cref="byte"/> array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Copy(byte[] bytes)
    {
        DeclareLocals(
            new LocalVar("len", typeof(int)),
            new LocalVar("newArray", typeof(byte[])));
        Emit.Ldarg(nameof(bytes));
        Emit.Ldlen();
        Emit.Stloc("len");
        Emit.Ldloc("len");
        Emit.Newarr<byte>();
        Emit.Stloc("newArray");
        Emit.Ldloca("newArray");
        Emit.Ldarga(nameof(bytes));
        Emit.Ldloc("len");
        Emit.Cpblk();
        Emit.Ldloc("newArray");
        return Return<byte[]>();
    }
#endregion

#region Offsets

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* OffsetBy<T>(void* source, int elementOffset)
    {
        Emit.Ldarg(nameof(source));
        Emit.Ldarg(nameof(elementOffset));
        Emit.Sizeof<T>();
        Emit.Conv_I();
        Emit.Mul();
        Emit.Add();
        return ReturnPointer();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* OffsetBy<T>(T* source, int elementOffset)
        where T : unmanaged
    {
        Emit.Ldarg(nameof(source));
        Emit.Ldarg(nameof(elementOffset));
        Emit.Sizeof<T>();
        Emit.Conv_I();
        Emit.Mul();
        Emit.Add();
        return ReturnPointer<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T OffsetBy<T>(ref T source, int elementOffset)
    {
        Emit.Ldarg(nameof(source));
        Emit.Ldarg(nameof(elementOffset));
        Emit.Sizeof<T>();
        Emit.Conv_I();
        Emit.Mul();
        Emit.Add();
        return ref ReturnRef<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T OffsetBy<T>(ref T source, nint elementOffset)
    {
        Emit.Ldarg(nameof(source));
        Emit.Ldarg(nameof(elementOffset));
        Emit.Sizeof<T>();
        Emit.Mul();
        Emit.Add();
        return ref ReturnRef<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* OffsetByBytes(void* source, int byteOffset)
    {
        Emit.Ldarg(nameof(source));
        Emit.Ldarg(nameof(byteOffset));
        Emit.Add();
        return ReturnPointer();
    }

#endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreSame<T>(ref T left, ref T right)
    {
        Emit.Ldarg(nameof(left));
        Emit.Ldarg(nameof(right));
        Emit.Ceq();
        return Return<bool>();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAddressGreaterThan<T>(ref T left, ref T right)
    {
        Emit.Ldarg(nameof(left));
        Emit.Ldarg(nameof(right));
        Emit.Cgt_Un();
        return Return<bool>();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAddressLessThan<T>(ref T left, ref T right)
    {
        Emit.Ldarg(nameof(left));
        Emit.Ldarg(nameof(right));
        Emit.Clt_Un();
        return Return<bool>();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullRef<T>(ref T source)
    {
        Emit.Ldarg(nameof(source));
        Emit.Ldc_I4_0();
        Emit.Conv_U();
        Emit.Ceq();
        return Return<bool>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T NullRef<T>()
    {
        Emit.Ldc_I4_0();
        Emit.Conv_U();
        return ref ReturnRef<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* NullPointer<T>()
        where T : unmanaged
    {
        Emit.Ldc_I4_0();
        Emit.Conv_U();
        return ReturnPointer<T>();
    }

#region Spans
   // in T

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static Span<char> AsSpan(in string text)
    // {
    //     fixed (char* ptr = text)
    //     {
    //         return PointerToSpan<char>(ptr, text.Length);
    //     }
    // }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<char> AsSpan(in string text)
    {
        ref readonly char ch = ref text.GetPinnableReference();
        return MemoryMarshal.CreateSpan<char>(ref InToRef(in ch), text.Length);
    }

#endregion
}