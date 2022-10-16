using Jay.Dumping;
using Jayflect;

namespace Jay.Reflection.Building.Deconstruction;

public ref struct ByteReader
{
    private readonly ReadOnlySpan<byte> _bytes;
    private int _position;

    public int Position => _position;
    public int Remaining => _bytes.Length - Position;

    public ByteReader(ReadOnlySpan<byte> bytes)
    {
        _bytes = bytes;
        _position = 0;
    }

    public bool TryReadByte(out byte b)
    {
        if (Remaining == 0)
        {
            b = default;
            return false;
        }
        b = _bytes[_position++];
        return true;
    }

    public byte ReadByte()
    {
        if (!TryReadByte(out var b))
            throw new InvalidOperationException("Cannot read a Byte");
        return b;
    }

    public bool TryReadBytes(int count, out ReadOnlySpan<byte> slice)
    {
        if (count > Remaining)
        {
            slice = default;
            return false;
        }
        slice = _bytes.Slice(_position, count);
        _position += count;
        return true;
    }

    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        if (!TryReadBytes(count, out var slice))
            throw new InvalidOperationException($"Cannot read {count} bytes");
        return slice;
    }

    public bool TryRead<T>(out T value)
        where T : unmanaged
    {
        var size = Unmanaged.SizeOf<T>();
        if (size > Remaining)
        {
            value = default;
            return false;
        }
        value = Danger.ReadUnaligned<T>(in _bytes[_position]);
        _position += size;
        return true;
    }

    public T Read<T>()
        where T : unmanaged
    {
        if (!TryRead<T>(out var value))
            throw Dumper.GetException<InvalidOperationException>($"Cannot read a {typeof(T)} value");
        return value;
    }
}