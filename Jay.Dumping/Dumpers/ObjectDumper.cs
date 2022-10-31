using Jay.Dumping.Interpolated;

namespace Jay.Dumping;

[DumpOptions(Priority = 100)]
internal sealed class ObjectDumper : Dumper<object>
{
    public override bool CanDump(Type type)
    {
        // Everything implements object,
        // so we need to overwrite the default behavior here
        return type == typeof(object);
    }

    protected override void DumpImpl(ref DumpStringHandler dumpHandler, [DisallowNull] object obj, DumpFormat format)
    {
        // Get the dumper for the object's type
        var objType = obj.GetType();
        var dumper = DumperCache.GetDumper(objType);
        // Use its object-based implementation
        dumper.DumpObjTo(ref dumpHandler, obj, format);
    }
}