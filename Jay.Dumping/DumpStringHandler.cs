using System.Collections;
using Jay.Dumping.Extensions;

namespace Jay.Dumping;

[InterpolatedStringHandler]
public ref struct DumpStringHandler
{
    private DefStringHandler _stringHandler;

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

    public string ToStringAndClear()
    {
        return _stringHandler.ToStringAndClear();
    }

    public override string ToString()
    {
        return _stringHandler.ToString();
    }
}