using Jay.Dumping.Interpolated;

namespace Jay.Dumping.Extensions;

public static class DumpingExtensions
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
        DumpStringHandler stringHandler = new();
        dumper.DumpTo(ref stringHandler, span[0], dumpFormat);
        for (var i = 1; i < len; i++)
        {
            stringHandler.Write(",");
            dumper.DumpTo(ref stringHandler, span[i], dumpFormat);
        }
        return stringHandler.ToStringAndDispose();
    }
    
    public static string Dump<T>(this T? value, DumpFormat dumpFormat = default)
    {
        var dumper = DumperCache.GetDumper<T>();
        DumpStringHandler stringHandler = new();
        dumper.DumpTo(ref stringHandler, value, dumpFormat);
        return stringHandler.ToStringAndDispose();
    }
    
    public static string Dump(this ref DumpStringHandler dumpStringHandler)
    {
       return dumpStringHandler.ToStringAndDispose();
    }
}