using System.Collections;
using System.ComponentModel;
using Jay.Dumping.Interpolated;

// ReSharper disable UnusedParameter.Local

namespace Jay.Dumping;

[InterpolatedStringHandler]
public ref struct DumpStringHandler
{
    private CharSpanBuilder _charSpanBuilder;

    public int Length => _charSpanBuilder.Length;
    
    public DumpStringHandler()
    {
        _charSpanBuilder = new(1024, 0);
    }

    public DumpStringHandler(Span<char> initialBuffer)
    {
        _charSpanBuilder = new(initialBuffer);
    }
    
    public DumpStringHandler(int literalLength, int formatCount)
    {
        _charSpanBuilder = new(literalLength, formatCount);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLiteral(string text)
    {
        Write(text);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted<T>(T? value)
    {
        Dump<T>(value);
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted<T>(T? value, string? format)
    {
        Dump<T>(value, format);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted(string? text)
    {
        Write(text);
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted(ReadOnlySpan<char> text)
    {
        Write(text);
    }

    public void Write(char ch)
    {
        _charSpanBuilder.AppendFormatted(ch);
    }

    public void Write(string? text)
    {
        if (text is not null)
        {
            _charSpanBuilder.AppendLiteral(text);
        }
    }

    public void Write(ReadOnlySpan<char> text)
    {
        _charSpanBuilder.AppendFormatted(text);
    }

    public void Write<T>(T? value)
    {
        if (value is not null)
        {
            _charSpanBuilder.AppendFormatted<T>(value);
        }
    }

    public void Write<T>(T? value, string? format)
    {
        if (value is not null)
        {
            _charSpanBuilder.AppendFormatted<T>(value, format);
        }
    }

    public void Dump<T>(T? value, DumpFormat dumpFormat = default)
    {
        if (value is IEnumerable enumerable)
        {
            DumpEnumerable(enumerable, dumpFormat);
        }
        else
        {
            var dumper = DumperCache.GetDumper<T>();
            dumper.DumpTo(ref this, value, dumpFormat);
        }
    }

    private void DumpEnumerable(IEnumerable enumerable, DumpFormat format)
    {
        ReadOnlySpan<char> delimiter = ", ";
        if (format.IsCustom)
            delimiter = format;
        IEnumerator? enumerator = null;
        try
        {
            enumerator = enumerable.GetEnumerator();
            // No values?
            if (!enumerator.MoveNext()) return;
            // Get the value dumper
            var objDumper = DumperCache.GetDumper<object>();
            objDumper.DumpTo(ref this, enumerator.Current, format);
            while (enumerator.MoveNext())
            {
                Write(delimiter);
                objDumper.DumpTo(ref this, enumerator.Current, format);
            }
        }
        finally
        {
            if (enumerator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        
    }

    public void DumpDelimited<T>(
        ReadOnlySpan<char> delimiter,
        IEnumerable<T> values,
        DumpFormat dumpFormat = default)
    {
        using var e = values.GetEnumerator();
        // No values?
        if (!e.MoveNext()) return;
        // Get the value dumper
        var dumper = DumperCache.GetDumper<T>();
        dumper.DumpTo(ref this, e.Current, dumpFormat);
        while (e.MoveNext())
        {
            Write(delimiter);
            dumper.DumpTo(ref this, e.Current, dumpFormat);
        }
    }

    public void Dispose()
    {
        _charSpanBuilder.Dispose();
    }
    
    public string ToStringAndDispose()
    {
        return _charSpanBuilder.ToStringAndDispose();
    }

    public override string ToString()
    {
        return _charSpanBuilder.ToString();
    }
}