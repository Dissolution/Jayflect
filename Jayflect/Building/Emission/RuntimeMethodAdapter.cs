using Jay;
using Jayflect.Extensions;

namespace Jayflect.Building.Emission;

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
    public RuntimeDelegateBuilder RuntimeDelegateBuilder { get; }

    public IFluentILEmitter Emitter => RuntimeDelegateBuilder.Emitter;

    public RuntimeMethodAdapter(MethodBase method, DelegateSignature delegateSig)
    {
        this.Method = method;
        this.MethodSig = DelegateSignature.For(method);
        this.DelegateSig = delegateSig;
        this.RuntimeDelegateBuilder = RuntimeBuilder.CreateRuntimeDelegateBuilder(delegateSig);
    }

    private Result TryLoadInstance()
    {
        // We must have an instance as the first parameter
        var instanceParameter = this.DelegateSig.Parameters.FirstOrDefault();
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
        return Emitter.TryEmitLoadParameter(instanceParameter, instanceType);
    }

    private Result TryLoadParams(int delegateParamOffset)
    {
        if (DelegateSig.ParameterCount - delegateParamOffset != 1)
            return new AdapterException(Method, DelegateSig, $"No params available as the only/last parameter");
        Emitter.EmitLoadParams(DelegateSig.Parameters.Last(), MethodSig.Parameters);
        return true;
    }

    private Result TryLoadArgs(int delegateParamOffset)
    {
        var lastInstructionNode = Emitter.Instructions.Last;
        var delParams = this.DelegateSig.Parameters;
        var methParams = this.MethodSig.Parameters;
        if (delParams.Length - delegateParamOffset != methParams.Length)
            return new AdapterException(Method, DelegateSig, $"Incorrect number of parameters available");
        Result result;
        for (var m = 0; m < methParams.Length; m++)
        {
            result = Emitter.TryEmitLoadParameter(
                delParams[m + delegateParamOffset],
                methParams[m]);
            if (!result)
            {
                Emitter.Instructions.RemoveAfter(lastInstructionNode);
                return result;
            }
        }
        return true;
    }

    private Result TryLoadInstanceArgs()
    {
        Result result;
        int offset;
        // Static Method
        if (Method.IsStatic)
        {
            // Check for throwaway
            result = TryLoadInstance();
            offset = result ? 1 : 0;
            for (; offset <= 1; offset++)
            {
                result = TryLoadParams(offset);
                if (result) return true;
                result = TryLoadArgs(offset);
                if (result) return true;
            }
            // Nothing worked
            return new AdapterException(Method, DelegateSig, $"Could not understand static method adapt");
        }
        
        // Instance Method
        result = TryLoadInstance();
        if (!result) return result;
        result = TryLoadParams(1);
        if (result) return true;
        result = TryLoadArgs(1);
        if (result) return true;
        return result;
    }

    public static Result TryAdapt(MethodBase method, DelegateSignature delegateSig, [NotNullWhen(true)] out Delegate? adapterDelegate)
    {
        // faster return
        adapterDelegate = default;

        var adapter = new RuntimeMethodAdapter(method, delegateSig);
        var result = adapter.TryLoadInstanceArgs();
        if (!result) return result;
        adapter.Emitter.Call(method)
            .EmitCast(method.ReturnType(), delegateSig.ReturnType)
            .Ret();
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