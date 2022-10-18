namespace Jay.Dumping;

internal sealed class ObjectDumper : Dumper<object>
{
    protected override void DumpValueImpl(ref DefaultInterpolatedStringHandler stringHandler, [NotNull] object obj, DumpFormat dumpFormat)
    {
        // Get the dumper for the object's type
        var objType = obj.GetType();
        var dumper = DumperCache.GetDumper(objType);
        // Use it's object-based implementation
        dumper.DumpObject(ref stringHandler, obj, dumpFormat);
    }
}