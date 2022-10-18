namespace Jay.Dumping;

public sealed class DumpableDumper : Dumper<IDumpable>
{
    protected override void DumpValueImpl(ref DefaultInterpolatedStringHandler stringHandler, [NotNull] IDumpable value, DumpFormat dumpFormat)
    {
        value.DumpTo(ref stringHandler, dumpFormat);
    }
}