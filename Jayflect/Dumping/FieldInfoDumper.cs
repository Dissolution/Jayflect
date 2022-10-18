using Jay.Dumping;
using Jay.Dumping.Extensions;
using Jayflect.Extensions;

namespace Jayflect.Dumping;

public sealed class FieldInfoDumper : Dumper<FieldInfo>
{
    protected override void DumpValueImpl(ref DefaultInterpolatedStringHandler stringHandler, [NotNull] FieldInfo field, DumpFormat dumpFormat)
    {
        stringHandler.Dump(field.FieldType, dumpFormat);
        stringHandler.Write(' ');
        if (dumpFormat > DumpFormat.View)
        {
            stringHandler.Dump(field.OwnerType(), dumpFormat);
            stringHandler.Write('.');
        }
        stringHandler.Write(field.Name);
    }
}