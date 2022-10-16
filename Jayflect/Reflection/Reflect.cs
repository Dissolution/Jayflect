using System.Linq.Expressions;
using System.Reflection;
using Jay.Collections;
using Jay.Comparision;
using Jay.Reflection.Building;
using Jay.Reflection.Exceptions;
using Jayflect;
using Jayflect.Extensions;

namespace Jay.Reflection;

public static partial class Reflect
{
    public const BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic |
                                         BindingFlags.Static | BindingFlags.Instance |
                                         BindingFlags.IgnoreCase;

    public const BindingFlags PublicFlags = BindingFlags.Public |
                                            BindingFlags.Static | BindingFlags.Instance |
                                            BindingFlags.IgnoreCase;

    public const BindingFlags NonPublicFlags = BindingFlags.NonPublic |
                                               BindingFlags.Static | BindingFlags.Instance |
                                               BindingFlags.IgnoreCase;

    public const BindingFlags StaticFlags = BindingFlags.Public | BindingFlags.NonPublic |
                                            BindingFlags.Static |
                                            BindingFlags.IgnoreCase;

    public const BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.Instance |
                                              BindingFlags.IgnoreCase;

    public static IEnumerable<Type> AllExportedTypes()
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .SelectMany(assembly => Result.InvokeOrDefault(() => assembly.ExportedTypes, Type.EmptyTypes));
    }
}

public static partial class Reflect
{
    public static Reflection<T> On<T>() => new Reflection<T>();

    public static TMember Get<TMember>(Expression<Action> memberExpression)
        where TMember : MemberInfo
    {
        var member = memberExpression.ExtractMember<TMember>();
        if (member is not null)
            return member;
        throw new ReflectionException($"Could not find {typeof(TMember)} from {memberExpression}");
    }

    public static TMember Get<TMember>(Expression<Func<object?>> memberExpression)
        where TMember : MemberInfo
    {
        var member = memberExpression.ExtractMember<TMember>();
        if (member is not null)
            return member;
        throw new ReflectionException($"Could not find {typeof(TMember)} from {memberExpression}");
    }
}

public static partial class Reflect
{
    /// <summary>
    /// A delegate that disposes a value.
    /// </summary>
    private delegate void DisposeRef<T>(ref T value);

    /// <summary>
    /// A cache of built <see cref="DisposeRef{T}"/> keyed on the <see cref="Type"/> of value disposed
    /// </summary>
    private static readonly ConcurrentTypeDictionary<Delegate?> _strongDisposeCache = new();

    /// <summary>
    ///     Disposes the given <paramref name="thing"/> by calling <see cref="M:IDisposable.Dispose"/> (if it exists)
    ///     and removing all event handlers.
    /// </summary>
    public static void StrongDispose<T>(ref T thing)
        where T : class
    {
        var strongDispose = _strongDisposeCache.GetOrAdd(typeof(T), type => CreateStrongDispose(type)) as DisposeRef<T>;
        // It's okay if strongDispose is null as that might indicate we don't have to do anything to dispose a T
        strongDispose?.Invoke(ref thing);
    }

    /// <summary>
    ///     Creates a <see cref="DisposeRef{T}"/> for the given <paramref name="type"/>.
    /// </summary>
    private static Delegate? CreateStrongDispose(Type type)
    {
        // Check if we have to create a delegate

        // Do we have a dispose method?
        var disposeMethod = type.GetMethod(nameof(IDisposable.Dispose), InstanceFlags);
        // Get all event fields
        var eventFields = type.GetEvents(InstanceFlags)
            .Select(ev => ev.GetBackingField())
            .Where(field => field is not null)
            .ToList();

        // If we have nothing to deal with, we can return null and it will skip execution
        if (disposeMethod is null && eventFields.Count == 0)
        {
            return null;
        }

        // Create our dynamic method
        var runtimeMethod = RuntimeBuilder.CreateRuntimeMethod(
            typeof(DisposeRef<>).MakeGenericType(type),
            $"StrongDispose_{type.Name}");
        var emitter = runtimeMethod.Emitter;
        // Null check = fast return
        if (!type.IsValueType)
        {
            emitter.DefineLabel(out var lblNotNull)
                .Ldarg_0()
                .Brtrue(lblNotNull)
                .Ret()
                .MarkLabel(lblNotNull);
        }

        // Do we have a dispose method?
        if (disposeMethod != null)
        {
            emitter.DefineLabel(out var lblEnd)
                // try { thing.Dispose(); }
                .BeginExceptionBlock(out _)
                .Ldarg_0()
                .Call(disposeMethod)
                .Br(lblEnd)
                // catch (Exception ex) { // ignore }
                .BeginCatchBlock<Exception>()
                .Pop()
                .EndExceptionBlock()
                .MarkLabel(lblEnd);
        }

        // Event Fields?
        foreach (var field in eventFields)
        {
            // Set the event backing field to null, thus freeing all references
            emitter.Ldarg_0()
                .Ldnull()
                .Stfld(field!);

            /* THandler? eventHandler = thing.field;
             * THandler? comparand;
             * do
             * {
             *     comparand = eventHandler;
             *     eventHandler = Interlocked.CompareExchange<THandler?>(ref thing.field, null, comparand);
             * }
             * while (eventHandler != comparand);
             */
        }

        // Fin
        emitter.Ret();

        // Done with emission
        return runtimeMethod.CreateDelegate();
    }
}