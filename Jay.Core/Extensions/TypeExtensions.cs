namespace Jay.Extensions;

public static class TypeExtensions
{
    public static bool Implements(this Type type, Type subType)
    {
        if (type == subType) return true;
        if (type.IsAssignableTo(subType)) return true;
        if (subType.IsGenericType != type.IsGenericType) return false;
        if (type.IsGenericType)
        {
            if (subType.IsGenericTypeDefinition)
            {
                return type.GetGenericTypeDefinition() == subType;
            }
            throw new NotImplementedException();
        }
        return false;
    }

    public static bool Implements<TSub>(this Type type) => Implements(type, typeof(TSub));
}