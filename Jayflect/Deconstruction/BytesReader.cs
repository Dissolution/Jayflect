using Jay;
using Jayflect.Exceptions;

namespace Jayflect.Deconstruction;

public ref struct BytesReader
{
    private readonly ReadOnlySpan<byte> _bytes;
    private int _position;

    public int Position
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _position;
    }

    public int AvailableByteCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _bytes.Length - _position;
    }

    public ReadOnlySpan<byte> AvailableBytes
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _bytes.Slice(_position);
    }

    public BytesReader(ReadOnlySpan<byte> bytes)
    {
        _bytes = bytes;
        _position = 0;
    }
    
    public bool TryPeek<T>(out T value)
        where T : unmanaged
    {
        if (Danger.SizeOf<T>() <= AvailableByteCount)
        {
            value = Danger.ReadUnaligned<T>(AvailableBytes);
            return true;
        }
        value = default;
        return false;
    }
    
    public bool TryPeek(out byte b)
    {
        if (AvailableByteCount > 0)
        {
            b = _bytes[_position];
            return true;
        }
        b = default;
        return false;
    }
    
    
    public Result TryRead<T>(out T value)
        where T : unmanaged
    {
        var size = Danger.SizeOf<T>();
        if (size <= AvailableByteCount)
        {
            value = Danger.ReadUnaligned<T>(AvailableBytes);
            _position += size;
            return true;
        }
        value = default;
        return new JayflectException($"Unable to read {typeof(T)}'s {size} bytes -- only {AvailableByteCount} available");
    }

    public T Read<T>() where T : unmanaged
    {
        TryRead<T>(out T value).ThrowIfFailed();
        return value;
    }
    
    public Result TryRead(out byte b)
    {
        if (AvailableByteCount > 0)
        {
            b = _bytes[_position++];
            return true;
        }
        b = default;
        return new JayflectException("Unable to read one byte");
    }

    public byte ReadByte()
    {
        TryRead(out byte b).ThrowIfFailed();
        return b;
    }
    
    public Result TryRead(Span<byte> buffer)
    {
        if (AvailableBytes.TryCopyTo(buffer))
        {
            _position += buffer.Length;
            return true;
        }
        return new JayflectException($"Unable to fill buffer's {buffer.Length} bytes -- only {AvailableByteCount} available");
    }

    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        if (count <= AvailableByteCount)
        {
            var bytes = _bytes.Slice(_position, count);
            _position += count;
            return bytes;
        }
        throw new JayflectException($"Unable to read {count} bytes -- only {AvailableByteCount} available");
    }


}