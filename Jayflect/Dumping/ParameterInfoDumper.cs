using Jay.Dumping;
using Jay.Dumping.Interpolated;

namespace Jayflect.Dumping;

public sealed class ParameterInfoDumper : Dumper<ParameterInfo>
{
    protected override void DumpImpl(ref DumpStringHandler stringHandler, [DisallowNull] ParameterInfo parameter, DumpFormat format)
    {
        stringHandler.Dump(parameter.ParameterType, format);
        stringHandler.Write(' ');
        stringHandler.Write(parameter.Name ?? "?");
        if (parameter.HasDefaultValue)
        {
            stringHandler.Write(" = ");
            stringHandler.Write(parameter.DefaultValue);
        }
    }
}