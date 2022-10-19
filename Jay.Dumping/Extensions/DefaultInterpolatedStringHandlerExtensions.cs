namespace Jay.Dumping.Extensions;

public static class DefaultInterpolatedStringHandlerExtensions
{
    public static void Write(this ref DefStringHandler stringHandler, char ch)
    {
        stringHandler.AppendFormatted(new ReadOnlySpan<char>(in ch));
    }

    public static void Write(this ref DefStringHandler stringHandler, string? text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            stringHandler.AppendLiteral(text);
        }
    }

    public static void Write(this ref DefStringHandler stringHandler, ReadOnlySpan<char> text)
    {
        stringHandler.AppendFormatted(text);
    }

    public static void Write<T>(this ref DefStringHandler stringHandler, T? value)
    {
        if (value is not null) stringHandler.AppendFormatted<T>(value);
    }

    public static void Dump<T>(this ref DefStringHandler stringHandler, T? value, DumpFormat dumpFormat = default)
    {
        var dumper = DumperCache.GetDumper<T>();
        dumper.DumpValue(ref stringHandler, value, dumpFormat);
    }

    public static void DumpDelimited<T>(this ref DefStringHandler stringHandler,
        ReadOnlySpan<char> delimiter,
        IEnumerable<T> values,
        DumpFormat dumpFormat = default)
    {
        using var e = values.GetEnumerator();
        if (!e.MoveNext()) return;
        var dumper = DumperCache.GetDumper<T>();
        dumper.DumpValue(ref stringHandler, e.Current, dumpFormat);
        while (e.MoveNext())
        {
            stringHandler.AppendFormatted(delimiter);
            dumper.DumpValue(ref stringHandler, e.Current, dumpFormat);
        }
    }
}