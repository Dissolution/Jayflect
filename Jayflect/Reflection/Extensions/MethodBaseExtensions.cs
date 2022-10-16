using System.Reflection;
using Jay.Reflection.Building;
using Jay.Reflection.Building.Adapting;
using Jay.Reflection.Building.Deconstruction;
using Jay.Reflection.Building.Emission;
using Jay.Reflection.Caching;
using Jayflect;

namespace Jay.Reflection;

public static class MethodBaseExtensions
{
    public static Visibility Visibility(this MethodBase? method)
    {
        Visibility visibility = Jayflect.Visibility.None;
        if (method is null)
            return visibility;
        if (method.IsPrivate)
            visibility |= Jayflect.Visibility.Private;
        if (method.IsFamily)
            visibility |= Jayflect.Visibility.Protected;
        if (method.IsAssembly)
            visibility |= Jayflect.Visibility.Internal;
        if (method.IsPublic)
            visibility |= Jayflect.Visibility.Public;
        return visibility;
    }

    public static bool IsStatic(this MethodBase? method)
    {
        return method is not null && method.IsStatic;
    }

    public static Type ReturnType(this MethodBase? method)
    {
        if (method is null)
            return typeof(void);
        if (method is MethodInfo methodInfo)
            return methodInfo.ReturnType;
        if (method is ConstructorInfo constructorInfo)
            return constructorInfo.DeclaringType!;
        return typeof(void);
    }

    public static InstructionStream GetInstructions(this MethodBase method)
    {
        return new RuntimeDeconstructor(method)
            .GetInstructions();
    }
    
    public static Result TryAdapt<TDelegate>(this MethodBase method, [NotNullWhen(true)] out TDelegate? @delegate)
        where TDelegate : Delegate
    {
        var dynamicMethod = RuntimeBuilder.CreateRuntimeMethod<TDelegate>($"{typeof(TDelegate)}_{method.GetType()}_adapter");
        var adapter = new DelegateMethodAdapter<TDelegate>(method);
        var result = adapter.TryAdapt(dynamicMethod.Emitter);
        if (!result)
        {
            @delegate = null;
            return result;
        }
        result = dynamicMethod.TryCreateDelegate(out @delegate);
        return result;
    }

    public static TDelegate Adapt<TDelegate>(this MethodBase method)
        where TDelegate : Delegate
    {
        return DelegateMemberCache.Instance
                                  .GetOrAdd(method, dm =>
                                  {
                                      TryAdapt<TDelegate>((dm.Member as MethodInfo)!, out var del).ThrowIfFailed();
                                      return del!;
                                  });
    }

    public static Type[] GetParameterTypes(this MethodBase methodBase)
    {
        var parameters = methodBase.GetParameters();
        var len = parameters.Length;
        if (len == 0) return Type.EmptyTypes;
        var types = new Type[len];
        for (var i = 0; i < len; i++)
        {
            types[i] = parameters[i].ParameterType;
        }
        return types;
    }
}