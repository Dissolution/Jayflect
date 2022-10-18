using System.Diagnostics;
using Jay.Dumping;
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

    private static HashSet<Type>? _allTypes = null;
    
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
            return null;
        }).ToList();
        var methodParams = method.GetParameters();
        if (methodArgs.Count != methodParams.Length)
            Debugger.Break();

        DumpStringHandler stringHandler = new();
        stringHandler.AppendLiteral("Could not find ");
        stringHandler.AppendFormatted(typeof(TMember));
        stringHandler.AppendLiteral(":");
        stringHandler.AppendLiteral(Environment.NewLine);
        stringHandler.AppendFormatted(method.OwnerType());
        stringHandler.AppendLiteral(".");
        stringHandler.AppendLiteral(method.Name);
        stringHandler.AppendLiteral("(");
        for (var i = 0; i < methodParams.Length; i++)
        {
            if (i > 0) stringHandler.AppendLiteral(",");
            var methodParam = methodParams[i];
            stringHandler.AppendFormatted(methodParam.ParameterType);
            stringHandler.AppendLiteral(" ");
            stringHandler.AppendLiteral(methodParam.Name ?? "?");
            stringHandler.AppendLiteral(" = ");
            stringHandler.AppendFormatted(methodArgs[i]);
        }
        stringHandler.AppendLiteral(")");
        stringHandler.AppendLiteral(Environment.NewLine);

        throw new ReflectionException(ref stringHandler, exception);
    }

    public static TMember FindMember<TMember>(Expression<Func<object?>> memberExpression)
        where TMember : MemberInfo
    {
        throw new NotImplementedException();
    }

    public static ConstructorInfo FindConstructor<T>(params Type[] ctorParameterTypes)
    {
        ConstructorInfo? constructor = typeof(T)
            .GetConstructors(Flags.Instance)
            .FirstOrDefault(ctor => MemoryExtensions.SequenceEqual<Type>(ctor.GetParameterTypes(), ctorParameterTypes));
        if (constructor is not null)
            return constructor;
        throw new ReflectionException($"Could not find constructor that matched {typeof(T)}({ctorParameterTypes})");
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