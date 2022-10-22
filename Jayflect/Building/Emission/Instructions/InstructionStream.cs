using Jay.Dumping;
using Jay.Dumping.Extensions;
using Jay.Extensions;

namespace Jayflect.Building.Emission.Instructions;

public class InstructionStream : LinkedList<InstructionLine>, IDumpable
{
    public InstructionLine? FindByOffset(int offset)
    {
        if (offset < 0 || this.Count == 0)
            return null;
        foreach (var streamLine in this)
        {
            if (streamLine.Offset.TryGetValue(out var lineOffset))
            {
                if (lineOffset == offset) return streamLine;
                if (lineOffset > offset) return null;
            }
        }
        return null;
    }

    public void DumpTo(ref DefaultInterpolatedStringHandler stringHandler, DumpFormat dumpFormat = default)
    {
        stringHandler.DumpDelimited(Environment.NewLine, this, dumpFormat);
    }

    public override string ToString()
    {
        return ((IDumpable)this).Dump();
    }
}