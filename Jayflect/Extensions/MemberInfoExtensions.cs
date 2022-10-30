using System.Diagnostics;

namespace Jayflect.Extensions;

public static class MemberInfoExtensions
{
    public static Type OwnerType(this MemberInfo memberInfo)
    {
        return memberInfo.ReflectedType ?? memberInfo.DeclaringType ?? memberInfo.Module.GetType();
    }
    
    public static bool TryGetInstanceType(this MemberInfo member, [NotNullWhen(true)] out Type? instanceType)
    {
        if (member.IsStatic())
        {
            // static methods have no instance
            instanceType = null;
            return false;
        }
        
        // Check first for reflected base type, then declared
        instanceType = member.ReflectedType ?? member.DeclaringType;
        if (instanceType is null)
        {
            // No valid instance type?
            throw new InvalidOperationException();
        }
        
        if (instanceType.IsStatic() || instanceType.IsByRef || instanceType.IsByRefLike)
        {
            // Possible?
            Debugger.Break();
        }

        if (instanceType.IsValueType)
        {
            // We want to load a ref to any structs (otherwise we only operate on a copy)
            instanceType = instanceType.MakeByRefType();
        }

        // We found a valid instance type that is the type we want to load
        return true;
    }

    public static Visibility Visibility(this MemberInfo? memberInfo)
    {
        if (memberInfo is FieldInfo fieldInfo)
            return fieldInfo.Visibility();
        if (memberInfo is PropertyInfo propertyInfo)
            return propertyInfo.Visibility();
        if (memberInfo is EventInfo eventInfo)
            return eventInfo.Visibility();
        if (memberInfo is MethodBase methodBase)
            return methodBase.Visibility();
        if (memberInfo is Type type)
            return type.Visibility();
        return Jayflect.Visibility.None;
    }

    public static bool IsStatic(this MemberInfo? memberInfo)
    {
        if (memberInfo is FieldInfo fieldInfo)
            return fieldInfo.IsStatic;
        if (memberInfo is PropertyInfo propertyInfo)
            return propertyInfo.IsStatic();
        if (memberInfo is EventInfo eventInfo)
            return eventInfo.IsStatic();
        if (memberInfo is MethodBase methodBase)
            return methodBase.IsStatic;
        if (memberInfo is Type type)
            return type.IsStatic();
        return false;
    }

    public static bool HasAttribute<TAttribute>(this MemberInfo member)
        where TAttribute : Attribute
    {
        return Attribute.GetCustomAttribute(member, typeof(TAttribute), true) is not null;
    }
}