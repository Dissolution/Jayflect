using Jay.Dumping.Interpolated;

namespace Jay.Dumping;

internal sealed class StructInterfaceDumper<T> : Dumper<T>
    //where T : struct
{
    private readonly IDumper _interfaceDumper;
    
    public StructInterfaceDumper(IDumper interfaceDumper)
    {
        _interfaceDumper = interfaceDumper;
    }

    protected override void DumpImpl(ref DumpStringHandler dumpHandler, [DisallowNull] T value, DumpFormat format)
    {
        _interfaceDumper.DumpObjTo(ref dumpHandler, (object?)value, format);
        
    }
}