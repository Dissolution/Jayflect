using System.Diagnostics;
using Jay.Collections;
using Jay.Reflection.Building;

namespace Jay.Reflection;

public static class GenericExtensions
{
    private static readonly ConcurrentTypeDictionary<Delegate> _unhookEventsDelegateCache;

    static GenericExtensions()
    {
        _unhookEventsDelegateCache = new();
    }

    private static Action<T> GetUnhooker<T>()
    {
        return (_unhookEventsDelegateCache.GetOrAdd<T>(CreateUnhooker) as Action<T>)!;
    }

    private static Delegate CreateUnhooker(Type type)
    {
        Debug.Assert(type is not null);
        Debug.Assert(!type.IsStatic());
        var fields = type.GetEvents(Reflect.InstanceFlags)
                         .Select(vent => vent.GetBackingField())
                         .Where(field => field is not null)
                         .ToList();
        return RuntimeBuilder.CreateDelegate(typeof(Action<>).MakeGenericType(type),
            emitter =>
            {
                foreach (var field in fields)
                {
                    emitter.Ldarg_0()
                           .Ldnull()
                           .Stfld(field!);
                }
                emitter.Ret();
            });
    }

    /// <summary>
    /// Unhook all <c>event</c>s on <c>this</c> <paramref name="instance"/>
    /// </summary>
    /// <remarks>
    /// Sets all underlying fields to null
    /// </remarks>
    public static void UnhookEvents<T>(this T instance)
    {
        GetUnhooker<T>()(instance);
    }
}