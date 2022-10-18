using Jay.Dumping.Extensions;

namespace Jay.Dumping;

internal sealed class DefaultDumper<T> : Dumper<T>
{
    public static DefaultDumper<T> Instance { get; } = new DefaultDumper<T>();
    
    private DefaultDumper() { }

    protected override void DumpValueImpl(ref DefStringHandler stringHandler, [NotNull] T value, DumpFormat dumpFormat)
    {
        if (dumpFormat >= DumpFormat.Inspect)
        {
            stringHandler.Write("(");
            stringHandler.Dump(typeof(T), dumpFormat);
            stringHandler.Write(") ");
        }
        // Do not call Dump, we want what DefStringHandler does!
        stringHandler.AppendFormatted<T>(value);
    }
}