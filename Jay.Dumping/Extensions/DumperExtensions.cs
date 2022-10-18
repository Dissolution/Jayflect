namespace Jay.Dumping.Extensions;

public static class DumperExtensions
{
    public static string Dump(this ReadOnlySpan<char> text)
    {
        return new string(text);
    }
    
    public static string Dump<T>(this ReadOnlySpan<T> span, DumpFormat dumpFormat = default)
    {
        int len = span.Length;
        if (len == 0) return "";
        var dumper = DumperCache.GetDumper<T>();
        DefStringHandler stringHandler = new();
        dumper.DumpValue(ref stringHandler, span[0], dumpFormat);
        for (var i = 1; i < len; i++)
        {
            stringHandler.AppendLiteral(",");
            dumper.DumpValue(ref stringHandler, span[i], dumpFormat);
        }
        return stringHandler.ToStringAndClear();
    }
    
    public static string Dump<T>(this T? value, DumpFormat dumpFormat = default)
    {
        var dumper = DumperCache.GetDumper<T>();
        DefStringHandler stringHandler = new();
        dumper.DumpValue(ref stringHandler, value, dumpFormat);
        return stringHandler.ToStringAndClear();
    }
}