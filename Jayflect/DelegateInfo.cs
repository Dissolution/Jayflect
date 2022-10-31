using Jay;
using Jay.Utilities;
using Jayflect.Extensions;

namespace Jayflect;

public sealed class DelegateInfo : IEquatable<DelegateInfo>
{
    public static bool operator ==(DelegateInfo left, DelegateInfo right) => left.Equals(right);
    public static bool operator !=(DelegateInfo left, DelegateInfo right) => !left.Equals(right);

    public static DelegateInfo For<TDelegate>(TDelegate? _ = null)
        where TDelegate : Delegate
        => new DelegateInfo(typeof(TDelegate));

    public static DelegateInfo For(MethodBase method)
    {
        ArgumentNullException.ThrowIfNull(method);
        return new DelegateInfo(method);
    }

    public static DelegateInfo For(Type delegateType)
    {
        Validate.IsDelegateType(delegateType);
        return new DelegateInfo(delegateType);
    }

    private readonly MethodBase _invokeMethod;
    private Type? _delegateType = null;

    public Type ReturnType { get; }

    public Type[] ParameterTypes { get; }

    public int ParameterCount => ParameterTypes.Length;

    public string Name => _invokeMethod.Name;

    public ParameterInfo[] Parameters { get; }

    private DelegateInfo(Type delegateType)
        : this(delegateType.GetInvokeMethod()!)
    {
        _delegateType = delegateType;
    }
    
    private DelegateInfo(MethodBase method)
    {
        _delegateType = null;
        _invokeMethod = method;
        this.ReturnType = method.ReturnType();
        this.Parameters = method.GetParameters();
        this.ParameterTypes = method.GetParameterTypes();
    }

    private Type GetGenericDelegateType()
    {
        // Action?
        if (this.ReturnType == typeof(void))
        {
            var actionType = this.ParameterCount switch
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
            return actionType.MakeGenericType(this.ParameterTypes);
        }
        // Func
        var funcType = this.ParameterCount switch
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
        var funcTypeArgs = new Type[this.ParameterCount + 1];
        Fast.Copy<Type>(this.ParameterTypes, funcTypeArgs);
        funcTypeArgs[^1] = this.ReturnType;
        return funcType.MakeGenericType(funcTypeArgs);
    }
    
    public Type GetDelegateType()
    {
        return _delegateType ??= GetGenericDelegateType();
    }

    public bool Equals(DelegateInfo? delegateInfo)
    {
        return delegateInfo is not null &&
               delegateInfo.ReturnType == this.ReturnType &&
               Fast.Equal<Type>(delegateInfo.ParameterTypes, this.ParameterTypes);
    }
    public override bool Equals(object? obj)
    {
        return obj is DelegateInfo delegateInfo && Equals(delegateInfo);
    }
    public override int GetHashCode()
    {
        var hasher = new HashCode();
        hasher.Add<Type>(ReturnType);
        hasher.Add<Type>(ParameterTypes);
        return hasher.ToHashCode();
    }
    public override string ToString()
    {
        return Dump($"{ReturnType} {Name}({ParameterTypes})");
    }
}