﻿using Jay.Dumping;
using Jay.Dumping.Extensions;
using Jay.Extensions;

namespace Jayflect.Building.Emission.Instructions;

public sealed record class InstructionLine(int? Offset, Instruction Instruction) : IDumpable
{
    public void DumpTo(ref DefaultInterpolatedStringHandler stringHandler, DumpFormat dumpFormat = default)
    {
        stringHandler.Write("IL_");
        if (Offset.TryGetValue(out int offset))
        {
            stringHandler.Write(offset, "x4");
        }
        else
        {
            stringHandler.Write("????");
        }
        stringHandler.Write(": ");
        stringHandler.Dump(Instruction, dumpFormat);
    }
}