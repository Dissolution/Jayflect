using Jay.Collections;
using Jay.Comparison;
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
                stringHandler.DumpDelimited(", ", instructions);
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

public sealed class LocalBuilderEqualityComparer : EqualityComparer<LocalBuilder>
{
    public override bool Equals(LocalBuilder? left, LocalBuilder? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.IsPinned == right.IsPinned &&
               left.LocalIndex == right.LocalIndex &&
               left.LocalType == right.LocalType;
    }

    public override int GetHashCode(LocalBuilder? localBuilder)
    {
        if (localBuilder is null) return 0;
        return HashCode.Combine(localBuilder.IsPinned, localBuilder.LocalIndex, localBuilder.LocalType);
    }
}

internal sealed class OpValueEqualityComparer : IEqualityComparer<object?>
{
    public bool Equals(LocalBuilder x, LocalBuilder y)
    {
        return x.IsPinned == y.IsPinned &&
               x.LocalIndex == y.LocalIndex &&
               x.LocalType == y.LocalType;
    }

    public bool Equals(Label[] x, Label[] y)
    {
        return EnumerableEqualityComparer<Label>.Default.Equals(x, y);
    }

    public bool Equals(Type[] x, Type[] y)
    {
        return EnumerableEqualityComparer<Type>.Default.Equals(x, y);
    }

    public bool Equals(object?[] x, object?[] y)
    {
        var len = x.Length;
        if (y.Length != len) return false;
        for (int i = 0; i < len; i++)
        {
            if (!this.Equals(x[i], y[i])) return false;
        }
        return true;
    }

    private static bool YIsEqual<T>(T left, object right, IEqualityComparer<T>? comparer = default)
    {
        if (right is not T rightTyped) return false;
        if (comparer is null)
        {
            return EqualityComparer<T>.Default.Equals(left, rightTyped);
        }
        return comparer.Equals(left, rightTyped);
    }

    public new bool Equals(object? x, object? y)
    {
        if (x is null) return y is null;
        if (y is null) return false;
        if (x is byte xByte) return YIsEqual(xByte, y);
        if (x is sbyte xSByte) return YIsEqual(xSByte, y);
        if (x is short xShort) return YIsEqual(xShort, y);
        if (x is int xInt) return YIsEqual(xInt, y);
        if (x is long xLong) return YIsEqual(xLong, y);
        if (x is float xFloat) return YIsEqual(xFloat, y);
        if (x is double xDouble) return YIsEqual(xDouble, y);
        if (x is string xString) return YIsEqual(xString, y);
        if (x is FieldInfo xFieldInfo) return YIsEqual(xFieldInfo, y);
        if (x is MethodInfo xMethodInfo) return YIsEqual(xMethodInfo, y);
        if (x is ConstructorInfo xConstructorInfo) return YIsEqual(xConstructorInfo, y);
        if (x is Type xType) return YIsEqual(xType, y);
        if (x is LocalBuilder xLocalBuilder)
            return y is LocalBuilder yLocalBuilder && Equals(xLocalBuilder, yLocalBuilder);
        if (x is Label xLabel) return YIsEqual(xLabel, y);
        if (x is Label[] xLabels)
            return y is Label[] yLabels && Equals(xLabels, yLabels);
        if (x is bool xBool) return YIsEqual(xBool, y);
        if (x is Type[] xTypes)
            return y is Type[] yTypes && Equals(xTypes, yTypes);
        if (x is CallingConvention xCallingConvention) return YIsEqual(xCallingConvention, y);
        if (x is CallingConventions xCallingConventions) return YIsEqual(xCallingConventions, y);
        if (x is object?[] xArray)
            return y is object?[] yArray && Equals(xArray, yArray);

        throw new NotImplementedException();
    }
    public int GetHashCode(object? opValue)
    {
        throw new NotImplementedException();
    }

    [Obsolete("You're probably looking for Equals(object?,object?)", true)]
    public override bool Equals(object? obj) => false;
    public override int GetHashCode() => throw new InvalidOperationException();
}