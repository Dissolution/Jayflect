using Jay.Dumping;
using Jay.Dumping.Extensions;
using Jayflect.Extensions;

namespace Jayflect.Dumping;

public sealed class MethodBaseDumper : Dumper<MethodBase>
{
    protected override void DumpValueImpl(ref DefaultInterpolatedStringHandler stringHandler, [NotNull] MethodBase method, DumpFormat dumpFormat)
    {
        stringHandler.Dump(method.ReturnType(), dumpFormat);
        stringHandler.Write(' ');
        if (dumpFormat > DumpFormat.View)
        {
            stringHandler.Dump(method.OwnerType(), dumpFormat);
            stringHandler.Write('.');
        }
        stringHandler.Write(method.Name);

        if (method.IsGenericMethod)
        {
            stringHandler.Write('<');
            var genericTypes = method.GetGenericArguments();
            for (var i = 0; i < genericTypes.Length; i++)
            {
                if (i > 0) stringHandler.Write(',');
                stringHandler.Dump(genericTypes[i]);
            }
            stringHandler.Write('>');
        }
        stringHandler.Write('(');
        var parameters = method.GetParameters();
        stringHandler.DumpDelimited(", ", parameters);
        stringHandler.Write(')');
    }
}