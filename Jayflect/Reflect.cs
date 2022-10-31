using System.Diagnostics;
using Jay.Dumping.Interpolated;
using Jayflect.Exceptions;
using Jayflect.Extensions;

namespace Jayflect;

public static partial class Reflect
{
    public static class Flags
    {
        public const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic |
                                        BindingFlags.Static | BindingFlags.Instance |
                                        BindingFlags.IgnoreCase;

        public const BindingFlags Public = BindingFlags.Public |
                                           BindingFlags.Static | BindingFlags.Instance |
                                           BindingFlags.IgnoreCase;

        public const BindingFlags NonPublic = BindingFlags.NonPublic |
                                              BindingFlags.Static | BindingFlags.Instance |
                                              BindingFlags.IgnoreCase;

        public const BindingFlags Static = BindingFlags.Public | BindingFlags.NonPublic |
                                           BindingFlags.Static |
                                           BindingFlags.IgnoreCase;

        public const BindingFlags Instance = BindingFlags.Public | BindingFlags.NonPublic |
                                             BindingFlags.Instance |
                                             BindingFlags.IgnoreCase;

        public const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;

        public const BindingFlags NonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.IgnoreCase;

        public const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        public const BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase;
    }

    private static HashSet<Type>? _allTypes;

    public static IReadOnlySet<Type> AllExportedTypes
    {
        get
        {
            if (_allTypes is null)
            {
                var allTypes = new HashSet<Type>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        foreach (var type in assembly.GetExportedTypes())
                        {
                            allTypes.Add(type);
                        }
                    }
                    catch
                    {
                        // Ignore this assembly
                    }
                }
                _allTypes = allTypes;
            }
            return _allTypes;
        }
    }
}

public static partial class Reflect
{
    public static TMember FindMember<TMember>(Expression<Func<TMember?>> memberExpression)
        where TMember : MemberInfo
    {
        Exception? exception = null;
        TMember? member = null;
        try
        {
            member = memberExpression.Compile().Invoke();
            if (member is not null)
                return member;
        }
        catch (Exception ex)
        {
            // Ignore, build error below
            exception = ex;
        }
        // Build our error
        var expressions = memberExpression.SelfAndDescendants()
            .SkipWhile(expr => expr is not MethodCallExpression)
            .ToList();

        // Method we were calling (likely Type.GetWhatever)
        var methodExpr = (expressions[0] as MethodCallExpression)!;
        var method = methodExpr.Method;
        var methodArgs = expressions.Skip(2).Select(expr =>
        {
            if (expr is ConstantExpression constantExpression)
                return constantExpression.Value;
            throw new NotImplementedException();

        }).ToList();
        var methodParams = method.GetParameters();
        if (methodArgs.Count != methodParams.Length)
            Debugger.Break();

        DumpStringHandler stringHandler = new();
        stringHandler.Write("Could not find ");
        stringHandler.Dump(typeof(TMember));
        stringHandler.Write(":");
        stringHandler.Write(Environment.NewLine);
        stringHandler.Dump(method.OwnerType());
        stringHandler.Write(".");
        stringHandler.Write(method.Name);
        stringHandler.Write("(");
        stringHandler.DumpDelimited(", ", methodParams);
        stringHandler.Write(")");
        stringHandler.Write(Environment.NewLine);

        throw new JayflectException(ref stringHandler, exception);
    }

    public static TMember FindMember<TMember>(Expression<Func<object?>> memberExpression)
        where TMember : MemberInfo
    {
        var member = memberExpression.ExtractMember<TMember>();
        if (member is not null)
            return member;
        var desc = memberExpression.SelfAndDescendants().ToList();
        var values = desc
            .SelectMany(expr => expr.ExtractValues<TMember>())
            .ToList();
        throw new NotImplementedException();
    }

    public static ConstructorInfo FindConstructor<TInstance>(params Type[] parameterTypes)
    {
        return FindConstructor(typeof(TInstance), parameterTypes);
    }

    public static ConstructorInfo FindConstructor(Type instanceType, params object[] arguments)
    {
        return FindConstructor(instanceType, Array.ConvertAll(arguments, arg => arg.GetType()));
    }

    public static ConstructorInfo FindConstructor(Type instanceType, params Type[] argTypes)
    {
        ConstructorInfo? constructor = instanceType
            .GetConstructors(Flags.Instance)
            .FirstOrDefault(ctor => MemoryExtensions.SequenceEqual<Type>(ctor.GetParameterTypes(), argTypes));
        if (constructor is not null)
            return constructor;
        throw new JayflectException($"Could not find constructor that matched {instanceType}({argTypes})");
    }

    public static FieldInfo FindField(Type ownerType, string name, BindingFlags flags, Type? fieldType)
    {
        var field = ownerType.GetField(name, flags);
        if (field is null)
            throw new JayflectException($"Could not find {flags:I} field {ownerType}.{name}");
        if (fieldType is not null && field.FieldType != fieldType)
            throw new JayflectException($"Field {field} does not contain a {fieldType}");
        return field;
    }

    public static PropertyInfo FindProperty(Type ownerType, string name, BindingFlags flags, Type? propertyType)
    {
        var property = ownerType.GetProperty(name, flags, null, propertyType, Type.EmptyTypes, null);
        if (property is null)
            throw new JayflectException($"Could not find {flags:I} {propertyType} property {ownerType}.{name}");
        Debug.Assert(propertyType is null || propertyType == property.PropertyType);
        return property;
    }
}

public static partial class Reflect<T>
{
    public static TMember FindMember<TMember>(Expression<Func<T, object?>> memberExpression)
        where TMember : MemberInfo
    {
        throw new NotImplementedException();
    }
}