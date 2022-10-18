using Jay.Dumping;
using Jay.Dumping.Extensions;
using Jayflect.Extensions;

namespace Jayflect.Dumping;

public sealed class PropertyInfoDumper : Dumper<PropertyInfo>
{
    protected override void DumpValueImpl(ref DefaultInterpolatedStringHandler stringHandler, [NotNull] PropertyInfo property, DumpFormat dumpFormat)
    {
        var getVis = property.GetGetter().Visibility();
        var setVis = property.GetSetter().Visibility();
        Visibility highVis = getVis >= setVis ? getVis : setVis;
        stringHandler.Write(highVis);
        stringHandler.Write(' ');
        stringHandler.Dump(property.PropertyType, dumpFormat);
        stringHandler.Write(' ');
        if (dumpFormat > DumpFormat.View)
        {
            stringHandler.Dump(property.OwnerType(), dumpFormat);
            stringHandler.Write('.');
        }

        stringHandler.Write(property.Name);
        stringHandler.Write(" {");
        if (getVis != Visibility.None)
        {
            if (getVis != highVis)
                stringHandler.Write(getVis);
            stringHandler.Write(" get; ");
        }

        if (setVis != Visibility.None)
        {
            if (setVis != highVis)
                stringHandler.Write(setVis);
            stringHandler.Write(" set; ");
        }
        stringHandler.Write('}');
    }
}