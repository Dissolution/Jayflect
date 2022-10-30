using Jay.Comparison;
using Jay.Dumping;
using Jay.Dumping.Extensions;
using Jay.Validation;

namespace Jayflect.Building.Emission.Instructions;

public sealed class GeneratorInstruction : Instruction
{
    public static GeneratorInstruction BeginCatchBlock(Type exceptionType)
    {
        Validate.IsExceptionType(exceptionType);
        return new(ILGeneratorMethod.BeginCatchBlock, exceptionType);
    }
    public static GeneratorInstruction BeginExceptFilterBlock()
    {
        return new(ILGeneratorMethod.BeginExceptFilterBlock);
    }
    public static GeneratorInstruction BeginExceptionBlock(EmitterLabel endOfBlock)
    {
        return new GeneratorInstruction(ILGeneratorMethod.BeginExceptionBlock, endOfBlock);
    }
    public static GeneratorInstruction EndExceptionBlock()
    {
        return new(ILGeneratorMethod.EndExceptionBlock);
    }
    public static GeneratorInstruction BeginFaultBlock()
    {
        return new(ILGeneratorMethod.BeginFaultBlock);
    }
    public static GeneratorInstruction BeginFinallyBlock()
    {
        return new(ILGeneratorMethod.BeginFinallyBlock);
    }
    public static GeneratorInstruction BeginScope()
    {
        return new(ILGeneratorMethod.BeginScope);
    }
    public static GeneratorInstruction EndScope()
    {
        return new(ILGeneratorMethod.EndScope);
    }
    public static GeneratorInstruction UsingNamespace(string @namespace)
    {
        return new(ILGeneratorMethod.UsingNamespace, @namespace);
    }
    public static GeneratorInstruction DeclareLocal(EmitterLocal local)
    {
        return new(ILGeneratorMethod.DeclareLocal, local);
    }
    public static GeneratorInstruction DefineLabel(EmitterLabel label)
    {
        return new(ILGeneratorMethod.DefineLabel, label);
    }
    public static GeneratorInstruction MarkLabel(EmitterLabel label)
    {
        return new(ILGeneratorMethod.MarkLabel, label);
    }
    public static GeneratorInstruction EmitCall(MethodInfo method, params Type[]? types)
    {
        return new(ILGeneratorMethod.EmitCall, new object[2]
        {
            method, 
            types ?? Type.EmptyTypes
        });
    }
    public static GeneratorInstruction EmitCalli(CallingConvention callingConvention, Type? returnType, params Type[]? parameterTypes)
    {
        return new(ILGeneratorMethod.EmitCalli, new object[3]
        {
            callingConvention,
            returnType ?? typeof(void), 
            parameterTypes ?? Type.EmptyTypes,
        });
    }
    public static GeneratorInstruction EmitCalli(CallingConventions callingConventions, Type? returnType, Type[]? parameterTypes, params Type[]? optionalParameterTypes)
    {
        return new(ILGeneratorMethod.EmitCalli, new object[4]
        {
            callingConventions, 
            returnType ?? typeof(void), 
            parameterTypes ?? Type.EmptyTypes, 
            optionalParameterTypes ?? Type.EmptyTypes,
        });
    }
    public static GeneratorInstruction WriteLine(string? text)
    {
        return new(ILGeneratorMethod.WriteLine, text ?? "");
    }
    public static GeneratorInstruction WriteLine(FieldInfo field)
    {
        return new(ILGeneratorMethod.WriteLine, field);
    }
    public static GeneratorInstruction WriteLine(EmitterLocal local)
    {
        return new(ILGeneratorMethod.WriteLine, local);
    }
    public static GeneratorInstruction ThrowException(Type exceptionType)
    {
        Validate.IsExceptionType(exceptionType);
        return new(ILGeneratorMethod.ThrowException, exceptionType);
    }

    public ILGeneratorMethod IlGeneratorMethod { get; }
    public object? Argument { get; }
    public object[]? ArgumentArray => Argument as object[];
    
    private GeneratorInstruction(ILGeneratorMethod ilGeneratorMethod, object? argument = null)
    {
        this.IlGeneratorMethod = ilGeneratorMethod;
        this.Argument = argument;
    }

    public override bool Equals(Instruction? instruction)
    {
        return instruction is GeneratorInstruction generatorInstruction &&
               generatorInstruction.IlGeneratorMethod == this.IlGeneratorMethod &&
               DefaultComparers.Instance.Equals(generatorInstruction.Argument, this.Argument);
    }

    public override void DumpTo(ref DumpStringHandler stringHandler, DumpFormat dumpFormat = default)
    {
        switch (this.IlGeneratorMethod)
        {
            case ILGeneratorMethod.None:
                return;
            case ILGeneratorMethod.BeginCatchBlock:
            {
                var exceptionType = Argument.ValidateInstanceOf<Type>();
                stringHandler.Write("catch (");
                stringHandler.Dump(exceptionType);
                stringHandler.Write(')');
                return;
            }
            case ILGeneratorMethod.BeginExceptFilterBlock:
            {
                stringHandler.Write("except filter");
                return;
            }
            case ILGeneratorMethod.BeginExceptionBlock:
            {
                var endOfBlock = Argument.ValidateInstanceOf<EmitterLabel>();
                stringHandler.Write("try");
                return;
            }
            case ILGeneratorMethod.EndExceptionBlock:
            {
                stringHandler.Write("end try");
                return;
            }
            case ILGeneratorMethod.BeginFaultBlock:
            {
                stringHandler.Write("fault");
                return;
            }
            case ILGeneratorMethod.BeginFinallyBlock:
            {
                stringHandler.Write("finally");
                return;
            }
            case ILGeneratorMethod.BeginScope:
            {
                stringHandler.Write("scope {");
                return;
            }
            case ILGeneratorMethod.EndScope:
            {
                stringHandler.Write("} end scope");
                return;
            }
            case ILGeneratorMethod.UsingNamespace:
            {
                var @namespace = Argument.ValidateInstanceOf<string>();
                stringHandler.Write("using ");
                stringHandler.Write(@namespace);
                return;
            }
            case ILGeneratorMethod.DeclareLocal:
            {
                var local = Argument.ValidateInstanceOf<EmitterLocal>();
                local.DumpTo(ref stringHandler, "D");
                return;
            }
            case ILGeneratorMethod.DefineLabel:
            {
                var label = Argument.ValidateInstanceOf<EmitterLabel>();
                label.DumpTo(ref stringHandler, "D");
                return;
            }
            case ILGeneratorMethod.MarkLabel:
            {
                var label = Argument.ValidateInstanceOf<EmitterLabel>();
                label.DumpTo(ref stringHandler, "M");
                return;
            }
            case ILGeneratorMethod.EmitCall:
            {
                if (ArgumentArray is null || ArgumentArray.Length != 2)
                    throw new InvalidOperationException();
                var method = ArgumentArray[0].ValidateInstanceOf<MethodInfo>();
                var types = ArgumentArray[1].ValidateInstanceOf<Type[]>();
                // varargs method
                stringHandler.Write("varargs ");
                stringHandler.Dump(method);
                stringHandler.Write(" [");
                stringHandler.DumpDelimited(", ", types);
                stringHandler.Write(']');
                return;
            }
            case ILGeneratorMethod.EmitCalli:
            {
                object[]? args = this.ArgumentArray;
                if (args is null) throw new InvalidOperationException();
                if (args.Length == 3)
                {
                    var callingConvention = args[0].ValidateInstanceOf<CallingConvention>();
                    var returnType = args[1].ValidateInstanceOf<Type>();
                    var parameterTypes = args[2].ValidateInstanceOf<Type[]>();
                    stringHandler.Write("calli ");
                    stringHandler.Write(callingConvention);
                    stringHandler.Write(' ');
                    stringHandler.Dump(returnType);
                    stringHandler.Write('(');
                    stringHandler.DumpDelimited(", ", parameterTypes);
                    stringHandler.Write(')');
                }
                else if (args.Length == 4)
                {
                    var callingConvention = args[0].ValidateInstanceOf<CallingConventions>();
                    var returnType = args[1].ValidateInstanceOf<Type>();
                    var parameterTypes = args[2].ValidateInstanceOf<Type[]>();
                    var optParameterTypes = args[3].ValidateInstanceOf<Type[]>();
                    stringHandler.Write("calli ");
                    stringHandler.Write(callingConvention);
                    stringHandler.Write(' ');
                    stringHandler.Dump(returnType);
                    stringHandler.Write('(');
                    stringHandler.DumpDelimited(", ", parameterTypes);
                    if (optParameterTypes.Length > 0)
                    {
                        stringHandler.Write("?, ");
                        stringHandler.DumpDelimited(", ", optParameterTypes);
                    }
                    stringHandler.Write(')');
                }
                else
                {
                    throw new InvalidOperationException();
                }
                return;
            }
            case ILGeneratorMethod.WriteLine:
            {
                stringHandler.Write("Console.WriteLine(");
                if (Argument is string text)
                {
                    stringHandler.Write('"');
                    stringHandler.Write(text);
                    stringHandler.Write('"');
                }
                else if (Argument is FieldInfo field)
                {
                    stringHandler.Dump(field);
                }
                else if (Argument is EmitterLocal local)
                {
                    local.DumpTo(ref stringHandler);
                }
                else
                {
                    throw new InvalidOperationException();
                }
                stringHandler.Write(')');
                return;
            }
            case ILGeneratorMethod.ThrowException:
            {
                var exceptionType = Argument.ValidateInstanceOf<Type>();
                stringHandler.Write("throw new ");
                stringHandler.Dump(exceptionType);
                return;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}