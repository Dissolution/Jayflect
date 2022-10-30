using System.Diagnostics;
using Jay;
using Jay.Dumping;
using Jayflect.Building.Emission;
using Jayflect.Exceptions;
using Jayflect.Extensions;

namespace Jayflect.Building.Adaption;

/// <summary>
/// Indicates that this Parameter is an instance parameter for Method Adapting
/// (and thus will be ignored for static member interactions)
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class InstanceAttribute : Attribute
{
    
}

/// <summary>
/// Creates a <see cref="Delegate"/> to call a <see cref="MethodBase"/>
/// </summary>
public class RuntimeMethodAdapter
{
    public MethodBase Method { get; }

    public MethodSignature MethodSig { get; }

    public DelegateSignature DelegateSig { get; }

    public RuntimeDelegateBuilder RuntimeDelegateBuilder { get; }

    public IFluentILEmitter Emitter => RuntimeDelegateBuilder.Emitter;

    public RuntimeMethodAdapter(MethodBase method, DelegateSignature delegateSig)
    {
        this.Method = method;
        this.MethodSig = DelegateSignature.For(method);
        this.DelegateSig = delegateSig;
        this.RuntimeDelegateBuilder = RuntimeBuilder.CreateRuntimeDelegateBuilder(delegateSig);
    }

    private JayflectException GetAdaptEx(ref DumpStringHandler message,
        Exception? innerException = null)
    {
        return new JayflectException(ref message, innerException)
        {
            Data =
            {
                { "Method", Method },
                { "DelegateSig", DelegateSig },
            },
        };
    }
    
    
    private Result HasInstanceParam(out int offset)
    {
        // The instance will be the first parameter
        var instanceParameter = this.DelegateSig.Parameters.FirstOrDefault();

            // We do not have to have an instance
            if (instanceParameter is null)
            {
                offset = 0;
                return true;
            }
            
            // If they use ref NoInstance as a helper, we can ignore
            if (instanceParameter.NonRefType() == typeof(NoInstance))
            {
                offset = 1;
                return true;
            }
            
            // Did they mark an instance attribute?
            if (instanceParameter.GetCustomAttribute<InstanceAttribute>() is not null)
            {
                offset = 1;
                return true;
            }

            // We're unsure
            offset = 0;
            return false;
    }

    private Result TryLoadParams(int delegateParamOffset, int methodParamOffset = 0)
    {
        if (DelegateSig.ParameterCount - delegateParamOffset != 1)
            return GetAdaptEx($"No params available as the only/last parameter");
        var paramsParameter = DelegateSig.Parameters.Last();
        if (!paramsParameter.IsParams())
            return GetAdaptEx($"Not params");
        Emitter.EmitLoadParams(paramsParameter, MethodSig.Parameters.AsSpan(methodParamOffset));
        return true;
    }

    private Result TryLoadArgs(int delegateParamOffset, int methodParamOffset = 0)
    {
        var lastInstructionNode = Emitter.Instructions.Last;
        var delParams = this.DelegateSig.Parameters;
        var methParams = this.MethodSig.Parameters;
        if (delParams.Length - delegateParamOffset != methParams.Length - methodParamOffset)
            return GetAdaptEx($"Incorrect number of parameters available");
        Result result;
        int m = methodParamOffset;
        int d = delegateParamOffset;
        while (m < methParams.Length && d < delParams.Length)
        {
            Arg delArg = delParams[d];
            Arg methArg = methParams[m];
            result = delArg.TryLoadAs(Emitter, methArg);
            if (!result)
            {
                Emitter.Instructions.RemoveAfter(lastInstructionNode);
                return result;
            }
            m++;
            d++;
        }
        return true;
    }

    private Result TryLoadInstanceArgs()
    {
        Result result;
        
        // Static Method
        if (Method.IsStatic)
        {
            int mOffset = 0;
            // Check for throwaway
            result = HasInstanceParam(out int dOffset);
            if (result)
            {
                // If the first required arg is the instance, maybe we should load it
                if (MethodSig.ParameterCount >= 1 && 
                    CanAdaptType(MethodSig.Parameters[0], Method.OwnerType()) &&
                    DelegateSig.ParameterCount > 0 &&
                    CanAdaptType(DelegateSig.Parameters[0], MethodSig.Parameters[0]))
                {
                    // We know we had an instance param
                    Arg instance = DelegateSig.Parameters[0];
                    result = instance.TryLoadAs(Emitter, MethodSig.Parameters[0]);
                    if (result)
                    {
                        mOffset = 1;
                        dOffset = 1;
                    }
                }
                
                // Check for params
                result = TryLoadParams(dOffset, mOffset);
                if (result) return true;
                // Check for Args
                result = TryLoadArgs(dOffset, mOffset);
                if (result) return true;
            }
            else
            {
                // First, assume we have an instance, then assume we don't if that doesn't work
                dOffset = 1;
                while (dOffset >= 0)
                {
                    // Check for params
                    result = TryLoadParams(dOffset);
                    if (result) return true;
                    
                    // Check for Args
                    result = TryLoadArgs(dOffset);
                    if (result) return true;
                    
                    dOffset--;
                }
            }
           
            // Nothing worked
            return GetAdaptEx($"Could not understand static method adapt");
        }

        // Instance Method
        if (DelegateSig.ParameterCount == 0)
            return GetAdaptEx($"There must be an instance argument for instance Methods");
        if (!Method.TryGetInstanceType(out var instanceType))
            return GetAdaptEx($"Could not find Instance Type for Method");
        Arg instanceArg = DelegateSig.Parameters[0];
        result = instanceArg.TryLoadAs(this.Emitter, instanceType);
        if (!result) return result;
        // Check for Params
        result = TryLoadParams(1);
        if (result) return true;
        // Otherwise, load args 1 by 1
        result = TryLoadArgs(1);
        
        // Cleanup result
        if (result) return true;
        return result;
    }

    public static Result TryAdapt(MethodBase method, DelegateSignature delegateSig, [NotNullWhen(true)] out Delegate? adapterDelegate)
    {
        // faster return
        adapterDelegate = default;

        var adapter = new RuntimeMethodAdapter(method, delegateSig);
        // Load (possible) instance + all args
        var result = adapter.TryLoadInstanceArgs();
        if (!result) 
            return result;
        // Call the original method
        adapter.Emitter.Call(method);
        Arg methodReturn = method.ReturnType();
        Arg delReturn = delegateSig.ReturnType;
        result = methodReturn.TryLoadAs(adapter.Emitter, delReturn);
        if (!result) return result;
        adapter.Emitter.Ret();
        var il = adapter.Emitter.ToString();
        adapterDelegate = adapter.RuntimeDelegateBuilder.CreateDelegate();
        return true;
    }

    public static Result TryAdapt<TDelegate>(MethodBase method, [NotNullWhen(true)] out TDelegate? @delegate)
        where TDelegate : Delegate
    {
        var result = TryAdapt(method, DelegateSignature.For<TDelegate>(), out var del);
        if (result)
        {
            @delegate = del as TDelegate;
            if (@delegate is not null) return true;
            return new InvalidOperationException("Del is not del");
        }
        @delegate = default;
        return result;
    }

    public static TDelegate Adapt<TDelegate>(MethodBase method)
        where TDelegate : Delegate
    {
        var result = TryAdapt<TDelegate>(method, out var del);
        if (!result)
            result.ThrowIfFailed();
        return del!;
    }

    public static bool CanAdaptType(Arg input, Arg output)
    {
        return input.CanLoadAs(output, out _);
    }
    
    public static bool CanAdaptTypes(Type[] inputTypes, Type[] outputTypes, out int exactness)
    {
        int len = inputTypes.Length;
        // Mismatched count?
        if (outputTypes.Length != len)
        {
            // Only works if inputParameters is just a params
            //return len == 1 && inputTypes[0] == typeof(object[]);
            exactness = int.MaxValue;
            return false;
        }

        exactness = 0;
        for (var i = 0; i < len; i++)
        {
            Arg input = inputTypes[i];
            Arg output = outputTypes[i];
            if (!input.CanLoadAs(output, out var e)) 
                return false;
            exactness += e;
        }
        
        return true;
    }
}