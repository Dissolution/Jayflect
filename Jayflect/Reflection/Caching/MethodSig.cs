using System.Linq.Expressions;
using System.Reflection;
using Jay.Dumping;
using Jay.Text;
using Jay.Validation;
using Jayflect;

namespace Jay.Reflection.Caching;

public partial class MethodSig
{
    static MethodSig()
    {
    }

    public static MethodSig Of(MethodBase method)
    {
        return new MethodSig(method);
    }

    public static MethodSig Of<TDelegate>() where TDelegate : Delegate
    {
        var invokeMethod = typeof(TDelegate).GetInvokeMethod()
            .ThrowIfNull($"{typeof(Delegate)} did not have an Invoke method!");
        return new MethodSig(invokeMethod);
    }

    public static MethodSig Of(Type delegateType)
    {
        if (!delegateType.Implements<Delegate>())
            throw new ArgumentException("You must specify a Type that implements Delegate", nameof(delegateType));
        var invokeMethod = delegateType.GetInvokeMethod()
            .ThrowIfNull($"{delegateType} did not have an Invoke method!");
        return new MethodSig(invokeMethod);
    }

    public static MethodSig Of<TDelegate>(out MethodInfo invokeMethod)
        where TDelegate : Delegate
    {
        invokeMethod = typeof(TDelegate).GetInvokeMethod()
            .ThrowIfNull($"{typeof(Delegate)} did not have an Invoke method!");
        return new MethodSig(invokeMethod);
    }

    public static MethodSig Of(Type delegateType, out MethodInfo invokeMethod)
    {
        if (!delegateType.Implements<Delegate>())
            throw new ArgumentException("You must specify a Type that implements Delegate", nameof(delegateType));
        invokeMethod = delegateType.GetInvokeMethod()
            .ThrowIfNull($"{delegateType} did not have an Invoke method!");
        return new MethodSig(invokeMethod);
    }

    public static bool operator ==(MethodSig left, MethodSig right) => left.Equals(right);
    public static bool operator !=(MethodSig left, MethodSig right) => !left.Equals(right);
}

public partial class MethodSig : IEquatable<MethodSig>, IEquatable<MethodBase>
{
    protected readonly MethodBase _method;

    internal MethodBase MethodBase => _method;

    public Attribute[] Attributes { get; }
    public Type ReturnType { get; }
    public ParameterInfo[] Parameters { get; }
    public Type[] ParameterTypes { get; }
    public int ParameterCount => Parameters.Length;

    public string Name => _method.Name;
    public Visibility Visibility { get; }

    public bool IsAction => ReturnType == typeof(void);
    public bool IsFunc => ReturnType != typeof(void);

    internal Type[] ParameterAndReturnTypes
    {
        get
        {
            var types = new Type[ParameterCount + 1];
            int i = ParameterCount;
            types[i] = ReturnType;
            for (; i >= 0; i--)
            {
                types[i] = ParameterTypes[i];
            }
            return types;
        }
    }
    internal Type DelegateType
    {
        get
        {
            if (IsAction)
                return Expression.GetActionType(ParameterTypes);
            return Expression.GetFuncType(ParameterAndReturnTypes);
        }
    }

    public MethodSig(MethodBase method)
    {
        _method = method;
        this.Attributes = Attribute.GetCustomAttributes(_method);
        this.ReturnType = _method.ReturnType();
        this.Parameters = _method.GetParameters();
        this.ParameterTypes = Array.ConvertAll(Parameters, p => p.ParameterType);
        this.Visibility = _method.Visibility();
    }

    public bool IsMethodInfo() => _method is MethodInfo;
    public bool IsMethodInfo([NotNullWhen(true)] out MethodInfo? methodInfo) => _method.Is(out methodInfo);

    public bool IsConstructorInfo() => _method is ConstructorInfo;
    public bool IsConstructorInfo([NotNullWhen(true)] out ConstructorInfo? constructorInfo) => _method.Is(out constructorInfo);

    public bool Equals<TDelegate>()
        where TDelegate : Delegate
        => Equals(Of<TDelegate>());

    public bool Equals(Type delegateType)
        => Equals(Of(delegateType));

    public bool Equals(MethodSig? methodSig)
    {
        return methodSig is not null &&
               _method.HasSameMetadataDefinitionAs(methodSig._method);
    }

    public bool Equals(MethodBase? method)
    {
        return method is not null &&
               _method.HasSameMetadataDefinitionAs(method);
    }

    public override bool Equals(object? obj)
    {
        if (obj is MethodBase method) return _method.HasSameMetadataDefinitionAs(method);
        if (obj is MethodSig methodSig) return _method.HasSameMetadataDefinitionAs(methodSig._method);
        return false;
    }

    public override int GetHashCode()
    {
        return _method.GetHashCode();
    }

    public override string ToString()
    {
        using var text = TextBuilder.Borrow();
        text.AppendDump(ReturnType).Append(' ')
            .AppendDump(_method.ReflectedType ?? _method.DeclaringType).Append('.')
            .Append(_method.Name).Append('(')
            .AppendDelimit(",", Parameters, (tb, param) =>
            {
                var access = param.GetAccess(out var paramType);
                tb.Append(access.ToString().ToLower())
                    .Append(' ')
                    .AppendDump(paramType)
                    .Append(' ')
                    .Append(param.Name);
                if (param.HasDefaultValue)
                {
                    tb.Append(" = ")
                        .AppendDump(param.DefaultValue);
                }
            }).Append(')');
        return text.ToString();
    }
}