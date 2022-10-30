﻿namespace Jay.Dumping.Extensions;

/// <summary>
/// Use with using / global using
/// </summary>
public static class DumperImport
{
    public static string Dump(ReadOnlySpan<char> text)
    {
        return new string(text);
    }
    
    public static string Dump<T>(ReadOnlySpan<T> span, DumpFormat dumpFormat = default)
    {
        int len = span.Length;
        if (len == 0) return "";
        var dumper = DumperCache.GetDumper<T>();
        DumpStringHandler stringHandler = new();
        dumper.DumpTo(ref stringHandler, span[0], dumpFormat);
        for (var i = 1; i < len; i++)
        {
            stringHandler.AppendLiteral(",");
            dumper.DumpTo(ref stringHandler, span[i], dumpFormat);
        }
        return stringHandler.ToStringAndDispose();
    }
    
    public static string Dump<T>(T? value, DumpFormat dumpFormat = default)
    {
        var dumper = DumperCache.GetDumper<T>();
        DumpStringHandler stringHandler = new();
        dumper.DumpTo(ref stringHandler, value, dumpFormat);
        return stringHandler.ToStringAndDispose();
    }
    
    public static string Dump(ref DumpStringHandler stringHandler)
    {
        return stringHandler.ToStringAndDispose();
    }
}