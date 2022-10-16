using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Jay.Debugging;
using Jay.Dumping;
using Jay.Reflection.Exceptions;
using Jay.Text;
using Jayflect.Extensions;

namespace Jay.Reflection.Search;

public static class MemberSearch
{
    public static class Static
    {
        public static TMember Find<TMember>(Type staticType, string memberName)
            where TMember : MemberInfo
        {
            var members = staticType
                          .GetMembers(Reflect.StaticFlags)
                          .OfType<TMember>()
                          .Where(member => string.Equals(member.Name, memberName, StringComparison.OrdinalIgnoreCase))
                          .ToList();
            if (members.Count != 1)
            {
                throw new ReflectionException(
                    $"Could not find {typeof(TMember)} {staticType}.{memberName}");
            }
            return members[0];
        }
    }

    public static class Instance<TInstance>
    {
        public static TMember Find<TMember>(Expression<Action<TInstance>> memberExpression)
            where TMember : MemberInfo
        {
            var member = memberExpression.ExtractMember<TMember>();
            if (member is null)
            {
                Debugger.Break();
                throw new MissingMemberException();
            }
            return member;
        }

        public static TMember Find<TMember>(string memberName)
            where TMember : MemberInfo
        {
            var members = typeof(TInstance)
                         .GetMembers(Reflect.InstanceFlags)
                         .OfType<TMember>()
                         .Where(member => string.Equals(member.Name, memberName, StringComparison.OrdinalIgnoreCase))
                         .ToList();
            if (members.Count != 1)
            {
                throw new ReflectionException(
                    $"Could not find {typeof(TMember)} {typeof(TInstance)}.{memberName}");
            }
            return members[0];
        }
    }

    public static TMember Find<TMember>(Expression expression)
        where TMember : MemberInfo
    {
        var member = expression.ExtractMember<TMember>();
        if (member is not null) return member;
       
        // Build an error
        using var text = TextBuilder.Borrow();
        var methodCall = expression.Descendants()
                                   .OfType<MethodCallExpression>()
                                   .FirstOrDefault();
        if (methodCall is not null)
        {
            var method = methodCall.Method;
            // Is this a search of a Type's Members?
            if (method.DeclaringType == typeof(Type))
            {
                if (method.Name.StartsWith("Get"))
                {
                    text.Append("Could not find ")
                        .AppendDump(methodCall.Object)
                        .Append("'s ")
                        .Append(method.Name.AsSpan(3))
                        .Write(" with ");

                    var parameters = method.GetParameters();
                    var args = methodCall.Arguments;
                    Debug.Assert(parameters.Length == args.Count);
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var p = parameters[i];
                        var a = args[i];
                        text.Append(p.Name)
                            .Write(" = ");
                        if (a is ConstantExpression ce)
                        {
                            text.Write(ce.Value);
                        }
                        else
                        {
                            Debugger.Break();
                        }
                        text.Write(" ");
                    }

                    throw new MissingMemberException(text.ToString());
                }
            }
        }

       

        var values = expression.ExtractValues().ToList();
        if (values.Count == 1)
        {
            var value = values[0];
            var valueType = value?.GetType();
            if (valueType is not null && valueType.IsEnum)
            {
                string? valueName = Enum.GetName(valueType, value!);
                member = valueType
                         .GetMembers(Reflect.StaticFlags)
                         .OfType<TMember>()
                         .FirstOrDefault(m => m.Name == valueName);
                if (member is not null) return member;
            }
        }

        Debugger.Break();
        throw new MissingMemberException();
    }

    public static TMember Find<TInstance, TMember>(TInstance? instance, Expression<Action<TInstance?>> memberExpression)
        where TMember : MemberInfo
    {
        return Find<TMember>(memberExpression);
    }

    public static TMember Find<TInstance, TMember>(Expression<Action<TInstance?>> memberExpression)
        where TMember : MemberInfo
    {
        return Find<TMember>(memberExpression);
    }

    public static bool HasDefaultConstructor(this Type type, [NotNullWhen(true)] out ConstructorInfo? ctor)
    {
        ctor = type.GetConstructor(Reflect.InstanceFlags, Type.EmptyTypes);
        return ctor is not null;
    }
    
    public static bool HasDefaultConstructor(this Type type)
    {
        return type.GetConstructor(Reflect.InstanceFlags, Type.EmptyTypes) is not null;
    }

    public static ConstructorInfo? FindBestConstructor(Type type,
                                                       BindingFlags flags,
                                                       params object?[]? args)
    {
        return FindBestConstructor(type, flags, MemberExactness.Exact, args);
    }

    public static ConstructorInfo? FindBestConstructor(Type type, 
                                                       BindingFlags flags, 
                                                       MemberExactness exactness, 
                                                       params object?[]? args)
    {
        Type?[]? argTypes;
        if (args is null)
        {
            argTypes = Type.EmptyTypes;
        }
        else
        {
            argTypes = new Type?[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                argTypes[i] = args[i]?.GetType();
            }
        }
        return FindBestConstructor(type, flags, exactness, argTypes);
    }


    [Flags]
    public enum MemberExactness
    {
        Exact = 0,
        CanIgnoreInputArgs = 1 << 0,
    }

    private static bool FastMatch(Type? argType, Type paramType)
    {
        if (argType is null)
        {
            return paramType.CanContainNull();
        }

        if (argType == paramType) return true;
        if (argType.Implements(paramType)) return true;
        if (argType == typeof(object) || paramType == typeof(object)) return true;
        return false;
    }

    public static ConstructorInfo? FindBestConstructor(Type type,
                                                       BindingFlags flags,
                                                       params Type?[] argTypes)
    {
        return FindBestConstructor(type, flags, MemberExactness.Exact, argTypes);
    }

    public static ConstructorInfo? FindConstructor(Type type,
        BindingFlags flags,
        params Type[] argTypes)
    {
        return type.GetConstructors(flags)
            .Where(ctor => ctor.HasParameterTypes(argTypes))
            .OneOrDefault();
    }

    public static ConstructorInfo? FindBestConstructor(Type type,
                                                       BindingFlags flags,
                                                       MemberExactness exactness,
                                                           params Type?[] argTypes)
    {
        return type.GetConstructors(flags)
                   .OrderByDescending(ctor => ctor.GetParameters().Length)
                    .SelectWhere((ConstructorInfo ctor, out (int Distance, ConstructorInfo Constructor) measuredCtor) =>
            {
                int distance = 0;
                var parameters = ctor.GetParameters();
                if (parameters.Length < argTypes.Length &&
                    !exactness.HasFlag(MemberExactness.CanIgnoreInputArgs))
                {
                    measuredCtor = default;
                    return false;
                }

                for (var p = 0; p < parameters.Length; p++)
                {
                    var parameter = parameters[p];
                    if (p < argTypes.Length)
                    {
                        var argType = argTypes[p];
                        if (!FastMatch(argType, parameter.ParameterType))
                        {
                            // We do not match
                            measuredCtor = default;
                            return false;
                        }
                    }
                    else
                    {
                        // Can we just use a default value?
                        if (parameter.HasDefaultValue)
                        {
                            // Not great
                            distance++;
                        }
                        else
                        {
                            // This ctor does not work
                            measuredCtor = default;
                            return false;
                        }
                    }
                }

                // If we got here, we matched everything and have a distance
                measuredCtor = (distance, ctor);
                return true;
            })
            .OrderBy(tuple => tuple.Distance)
            .Select(tuple => tuple.Constructor)
            .FirstOrDefault();
    }
}