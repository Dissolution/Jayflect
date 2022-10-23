using System.Collections;
using Jay.Dumping.Extensions;

namespace Jay.Dumping;

[InterpolatedStringHandler]
public ref struct DumpStringHandler
{
    private DefStringHandler _stringHandler;

    public DumpStringHandler()
    {
        _stringHandler = new(literalLength: 64, formattedCount: 0);
    }
    
    public DumpStringHandler(int literalLength, int formatCount)
    {
        _stringHandler = new(literalLength, formatCount);
    }
    
    public DumpStringHandler(int literalLength, int formatCount, DefStringHandler stringHandler)
    {
        _stringHandler = stringHandler;
    }

    public void AppendLiteral(string text)
    {
        _stringHandler.Write(text);
    }

    public void AppendFormatted<T>(T? value, string? format = null)
    {
        _stringHandler.Dump(value, format);
    }

    public void AppendFormatted(string? text)
    {
        _stringHandler.Write(text);
    }

    public void AppendFormatted(IEnumerable enumerable, string? format = null)
    {
        _stringHandler.DumpDelimited(", ", enumerable.Cast<object?>());
    }
    
    public void AppendFormatted<T>(IEnumerable<T> enumerable, string? format = null)
    {
        _stringHandler.DumpDelimited<T>(", ", enumerable);
    }

    public void Write(char ch)
    {
        _stringHandler.AppendFormatted(new ReadOnlySpan<char>(in ch));
    }

    public void Write(string? text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _stringHandler.AppendLiteral(text);
        }
    }

    public void Write(ReadOnlySpan<char> text)
    {
        _stringHandler.AppendFormatted(text);
    }

    public void Write<T>(T? value)
    {
        if (value is not null)
        {
            _stringHandler.AppendFormatted<T>(value);
        }
    }

    public void Write<T>(T? value, string? format)
    {
        if (value is not null)
        {
            _stringHandler.AppendFormatted<T>(value, format);
        }
    }

    public void Dump<T>(T? value, DumpFormat dumpFormat = default)
    {
        var dumper = DumperCache.GetDumper<T>();
        dumper.DumpValue(ref _stringHandler, value, dumpFormat);
    }

    public void DumpDelimited<T>(
        ReadOnlySpan<char> delimiter,
        IEnumerable<T> values,
        DumpFormat dumpFormat = default)
    {
        using var e = values.GetEnumerator();
        if (!e.MoveNext()) return;
        var dumper = DumperCache.GetDumper<T>();
        dumper.DumpValue(ref _stringHandler, e.Current, dumpFormat);
        while (e.MoveNext())
        {
            _stringHandler.AppendFormatted(delimiter);
            dumper.DumpValue(ref _stringHandler, e.Current, dumpFormat);
        }
    }
    
    public string ToStringAndClear()
    {
        return _stringHandler.ToStringAndClear();
    }

    public override string ToString()
    {
        return _stringHandler.ToString();
    }
}