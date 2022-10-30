namespace Jayflect.Extensions;

public static class TypeExtensions
{
    public static MethodInfo? GetInvokeMethod(this Type? delegateType)
    {
        return delegateType?.GetMethod("Invoke", Reflect.Flags.Public);
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

    public static bool HasDefaultConstructor(this Type type, [NotNullWhen(true)] out ConstructorInfo? defaultCtor)
    {
        defaultCtor = type.GetConstructor(Reflect.Flags.Instance, Type.EmptyTypes);
        return defaultCtor is not null;
    }
    
    //public static Type OwnerType(this Type type) => type.ReflectedType ?? type.DeclaringType ?? type.Module.GetType();

    private static readonly MethodInfo _isReferenceMethod;
    private static readonly MethodInfo _sizeOfMethod;
    
    static TypeExtensions()
    {
        _isReferenceMethod = Reflect.FindMember(() => typeof(RuntimeHelpers)
            .GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences),
                Reflect.Flags.PublicStatic,
                Type.EmptyTypes));
        _sizeOfMethod = Reflect.FindMember(() => typeof(Unsafe)
            .GetMethod(nameof(Unsafe.SizeOf),
                Reflect.Flags.PublicStatic,
                Type.EmptyTypes));
    }

    public static bool IsReferenceOrContainsReferences(this Type type)
    {
        return (bool)_isReferenceMethod.MakeGenericMethod(type).Invoke(null, null)!;
    }

    public static bool IsUnmanaged(this Type type) => !IsReferenceOrContainsReferences(type);

    public static object? GetDefault(this Type type)
    {
        if (type.IsClass || type.IsInterface)
            return null;
        return Activator.CreateInstance(type);
    }

    public static int? GetSize(this Type type)
    {
        if (IsReferenceOrContainsReferences(type)) return null;
        return (int?)_sizeOfMethod.MakeGenericMethod(type).Invoke(null, null);
    }

    public static object GetUninitialized(this Type type)
    {
        return RuntimeHelpers.GetUninitializedObject(type);
    }
    
    public static bool IsObjectArray(this Type type) => type == typeof(object[]);
    
    public static ParameterAccess GetAccess(this Type type, out Type baseType)
    {
        if (type.IsByRef)
        {
            baseType = type.GetElementType()!;
            return ParameterAccess.Ref;
        }
        else
        {
            baseType = type;
            return ParameterAccess.Default;
        }
    }
}