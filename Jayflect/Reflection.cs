using System.Diagnostics;
using Jay.Extensions;
using Jayflect.Extensions;
using MemberFlags = System.Reflection.BindingFlags;

namespace Jayflect;

public static partial class Reflect
{
    public static class BindingFlags
    {
        public const MemberFlags All = MemberFlags.Public | MemberFlags.NonPublic |
                                       MemberFlags.Static | MemberFlags.Instance |
                                       MemberFlags.IgnoreCase;

        public const MemberFlags Public = MemberFlags.Public |
                                          MemberFlags.Static | MemberFlags.Instance |
                                          MemberFlags.IgnoreCase;

        public const MemberFlags NonPublic = MemberFlags.NonPublic |
                                             MemberFlags.Static | MemberFlags.Instance |
                                             MemberFlags.IgnoreCase;

        public const MemberFlags Static = MemberFlags.Public | MemberFlags.NonPublic |
                                          MemberFlags.Static |
                                          MemberFlags.IgnoreCase;

        public const MemberFlags Instance = MemberFlags.Public | MemberFlags.NonPublic |
                                            MemberFlags.Instance |
                                            MemberFlags.IgnoreCase;
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
        var expressions = memberExpression.SelfAndDescendants().ToList();
        
        // Find the method that failed
        var methods = expressions.SelectMany(e =>
        {
            var values = e.ExtractValues<MethodBase>().ToList();
            return values;
        }).ToList();
        
        Debugger.Break();


        throw new InvalidOperationException();
    }
}

public class Reflection<T>
{
    public TMember Get<TMember>(Expression<Action<T>> memberExpression)
        where TMember : MemberInfo
    {
        var member = memberExpression.ExtractMember<TMember>();
        if (member is not null)
            return member;
        throw new ReflectionException($"Could not find {typeof(T)} {typeof(TMember)} from {memberExpression}");
    }

    public TMember Get<TMember>(Expression<Func<T, object?>> memberExpression)
        where TMember : MemberInfo
    {
        var member = memberExpression.ExtractMember<TMember>();
        if (member is not null)
            return member;
        throw new ReflectionException($"Could not find {typeof(T)} {typeof(TMember)} from {memberExpression}");
    }

    public ConstructorInfo GetConstructor(params Type[] parameterTypes)
    {
        var ctor = typeof(T).GetConstructors(Reflect.BindingFlags.Instance)
            .Where(ctor => MemoryExtensions.SequenceEqual<Type>(ctor.GetParameterTypes(), parameterTypes))
            .OneOrDefault();
        if (ctor is not null)
            return ctor;
        throw new ReflectionException($"Could not find {typeof(T)} constructor with parameter types: {parameterTypes}");
    }
}