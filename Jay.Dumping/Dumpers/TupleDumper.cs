using Jay.Dumping.Interpolated;

namespace Jay.Dumping;

public sealed class TupleDumper : Dumper<ITuple>
{
    protected override void DumpImpl(ref DumpStringHandler dumpHandler, 
        [DisallowNull] ITuple tuple, DumpFormat format)
    {
        dumpHandler.Write('(');
        var objDumper = DumperCache.GetDumper<object>();
        for (var i = 0; i < tuple.Length; i++)
        {
            if (i > 0) dumpHandler.Write(", ");
            objDumper.DumpTo(ref dumpHandler, tuple[i], DumpFormat.WithType);
        }
        dumpHandler.Write(')');
    }
}