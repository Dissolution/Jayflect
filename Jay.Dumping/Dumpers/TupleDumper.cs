namespace Jay.Dumping;

public sealed class TupleDumper : Dumper<ITuple>
{
    protected override void DumpImpl(ref DumpStringHandler dumpHandler, 
        [NotNull] ITuple tuple, DumpFormat format)
    {
        dumpHandler.Write('(');
        var objDumper = DumperCache.GetDumper<object>();
        if (format < DumpFormat.Inspect)
            format = DumpFormat.Inspect;
        for (var i = 0; i < tuple.Length; i++)
        {
            if (i > 0) dumpHandler.Write(", ");
            objDumper.DumpTo(ref dumpHandler, tuple[i], format);
        }
        dumpHandler.Write(')');
    }
}