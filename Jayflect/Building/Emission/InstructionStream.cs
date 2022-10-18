using Jay.Collections;
using Jay.Dumping;
using Jay.Dumping.Extensions;
using Jay.Reflection.Building.Emission;

namespace Jayflect.Building.Emission;

public class InstructionStream : LinkedList<InstructionStreamLine>, IDumpable
{
    public InstructionStreamLine? FindByOffset(int offset)
    {
        if (offset < 0 || this.Count == 0)
            return null;
        foreach (var streamLine in this)
        {
            if (streamLine.Offset == offset)
                return streamLine;
            if (streamLine.Offset > offset)
                return null;
        }
        return null;
    }

    public void DumpTo(ref DefaultInterpolatedStringHandler stringHandler, DumpFormat dumpFormat = default)
    {
        using var e = this.GetEnumerator();
        if (!e.MoveNext()) return;
        e.Current.DumpTo(ref stringHandler, dumpFormat);
        while (e.MoveNext())
        {
            stringHandler.AppendLiteral(Environment.NewLine);
            e.Current.DumpTo(ref stringHandler, dumpFormat);
        }
    }
}

public sealed record class InstructionStreamLine(int Offset, Instruction Instruction) : IDumpable
{
    public void DumpTo(ref DefaultInterpolatedStringHandler stringHandler, DumpFormat dumpFormat = default)
    {
        stringHandler.AppendLiteral("IL_");
        stringHandler.AppendFormatted(Offset, "x4");
        stringHandler.AppendLiteral(": ");
        Instruction.DumpTo(ref stringHandler, dumpFormat);
    }
}

public abstract class Instruction : IEquatable<Instruction>, IDumpable
{
    public abstract bool Equals(Instruction? instruction);

    public abstract void DumpTo(ref DefaultInterpolatedStringHandler stringHandler, DumpFormat dumpFormat = default);
}

public class OpInstruction : Instruction
{
    public OpCode OpCode { get; }
    public object? Value { get; }
    
    public int Size
    {
        get
        {
            int size = OpCode.Size;

            switch (OpCode.OperandType)
            {
                case OperandType.InlineSwitch:
                {
                    if (!(Value is Instruction[] instructions))
                        throw new InvalidOperationException();
                    size += (1 + instructions.Length) * 4;
                    break;
                }
                case OperandType.InlineI8:
                case OperandType.InlineR:
                    size += 8;
                    break;
                case OperandType.InlineBrTarget:
                case OperandType.InlineField:
                case OperandType.InlineI:
                case OperandType.InlineMethod:
                case OperandType.InlineString:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.ShortInlineR:
                    size += 4;
                    break;
                case OperandType.InlineVar:
                    size += 2;
                    break;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    size += 1;
                    break;
            }

            return size;
        }
    }

    public OpInstruction(OpCode opCode, object? value = null)
    {
        this.OpCode = opCode;
        this.Value = value;
    }

    public override bool Equals(Instruction? instruction)
    {
        return instruction is OpInstruction opInstruction &&
               opInstruction.OpCode == this.OpCode &&
               object.Equals(opInstruction.Value, this.Value);
    }
    
    public override void DumpTo(ref DefaultInterpolatedStringHandler stringHandler, DumpFormat dumpFormat = default)
    {
        stringHandler.Write(OpCode.Name);
        if (Value is not null)
        {
            stringHandler.Write(' ');

            if (Value is Instruction instruction)
            {
                instruction.DumpTo(ref stringHandler, dumpFormat);
            }
            else if (Value is Instruction[] instructions)
            {
                stringHandler.Write('[');
                var len = instructions.Length;
                if (len > 0)
                {
                    instructions[0].DumpTo(ref stringHandler, dumpFormat);
                    for (var i = 1; i < len; i++)
                    {
                        stringHandler.Write(", ");
                        instructions[i].DumpTo(ref stringHandler, dumpFormat);
                    }
                }
                stringHandler.Write(']');
            }
            else if (Value is string str)
            {
                stringHandler.Write('"');
                stringHandler.Write(str);
                stringHandler.Write('"');
            }
            else
            {
                stringHandler.Dump(Value);
            }
        }
    }
}