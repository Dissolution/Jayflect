using Jay.Dumping;
using Jay.Dumping.Interpolated;
using Jay.Extensions;

namespace Jayflect.Building.Emission.Instructions;

public sealed record class InstructionLine(int? Offset, Instruction Instruction) : IDumpable
{
    public void DumpTo(ref DumpStringHandler dumpHandler, DumpFormat dumpFormat = default)
    {
        dumpHandler.Write("IL_");
        if (Offset.TryGetValue(out int offset))
        {
            dumpHandler.Write(offset, "X4");
        }
        else
        {
            dumpHandler.Write("????");
        }
        dumpHandler.Write(": ");
        Instruction.DumpTo(ref dumpHandler, dumpFormat);
    }
}