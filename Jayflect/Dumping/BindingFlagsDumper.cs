using Jay.Dumping;
using Jay.Dumping.Interpolated;

namespace Jayflect.Dumping;

public sealed class BindingFlagsDumper : Dumper<BindingFlags>
{
    protected override void DumpImpl(ref DumpStringHandler stringHandler, [DisallowNull] BindingFlags value, DumpFormat format)
    {
        // Inspect is for debuggers!
        if (format.IsWithType)
        {
            if (value.HasFlag(BindingFlags.Public | BindingFlags.NonPublic))
            {
                stringHandler.Write("public|private");
            }
            else if (value.HasFlag(BindingFlags.Public))
            {
                stringHandler.Write("public");
            }
            else if (value.HasFlag(BindingFlags.NonPublic))
            {
                stringHandler.Write("private");
            }

            if (value.HasFlag(BindingFlags.Static))
            {
                stringHandler.Write(" static");
            }

            return;
        }
        
        // Default
        stringHandler.Write<BindingFlags>(value);
    }
}