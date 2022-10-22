using System.Diagnostics;
using Jay.Extensions;
using Jayflect.Building.Emission.Instructions;

// ReSharper disable IdentifierTypo

namespace Jayflect.Building.Emission;

public sealed class FluentILGenerator : IFluentILEmitter<FluentILGenerator>
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

    private EmitterLabel GetLabel(Label label)
    {
        foreach (EmitterLabel emitterLabel in _labels)
        {
            if (emitterLabel == label) return emitterLabel;
        }
        throw new ArgumentException("The given Label does not belong to this Emitter", nameof(label));
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
    
    private EmitterLocal GetLocal(LocalBuilder localBuilder)
    {
        foreach (EmitterLocal emitterLocal in _locals)
        {
            if (emitterLocal == localBuilder) return emitterLocal;
        }
        throw new ArgumentException("The given LocalBuilder does not belong to this Emitter", nameof(localBuilder));
    }

    private void AddInstruction(Instruction instruction)
    {
        var line = new InstructionLine(this.ILOffset, instruction);
        this.Instructions.AddLast(line);
    }

    private sealed class TryCatchFinallyEmitter : ITryCatchFinallyEmitter<FluentILGenerator>
    {
        private readonly FluentILGenerator _emitter;

        public FluentILGenerator EndTry
        {
            get
            {
                _emitter.EndExceptionBlock();
                return _emitter;
            }
        }

        public TryCatchFinallyEmitter(FluentILGenerator emitter)
        {
            _emitter = emitter;
        }

        public void Try(Action<FluentILGenerator> tryBlock)
        {
            _emitter.BeginExceptionBlock(out _);
            tryBlock(_emitter);
        }

        public ITryCatchFinallyEmitter<FluentILGenerator> Catch(Type exceptionType, Action<FluentILGenerator> catchBlock)
        {
            _emitter.BeginCatchBlock(exceptionType);
            catchBlock(_emitter);
            return this;
        }

        public FluentILGenerator Finally(Action<FluentILGenerator> finallyBlock)
        {
            _emitter.BeginFinallyBlock();
            finallyBlock(_emitter);
            _emitter.EndExceptionBlock();
            return _emitter;
        }
    }

    public ITryCatchFinallyEmitter<FluentILGenerator> Try(Action<FluentILGenerator> tryBlock)
    {
        var tcf = new TryCatchFinallyEmitter(this);
        tcf.Try(tryBlock);
        return tcf;
    }

    public FluentILGenerator BeginCatchBlock(Type exceptionType)
    {
        ArgumentNullException.ThrowIfNull(exceptionType);
        if (!exceptionType.Implements<Exception>())
            throw new ArgumentException($"{nameof(exceptionType)} is not a valid Exception Type", nameof(exceptionType));
        _ilGenerator.BeginCatchBlock(exceptionType);
        AddInstruction(GeneratorInstruction.BeginCatchBlock(exceptionType));
        return this;
    }

    public FluentILGenerator BeginExceptFilterBlock()
    {
        _ilGenerator.BeginExceptFilterBlock();
        AddInstruction(GeneratorInstruction.BeginExceptFilterBlock());
        return this;
    }

    public FluentILGenerator BeginExceptionBlock(out Label label, [CallerArgumentExpression("label")] string lblName = "")
    {
        label = _ilGenerator.BeginExceptionBlock();
        var emitterLabel = AddLabel(label, lblName);
        AddInstruction(GeneratorInstruction.BeginExceptionBlock(emitterLabel));
        return this;
    }

    public FluentILGenerator EndExceptionBlock()
    {
        _ilGenerator.EndExceptionBlock();
        AddInstruction(GeneratorInstruction.EndExceptionBlock());
        return this;
    }

    public FluentILGenerator BeginFaultBlock()
    {
        _ilGenerator.BeginFaultBlock();
        AddInstruction(GeneratorInstruction.BeginFaultBlock());
        return this;
    }

    public FluentILGenerator BeginFinallyBlock()
    {
        _ilGenerator.BeginFinallyBlock();
        AddInstruction(GeneratorInstruction.BeginFinallyBlock());
        return this;
    }

    public FluentILGenerator BeginScope()
    {
        _ilGenerator.BeginScope();
        AddInstruction(GeneratorInstruction.BeginScope());
        return this;
    }

    public FluentILGenerator EndScope()
    {
        _ilGenerator.EndScope();
        AddInstruction(GeneratorInstruction.EndScope());
        return this;
    }

    public FluentILGenerator UsingNamespace(string @namespace)
    {
        // TODO: Validate namespace
        _ilGenerator.UsingNamespace(@namespace);
        AddInstruction(GeneratorInstruction.UsingNamespace(@namespace));
        return this;
    }

    public FluentILGenerator DeclareLocal(Type localType, out LocalBuilder local, [CallerArgumentExpression("local")] string localName = "")
    {
        ArgumentNullException.ThrowIfNull(localType);
        local = _ilGenerator.DeclareLocal(localType);
        var emitterLocal = AddLocal(local, localName);
       AddInstruction(GeneratorInstruction.DeclareLocal(emitterLocal));
        return this;
    }

    public FluentILGenerator DeclareLocal(Type localType, bool pinned, out LocalBuilder local,
        [CallerArgumentExpression("local")] string localName = "")
    {
        ArgumentNullException.ThrowIfNull(localType);
        local = _ilGenerator.DeclareLocal(localType, pinned);
        var emitterLocal = AddLocal(local, localName);
        AddInstruction(GeneratorInstruction.DeclareLocal(emitterLocal));
        return this;
    }

    public FluentILGenerator DefineLabel(out Label label, [CallerArgumentExpression("label")] string lblName = "")
    {
        label = _ilGenerator.DefineLabel();
        var emitterLable = AddLabel(label, lblName);
        AddInstruction(GeneratorInstruction.DefineLabel(emitterLable));
        return this;
    }

    public FluentILGenerator MarkLabel(Label label)
    {
        _ilGenerator.MarkLabel(label);
        var emitterLabel = GetLabel(label);
        AddInstruction(GeneratorInstruction.MarkLabel(emitterLabel));
        return this;
    }

    public FluentILGenerator EmitCall(MethodInfo method, Type[]? optionParameterTypes)
    {
        _ilGenerator.EmitCall(method.GetCallOpCode(),
            method,
            optionParameterTypes);
        AddInstruction(GeneratorInstruction.EmitCall(method, optionParameterTypes));
        return this;
    }

    public FluentILGenerator EmitCalli(CallingConvention convention, Type? returnType, Type[]? parameterTypes)
    {
        _ilGenerator.EmitCalli(
            OpCodes.Calli,
            convention,
            returnType,
            parameterTypes);
        AddInstruction(GeneratorInstruction.EmitCalli(convention, returnType, parameterTypes));
        return this;
    }

    public FluentILGenerator EmitCalli(CallingConventions conventions, Type? returnType, Type[]? parameterTypes,
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

    public FluentILGenerator ThrowException(Type exceptionType)
    {
        ArgumentNullException.ThrowIfNull(exceptionType);
        if (!exceptionType.IsAssignableTo(typeof(Exception)))
            throw new ArgumentException($"{nameof(exceptionType)} is not a valid Exception Type", nameof(exceptionType));
        _ilGenerator.ThrowException(exceptionType);
        AddInstruction((GeneratorInstruction.ThrowException(exceptionType)));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode)
    {
        _ilGenerator.Emit(opCode);
        AddInstruction(new OpInstruction(opCode));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, byte value)
    {
        _ilGenerator.Emit(opCode, value);
        AddInstruction(new OpInstruction(opCode, value));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, sbyte value)
    {
        _ilGenerator.Emit(opCode, value);
        AddInstruction(new OpInstruction(opCode, value));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, short value)
    {
        _ilGenerator.Emit(opCode, value);
        AddInstruction(new OpInstruction(opCode, value));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, int value)
    {
        _ilGenerator.Emit(opCode, value);
        AddInstruction(new OpInstruction(opCode, value));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, long value)
    {
        _ilGenerator.Emit(opCode, value);
        AddInstruction(new OpInstruction(opCode, value));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, float value)
    {
        _ilGenerator.Emit(opCode, value);
        AddInstruction(new OpInstruction(opCode, value));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, double value)
    {
        _ilGenerator.Emit(opCode, value);
        AddInstruction(new OpInstruction(opCode, value));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, string? str)
    {
        _ilGenerator.Emit(opCode, str);
        AddInstruction(new OpInstruction(opCode, str ?? ""));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, FieldInfo field)
    {
        _ilGenerator.Emit(opCode, field);
        AddInstruction(new OpInstruction(opCode, field));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, MethodInfo method)
    {
        _ilGenerator.Emit(opCode, method);
        AddInstruction(new OpInstruction(opCode, method));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, ConstructorInfo ctor)
    {
        _ilGenerator.Emit(opCode, ctor);
        AddInstruction(new OpInstruction(opCode, ctor));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, SignatureHelper signature)
    {
        _ilGenerator.Emit(opCode, signature);
        AddInstruction(new OpInstruction(opCode, signature));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, Type type)
    {
        _ilGenerator.Emit(opCode, type);
        AddInstruction(new OpInstruction(opCode, type));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, LocalBuilder local)
    {
        _ilGenerator.Emit(opCode, local);
        AddInstruction(new OpInstruction(opCode, local));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, Label label)
    {
        _ilGenerator.Emit(opCode, label);
        AddInstruction(new OpInstruction(opCode, label));
        return this;
    }

    public FluentILGenerator Emit(OpCode opCode, params Label[] labels)
    {
        _ilGenerator.Emit(opCode, labels);
        AddInstruction(new OpInstruction(opCode, labels));
        return this;
    }

    public override string ToString()
    {
        return Instructions.ToString();
    }
}