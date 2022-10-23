using Jay.Extensions;
using Jayflect.Building.Emission.Instructions;

// ReSharper disable IdentifierTypo

namespace Jayflect.Building.Emission;

public sealed class FluentILGenerator : IFluentILEmitter
{
    private readonly ILGenerator _ilGenerator;
    private readonly List<EmitterLabel> _labels;
    private readonly List<EmitterLocal> _locals;

    public InstructionStream Instructions { get; }

    public int ILOffset => _ilGenerator.ILOffset;

    public FluentILGenerator(ILGenerator ilGenerator)
    {
        ArgumentNullException.ThrowIfNull(ilGenerator);
        _ilGenerator = ilGenerator;
        _labels = new(0);
        _locals = new(0);
        this.Instructions = new();
    }

    [return: NotNullIfNotNull(nameof(name))]
    private static string? GetVariableName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        // Fix 'out var XYZ' having 'var XYZ' as a name
        var i = name.LastIndexOf(' ');
        if (i >= 0)
        {
            return name.Substring(i + 1);
        }
        return name;
    }

    private EmitterLabel AddLabel(Label label, string? lblName)
    {
        if (label.GetHashCode() != _labels.Count)
        {
            throw new ArgumentException("The given label does not fit in sequence", nameof(label));
        }

        var emitterLabel = new EmitterLabel(label, GetVariableName(lblName));
        _labels.Add(emitterLabel);
        return emitterLabel;
    }

    private EmitterLocal AddLocal(LocalBuilder local, string? localName)
    {
        if (local.LocalIndex != _locals.Count)
        {
            throw new ArgumentException("The given local does not fit in sequence", nameof(local));
        }

        var emitterLocal = new EmitterLocal(local, GetVariableName(localName));
        _locals.Add(emitterLocal);
        return emitterLocal;
    }

    private void AddInstruction(Instruction instruction)
    {
        var line = new InstructionLine(this.ILOffset, instruction);
        this.Instructions.AddLast(line);
    }


    public ITryCatchFinallyBuilder<IFluentILEmitter> Try(Action<IFluentILEmitter> tryBlock)
    {
        return new TCFBuilder<IFluentILEmitter>(this).Try(tryBlock);
    }

    public IFluentILEmitter BeginCatchBlock(Type exceptionType)
    {
        ArgumentNullException.ThrowIfNull(exceptionType);
        if (!exceptionType.Implements<Exception>())
            throw new ArgumentException($"{nameof(exceptionType)} is not a valid Exception Type", nameof(exceptionType));
        _ilGenerator.BeginCatchBlock(exceptionType);
        AddInstruction(GeneratorInstruction.BeginCatchBlock(exceptionType));
        return this;
    }

    public IFluentILEmitter BeginExceptFilterBlock()
    {
        _ilGenerator.BeginExceptFilterBlock();
        AddInstruction(GeneratorInstruction.BeginExceptFilterBlock());
        return this;
    }

    public IFluentILEmitter BeginExceptionBlock(out EmitterLabel emitterLabel, [CallerArgumentExpression("emitterLabel")] string lblName = "")
    {
        var label = _ilGenerator.BeginExceptionBlock();
        emitterLabel = AddLabel(label, lblName);
        AddInstruction(GeneratorInstruction.BeginExceptionBlock(emitterLabel));
        return this;
    }

    public IFluentILEmitter EndExceptionBlock()
    {
        _ilGenerator.EndExceptionBlock();
        AddInstruction(GeneratorInstruction.EndExceptionBlock());
        return this;
    }

    public IFluentILEmitter BeginFaultBlock()
    {
        _ilGenerator.BeginFaultBlock();
        AddInstruction(GeneratorInstruction.BeginFaultBlock());
        return this;
    }

    public IFluentILEmitter BeginFinallyBlock()
    {
        _ilGenerator.BeginFinallyBlock();
        AddInstruction(GeneratorInstruction.BeginFinallyBlock());
        return this;
    }

    public IFluentILEmitter BeginScope()
    {
        _ilGenerator.BeginScope();
        AddInstruction(GeneratorInstruction.BeginScope());
        return this;
    }

    public IFluentILEmitter EndScope()
    {
        _ilGenerator.EndScope();
        AddInstruction(GeneratorInstruction.EndScope());
        return this;
    }

    public IFluentILEmitter UsingNamespace(string @namespace)
    {
        // TODO: Validate namespace
        _ilGenerator.UsingNamespace(@namespace);
        AddInstruction(GeneratorInstruction.UsingNamespace(@namespace));
        return this;
    }

    public IFluentILEmitter DeclareLocal(Type localType, out EmitterLocal emitterLocal,
        [CallerArgumentExpression("emitterLocal")] string localName = "")
    {
        ArgumentNullException.ThrowIfNull(localType);
        var local = _ilGenerator.DeclareLocal(localType);
        emitterLocal = AddLocal(local, localName);
        AddInstruction(GeneratorInstruction.DeclareLocal(emitterLocal));
        return this;
    }

    public IFluentILEmitter DeclareLocal(Type localType, bool pinned, out EmitterLocal emitterLocal,
        [CallerArgumentExpression("emitterLocal")] string localName = "")
    {
        ArgumentNullException.ThrowIfNull(localType);
        var local = _ilGenerator.DeclareLocal(localType, pinned);
        emitterLocal = AddLocal(local, localName);
        AddInstruction(GeneratorInstruction.DeclareLocal(emitterLocal));
        return this;
    }

    public IFluentILEmitter DefineLabel(out EmitterLabel emitterLabel, [CallerArgumentExpression("emitterLabel")] string lblName = "")
    {
        var label = _ilGenerator.DefineLabel();
        emitterLabel = AddLabel(label, lblName);
        AddInstruction(GeneratorInstruction.DefineLabel(emitterLabel));
        return this;
    }

    public IFluentILEmitter MarkLabel(EmitterLabel emitterLabel)
    {
        _ilGenerator.MarkLabel(emitterLabel);
        AddInstruction(GeneratorInstruction.MarkLabel(emitterLabel));
        return this;
    }

    public IFluentILEmitter EmitCall(MethodInfo method, Type[]? optionParameterTypes)
    {
        _ilGenerator.EmitCall(method.GetCallOpCode(),
            method,
            optionParameterTypes);
        AddInstruction(GeneratorInstruction.EmitCall(method, optionParameterTypes));
        return this;
    }

    public IFluentILEmitter EmitCalli(CallingConvention convention, Type? returnType, Type[]? parameterTypes)
    {
        _ilGenerator.EmitCalli(
            OpCodes.Calli,
            convention,
            returnType,
            parameterTypes);
        AddInstruction(GeneratorInstruction.EmitCalli(convention, returnType, parameterTypes));
        return this;
    }

    public IFluentILEmitter EmitCalli(CallingConventions conventions, Type? returnType, Type[]? parameterTypes,
        params Type[]? optionParameterTypes)
    {
        _ilGenerator.EmitCalli(OpCodes.Calli,
            conventions,
            returnType,
            parameterTypes,
            optionParameterTypes);
        AddInstruction(GeneratorInstruction.EmitCalli(conventions, returnType, parameterTypes, optionParameterTypes));
        return this;
    }

    public IFluentILEmitter ThrowException(Type exceptionType)
    {
        ArgumentNullException.ThrowIfNull(exceptionType);
        if (!exceptionType.IsAssignableTo(typeof(Exception)))
            throw new ArgumentException($"{nameof(exceptionType)} is not a valid Exception Type", nameof(exceptionType));
        _ilGenerator.ThrowException(exceptionType);
        AddInstruction((GeneratorInstruction.ThrowException(exceptionType)));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode)
    {
        _ilGenerator.Emit(opCode);
        AddInstruction(new OpInstruction(opCode));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, byte value)
    {
        _ilGenerator.Emit(opCode, value);
        AddInstruction(new OpInstruction(opCode, value));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, sbyte value)
    {
        _ilGenerator.Emit(opCode, value);
        AddInstruction(new OpInstruction(opCode, value));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, short value)
    {
        _ilGenerator.Emit(opCode, value);
        AddInstruction(new OpInstruction(opCode, value));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, int value)
    {
        _ilGenerator.Emit(opCode, value);
        AddInstruction(new OpInstruction(opCode, value));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, long value)
    {
        _ilGenerator.Emit(opCode, value);
        AddInstruction(new OpInstruction(opCode, value));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, float value)
    {
        _ilGenerator.Emit(opCode, value);
        AddInstruction(new OpInstruction(opCode, value));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, double value)
    {
        _ilGenerator.Emit(opCode, value);
        AddInstruction(new OpInstruction(opCode, value));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, string? str)
    {
        str ??= "";
        _ilGenerator.Emit(opCode, str);
        AddInstruction(new OpInstruction(opCode, str));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, FieldInfo field)
    {
        _ilGenerator.Emit(opCode, field);
        AddInstruction(new OpInstruction(opCode, field));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, MethodInfo method)
    {
        _ilGenerator.Emit(opCode, method);
        AddInstruction(new OpInstruction(opCode, method));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, ConstructorInfo ctor)
    {
        _ilGenerator.Emit(opCode, ctor);
        AddInstruction(new OpInstruction(opCode, ctor));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, SignatureHelper signature)
    {
        _ilGenerator.Emit(opCode, signature);
        AddInstruction(new OpInstruction(opCode, signature));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, Type type)
    {
        _ilGenerator.Emit(opCode, type);
        AddInstruction(new OpInstruction(opCode, type));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, EmitterLocal local)
    {
        _ilGenerator.Emit(opCode, local);
        AddInstruction(new OpInstruction(opCode, local));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, EmitterLabel label)
    {
        _ilGenerator.Emit(opCode, label);
        AddInstruction(new OpInstruction(opCode, label));
        return this;
    }

    public IFluentILEmitter Emit(OpCode opCode, params EmitterLabel[] labels)
    {
        _ilGenerator.Emit(opCode, Array.ConvertAll(labels, el => el.Label));
        AddInstruction(new OpInstruction(opCode, labels));
        return this;
    }

    public override string ToString()
    {
        return Instructions.ToString();
    }
}