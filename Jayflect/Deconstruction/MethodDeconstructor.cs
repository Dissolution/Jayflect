using Jay.Extensions;
using Jayflect.Building.Emission.Instructions;
using Jayflect.Exceptions;

namespace Jayflect.Deconstruction;

public sealed class MethodDeconstructor
{
    private static readonly OpCode[] _oneByteOpCodes;
    private static readonly OpCode[] _twoByteOpCodes;
    
    static MethodDeconstructor()
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

    public static InstructionStream ReadInstructions(MethodBase method)
    {
        return new MethodDeconstructor(method).GetInstructions();
    }
    
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
    
    private readonly Type[] _methodGenericArguments;
    private readonly Type[] _declaringTypeGenericArguments;

    private readonly byte[] _ilBytes;
    
    public MethodBase Method { get; }
    public ParameterInfo[] Parameters { get; }

    public IList<LocalVariableInfo> Locals { get; }

    private Module Module => Method.Module;
   
    public MethodDeconstructor(MethodBase methodBase)
    {
        ArgumentNullException.ThrowIfNull(methodBase);
        this.Method = methodBase;
        MethodBody body = methodBase.GetMethodBody() ?? throw new ArgumentException("Method has no Body", nameof(methodBase));
        this.Locals = body.LocalVariables;
        _ilBytes = body.GetILAsByteArray() ?? throw new ArgumentException("Method Body has no IL Bytes", nameof(methodBase));
        
        // We need to get all parameters, including the implicit This for instance methods
        if (methodBase.IsStatic)
        {
            this.Parameters = methodBase.GetParameters();
        }
        else
        {
            var methodParams = methodBase.GetParameters();
            Parameters = new ParameterInfo[methodParams.Length + 1];
            Parameters[0] = new ThisParameter(methodBase);
            methodParams.CopyTo(Parameters.AsSpan(1));
        }

        // Note: used to check `methodBase is Ctor` to do Type.EmptyTypes
        
        if (methodBase.IsGenericMethod)
        {
            _methodGenericArguments = methodBase.GetGenericArguments();
        }
        else
        {
            _methodGenericArguments = Type.EmptyTypes;
        }

        // Note: `if (methodBase.DeclaringType != null)` was the original first check
        
        var ownerType = methodBase.OwnerType();
        if (ownerType.IsGenericType)
        {
            _declaringTypeGenericArguments = ownerType.GetGenericArguments();
        }
        else
        {
            _declaringTypeGenericArguments = Type.EmptyTypes;
        }
    }
    
    private OpCode ReadOpCode(ref BytesReader ilBytes)
    {
        if (!ilBytes.TryRead(out byte op))
        {
            throw new JayflectException("Unable to read the first byte of an OpCode");
        }
        // Is not two-byte opcode signifier
        if (op != 0XFE)
        {
            return _oneByteOpCodes[op];
        }
        if (!ilBytes.TryRead(out op))
        {
            throw new JayflectException("Unable to read the second byte of an OpCode");
        }
        return _twoByteOpCodes[op];
    }
    
    private object GetVariable(OpCode opCode, int index)
    {
        if (MemoryExtensions.IndexOf<char>(opCode.Name, "loc") >= 0)
        {
            return Locals[index];
        }
        else
        {
            return Parameters[index];
        }
    }
    
    private object? ReadOperand(OpCode opcode, ref BytesReader ilBytes)
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
        var ilStream = new InstructionStream();
        BytesReader ilBytes = new BytesReader(_ilBytes);
        while (ilBytes.AvailableByteCount > 0)
        {
            var opCode = ReadOpCode(ref ilBytes);
            object? operand = ReadOperand(opCode, ref ilBytes);
            var instruction = new OpInstruction(opCode, operand);
            var line = new InstructionLine(ilBytes.Position, instruction);
            ilStream.AddLast(line);
        }
        
        // Resolve branches
        foreach (var opInstruction in ilStream.Select(line => (line.Instruction as OpInstruction)!))
        {
            switch (opInstruction.OpCode.OperandType)
            {
                case OperandType.ShortInlineBrTarget:
                case OperandType.InlineBrTarget:
                {
                    if (!opInstruction.Value.Is<int>(out var offset))
                        throw new InvalidOperationException();
                    var line = ilStream.FindByOffset(offset);
                    if (line is null) 
                        throw new InvalidOperationException();
                    opInstruction.Value = line.Instruction;
                    break;
                }
                case OperandType.InlineSwitch:
                {
                    if (!opInstruction.Value.Is<int[]>(out var offsets))
                        throw new InvalidOperationException();
                    var branches = new Instruction[offsets.Length];
                    for (int i = 0; i < offsets.Length; i++)
                    {
                        var line = ilStream.FindByOffset(offsets[i]);
                        if (line is null) 
                            throw new InvalidOperationException();
                        branches[i] = line.Instruction;
                    }
                    opInstruction.Value = branches;
                    break;
                }
            }
        }
        
        // fin
        return ilStream;
    }
}