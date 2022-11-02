using Jay.Collections;
using Jay.Extensions;
using Jayflect.Building;
using Jayflect.Searching;

namespace Jayflect;

public static class Disposal
{
    /// <summary>
    /// A delegate that disposes a value.
    /// </summary>
    private delegate void StrongDisposeValue<in T>(T value);

    /// <summary>
    /// A cache of built <see cref="StrongDisposeValue{T}"/> keyed on the <see cref="Type"/> of value disposed
    /// </summary>
    private static readonly ConcurrentTypeDictionary<Delegate?> _strongDisposeCache = new();


    /// <summary>
    ///     Creates a <see cref="StrongDisposeValue{T}"/> for the given <paramref name="type"/>.
    /// </summary>
    private static Delegate? CreateDeepDispose(Type type)
    {
        // Do we have a dispose method?
        var disposeMethod = MemberSearch.GetMethod(type, new(
                "Dispose",
                Visibility.Instance | Visibility.Public | Visibility.NonPublic,
                typeof(void),
                Type.EmptyTypes));

        // Get all event fields
        var eventFields = type.GetEvents(Reflect.Flags.Instance)
            .Select(ev => ev.GetBackingField())
            .WhereNotNull()
            .ToList();

        // If we have nothing to deal with, we can return null and it will skip execution
        if (disposeMethod is null && eventFields.Count == 0)
        {
            return null;
        }

        // Create our dynamic method
        var runtimeMethod = RuntimeBuilder.CreateRuntimeDelegateBuilder(
            typeof(StrongDisposeValue<>).MakeGenericType(type),
            $"{type.Name}_StrongDispose");
        var emitter = runtimeMethod.Emitter;
        // Do we have a dispose method?
        if (disposeMethod != null)
        {
            // try { value.Dispose(); }
            emitter.BeginExceptionBlock(out var lblEndTry)
                .Ldarg_0()
                .Call(disposeMethod)
                .Br(lblEndTry)
                // catch (Exception ex) { // ignore }
                .BeginCatchBlock<Exception>()
                .Pop()
                .EndExceptionBlock()
                .MarkLabel(lblEndTry);
        }

        // Event Fields?
        foreach (var field in eventFields)
        {
            // Set the event backing field to null, thus freeing all references
            emitter.Ldarg_0()
                .Ldnull()
                .Stfld(field);
        }

        // Fin
        emitter.Ret();

        // Done with emission
        return runtimeMethod.CreateDelegate();
    }

    /// <summary>
    ///     Disposes the given <paramref name="thing"/> by calling <see cref="M:IDisposable.Dispose"/> (if it exists)
    ///     and removing all event handlers.
    /// </summary>
    public static void DeepDispose<T>(this T? thing)
        where T : class
    {
        if (thing is null) return;
        var del = _strongDisposeCache.GetOrAdd<T>(CreateDeepDispose);
        if (del is null) return;
        if (del is StrongDisposeValue<T> strongDispose)
        {
            strongDispose(thing);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}