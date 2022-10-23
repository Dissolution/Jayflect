using System.Diagnostics;
using Jay;
using Jay.Extensions;
using Jayflect.Extensions;


namespace Jayflect.Building.Emission;

public static class EmitterExtensions
{
    
    public static Result TryEmitCast(this IFluentILEmitter emitter,
        ParameterSignature source, ParameterSignature dest)
    {
        // ?T -> ?object     (Needs to be checked early, as everything implements object)
        if (dest.Type == typeof(object))
        {
            // ?T -> object
            if (dest.Access != ParameterAccess.Default)
                return new NotImplementedException(Dump($"Cannot cast a {source} to a {dest}"));
            
            // T -> object
            if (source.Access == ParameterAccess.Default)
            {
                // skip object -> object
                if (source.Type != typeof(object))
                    emitter.Box(source.Type);
            }
            // ref T -> object
            else
            {
                emitter.Ldind(source.Type);
                // skip (ref object) -> object -> object
                if (source.Type != typeof(object))
                    emitter.Box(source.Type);
            }
        }
        // ?object -> ?T
        else if (source.Type == typeof(object))
        {
            // object -> ?T
            if (source.Access != ParameterAccess.Default)
                return new NotImplementedException(Dump($"Cannot cast {source} to a {dest}"));
            
            // object -> T
            if (dest.Access == ParameterAccess.Default)
            {
                // object -> struct
                if (dest.Type.IsValueType)
                {
                    emitter.Unbox_Any(dest.Type);
                }
                // object -> class
                else
                {
                    emitter.Castclass(dest.Type);
                }
            }
            // object -> ref T
            else
            {
                // object -> ref struct
                if (dest.Type.IsValueType)
                {
                    emitter.Unbox(dest.Type);
                }
                // object -> ref class
                else
                {
                    emitter.Castclass(dest.Type)
                        .DeclareLocal(dest.Type, out var localDest)
                        .Stloc(localDest)
                        .Ldloca(localDest);
                }
            }
        }
        // ?T -> ?T
        else if (source.Type == dest.Type)
        {
            // T -> ?T
            if (source.Access == ParameterAccess.Default)
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
            // ref T -> ?T
            else
            {
                // ref T -> T
                if (dest.Access == ParameterAccess.Default)
                {
                    emitter.Ldind(source.Type);
                }
                // ref T -> ref T
                else
                {
                    // Do nothing
                }
            }
        }
        // ?T:U -> ?U
        else if (source.Type.Implements(dest.Type))
        {
            // T:U -> U
            if (source.Access != ParameterAccess.Default || dest.Access != ParameterAccess.Default)
                return new NotImplementedException(Dump($"Cannot cast {source} to a {dest}"));

            // struct T:U -> U
            if (source.Type.IsValueType)
            {
                // We have to be converting to an interface
                if (!dest.Type.IsInterface) throw new InvalidOperationException();
                Debugger.Break();
                emitter.Castclass(dest.Type);
            }
            // class T:U -> U
            else
            {
                emitter.Castclass(dest.Type);
            }
        }
        else
        {
            return new InvalidOperationException(Dump($"Cannot cast {source} to a {dest}"));
        }
        
        // Done
        return true;
    }

    public static IFluentILEmitter EmitCast(this IFluentILEmitter emitter,
        ParameterSignature source, ParameterSignature dest)
    {
        TryEmitCast(emitter, source, dest).ThrowIfFailed();
        return emitter;
    }

    public static Result TryEmitLoadParameter(this IFluentILEmitter emitter,
        ParameterInfo sourceParameter, ParameterSignature destParameterSig)
    {
        var sourceSig = ParameterSignature.FromParameter(sourceParameter);

        // ?T -> ?object     (Needs to be checked early, as everything implements object)
        if (destParameterSig.Type == typeof(object))
        {
            // ?T -> object
            if (destParameterSig.Access != ParameterAccess.Default)
                return new NotImplementedException(Dump($"Cannot load {sourceParameter} to a {destParameterSig}"));
            
            // T -> object
            if (sourceSig.Access == ParameterAccess.Default)
            {
                emitter.Ldarg(sourceParameter);
                if (sourceSig.Type != typeof(object))
                    emitter.Box(sourceSig.Type);
            }
            // ref T -> object
            else
            {
                emitter.Ldarg(sourceParameter)
                    .Ldind(sourceSig.Type);
                if (sourceSig.Type != typeof(object))
                    emitter.Box(sourceSig.Type);
            }
        }
        // ?object -> ?T
        else if (sourceSig.Type == typeof(object))
        {
            // object -> ?T
            if (sourceSig.Access != ParameterAccess.Default)
                return new NotImplementedException(Dump($"Cannot load {sourceParameter} to a {destParameterSig}"));
            
            // object -> T
            if (destParameterSig.Access == ParameterAccess.Default)
            {
                // object -> struct
                if (destParameterSig.Type.IsValueType)
                {
                    emitter.Ldarg(sourceParameter)
                        .Unbox_Any(destParameterSig.Type);
                }
                // object -> class
                else
                {
                    emitter.Ldarg(sourceParameter)
                        .Castclass(destParameterSig.Type);
                }
            }
            // object -> ref T
            else
            {
                // object -> ref struct
                if (destParameterSig.Type.IsValueType)
                {
                    emitter.Ldarg(sourceParameter)
                        .Unbox(destParameterSig.Type);
                }
                // object -> ref class
                else
                {
                    emitter.Ldarg(sourceParameter)
                        .Castclass(destParameterSig.Type)
                        .DeclareLocal(destParameterSig.Type, out var localDest)
                        .Stloc(localDest)
                        .Ldloca(localDest);
                }
            }
        }
        // ?T -> ?T
        else if (sourceSig.Type == destParameterSig.Type)
        {
            // T -> ?T
            if (sourceSig.Access == ParameterAccess.Default)
            {
                // T -> T
                if (destParameterSig.Access == ParameterAccess.Default)
                {
                    emitter.Ldarg(sourceParameter);
                }
                // T -> ref T
                else
                {
                    emitter.Ldarga(sourceParameter);
                }
            }
            // ref T -> ?T
            else
            {
                // ref T -> T
                if (destParameterSig.Access == ParameterAccess.Default)
                {
                    emitter.Ldarg(sourceParameter).Ldind(sourceSig.Type);
                }
                // ref T -> ref T
                else
                {
                    emitter.Ldarg(sourceParameter);
                }
            }
        }
        // ?T:U -> ?U
        else if (sourceSig.Type.Implements(destParameterSig.Type))
        {
            // T:U -> U
            if (sourceSig.Access != ParameterAccess.Default || destParameterSig.Access != ParameterAccess.Default)
                return new NotImplementedException(Dump($"Cannot load {sourceParameter} to a {destParameterSig}"));

            // struct T:U -> U
            if (sourceSig.Type.IsValueType)
            {
                // We have to be converting to an interface
                if (!destParameterSig.Type.IsInterface) throw new InvalidOperationException();
                Debugger.Break();
                emitter.Ldarg(sourceParameter)
                    .Castclass(destParameterSig.Type);
            }
            // class T:U -> U
            else
            {
                emitter.Ldarg(sourceParameter)
                    .Castclass(destParameterSig.Type);
            }
        }
        else
        {
            return new InvalidOperationException(Dump($"Cannot load {sourceParameter} to a {destParameterSig}"));
        }
        
        // Done
        return true;
    }

    public static IFluentILEmitter EmitLoadParameter(this IFluentILEmitter emitter,
        ParameterInfo parameter, ParameterSignature destParameterSig)
    {
        TryEmitLoadParameter(emitter, parameter, destParameterSig).ThrowIfFailed();
        return emitter;
    }

    public static IFluentILEmitter EmitLoadInstance(this IFluentILEmitter emitter,
        ParameterInfo? instanceParameter, MemberInfo member)
    {
        if (member.IsStatic())
        {
            // Do nothing
            return emitter;
        }

        if (instanceParameter is null)
            throw new AdapterException($"Required instance parameter is missing");

        var ownerType = member.OwnerType();
        if (ownerType.IsValueType)
        {
            // We need it as a ref
            ownerType = ownerType.MakeByRefType();
        }
        
        TryEmitLoadParameter(emitter, instanceParameter, ownerType).ThrowIfFailed();
        return emitter;
    }

    public static IFluentILEmitter EmitParamsLengthCheck(this IFluentILEmitter emitter,
        ParameterInfo paramsParameter, int length)
    {
        return emitter
            .Ldarg(paramsParameter)
            .Ldlen()
            .Ldc_I4(length)
            .Beq(out var lenEqual)
            .Ldstr($"{length} parameters are required in the params array")
            .Ldstr(paramsParameter.Name)
            .Newobj(Reflect.FindConstructor<ArgumentException>(typeof(string), typeof(string)))
            .Throw()
            .MarkLabel(lenEqual);
    }
    
    public static IFluentILEmitter EmitLoadParams(this IFluentILEmitter emitter,
        ParameterInfo paramsParameter,
        ReadOnlySpan<ParameterInfo> destParameters,
        bool emitLengthCheck = true)
    {
        int len = destParameters.Length;

        if (emitLengthCheck)
        {
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
        }

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
        return emitter;
    }
}