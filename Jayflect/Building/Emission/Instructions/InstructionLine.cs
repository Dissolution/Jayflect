using Jay.Dumping;
using Jay.Dumping.Interpolated;
using Jay.Extensions;

namespace Jayflect.Building.Emission.Instructions;

public sealed record class InstructionLine(int? Offset, Instruction Instruction) : IDumpable
{
    public void DumpTo(ref DumpStringHandler stringHandler, DumpFormat dumpFormat = default)
    {
        stringHandler.Write("IL_");
        if (Offset.TryGetValue(out int offset))
        {
            stringHandler.Write(offset, "X4");
        }
        else
        {
            stringHandler.Write("????");
        }
        stringHandler.Write(": ");
        Instruction.DumpTo(ref stringHandler, dumpFormat);
    }
}