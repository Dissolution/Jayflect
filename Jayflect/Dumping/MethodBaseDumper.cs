using Jay.Dumping;
using Jay.Dumping.Interpolated;
using Jayflect.Extensions;

namespace Jayflect.Dumping;

public sealed class MethodBaseDumper : Dumper<MethodBase>
{
    protected override void DumpImpl(ref DumpStringHandler stringHandler, [DisallowNull] MethodBase method, DumpFormat format)
    {
        if (format.IsWithType)
        {
            stringHandler.Write(method.Visibility());
            stringHandler.Write(' ');
            if (method.IsStatic)
                stringHandler.Write("static ");
        }
        
        stringHandler.Dump(method.ReturnType(), format);
        stringHandler.Write(' ');
        if (format.IsWithType)
        {
            stringHandler.Dump(method.OwnerType(), format);
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