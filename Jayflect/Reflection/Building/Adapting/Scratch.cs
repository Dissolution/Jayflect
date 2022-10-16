using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Jay.Dumping;
using Jay.Reflection.Building.Emission;
using Jay.Reflection.Exceptions;
using Jay.Validation;
using Type = System.Type;

namespace Jay.Reflection.Building.Adapting;

public class AdaptException : RuntimeException
{
    public AdaptException(string? message = null, 
                          Exception? innerException = null) : base(message, innerException)
    {

    }
}

public static class TypeAdapter
{
    internal static AdaptException CreateAdaptException(
        Type? inputType,
        Type? outputType,
        string? additionalInfo = null)
    {
        var message = Dumper.StartDump($"Could not adapt input type '{inputType}' to output type '{outputType}'");
        if (additionalInfo.IsNonWhiteSpace())
        {
            message.AppendFormatted(": ");
            message.AppendLiteral(additionalInfo);
        }
        return new AdaptException(message.ToStringAndDispose());
    }

    public static Result TryCast<TEmitter>(TEmitter emitter,
                                           Type? inputType,
                                           Type? outputType)
        where TEmitter : class, IFluentEmitter<TEmitter>
    {
        // We have nothing incoming?
        if (inputType is null || inputType == typeof(void))
        {
            // We can only expect nothing
            return (outputType is null || outputType == typeof(void));
        }

        // We have nothing outgoing?
        if (outputType is null || outputType == typeof(void))
        {
            // Just pop the value off the stack
            emitter.Pop();
            return true;
        }

        bool inputIsRef = inputType.IsByRef;
        if (inputIsRef) inputType = inputType.GetElementType();
        bool outputIsRef = outputType.IsByRef;
        if (outputIsRef) outputType = outputType.GetElementType();

        // inputType == outputType
        if (inputType == outputType)
        {
            // Do nothing
            return true;
        }

        throw new NotImplementedException();
    }
}

public static class ParameterAdapter
{
    internal static AdaptException CreateAdaptException(
        ParameterInfo inputParameter, 
        Type? outputType,
        string? additionalInfo = null)
    {
        var message = Dumper.StartDump($"Could not adapt input parameter '{inputParameter}' to output type '{outputType}'");
        if (additionalInfo.IsNonWhiteSpace())
        {
            message.AppendFormatted(": ");
            message.AppendLiteral(additionalInfo);
        }

        return new AdaptException(message.ToStringAndDispose());
    }

    public static Result TryLoadAs<TEmitter>(TEmitter emitter,
                                             ParameterInfo parameter,
                                             Type? type)
        where TEmitter : class, IFluentEmitter<TEmitter>
    {
        if (type is null || type == typeof(void))
        {
            // Nothing required; nothing to load.
            return true;
        }

        // Check for currently unsupported types
        if (type == typeof(void*) || parameter.ParameterType == typeof(void*))
        {
            return new NotImplementedException();
        }

        if (parameter.ParameterType == typeof(void))
        {
            // We're expecting a value that we're not given
            return CreateAdaptException(parameter, type, "Expecting a value");
        }

        var inputAccess = parameter.GetAccess(out var inputType);
        var inputIsByRef = inputAccess != ParameterAccess.Default;
        var outputIsByRef = type.IsByRef(out var outputType);
        
        // 99% case is Input : Output
        if (inputType == outputType)
        {
            if (inputIsByRef == outputIsByRef)
            {
                // Load arg exactly as presented
                emitter.Ldarg(parameter.Position);
                return true;
            }
            // Non-Ref Input
            if (!inputIsByRef)
            {
                // If out is a ref, we can load as pointer
                emitter.Ldarga(parameter.Position);
                return true;
            }
            Debug.Assert(!outputIsByRef);
            // Load the parameter, then load a pointer to it
            emitter.Ldarg(parameter.Position)
                   .Ldind(outputType);
            return true;
        }

        // (object) : ?
        if (inputType == typeof(object))
        {
            if (inputIsByRef)
                return new NotSupportedException();
            // struct?
            if (outputType.IsValueType)
            {
                // non-ref struct?
                if (!outputIsByRef)
                {
                    // We can load the object parameter, then unbox to the value
                    emitter.Ldarg(parameter.Position)
                           .Unbox_Any(outputType);
                    return true;
                }
                // We can load the object parameter, then unbox to a value pointer
                emitter.Ldarg(parameter.Position)
                       .Unbox(outputType);
                return true;
            }
            // class / interface
            // non-ref?
            if (!outputIsByRef)
            {
                // Load object, cast to class
                emitter.Ldarg(parameter.Position)
                       .Castclass(outputType);
                return true;
            }
            return new NotSupportedException();
            // Can we unbox anything, not just value types?
        }

        // ? : (object)
        if (outputType == typeof(object))
        {

        }

        throw new NotImplementedException();
    }
}


public static class MethodAdapter
{
    public static Result TryAdapt(MethodBase method, Type delegateType, [NotNullWhen(true)] out Delegate? adapter)
    {
        // For fast return
        adapter = null;

        if (method is null)
            return new ArgumentNullException(nameof(method));
        if (delegateType is null)
            return new ArgumentNullException(nameof(delegateType));
        if (!delegateType.Implements<Delegate>())
            return new ArgumentException("Invalid Delegate Type", nameof(delegateType));

        var runtimeMethod = RuntimeBuilder.CreateRuntimeMethod(
            delegateType,
            Dumper.Dump($"call_{method}"));
        
        var emitter = runtimeMethod.Emitter;

        // Attempt to load an instance
        throw new NotImplementedException();
    }

    private static Result<int> TryLoadInstanceParam(MethodBase method,
                                                    RuntimeMethod adapter)
    {
        // Static method?
        if (method.IsStatic)
        {
            // Okay to have no parameters
            if (adapter.ParameterCount == 0)
                return 0;
            // Check actual instance (non-ref)
            var instanceType = adapter.Parameters[0].NonRefType();
            // Known throwaway type?
            if (instanceType == typeof(Types.Static) ||
                instanceType == typeof(Types.Void) ||
                instanceType == typeof(void))
            {
                return 1;
            }
            // Possible throwaway type?
            if (instanceType == typeof(object) || // just object?, expecting null
                instanceType == typeof(Type))     // typeof(Static)
            {
                // Have to have 1:1 other parameters or end in an 'params object[]'
                var methodParams = method.GetParameters();
                if ((methodParams.Length == (adapter.ParameterCount + 1)) ||
                    (adapter.Parameters[^1].IsParams()))
                {
                    return 1;
                }
            }

            // Assume no throwaway
            return 0;
        }

        // Instance needed!
        //var instanceType = method.OwnerType().ThrowIfNull();

        if (adapter.ParameterCount == 0)
            return new RuntimeException(
                Dumper.Dump($"{method.OwnerType()} instance parameter is required to invoke {method}"));

        throw new NotImplementedException();
    }
}