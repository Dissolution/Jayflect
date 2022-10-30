using Jay.Dumping;
using Jay.Dumping.Extensions;
using Jayflect.Extensions;

namespace Jayflect.Dumping;

public sealed class PropertyInfoDumper : Dumper<PropertyInfo>
{
    protected override void DumpImpl(ref DumpStringHandler stringHandler, [NotNull] PropertyInfo property, DumpFormat format)
    {
        if (format == DumpFormat.None)
        {
            stringHandler.Dump(property.PropertyType);
            stringHandler.Write(' ');
            stringHandler.Write(property.Name);
        }
        else // format >= DumpFormat.Inspect)
        {
            var getVis = property.GetGetter().Visibility();
            var setVis = property.GetSetter().Visibility();
            Visibility highVis = getVis >= setVis ? getVis : setVis;
            stringHandler.Write(highVis);
            stringHandler.Write(' ');

            stringHandler.Dump(property.PropertyType, format);
            stringHandler.Write(' ');
            stringHandler.Dump(property.OwnerType(), format);
            stringHandler.Write('.');
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
}