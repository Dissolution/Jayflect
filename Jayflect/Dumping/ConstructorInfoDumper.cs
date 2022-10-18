using Jay.Dumping;
using Jay.Dumping.Extensions;

namespace Jayflect.Dumping;

public sealed class ConstructorInfoDumper : Dumper<ConstructorInfo>
{
    protected override void DumpValueImpl(ref DefaultInterpolatedStringHandler stringHandler, [NotNull] ConstructorInfo ctor, DumpFormat dumpFormat)
    {
        stringHandler.Dump(ctor.DeclaringType!, dumpFormat);
        stringHandler.Write(".ctor(");
        var parameters = ctor.GetParameters();
        stringHandler.DumpDelimited<ParameterInfo>(", ", parameters);
        stringHandler.Write(')');
    }
}