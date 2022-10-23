using Jayflect.Extensions;

namespace Jayflect;

public sealed class ParameterSignature : IEquatable<ParameterSignature>
{
    public static implicit operator ParameterSignature(ParameterInfo parameterInfo) => FromParameter(parameterInfo);
    public static implicit operator ParameterSignature(Type type) => FromType(type);
    
    public static bool operator ==(ParameterSignature left, ParameterSignature right) => left.Equals(right);
    public static bool operator !=(ParameterSignature left, ParameterSignature right) => !left.Equals(right);

    public static ParameterSignature FromParameter(ParameterInfo parameterInfo)
    {
        return new ParameterSignature(parameterInfo.GetAccess(out var parameterType), parameterType);
    }
    public static ParameterSignature FromAccessType(ParameterAccess access, Type type)
    {
        if (type.IsByRef)
            throw new ArgumentException($"Type must not be ref, handle that with {nameof(access)}", nameof(type));
        return new ParameterSignature(access, type);
    }
    public static ParameterSignature FromType(Type type)
    {
        return new ParameterSignature(type.GetAccess(out var baseType), baseType);
    }

    public ParameterAccess Access { get; }
    public Type Type { get; }
    
    private ParameterSignature(ParameterAccess access, Type type)
    {
        this.Access = access;
        this.Type = type;
    }

    public bool Equals(ParameterSignature? parameterSignature)
    {
        return parameterSignature is not null &&
               parameterSignature.Access == this.Access &&
               parameterSignature.Type == this.Type;
    }

    public override bool Equals(object? obj)
    {
        return obj is ParameterSignature sig && Equals(sig);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Access, Type);
    }
    public override string ToString()
    {
        return Dump($"{Access} {Type}");
    }
}