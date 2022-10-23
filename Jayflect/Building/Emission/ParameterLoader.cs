using System.Diagnostics;
using Jay;
using Jay.Dumping;
using Jay.Extensions;
using Jayflect.Exceptions;
using Jayflect.Extensions;
using Jay.Dumping.Extensions;
using Jayflect;
using Jayflect.Building.Emission;


namespace Jayflect.Building.Emission;

public class AdapterException : RuntimeBuildException
{
    private static string CreateMessage(MethodBase method,
        DelegateSignature delegateSig,
        ref DumpStringHandler message)
    {
        var stringHandler = new DumpStringHandler();
        stringHandler.Write("Could not adapt '");
        stringHandler.Dump(delegateSig);
        stringHandler.Write(" to call ");
        stringHandler.Dump(method);
        string msg = message.ToStringAndClear();
        if (msg.Length > 0)
        {
            stringHandler.Write(": ");
            stringHandler.Write(msg);
        }
        return stringHandler.ToStringAndClear();
    }

    public AdapterException(MethodBase method, DelegateSignature delegateSig,
        ref DumpStringHandler message,
        Exception? innerException = null)
        : base(CreateMessage(method, delegateSig, ref message), innerException)
    {

    }
    
    public AdapterException(ref DumpStringHandler message,
        Exception? innerException = null)
        : base(ref message, innerException)
    {

    }
}

/// <summary>
/// Creates a <see cref="Delegate"/> to call a <see cref="MethodBase"/>
/// </summary>
public class RuntimeMethodAdapter
{
    private static Type GetInstanceType(MethodBase method)
    {
        if (method.IsStatic)
            throw new ArgumentException("A static method has no Instance", nameof(method));
        var instanceType = method.ReflectedType ?? method.DeclaringType;
        if (instanceType is null)
            throw new ArgumentException(Dump($"{method} has no valid Instance Type"));
        if (instanceType.IsValueType)
            return instanceType.MakeByRefType();
        return instanceType;
    }

    public MethodBase Method { get; }
    public DelegateSignature MethodSig { get; }
    public DelegateSignature DelegateSig { get; }

    public ParameterInfo? InstanceParameter => DelegateSig.Parameters.FirstOrDefault();

    private Result TryLoadInstance(IFluentILEmitter emitter)
    {
        // We must have an instance as the first parameter
        var instanceParameter = this.InstanceParameter;
        if (instanceParameter is null)
            return new AdapterException(Method, DelegateSig, $"Missing instance parameter");
        // If we're static, we can 'load' NoInstance
        if (Method.IsStatic)
        {
            if (instanceParameter.NonRefType() == typeof(NoInstance))
                return true;
            return false; // Cannot load
        }
        // Load it
        var instanceType = GetInstanceType(Method);
        return ParameterLoader.TryLoadParameter(emitter, instanceParameter, instanceType);
    }

    private Result TryLoadParams(IFluentILEmitter emitter, int delegateParamOffset)
    {
        if (DelegateSig.ParameterCount - delegateParamOffset != 1)
            return new AdapterException(Method, DelegateSig, $"No params available as the only/last parameter");
        ParameterLoader.LoadParams(emitter, DelegateSig.Parameters.Last(), MethodSig.Parameters);
        return true;
    }

    private Result TryLoadArgs(IFluentILEmitter emitter, int delegateParamOffset)
    {
        var lastInstructionNode = emitter.Instructions.Last;
        var delParams = this.DelegateSig.Parameters;
        var methParams = this.MethodSig.Parameters;
        if (delParams.Length - delegateParamOffset != methParams.Length)
            return new AdapterException(Method, DelegateSig, $"Incorrect number of parameters available");
        Result result;
        for (var m = 0; m < methParams.Length; m++)
        {
            result = ParameterLoader.TryLoadParameter(emitter,
                delParams[m + delegateParamOffset],
                methParams[m]);
            if (!result)
            {
                emitter.Instructions.RemoveAfter(lastInstructionNode);
                return result;
            }
        }
        return true;
    }

    private Result TryLoadInstanceArgs(IFluentILEmitter emitter)
    {
        Result result;
        int offset;
        // Static Method
        if (Method.IsStatic)
        {
            // Check for throwaway
            result = TryLoadInstance(emitter);
            offset = result ? 1 : 0;
            for (; offset <= 1; offset++)
            {
                result = TryLoadParams(emitter, offset);
                if (result) return true;
                result = TryLoadArgs(emitter, offset);
                if (result) return true;
            }
            // Nothing worked
            return new AdapterException(Method, DelegateSig, $"Could not understand static method adapt");
        }
        
        // Instance Method
        result = TryLoadInstance(emitter);
        if (!result) return result;
        result = TryLoadParams(emitter, 1);
        if (result) return true;
        result = TryLoadArgs(emitter, 1);
        if (result) return true;
        return result;
    }

    public static Result TryAdapt(MethodBase method, DelegateSignature delegateSig, [NotNullWhen(true)] out Delegate? adapter)
    {
        // faster return
        adapter = default;

        Result result;
        RuntimeDelegateBuilder builder = RuntimeBuilder.CreateRuntimeDelegateBuilder(delegateSig);
        var emitter = builder.Emitter;
        var methodSig = DelegateSignature.For(method);

        // Static Method
        if (method.IsStatic)
        {
            // No passed parameters?
            if (delegateSig.ParameterCount == 0)
            {
                // Only okay if method has none
                if (methodSig.ParameterCount == 0)
                {
                    // Nothing to load
                    goto CALL;
                }
                return new AdapterException(method, delegateSig, $"Invalid parameter count");
            }

            // The last one is params?
            if (delegateSig.Parameters.Last().IsParams())
            {
                // Only okay with 0 or 1 other parameters
                if (delegateSig.ParameterCount is 0 or 1)
                {
                    ParameterLoader.LoadParams(emitter,
                        delegateSig.Parameters.Last(),
                        methodSig.Parameters);
                    goto CALL;
                }
                return new AdapterException(method, delegateSig, $"Params only supported as the last parameter");
            }

            // 1-1 : 1
            if (methodSig.ParameterCount == delegateSig.ParameterCount - 1)
            {
                // Ignored instance

            }

        }
        // Instance Method
        else
        {
            // We must have an instance as the first parameter
            if (builder.ParameterCount < 1)
                return new AdapterException(method, delegateSig, $"Missing instance parameter");
            // Load it
            var instanceType = GetInstanceType(method);
            result = ParameterLoader.TryLoadParameter(emitter, builder.Parameters[0], instanceType);
            if (!result) return result;

            // Check if we only have params left
            if (builder.ParameterCount == 2 &&
                builder.Parameters[1].IsParams())
            {
                ParameterLoader.LoadParams(emitter, builder.Parameters[1], method.GetParameters());
            }

        }

        CALL:

    }
    
    public static Result TryAdapt<TDelegate>(MethodBase method, [NotNullWhen(true)] out TDelegate? @delegate)
        where TDelegate : Delegate
    {
        var result = TryAdapt(method, DelegateSignature.For<TDelegate>(), out var del);
        if (result)
        {
            @delegate = del as TDelegate;
        }
        else
        {
            @delegate = default;
        }
        return @delegate is not null;
    }

    public static TDelegate Adapt<TDelegate>(MethodBase method)
        where TDelegate : Delegate
    {
        TryAdapt<TDelegate>(method, out var del).ThrowIfFailed();
        return del!;
    }
}





public static class ParameterLoader
{
    /* Arg Casting
     * A subset of Parameter Loading
     * T -> T       Any value can load as itself
     * U:T -> T     Any value that implements a destination type can be cast
     * T -> object  Any value can be boxed
     * object -> T  Any value can be unboxed
     *
     * ref T -> ref T     Same ref type is always fine
     * ref T -> T         We can load the value from the reference
     * T -> ref T         We have to create a local variable to reference, but we can do this
     *
     * ref T -> object          We can box it
     * object -> ref V          Object can be unboxed to a reference value
     * object -> ref C          Trickier, but can be done
     *
     * ref object        Unsupported (all object references)
     * void|T|unmanaged *       Unsupported (all pointers)
     */
    public static Result TryCastArg(IFluentILEmitter emitter,
        ParameterSignature source, ParameterSignature dest)
    {
        // object ->
        if (source.Type == typeof(object) && source.Access == ParameterAccess.Default)
        {
            if (dest.Access == ParameterAccess.Default)
            {
                if (dest.Type.IsValueType)
                {
                    emitter.Unbox_Any(dest.Type);
                }
                else
                {
                    emitter.Castclass(dest.Type);
                }
            }
            else
            {
                if (dest.Type.IsValueType)
                {
                    emitter.Unbox(dest.Type);
                }
                else
                {
                    emitter.Castclass(dest.Type)
                        .DeclareLocal(dest.Type, out var localDest)
                        .Stloc(localDest)
                        .Ldloca(localDest);
                }
            }
        }
        // ?T ->
        else
        {
            // T ->
            if (source.Access == ParameterAccess.Default)
            {
                // T -> ?T
                if (dest.Type == source.Type)
                {
                    // T -> T
                    if (dest.Access == ParameterAccess.Default)
                    {
                        // Do nothing
                    }
                    // T -> ref T
                    else
                    {
                        emitter.DeclareLocal(source.Type, out var localSource)
                            .Stloc(localSource)
                            .Ldloca(localSource);
                    }
                }
                // U:T -> T
                else if (source.Type.Implements(dest.Type))
                {
                    if (dest.Access == ParameterAccess.Default)
                    {
                        if (!source.Type.IsValueType)
                        {
                            emitter.Castclass(dest.Type);
                        }
                        else
                        {
                            return new NotImplementedException(Dump($"{source} -> {dest}"));
                        }
                    }
                    else
                    {
                        return new NotImplementedException(Dump($"{source} -> {dest}"));
                    }
                }
                // T -> ?object
                else if (dest.Type == typeof(object))
                {
                    if (dest.Access == ParameterAccess.Default)
                    {
                        emitter.Box(source.Type);
                    }
                    else
                    {
                        return new NotImplementedException(Dump($"{source} -> {dest}"));
                    }
                }
                else
                {
                    return new NotImplementedException(Dump($"{source} -> {dest}"));
                }
            }
            // ref T ->
            else
            {
                // ref T -> ?T
                if (dest.Type == source.Type)
                {
                    // ref T -> T
                    if (dest.Access == ParameterAccess.Default)
                    {
                        emitter.Ldind(dest.Type);
                    }
                    // ref T -> ref T
                    else
                    {
                        // do nothing
                    }
                }
                // ref T -> ?object
                else if (dest.Type == typeof(object))
                {
                    // ref T -> object
                    if (dest.Access == ParameterAccess.Default)
                    {
                        emitter.Ldind(source.Type)
                            .Box(source.Type);
                    }
                    else
                    {
                        return new NotImplementedException(Dump($"{source} -> {dest}"));
                    }
                }
                else
                {
                    return new NotImplementedException(Dump($"{source} -> {dest}"));
                }
            }
        }

        // Okay!
        return true;
    }

    public static IFluentILEmitter EmitCast(this IFluentILEmitter emitter,
        ParameterSignature source, ParameterSignature dest)
    {
        TryCastArg(emitter, source, dest).ThrowIfFailed();
        return emitter;
    }


    /* Parameter Loading
 * -----------------
 * A very important part both emission and method adapting
 * To keep things sane, we only support a basic set of parameter loading:
 *
 * T -> T       Any value can load as itself
 * U:T -> T     Any value that implements a destination type can be cast
 * T -> object  Any value can be boxed
 * object -> T  Any value can be unboxed
 *
 * in|out|ref T -> in|out|ref T     Same ref type is always fine
 * ref T -> in|out T                This is also fine, just odd
 * in T -> ref T                    Will just ignore T changes
 * out T -> ref T                   Dangerous if T's initial value is used
 *
 * in|ref T -> object               We can box it
 * out T -> object                  Dangerous
 * object -> in|ref|out V           Object can be unboxed to a reference value
 * object -> in|ref|out C           Trickier, but can be done
 *
 * in|out|ref object        Unsupported (all object references)
 * void|T|unmanaged *       Unsupported (all pointers)
 */
    public static Result TryLoadParameter(
        IFluentILEmitter emitter,
        ParameterInfo sourceParameter,
        ParameterSignature dest)
    {
        var sourceAccess = sourceParameter.GetAccess(out var sourceType);

        // Same parameter access?
        if (sourceAccess == dest.Access)
        {
            // Default
            if (sourceAccess == ParameterAccess.Default)
            {
                // T -> T       Any value can load as itself
                if (dest.Type == sourceType)
                {
                    emitter.Ldarg(sourceParameter);
                }
                // T -> object  Any value can be boxed
                else if (dest.Type == typeof(object))
                {
                    emitter.Box(sourceType);
                }
                // U:T -> T     Any value that implements a destination type can be cast
                else if (sourceType.Implements(dest.Type))
                {
                    if (!sourceType.IsValueType)
                    {
                        emitter.Ldarg(sourceParameter)
                            .Castclass(dest.Type);
                    }
                    else
                    {
                        return new NotImplementedException(Dump($"{sourceAccess} {sourceType} -> {dest}"));
                    }
                }
                // object -> T  Any value can be unboxed 
                else if (sourceType == typeof(object))
                {
                    if (dest.Type.IsValueType)
                    {
                        emitter.Ldarg(sourceParameter)
                            .Unbox_Any(dest.Type);
                    }
                    else
                    {
                        emitter.Ldarg(sourceParameter)
                            .Castclass(dest.Type);
                    }
                }
                else
                {
                    return new NotImplementedException(Dump($"{sourceAccess} {sourceType} -> {dest}"));
                }
            }
            // in|out|ref
            else
            {
                // in|out|ref T -> in|out|ref T     Same ref type is always fine
                if (sourceType == dest.Type)
                {
                    emitter.Ldarg(sourceParameter);
                }
                else
                {
                    return new NotImplementedException(Dump($"{sourceAccess} {sourceType} -> {dest}"));
                }
            }
        }
        // Different parameter access
        else
        {
            // ? T -> ? T
            if (sourceType == dest.Type)
            {
                if (sourceAccess == ParameterAccess.In)
                {
                    //in T -> ref T                    Will just ignore T changes
                    if (dest.Access == ParameterAccess.Ref)
                    {
                        emitter.Ldarg(sourceParameter);
                    }
                    else
                    {
                        return new NotImplementedException(Dump($"{sourceAccess} {sourceType} -> {dest}"));
                    }
                }
                else if (sourceAccess == ParameterAccess.Out)
                {
                    // out T -> ref T                   Dangerous if T's initial value is used
                    if (dest.Access == ParameterAccess.Ref)
                    {
                        Debugger.Break();
                        emitter.Ldarg(sourceParameter);
                    }
                    else
                    {
                        return new NotImplementedException(Dump($"{sourceAccess} {sourceType} -> {dest}"));
                    }
                }
                else if (sourceAccess == ParameterAccess.Ref)
                {
                    // ref T -> in|out T                This is also fine, just odd
                    if (dest.Access is ParameterAccess.In or ParameterAccess.Out)
                    {
                        emitter.Ldarg(sourceParameter);
                    }
                    else
                    {
                        return new NotImplementedException(Dump($"{sourceAccess} {sourceType} -> {dest}"));
                    }
                }
            }
            // ? -> ?
            else
            {
                if (dest.Type == typeof(object) && dest.Access == ParameterAccess.Default)
                {
                    // in|ref T -> object               We can box it
                    if (sourceAccess is ParameterAccess.In or ParameterAccess.Ref)
                    {
                        emitter.Ldarg(sourceParameter)
                            .Ldind(sourceType)
                            .Box(sourceType);
                    }
                    // out T -> object                  Dangerous
                    else
                    {
                        Debug.Assert(sourceAccess == ParameterAccess.Out);
                        return new NotImplementedException(Dump($"{sourceAccess} {sourceType} -> {dest}"));
                    }
                }
                else if (sourceType == typeof(object) && sourceAccess == ParameterAccess.Default)
                {
                    // object -> in|ref|out V           Object can be unboxed to a reference value
                    if (dest.Type.IsValueType)
                    {
                        emitter.Ldarg(sourceParameter)
                            .Unbox(dest.Type);
                    }
                    // object -> in|ref|out C           Trickier, but can be done
                    else
                    {
                        emitter.Ldarg(sourceParameter)
                            .Castclass(dest.Type)
                            .DeclareLocal(dest.Type, out var localDest)
                            .Stloc(localDest)
                            .Ldloca(localDest);
                    }
                }
                else
                {
                    return new NotImplementedException(Dump($"{sourceAccess} {sourceType} -> {dest}"));
                }
            }
        }

        // We emitted!
        return true;
    }


    public static void LoadParams(IFluentILEmitter emitter,
        ParameterInfo paramsParameter,
        ReadOnlySpan<ParameterInfo> destParameters)
    {
        int len = destParameters.Length;

        // if (params.Length != {len}) {
        emitter.Ldarg(paramsParameter)
            .Ldlen()
            .Ldc_I4(len)
            .Beq(out var lenEqual);
        // throw }
        emitter.Ldstr("Not enough values passed in params")
            .Ldstr(paramsParameter.Name)
            .Newobj(Reflect.FindConstructor<ArgumentException>(typeof(string), typeof(string)))
            .Throw()
            .MarkLabel(lenEqual);
        // extract each parameter in turn
        for (var i = 0; i < len; i++)
        {
            emitter.Ldarg(paramsParameter)
                .Ldc_I4(i)
                .Ldelem<object>();
            var destAccess = destParameters[i].GetAccess(out var destType);
            if (destAccess == ParameterAccess.Default)
            {
                emitter.Unbox_Any(destType);
            }
            else
            {
                emitter.Unbox(destType);
            }
        }
        // Everything will be loaded!
    }
}