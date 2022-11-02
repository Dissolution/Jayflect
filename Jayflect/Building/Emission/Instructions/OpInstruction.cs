using Jay.Comparison;
using Jay.Dumping;
using Jay.Dumping.Interpolated;

namespace Jayflect.Building.Emission.Instructions;

public sealed class OpInstruction : Instruction
{
    public OpCode OpCode { get; }

    public object? Value { get; internal set; }

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
               DefaultComparers.Instance.Equals(opInstruction.Value, this.Value);
    }

    public override void DumpTo(ref DumpStringHandler dumpHandler, DumpFormat dumpFormat = default)
    {
        dumpHandler.Write(OpCode.Name);
        if (Value is not null)
        {
            dumpHandler.Write(' ');

            if (Value is Instruction instruction)
            {
                instruction.DumpTo(ref dumpHandler, dumpFormat);
            }
            else if (Value is Instruction[] instructions)
            {
                dumpHandler.Write('[');
                dumpHandler.DumpDelimited(", ", instructions);
                dumpHandler.Write(']');
            }
            else if (Value is string str)
            {
                dumpHandler.Write('"');
                dumpHandler.Write(str);
                dumpHandler.Write('"');
            }
            else if (Value is IDumpable dumpable)
            {
                dumpable.DumpTo(ref dumpHandler, dumpFormat);
            }
            else
            {
                dumpHandler.Dump(Value);
            }
        }
    }
}