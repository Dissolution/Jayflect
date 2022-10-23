using Jay.Extensions;
using Jayflect.Extensions;

namespace Jayflect;

public sealed class DelegateSignature : IEquatable<DelegateSignature>
{
    public static bool operator ==(DelegateSignature left, DelegateSignature right) => left.Equals(right);
    public static bool operator !=(DelegateSignature left, DelegateSignature right) => !left.Equals(right);

    public static DelegateSignature For(MethodBase method)
    {
        return new DelegateSignature(method, null);
    }
    public static DelegateSignature For<TDelegate>(TDelegate? @delegate = default)
        where TDelegate : Delegate
    {
        return new DelegateSignature(typeof(TDelegate).GetInvokeMethod()!, typeof(TDelegate));
    }

    public static DelegateSignature For(Type delegateType)
    {
        Validate.IsDelegateType(delegateType);
        return new DelegateSignature(delegateType.GetInvokeMethod()!, delegateType);
    }

    private static Type GetGenericDelegateType(DelegateSignature sig)
    {
        var parameterTypes = sig.ParameterTypes;
        var parameterCount = sig.ParameterCount;
        var returnType = sig.ReturnType;
        
        
        // Action?
        if (returnType == typeof(void))
        {
            var actionType = parameterCount switch
                             {
                                 00 => typeof(Action),
                                 01 => typeof(Action<>),
                                 02 => typeof(Action<,>),
                                 03 => typeof(Action<,,>),
                                 04 => typeof(Action<,,,>),
                                 05 => typeof(Action<,,,,>),
                                 06 => typeof(Action<,,,,,>),
                                 07 => typeof(Action<,,,,,,>),
                                 08 => typeof(Action<,,,,,,,>),
                                 09 => typeof(Action<,,,,,,,,>),
                                 10 => typeof(Action<,,,,,,,,,>),
                                 11 => typeof(Action<,,,,,,,,,,>),
                                 12 => typeof(Action<,,,,,,,,,,,>),
                                 13 => typeof(Action<,,,,,,,,,,,,>),
                                 14 => typeof(Action<,,,,,,,,,,,,,>),
                                 15 => typeof(Action<,,,,,,,,,,,,,,>),
                                 16 => typeof(Action<,,,,,,,,,,,,,,,>),
                                 _ => throw new NotImplementedException(),
                             };
            return actionType.MakeGenericType(parameterTypes);
        }
        // Func
        var funcType = parameterCount switch
                       {
                           00 => typeof(Func<>),
                           01 => typeof(Func<,>),
                           02 => typeof(Func<,,>),
                           03 => typeof(Func<,,,>),
                           04 => typeof(Func<,,,,>),
                           05 => typeof(Func<,,,,,>),
                           06 => typeof(Func<,,,,,,>),
                           07 => typeof(Func<,,,,,,,>),
                           08 => typeof(Func<,,,,,,,,>),
                           09 => typeof(Func<,,,,,,,,,>),
                           10 => typeof(Func<,,,,,,,,,,>),
                           11 => typeof(Func<,,,,,,,,,,,>),
                           12 => typeof(Func<,,,,,,,,,,,,>),
                           13 => typeof(Func<,,,,,,,,,,,,,>),
                           14 => typeof(Func<,,,,,,,,,,,,,,>),
                           15 => typeof(Func<,,,,,,,,,,,,,,,>),
                           16 => typeof(Func<,,,,,,,,,,,,,,,,>),
                           _ => throw new NotImplementedException(),
                       };
        var funcTypeArgs = new Type[parameterTypes.Length + 1];
        parameterTypes.CopyTo<Type>((Span<Type>)funcTypeArgs);
        funcTypeArgs[^1] = returnType;
        return funcType.MakeGenericType(funcTypeArgs);
    }
    
    private readonly MethodBase _method;
    private Type? _delegateType;

    internal MethodBase Method => _method;
    internal ParameterInfo[] Parameters => _method.GetParameters();
    
    public Type ReturnType { get; }
    public Type[] ParameterTypes { get; }
    public int ParameterCount => ParameterTypes.Length;

    private DelegateSignature(MethodBase method, Type? delegateType)
    {
        _method = method;
        this.ReturnType = method.ReturnType();
        this.ParameterTypes = method.GetParameterTypes();
        _delegateType = delegateType;
    }

    public Type GetDelegateType()
    {
        return (_delegateType ??= GetGenericDelegateType(this));
    }
    
    public bool Matches(MethodBase? method)
    {
        if (method is null) return false;
        if (method.ReturnType() != this.ReturnType) return false;
        var methodParams = method.GetParameters();
        if (methodParams.Length != ParameterCount) return false;
        for (var i = 0; i < ParameterCount; i++)
        {
            if (methodParams[i].ParameterType != ParameterTypes[i]) return false;
        }
        return true;
    }

    public bool Matches<TDelegate>(TDelegate? del = default) where TDelegate : Delegate
    {
        return Matches(typeof(TDelegate).GetInvokeMethod()!);
    }

    public bool Matches(Type delegateType)
    {
        return delegateType.Implements<Delegate>() &&
               Matches(delegateType.GetInvokeMethod());
    }
    
    public bool Equals(DelegateSignature? delSig)
    {
        return delSig is not null &&
               delSig.ReturnType == this.ReturnType &&
               MemoryExtensions.SequenceEqual<Type>(delSig.ParameterTypes, this.ParameterTypes);
    }

    public override bool Equals(object? obj)
    {
        return obj is DelegateSignature sig && Equals(sig);
    }
    public override int GetHashCode()
    {
        var hasher = new HashCode();
        hasher.Add(ReturnType);
        foreach (var parameterType in ParameterTypes)
        {
            hasher.Add(parameterType);
        }
        return hasher.ToHashCode();
    }
    public override string ToString()
    {
        return Dump($"{ReturnType}({ParameterTypes})");
    }
}