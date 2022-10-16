using System.Reflection;
using System.Reflection.Emit;
using Jay.Reflection.Building.Emission;

namespace Jay.Reflection.Building.Deconstruction;

public sealed class RuntimeDeconstructor
{
    private sealed class ThisParameter : ParameterInfo
    {
        public ThisParameter(MemberInfo member)
        {
            this.MemberImpl = member;
            this.ClassImpl = member.DeclaringType;
            this.NameImpl = "this";
            this.PositionImpl = 0;
        }
    }
    
    private static readonly OpCode[] _oneByteOpCodes;
    private static readonly OpCode[] _twoByteOpCodes;

    static RuntimeDeconstructor()
    {
        _oneByteOpCodes = new OpCode[0xE1];
        _twoByteOpCodes = new OpCode[0x1F];

        var fields = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var field in fields)
        {
            var opCode = (OpCode)field.GetValue(null)!;
            if (opCode.OpCodeType == OpCodeType.Nternal)
                continue;

            if (opCode.Size == 1)
                _oneByteOpCodes[opCode.Value] = opCode;
            else
                _twoByteOpCodes[opCode.Value & 0xFF] = opCode;
        }
    }

    private readonly MethodBase _method;
    private readonly ParameterInfo[] _parameters;

    private readonly Type[] _methodGenericArguments;
    private readonly Type[] _declaringTypeGenericArguments;
    
    private readonly IList<LocalVariableInfo> _locals;
    private readonly byte[] _ilBytesBytes;

    private Module Module => _method.Module;

    public RuntimeDeconstructor(MethodBase method)
    {
        ArgumentNullException.ThrowIfNull(method);
        _method = method;
        MethodBody body = method.GetMethodBody() ?? throw new ArgumentException("Method has no Body", nameof(method));
        _locals = body.LocalVariables;
        _ilBytesBytes = body.GetILAsByteArray() ?? throw new ArgumentException("Method Body has no IL Bytes", nameof(method));
        
        if (method.IsStatic)
        {
            _parameters = method.GetParameters();
        }
        else
        {
            var methodParams = method.GetParameters();
            _parameters = new ParameterInfo[methodParams.Length + 1];
            _parameters[0] = new ThisParameter(method);
            methodParams.CopyTo(_parameters.AsSpan(1));
        }
        if (method is ConstructorInfo ctor)
        {
            _methodGenericArguments = Type.EmptyTypes;
        }
        else
        {
            _methodGenericArguments = method.GetGenericArguments();
        }

        if (method.DeclaringType != null)
        {
            _declaringTypeGenericArguments = method.DeclaringType.GetGenericArguments();
        }
        else
        {
            _declaringTypeGenericArguments = Type.EmptyTypes;
        }
    }

    private static OpCode ReadOpCode(ref ByteReader ilBytes)
    {
        byte op = ilBytes.ReadByte();
        if (op != 0xFE)
        {
            return _oneByteOpCodes[op];
        }
        else
        {
            return _twoByteOpCodes[ilBytes.ReadByte()];
        }
    }

    private object GetVariable(OpCode opCode, int index)
    {
        if (opCode.Name!.Contains("loc"))
        {
            return _locals[index];
        }
        else
        {
            return _parameters[index];
        }
    }
    
    private object? ReadOperand(OpCode opcode, ref ByteReader ilBytes)
    {
        switch (opcode.OperandType)
        {
            case OperandType.InlineSwitch:
                int length = ilBytes.Read<int>();
                int baseOffset = ilBytes.Position + (4 * length);
                int[] branches = new int[length];
                for (int i = 0; i < length; i++)
                {
                    branches[i] = ilBytes.Read<int>() + baseOffset;
                }
                return branches;
            case OperandType.ShortInlineBrTarget:
                return (ilBytes.Read<sbyte>() + ilBytes.Position);
            case OperandType.InlineBrTarget:
                return ilBytes.Read<int>() + ilBytes.Position;
            case OperandType.ShortInlineI:
                if (opcode == OpCodes.Ldc_I4_S)
                    return ilBytes.Read<sbyte>();
                else
                    return ilBytes.ReadByte();
            case OperandType.InlineI:
                return ilBytes.Read<int>();
            case OperandType.ShortInlineR:
                return ilBytes.Read<float>();
            case OperandType.InlineR:
                return ilBytes.Read<double>();
            case OperandType.InlineI8:
                return ilBytes.Read<long>();
            case OperandType.InlineSig:
                return Module.ResolveSignature(ilBytes.Read<int>());
            case OperandType.InlineString:
                return Module.ResolveString(ilBytes.Read<int>());
            case OperandType.InlineTok:
            case OperandType.InlineType:
            case OperandType.InlineMethod:
            case OperandType.InlineField:
                return Module.ResolveMember(ilBytes.Read<int>(), _declaringTypeGenericArguments, _methodGenericArguments);
            case OperandType.ShortInlineVar:
                return GetVariable(opcode, ilBytes.ReadByte());
            case OperandType.InlineVar:
                return GetVariable(opcode, ilBytes.Read<short>());
            case OperandType.InlineNone:
            default:
                return null;
        }
    }
    
    public InstructionStream GetInstructions()
    {
        var instructions = new InstructionStream();
        ByteReader ilBytes = new ByteReader(_ilBytesBytes);
        while (ilBytes.Remaining > 0)
        {
            var offset = ilBytes.Position;
            var opCode = ReadOpCode(ref ilBytes);
            object? operand = ReadOperand(opCode, ref ilBytes);
            var inst = new Instruction(offset, opCode, operand);
            instructions.AddLast(inst);
        }
        // Resolve branches
        foreach (var instruction in instructions)
        {
            switch (instruction.OpCode.OperandType)
            {
                case OperandType.ShortInlineBrTarget:
                case OperandType.InlineBrTarget:
                    instruction.Arg = instructions.FindByOffset((int)instruction.Arg!);
                    break;
                case OperandType.InlineSwitch:
                    var offsets = (int[])instruction.Arg!;
                    var branches = new Instruction[offsets.Length];
                    for (int j = 0; j < offsets.Length; j++)
                    {
                        branches[j] = instructions.FindByOffset(offsets[j])!;
                    }
                    instruction.Arg = branches;
                    break;
            }
        }
        // fin
        return instructions;
    }
}