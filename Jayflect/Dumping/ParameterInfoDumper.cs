using Jay.Dumping;
using Jay.Dumping.Extensions;

namespace Jayflect.Dumping;

public sealed class ParameterInfoDumper : Dumper<ParameterInfo>
{
    protected override void DumpValueImpl(ref DefaultInterpolatedStringHandler stringHandler, [NotNull] ParameterInfo parameter, DumpFormat dumpFormat)
    {
        stringHandler.Dump(parameter.ParameterType, dumpFormat);
        stringHandler.Write(' ');
        stringHandler.Write(parameter.Name);
        if (parameter.HasDefaultValue)
        {
            stringHandler.Write(" = ");
            stringHandler.Write(parameter.DefaultValue);
        }
    }
}