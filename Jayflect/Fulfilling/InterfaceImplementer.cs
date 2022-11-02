/*
using System.Reflection;
using System.Reflection.Emit;
using Jay.Comparision;
using Jay.Reflection.Building;

namespace Jay.Reflection.Implementation;

public sealed class MemberSignatureEqualityComparer : EqualityComparer<MemberInfo>
{
    public static MemberSignatureEqualityComparer Instance { get; } = new();
    
    /// <inheritdoc />
    public override bool Equals(MemberInfo? x, MemberInfo? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        if (x.MemberType != y.MemberType) return false;
        if (x.Name != y.Name) return false;
        if (x is FieldInfo xField && y is FieldInfo yField)
            return xField.FieldType == yField.FieldType;
        if (x is PropertyInfo xProperty && y is PropertyInfo yProperty)
            return xProperty.PropertyType == yProperty.PropertyType;
        if (x is EventInfo xEvent && y is EventInfo yEvent)
            return xEvent.EventHandlerType == yEvent.EventHandlerType;
        if (x is MethodBase xMethod && y is MethodBase yMethod)
        {
            if (xMethod.ReturnType() != yMethod.ReturnType()) return false;
            return xMethod.GetParameterTypes().SequenceEqual(yMethod.GetParameterTypes());
        }
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override int GetHashCode(MemberInfo? member)
    {
        if (member is null) return 0;
        var hasher = new Hasher();
        hasher.Add(member.MemberType);
        hasher.Add(member.Name);
        if (member is FieldInfo field)
        {
            hasher.Add(field.FieldType);
        }
        else if (member is PropertyInfo property)
        {
            hasher.Add(property.PropertyType);
            hasher.Add(property.GetIndexParameterTypes());
        }
        else if (member is EventInfo @event)
        {
            hasher.Add(@event.EventHandlerType);
        }
        else if (member is MethodBase method)
        {
            hasher.Add(method.ReturnType());
            hasher.Add(method.GetParameterTypes());
        }
        else
        {
            throw new NotImplementedException();
        }
        return hasher.CreateHashCode();
    }
}

public class InterfaceImplementer
{
    private readonly HashSet<Type> _interfaceTypes;
    private readonly TypeBuilder _typeBuilder;
    private readonly Dictionary<MemberInfo, >

    public InterfaceImplementer(Type interfaceType)
    {
        if (!interfaceType.IsInterface)
            throw new ArgumentException("Must be a valid Interface Type", nameof(interfaceType));
        _interfaceTypes = new HashSet<Type>(interfaceType.GetInterfaces())
        {
            // Don't forget to include the interface itself
            interfaceType,
        };
        _typeBuilder = RuntimeBuilder.DefineType(TypeAttributes.Public | TypeAttributes.Class,
            MemberNaming.CreateInterfaceImplementationName(interfaceType));
    }

    public Type CreateImplementationType()
    {
        
    }
}
*/
