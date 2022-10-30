using Jay.Dumping;
using Jay.Dumping.Extensions;

namespace Jayflect.Dumping;

public sealed class ConstructorInfoDumper : Dumper<ConstructorInfo>
{
    protected override void DumpImpl(ref DumpStringHandler stringHandler, [NotNull] ConstructorInfo ctor, DumpFormat format)
    {
        stringHandler.Dump(ctor.DeclaringType!, format);
        stringHandler.Write(".ctor(");
        var parameters = ctor.GetParameters();
        stringHandler.DumpDelimited<ParameterInfo>(", ", parameters);
        stringHandler.Write(')');
    }
}