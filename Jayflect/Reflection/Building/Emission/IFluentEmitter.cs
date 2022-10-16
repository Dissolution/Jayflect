using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Jay.Dumping;
using Jay.Reflection.Exceptions;
using Jay.Reflection.Internal;
using Jay.Validation;

// ReSharper disable UnusedMember.Global
#pragma warning disable CS8321

// ReSharper disable IdentifierTypo

namespace Jay.Reflection.Building.Emission;

public interface IFluentEmitter<out TEmitter> : IOpEmitter<TEmitter>
    where TEmitter : class, IFluentEmitter<TEmitter>
{

    TEmitter Ldarg(ParameterInfo parameter) => Ldarg(parameter.Position);
    
    ITryCatchFinallyEmitter<TEmitter> Try(Action<TEmitter> tryBlock);

    TEmitter DefineAndMarkLabel(out Label label, [CallerArgumentExpression("label")] string lblName = "") 
        => DefineLabel(out label, lblName).MarkLabel(label);

    TEmitter ThrowException(Type exceptionType)
    {
        ArgumentNullException.ThrowIfNull(exceptionType);
        if (!exceptionType.Implements<Exception>())
            throw new ArgumentException("Invalid Exception Type", nameof(exceptionType));
        var ctor = exceptionType.GetConstructor(Reflect.InstanceFlags, Type.EmptyTypes);
        if (ctor is null)
        {
            return LoadUninitialized(exceptionType).Throw();
        }
        else
        {
            return Newobj(ctor).Throw();
        }
    }

    /// <summary>
    /// Emits the instructions to throw an <see cref="Exception"/>.
    /// </summary>
    /// <typeparam name="TException">The <see cref="Type"/> of <see cref="Exception"/> to throw.</typeparam>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.throwexception?view=netcore-3.0"/>
    TEmitter ThrowException<TException>()
        where TException : Exception, new() => ThrowException(typeof(TException));

    TEmitter ThrowException<TException>(params object?[] ctorArgs)
        where TException : Exception
    {
        var argTypes = ctorArgs.ToTypeArray();
        var ctor = EmitterHelpers.FindConstructors(typeof(TException), argTypes)
                                 .FirstOrDefault(ctor => ctor.GetParameters().All(p => CanLoad(p.ParameterType)));
        if (ctor is null)
            throw new ArgumentException($"Cannot construct a {typeof(TException)} exception with these arguments",
                                        nameof(argTypes));
        foreach (var arg in ctorArgs)
        {
            Load(arg);
        }
        return Newobj(ctor).Throw();
    }

    internal TEmitter Do(Action<TEmitter> emission)
    {
        emission((TEmitter)this);
        return (TEmitter)this;
    }
    
    TEmitter Scoped(Action<TEmitter> scopedBlock)
    {
        return BeginScope().Do(scopedBlock).EndScope();
    }
    
    TEmitter Br(out Label label, [CallerArgumentExpression("label")] string lblName = "") => DefineLabel(out label, lblName).Br(label);
    TEmitter Leave(out Label label, [CallerArgumentExpression("label")] string lblName = "") => DefineLabel(out label, lblName).Leave(label);
    TEmitter Brtrue(out Label label, [CallerArgumentExpression("label")] string lblName = "") => DefineLabel(out label, lblName).Brtrue(label);
    TEmitter Brfalse(out Label label, [CallerArgumentExpression("label")] string lblName = "") => DefineLabel(out label, lblName).Brfalse(label);
    TEmitter Beq(out Label label, [CallerArgumentExpression("label")] string lblName = "") => DefineLabel(out label, lblName).Beq(label);
    TEmitter Bne_Un(out Label label, [CallerArgumentExpression("label")] string lblName = "") => DefineLabel(out label, lblName).Bne_Un(label);
    TEmitter Bge(out Label label, [CallerArgumentExpression("label")] string lblName = "") => DefineLabel(out label, lblName).Bge(label);
    TEmitter Bge_Un(out Label label, [CallerArgumentExpression("label")] string lblName = "") => DefineLabel(out label, lblName).Bge_Un(label);
    TEmitter Bgt(out Label label, [CallerArgumentExpression("label")] string lblName = "") => DefineLabel(out label, lblName).Bgt(label);
    TEmitter Bgt_Un(out Label label, [CallerArgumentExpression("label")] string lblName = "") => DefineLabel(out label, lblName).Bgt_Un(label);
    TEmitter Ble(out Label label, [CallerArgumentExpression("label")] string lblName = "") => DefineLabel(out label, lblName).Ble(label);
    TEmitter Ble_Un(out Label label, [CallerArgumentExpression("label")] string lblName = "") => DefineLabel(out label, lblName).Ble_Un(label);
    TEmitter Blt(out Label label, [CallerArgumentExpression("label")] string lblName = "") => DefineLabel(out label, lblName).Blt(label);
    TEmitter Blt_Un(out Label label, [CallerArgumentExpression("label")] string lblName = "") => DefineLabel(out label, lblName).Blt_Un(label);

    TEmitter Cge() => Clt().Not();
    TEmitter Cge_Un() => Clt_Un().Not();
    TEmitter Cle() => Cgt().Not();
    TEmitter Cle_Un() => Cgt_Un().Not();

    Result CanLoad(Type? type)
    {
        if (type is null)
            return true;
        if (type == typeof(bool) || type == typeof(byte) || type == typeof(sbyte) ||
            type == typeof(short) || type == typeof(ushort) ||
            type == typeof(int) || type == typeof(uint) ||
            type == typeof(long) || type == typeof(ulong) ||
            type == typeof(float) || type == typeof(double) ||
            type == typeof(string) || type == typeof(Type)
           )
        {
            return true;
        }
        return false;
    }

    TEmitter Load<T>(T value)
    {
        if (value is null)
            return Ldnull();
        if (value is bool boolean)
            return boolean ? Ldc_I4_1() : Ldc_I4_0();
        if (value is byte b)
            return Ldc_I4(b);
        if (value is sbyte sb)
            return Ldc_I4(sb);
        if (value is short s)
            return Ldc_I4(s);
        if (value is ushort us)
            return Ldc_I4(us);
        if (value is int i)
            return Ldc_I4(i);
        if (value is uint ui)
            return Ldc_I8(ui);
        if (value is long l)
            return Ldc_I8(l);
        if (value is ulong ul)
            return Ldc_I8((long)ul);
        if (value is float f)
            return Ldc_R4(f);
        if (value is double d)
            return Ldc_R8(d);
        if (value is string str)
            return Ldstr(str);
        if (value is Type type)
            return LoadType(type);
        if (value is LocalBuilder local)
            return Ldloc(local);

        throw new NotImplementedException();
    }

    TEmitter LoadArg(int index) => Ldarg(index);

    TEmitter LoadArg(ParameterInfo parameter) => Ldarg(parameter.Position);

    TEmitter LoadType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return Ldtoken(type).Call(MethodInfoCache.Type_GetTypeFromHandle);
    }
    TEmitter LoadType<T>() => LoadType(typeof(T));

    TEmitter LoadUninitialized(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return LoadType(type)
               .Call(MethodInfoCache.RuntimeHelpers_GetUninitializedObject)
               .Cast(typeof(object), type);
    }
    TEmitter LoadUninitialized<T>() => LoadUninitialized(typeof(T));

    //Always the most up-to-date
    TEmitter LoadAs(ParameterInfo input, Type? outputType)
    {
        if (outputType is null || outputType == typeof(void))
        {
            // Output requires nothing, we have nothing loaded
            return (TEmitter)this;
        }

        if (outputType == typeof(void*))
        {
            throw new NotImplementedException("Does not support void*");
        }

        if (input.ParameterType == typeof(void))
        {
            // We're expecting a value that we're not given
            throw new ArgumentException("We're expecting a value that we're not given", nameof(input));
        }

        if (input.ParameterType == typeof(void*))
        {
            throw new NotImplementedException("Does not support void*");
        }

        var inputAccess = input.GetAccess(out var inputType);
        bool inputIsRef = inputAccess != ParameterAccess.Default;

        bool outputIsRef = outputType.IsByRef;
        if (outputIsRef)
        {
            outputType = outputType.GetElementType()!;
        }

        // If we have exactly what we need, it's fairly easy
        if (inputType == outputType)
        {
            if (inputIsRef == outputIsRef)
            {
                // Exactly what we want
                Ldarg(input.Position);
            }
            else if (!inputIsRef)
            {
                // out is ref
                // todo: safety shit
                Ldarga(input.Position);
            }
            else
            {
                Debug.Assert(!outputIsRef);
                // in is ref
                // todo: safety shit
                Ldarg(input.Position).Ldind(outputType);
            }
            return (TEmitter)this;
        }

        // Coming from object?
        if (inputType == typeof(object) && !inputIsRef)
        {
            if (outputType.IsValueType)
            {
                if (!outputIsRef)
                {
                    Ldarg(input.Position)
                           .Unbox_Any(outputType);
                }
                else
                {
                    Ldarg(input.Position)
                           .Unbox(outputType);
                }
            }
            else
            {
                if (!outputIsRef)
                {
                    Ldarg(input.Position)
                           .Castclass(outputType);
                }
                else
                {
                    throw new NotImplementedException("Non-default outputs are not yet supported");
                }
            }
            return (TEmitter)this;
        }

        if (inputIsRef)
        {
            throw new NotImplementedException("Non-default inputs are not yet supported");
        }

        if (outputIsRef)
        {
            throw new NotImplementedException("Non-default outputs are not yet supported");
        }

        // Going to object?
        if (outputType == typeof(object))
        {
            if (inputType.IsValueType)
            {
                Ldarg(input.Position)
                       .Box(inputType);
            }
            else
            {
                // Is already object
                Ldarg(input.Position);
            }
            return (TEmitter)this;
        }

        // Implements?
        // TODO: More advanced logic here?
        if (inputType.IsAssignableTo(outputType))
        {
            Debug.Assert(inputType.IsClass);
            Debug.Assert(outputType.IsClass);
            Ldarg(input.Position)
                   .Castclass(outputType);
            return (TEmitter)this;
        }

        throw new NotImplementedException($"Cannot cast from {input} to {outputType}");

    }

    internal Result TryEmitCast(Type? inputType, Type? outputType)
    {
        inputType ??= typeof(void);
        outputType ??= typeof(void);

        if (inputType.IsPointer || outputType.IsPointer)
            return new NotImplementedException(Dumper.Dump($"Cannot cast from {inputType} to {outputType}: Pointers not supported"));
        
        // Have nothing?
        if (inputType == typeof(void))
        {
            // Only okay if we also expect nothing
            if (outputType == typeof(void))
                return true;
            return new ArgumentException(Dumper.Dump($"Cannot cast from {inputType} to {outputType}: Nothing to input"), nameof(inputType));
        }
        
        // Expect nothing?
        if (outputType == typeof(void))
        {
            // We know that InputType != null (from above)
            // So just pop the value off the stack
            Pop();
            return true;
        }
        
        bool inputIsRef = inputType.IsByRef;
        if (inputIsRef)
        {
            inputType = inputType.GetElementType()!;
        }

        bool outputIsRef = outputType.IsByRef;
        if (outputIsRef)
        {
            outputType = outputType.GetElementType()!;
        }

        // If we have exactly what we need, it's fairly easy
        if (inputType == outputType)
        {
            if (inputIsRef == outputIsRef)
            {
                // Exactly what we want, do nothing
            }
            else if (!inputIsRef)  // !inputIsRef == outputIsRef
            {
                // in -> ref out
                DeclareLocal(outputType, out var lclOutput)
                    .Stloc(lclOutput)
                    .Ldloca(lclOutput);
            }
            else   // inputIsRef == !outputIsRef
            {
                // ref in -> out
                Ldind(outputType);
            }
            return true;
        }

        // Coming from object?
        if (inputType == typeof(object) && !inputIsRef)
        {
            // To struct?
            if (outputType.IsValueType)
            {
                if (!outputIsRef)
                {
                    // object -> struct
                    Unbox_Any(outputType);
                }
                else
                {
                    // object -> ref struct
                    Unbox(outputType);
                }
            }
            // To class
            else
            {
                if (!outputIsRef)
                {
                    // object -> class
                    Castclass(outputType);
                }
                else
                {
                    // object -> ref class
                    return new ArgumentException(Dumper.Dump($"Cannot cast from {inputType} to {outputType}: Not possible"), nameof(outputType));
                }
            }
            return true;
        }
        
        // Going to object?
        if (outputType == typeof(object) && !outputIsRef)
        {
            if (inputType.IsValueType)
            {
                Box(inputType);
            }
            else
            {
                // Is already object, do nothing
            }
            return true;
        }

        // Implements?
        if (inputType.IsAssignableTo(outputType))
        {
            Castclass(outputType);
            return true;
        }

        return new NotImplementedException(Dumper.Dump($"Cannot cast from {inputType} to {outputType}: Not implemented"));
    }
    
    TEmitter Cast(Type? inputType, Type? outputType)
    {
        TryEmitCast(inputType, outputType).ThrowIfNull();
        return (TEmitter)this;
    }

    TEmitter LoadParams(ParameterInfo paramsParameter, IReadOnlyList<ParameterInfo> parameters)
    {
        if (/*!paramsParameter.IsParams() || */paramsParameter.ParameterType != typeof(object[]))
            throw new ArgumentException("Parameter is not params", nameof(paramsParameter));
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters));
        var count = parameters.Count;

        DefineLabel(out Label lblOk)
               // Load the params value (object[])
               .Ldarg(paramsParameter.Position)
               // Load its Length
               .Ldlen()
               // Check that it is equal to the number of parameters we have to fill
               .Ldc_I4(count)
               .Beq(lblOk)
               // They weren't, throw
               //TODO: Build better thrower
               .ThrowException<InvalidOperationException>()
               .MarkLabel(lblOk);
        // Load each item in turn and cast it to the parameter
        for (var i = 0; i < count; i++)
        {
            // Load object[]
            Ldarg(paramsParameter.Position)
                   // Load element index
                   .Ldc_I4(i);
            var parameter = parameters[i];
            var access = parameter.GetAccess(out _);
            // TODO: Test this!
            if (access == ParameterAccess.Default)
            {
                // Load the element
                Ldelem(parameter.ParameterType);
            }
            else
            {
                // TODO: Safety checks
                // Load the element reference
                Ldelema(parameter.ParameterType);
            }
        }
        // All params are loaded in order with nothing extra laying on the stack
        return (TEmitter)this;
    }


    TEmitter LoadDefault(Type type)
    {
        if (type.IsValueType)
        {
            return DeclareLocal(type, out var def)
                   .Ldloca(def)
                   .Initobj(type)
                   .Ldloc(def);
        }
        return Ldnull();
    }
    TEmitter LoadDefault<T>() => LoadDefault(typeof(T));

    TEmitter LoadInstanceFor(MemberInfo member, ParameterInfo? possibleInstanceParameter, out int offset)
    {
        var result = TryLoadInstanceFor(member, possibleInstanceParameter);
        result.ThrowIfFailed(out offset);
        return (TEmitter)this;
    }

    Result<int> TryLoadInstanceFor(MemberInfo member,
                                   ParameterInfo? possibleInstanceParameter)
    {
        // Static method?
        if (member.IsStatic())
        {
            // Null possible is okay
            if (possibleInstanceParameter is null)
                return 0;

            // Fast get actual instance type minus in/out/ref
            Type instanceType = possibleInstanceParameter.NonRefType();

            // Look for a throwaway instance type
            if (instanceType == typeof(Types.Static) || instanceType == typeof(Types.Void) || instanceType == typeof(void))
            {
                // This is a throwaway
                return 1;
            }

            // Assume there is no throwaway
            return 0;
        }
        else
        {
            if (possibleInstanceParameter is null)
                return new ArgumentNullException(nameof(possibleInstanceParameter));

            Result result = member.TryGetInstanceType(out var methodInstanceType);
            if (!result)
                return result.WithValue<int>(0);
            result = Result.TryInvoke(() => this.LoadAs(possibleInstanceParameter, methodInstanceType!));
            if (!result)
                return result.WithValue<int>(0);

            // We loaded the instance, the rest of the parameters are used
            return 1;
        }

        TEmitter EmitInstructions(IEnumerable<Instruction> instructions)
        {
            var lblTranslation = new Dictionary<Label, Label>(0);
            var localTranslation = new Dictionary<LocalBuilder, LocalBuilder>(0);

            foreach (var instruction in instructions)
            {
                if (instruction.GenMethod != ILGeneratorMethod.None)
                {
                    switch (instruction.GenMethod)
                    {
                        case ILGeneratorMethod.BeginCatchBlock:
                        {
                            if (instruction.Arg is not Type exceptionType)
                                throw new ReflectionException();
                            BeginCatchBlock(exceptionType);
                            continue;
                        }
                        case ILGeneratorMethod.BeginExceptFilterBlock:
                        {
                            if (instruction.Arg is not null)
                                throw new ReflectionException();
                            BeginExceptFilterBlock();
                            continue;
                        }
                        case ILGeneratorMethod.BeginExceptionBlock:
                        {
                            if (instruction.Arg is not Label label)
                                throw new ReflectionException();
                            BeginExceptionBlock(out var lbl);
                            lblTranslation[label] = lbl;
                            continue;
                        }
                        case ILGeneratorMethod.EndExceptionBlock:
                        {
                            if (instruction.Arg is not null)
                                throw new ReflectionException();
                            EndExceptionBlock();
                            continue;
                        }
                        case ILGeneratorMethod.BeginFaultBlock:
                        {
                            if (instruction.Arg is not null)
                                throw new ReflectionException();
                            BeginFaultBlock();
                            continue;
                        }
                        case ILGeneratorMethod.BeginFinallyBlock:
                        {
                            if (instruction.Arg is not null)
                                throw new ReflectionException();
                            BeginFinallyBlock();
                            continue;
                        }
                        case ILGeneratorMethod.BeginScope:
                        {
                            if (instruction.Arg is not null)
                                throw new ReflectionException();
                            BeginScope();
                            continue;
                        }
                        case ILGeneratorMethod.EndScope:
                        {
                            if (instruction.Arg is not null)
                                throw new ReflectionException();
                            EndScope();
                            continue;
                        }
                        case ILGeneratorMethod.UsingNamespace:
                        {
                            if (instruction.Arg is not string usingNamespace)
                                throw new ReflectionException();
                            UsingNamespace(usingNamespace);
                            continue;
                        }
                        case ILGeneratorMethod.DeclareLocal:
                        {
                            if (instruction.Arg is not object[] args)
                                throw new ReflectionException();
                            if (args.Length == 2)
                            {
                                var type = args[0] as Type;
                                if (type is null) throw new ReflectionException();
                                var lb = args[1] as LocalBuilder;
                                if (lb is null) throw new ReflectionException();
                                DeclareLocal(type, out var localBuilder);
                                localTranslation[lb] = localBuilder;
                                continue;
                            }
                            if (args.Length == 3)
                            {
                                var type = args[0] as Type;
                                if (type is null) throw new ReflectionException();
                                if (args[1] is not bool pinned)
                                    throw new ReflectionException();
                                var lb = args[2] as LocalBuilder;
                                if (lb is null) throw new ReflectionException();
                                DeclareLocal(type, pinned, out var localBuilder);
                                localTranslation[lb] = localBuilder;
                                continue;
                            }
                            throw new ReflectionException();
                        }
                        case ILGeneratorMethod.DefineLabel:
                        {
                            if (instruction.Arg is not Label label)
                                throw new ReflectionException();
                            DefineLabel(out var lbl);
                            lblTranslation[label] = lbl;
                            continue;
                        }
                        case ILGeneratorMethod.MarkLabel:
                        {
                            if (instruction.Arg is not Label label)
                                throw new ReflectionException();
                            var lbl = lblTranslation[label];
                            MarkLabel(lbl);
                            continue;
                        }
                        case ILGeneratorMethod.EmitCall:
                        {
                            if (instruction.Arg is not object[] args)
                                throw new ReflectionException();
                            var method = args[0] as MethodInfo;
                            if (method is null) throw new ReflectionException();
                            var types = args[1] as Type[];
                            if (types is null) throw new ReflectionException();
                            EmitCall(method, types);
                            continue;
                        }
                        case ILGeneratorMethod.EmitCalli:
                        {
                            if (instruction.Arg is not object[] args)
                                throw new ReflectionException();
                            if (args.Length == 3)
                            {
                                var cc = (CallingConvention)args[0];
                                var returnType = args[1] as Type;
                                if (returnType is null) throw new ReflectionException();
                                var parameterTypes = args[2] as Type[];
                                if (parameterTypes is null) throw new ReflectionException();
                                EmitCalli(cc, returnType, parameterTypes);
                                continue;
                            }
                            if (args.Length == 4)
                            {
                                var cc = (CallingConventions)args[0];
                                var returnType = args[1] as Type;
                                if (returnType is null) throw new ReflectionException();
                                var parameterTypes = args[2] as Type[];
                                if (parameterTypes is null) throw new ReflectionException();
                                var optionalParameterTypes = args[3] as Type[];
                                if (optionalParameterTypes is null) throw new ReflectionException();
                                EmitCalli(cc, returnType, parameterTypes, optionalParameterTypes);
                                continue;
                            }
                            throw new ReflectionException();
                        }
                        case ILGeneratorMethod.WriteLine:
                            throw new NotImplementedException();
                            //break;
                        case ILGeneratorMethod.ThrowException:
                        {
                            if (instruction.Arg is not Type exceptionType)
                                throw new ReflectionException();
                            ThrowException(exceptionType);
                            continue;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    var opCode = instruction.OpCode;
                    var arg = instruction.Arg;
                    switch (arg)
                    {
                        case null:
                            Emit(opCode);
                            continue;
                        case byte b:
                            Emit(opCode, b);
                            continue;
                        case sbyte sb:
                            Emit(opCode, sb);
                            continue;
                        case short s:
                            Emit(opCode, s);
                            continue;
                        case ushort us:
                            Emit(opCode, us);
                            continue;
                        case int i:
                            Emit(opCode, i);
                            continue;
                        case uint ui:
                            Emit(opCode, ui);
                            continue;
                        case long l:
                            Emit(opCode, l);
                            continue;
                        case float f:
                            Emit(opCode, f);
                            continue;
                        case double d:
                            Emit(opCode, d);
                            continue;
                        case string str:
                            Emit(opCode, str);
                            continue;
                        case Label label:
                            var lbl = lblTranslation[label];
                            Emit(opCode, lbl);
                            continue;
                        case Label[] labels:
                            var tLabels = new Label[labels.Length];
                            for (var i = 0; i < labels.Length; i++)
                            {
                                tLabels[i] = lblTranslation[labels[i]];
                            }
                            Emit(opCode, tLabels);
                            continue;
                        case LocalBuilder local:
                            var lcl = localTranslation[local];
                            Emit(opCode, lcl);
                            continue;
                        case FieldInfo field:
                            Emit(opCode, field);
                            continue;
                        case ConstructorInfo ctor:
                            Emit(opCode, ctor);
                            continue;
                        case MethodInfo method:
                            Emit(opCode, method);
                            continue;
                        case Type type:
                            Emit(opCode, type);
                            continue;
                        case SignatureHelper signature:
                            Emit(opCode, signature);
                            continue;
                        default:
                            throw new ReflectionException();
                    }
                }
            }
            return (TEmitter)this;
        }
        
        /// <remarks> Does not account for null! </remarks>
        TEmitter EmitCompareEqual(Type primaryType, Type compareType)
        {
            // We can use Emit.Ceq if the two are the same and they are unmanaged
            if (primaryType.IsUnmanaged() && primaryType == compareType)
            {
                return Ceq();
            }

            // Look for primary.Equals(compare)
            var equalsMethod = primaryType.GetMethod("Equals",
                BindingFlags.Public | BindingFlags.Instance,
                new Type[1] { compareType });
            if (equalsMethod is not null)
            {
                return Call(equalsMethod);
            }

            // Look for ==
            equalsMethod = primaryType.GetMethod("op_Equality",
                BindingFlags.Public | BindingFlags.Static,
                new Type[2] { primaryType, compareType });
            if (equalsMethod is not null)
            {
                return Call(equalsMethod);
            }
            
            // Do we have any other Equals(?) methods (other than the default Equals(obj?))
            // that can be fulfilled by a compareType?
            equalsMethod = primaryType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => method.Name == nameof(Equals))
                .Where(method => method.GetParameters().TryGetItem(0, out var param) &&
                                 compareType.Implements(param.ParameterType))
                .OneOrDefault();
            if (equalsMethod is not null)
            {
                return Cast(compareType, equalsMethod.GetParameterTypes()[0])
                    .Call(equalsMethod);
            }

            // Fallback to Ceq, which is approx ReferenceEquals
            return Ceq();
        }

        TEmitter EmitDefaultEqualityComparerEquals(Type valueType)
        {
            var equalityComparer = typeof(EqualityComparer<>).MakeGenericType(valueType);
            
            var getDefaultComparerMethod = equalityComparer.GetMethod("get_Default", Reflect.InstanceFlags)
                .ThrowIfNull();
            var equalsMethod = equalityComparer.GetMethod("Equals",
                    BindingFlags.Public | BindingFlags.Instance,
                    new Type[2] { valueType, valueType })
                .ThrowIfNull();
            return Call(getDefaultComparerMethod)
                .Call(equalsMethod);
        }
    }
}