using Jay.Dumping.Interpolated;

namespace Jay.Dumping;

public interface IDumpable : ISpanFormattable, IFormattable
{
    void DumpTo(ref DumpStringHandler dumpStringHandler, DumpFormat format = default);

    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        var dumper = new DumpStringHandler(destination);
        try
        {
            DumpTo(ref dumper, format);
            if (dumper.Length > destination.Length)
            {
                charsWritten = 0;
                destination.Clear();
                return false;
            }
            charsWritten = dumper.Length;
            return true;
        }
        finally
        {
            dumper.Dispose();
        }
    }
    
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
    {
        var dumpHandler = new DumpStringHandler();
        DumpTo(ref dumpHandler, format);
        return dumpHandler.ToStringAndDispose();
    }

    string Dump(DumpFormat dumpFormat = default)
    {
        var dumpHandler = new DumpStringHandler();
        DumpTo(ref dumpHandler, dumpFormat);
        return dumpHandler.ToStringAndDispose();
    }

    string? ToString() => Dump(null);
}