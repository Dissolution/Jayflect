using Jay.Dumping;
using Jay.Dumping.Extensions;

namespace Jayflect.Dumping;

public sealed class BindingFlagsDumper : Dumper<BindingFlags>
{
    protected override void DumpImpl(ref DumpStringHandler stringHandler, [NotNull] BindingFlags value, DumpFormat format)
    {
        if (format.IsCustom)
        {
            stringHandler.AppendFormatted<BindingFlags>(value, format.GetCustomFormatString());
            return;
        }

        // Inspect is for debuggers!
        if (format >= DumpFormat.Inspect)
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
        stringHandler.AppendFormatted<BindingFlags>(value);
    }
}