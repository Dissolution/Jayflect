using Jay.Dumping;
using Jay.Dumping.Interpolated;

namespace Jayflect.Dumping;

public sealed class FieldInfoDumper : Dumper<FieldInfo>
{
    protected override void DumpImpl(ref DumpStringHandler stringHandler, [DisallowNull] FieldInfo field, DumpFormat format)
    {
        stringHandler.Dump(field.FieldType, format);
        stringHandler.Write(' ');
        if (format.IsWithType)
        {
            stringHandler.Dump(field.OwnerType(), format);
            stringHandler.Write('.');
        }
        stringHandler.Write(field.Name);
    }
}