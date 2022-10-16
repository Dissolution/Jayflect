using System.Reflection;
using Jayflect;

namespace Jay.Reflection;

public static class TypeExtensions
{
    public static MethodInfo? GetInvokeMethod(this Type? type)
    {
        return type?.GetMethod("Invoke", Reflect.PublicFlags);
    }

    public static bool IsStatic(this Type? type)
    {
        return type is null || (type.IsAbstract && type.IsSealed);
    }

    public static Visibility Visibility(this Type? type)
    {
        var visibility = Jayflect.Visibility.None;
        if (type is null) return visibility;
        if (IsPublic(type))
            visibility |= Jayflect.Visibility.Public;
        if (IsInternal(type))
            visibility |= Jayflect.Visibility.Internal;
        if (IsProtected(type))
            visibility |= Jayflect.Visibility.Protected;
        if (IsPrivate(type))
            visibility |= Jayflect.Visibility.Private;
        return visibility;
    }

    private static bool IsPublic(Type type)
    {
        return type.IsVisible
               && type.IsPublic
               && !type.IsNotPublic
               && !type.IsNested
               && !type.IsNestedPublic
               && !type.IsNestedFamily
               && !type.IsNestedPrivate
               && !type.IsNestedAssembly
               && !type.IsNestedFamORAssem
               && !type.IsNestedFamANDAssem;
    }

    private static bool IsInternal(Type type)
    {
        return !type.IsVisible
               && !type.IsPublic
               && type.IsNotPublic
               && !type.IsNested
               && !type.IsNestedPublic
               && !type.IsNestedFamily
               && !type.IsNestedPrivate
               && !type.IsNestedAssembly
               && !type.IsNestedFamORAssem
               && !type.IsNestedFamANDAssem;
    }

// only nested types can be declared "protected"
    private static bool IsProtected(Type type)
    {
        return !type.IsVisible
               && !type.IsPublic
               && !type.IsNotPublic
               && type.IsNested
               && !type.IsNestedPublic
               && type.IsNestedFamily
               && !type.IsNestedPrivate
               && !type.IsNestedAssembly
               && !type.IsNestedFamORAssem
               && !type.IsNestedFamANDAssem;
    }

// only nested types can be declared "private"
    private static bool IsPrivate(Type type)
    {
        return !type.IsVisible
               && !type.IsPublic
               && !type.IsNotPublic
               && type.IsNested
               && !type.IsNestedPublic
               && !type.IsNestedFamily
               && type.IsNestedPrivate
               && !type.IsNestedAssembly
               && !type.IsNestedFamORAssem
               && !type.IsNestedFamANDAssem;
    }
}