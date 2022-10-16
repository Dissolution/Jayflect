using System.Reflection;
using Jay.Enums;
using Jay.Reflection.Building.Emission;
using Jay.Reflection.Caching;
using Jay.Reflection.Exceptions;

namespace Jay.Reflection.Building.Adapting;

public class DelegateMethodAdapter
{
    public MethodSig MethodSig { get; }
    public MethodBase Method { get; }
    public MethodSig MethodSignature { get; }
    public Safety Safety { get; }

    public DelegateMethodAdapter(MethodSig methodSig,
                                 MethodBase method,
                                 Safety safety = Safety.Safe)
    {
        this.MethodSig = methodSig;
        this.Method = method ?? throw new ArgumentNullException(nameof(method));
        this.MethodSignature = MethodSig.Of(Method);
        this.Safety = safety;
    }

    protected Result TryLoadArgs<TEmitter>(TEmitter emitter, int offset)
        where TEmitter : class, IFluentEmitter<TEmitter>
    {
        // None?
        if (MethodSignature.ParameterCount == 0)
        {
            return true;
        }

        // Check 1:1
        if (MethodSignature.ParameterCount == (MethodSig.ParameterCount - offset))
        {
            return new NotImplementedException();
        }
        // Check for only params
        else if (MethodSig.ParameterCount == offset + 1 &&
                 MethodSig.ParameterTypes[offset].IsObjectArray() &&
                 !MethodSignature.ParameterTypes[0].IsObjectArray())
        {
            return Result.TryInvoke(() => emitter.LoadParams(MethodSig.Parameters[offset], MethodSignature.Parameters));
        }
        // Check for optional method params
        else if (MethodSignature.Parameters.Reverse().Any(p => p.HasDefaultValue))
        {
            return new NotImplementedException();
        }
        // TODO: Other checks?
        else
        {
            return new NotImplementedException();
        }
    }

    protected Result TryCastReturn<TEmitter>(TEmitter emitter)
        where TEmitter : class, IFluentEmitter<TEmitter>
    {
        // Does delegate have one?
        if (MethodSig.ReturnType != typeof(void))
        {
            // Does method?
            if (MethodSignature.ReturnType != typeof(void))
            {
                return Result.TryInvoke(() => emitter.Cast(MethodSignature.ReturnType, MethodSig.ReturnType));
            }
            else
            {
                if (Safety.HasFlag<Safety>(Safety.AllowReturnDefault))
                {
                    emitter.LoadDefault(MethodSig.ReturnType);
                    return true;
                }
                return new AdapterException($"Delegate requires a returned {MethodSig.ReturnType} value, Method does not return one");
            }
        }
        else
        {
            // Does method?
            if (MethodSignature.ReturnType != typeof(void))
            {
                if (Safety.HasFlag<Safety>(Safety.AllowReturnDiscard))
                {
                    emitter.Pop();
                    return true;
                }
                return new AdapterException($"Delegate is an action, Method returns a {MethodSignature.ReturnType}");
            }
            else
            {
                // void -> void
                return true;
            }
        }
    }

    public Result TryAdapt<TEmitter>(TEmitter emitter)
        where TEmitter : class, IFluentEmitter<TEmitter>
    {
        Result result;
        ParameterInfo? possibleInstanceParam;
        if (MethodSig.ParameterCount > 0)
        {
            possibleInstanceParam = MethodSig.Parameters[0];
        }
        else
        {
            possibleInstanceParam = null;
        }
        int offset = default;
        result = Result.TryInvoke(() => emitter.LoadInstanceFor(Method, possibleInstanceParam, out offset));
        if (!result) return result;
        result = TryLoadArgs(emitter, offset);
        if (!result) return result;
        result = TryCastReturn(emitter);
        if (!result) return result; 
        emitter.Ret();
        return true;
    }
}

public class DelegateMethodAdapter<TDelegate> : DelegateMethodAdapter
    where TDelegate : Delegate
{
    public DelegateMethodAdapter(MethodBase method,
                                 Safety safety = Safety.Safe)
        : base(MethodSig.Of<TDelegate>(), method, safety)
    {

    }
}