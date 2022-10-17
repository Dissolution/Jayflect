/*using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Reflection;
using Jay.Reflection.Building;
using Jay.Reflection.Building.Adapting;
using Jay.Text;

namespace Jay.Reflection;

public sealed record MemberAndDelegateType(MemberInfo Member, Type DelegateType)
{
    public FieldInfo? Field => Member as FieldInfo;
    public PropertyInfo? Property => Member as PropertyInfo;
    public EventInfo? Event => Member as EventInfo;
    public ConstructorInfo? Constructor => Member as ConstructorInfo;
    public MethodInfo? Method => Member as MethodInfo;
}

public class MemberDelegateMap
{
    private readonly ConcurrentDictionary<MemberAndDelegateType, Delegate> _cache = new();

    internal MemberAndDelegateType Key(MemberInfo member, Type delegateType)
    {
        return new(member, delegateType);
    }

    internal MemberAndDelegateType Key<TDelegate>(MemberInfo member)
        where TDelegate : Delegate
    {
        return new(member, typeof(TDelegate));
    }

    public TDelegate GetOrAdd<TMember, TDelegate>(TMember member, TDelegate addDelegate)
        where TMember : MemberInfo
        where TDelegate : Delegate
    {
        var del = _cache.GetOrAdd(Key<TDelegate>(member), addDelegate);
        if (del is TDelegate typedDelegate)
            return typedDelegate;
        throw new InvalidOperationException();
    }

    public TDelegate GetOrAdd<TMember, TDelegate>(TMember member, Func<MemberAndDelegateType, TDelegate> createDelegate)
        where TMember : MemberInfo
        where TDelegate : Delegate
    {
        var del = _cache.GetOrAdd(Key<TDelegate>(member), createDelegate);
        if (del is TDelegate typedDelegate)
            return typedDelegate;
        throw new InvalidOperationException();
    }

    public TDelegate GetOrAdd<TDelegate>(MemberInfo member, TDelegate addDelegate)
        where TDelegate : Delegate
    {
        var del = _cache.GetOrAdd(Key<TDelegate>(member), addDelegate);
        if (del is TDelegate typedDelegate)
            return typedDelegate;
        throw new InvalidOperationException();
    }

    public TDelegate GetOrAdd<TDelegate>(MemberInfo member, Func<MemberAndDelegateType, TDelegate> createDelegate)
        where TDelegate : Delegate
    {
        var del = _cache.GetOrAdd(Key<TDelegate>(member), createDelegate);
        if (del is TDelegate typedDelegate)
            return typedDelegate;
        throw new InvalidOperationException();
    }


}

public static class CachedReflectionExtensions
{
    private static readonly MemberDelegateMap _cache = new();

    private static void ValidateStaticInstanceType<TInstance>()
    {
        var type = typeof(TInstance);
        if (type.IsStatic() || type == typeof(Types.Static) || type == typeof(void))
            return;
        throw new ArgumentException("Static members called with TInstance must use Types.Static", nameof(TInstance));
    }

    private static void ValidateInstanceType<TInstance>()
    {
        var type = typeof(TInstance);
        if (type.IsStatic() || type == typeof(Types.Static) || type == typeof(void))
            throw new ArgumentException("Instance members called with TInstance must use a valid Instance Type",
                nameof(TInstance));
    }

    private static Getter<TInstance, TValue> CreateGetter<TInstance, TValue>(FieldInfo field)
    {
        using var name = TextBuilder.Borrow();
        name.Write("get_");
        if (field.IsStatic)
        {
            ValidateStaticInstanceType<TInstance>();
            name.Write(typeof(TInstance).Name);
        }
        else
        {
            ValidateInstanceType<TInstance>();
            var instanceTypeName = typeof(TInstance).Name;
            name.Write(char.ToLower(instanceTypeName[0]));
            name.Write(instanceTypeName[1..]);
        }

        name.Append('_').Append(field.Name);
        if (typeof(TValue) != field.FieldType)
        {
            name.Append(" as ").Append(typeof(TValue).Name);
        }

        return RuntimeBuilder.CreateDelegate<Getter<TInstance, TValue>>(name.ToString(),
            runtimeMethod =>
            {
                var emitter = runtimeMethod.Emitter;
                if (!field.IsStatic)
                {
                    emitter.LoadInstanceFor(field, runtimeMethod.Parameters[0], out _)
                           .Ldfld(field);
                }
                else
                {
                    emitter.Ldsfld(field);
                }

                emitter.Cast(field.FieldType, typeof(TValue))
                       .Ret();
            });
    }

    public static TValue GetValue<TInstance, TValue>(this FieldInfo field, ref TInstance instance)
    {
        var getter = _cache.GetOrAdd(field,
            mad => CreateGetter<TInstance, TValue>(mad.Field!));
        return getter(ref instance);
    }

    private static Setter<TInstance, TValue> CreateSetter<TInstance, TValue>(FieldInfo field)
    {
        using var name = TextBuilder.Borrow();
        name.Write("set_");
        if (field.IsStatic)
        {
            ValidateStaticInstanceType<TInstance>();
            name.Write(typeof(TInstance).Name);
        }
        else
        {
            ValidateInstanceType<TInstance>();
            var instanceTypeName = typeof(TInstance).Name;
            name.Write(char.ToLower(instanceTypeName[0]));
            name.Write(instanceTypeName[1..]);
        }

        name.Append('_').Append(field.Name);
        if (typeof(TValue) != field.FieldType)
        {
            name.Append(" to ").Append(typeof(TValue).Name);
        }

        return RuntimeBuilder.CreateDelegate<Setter<TInstance, TValue>>(name.ToString(),
            runtimeMethod =>
            {
                var emitter = runtimeMethod.Emitter;

                if (!field.IsStatic)
                {
                    emitter.LoadInstanceFor(field, runtimeMethod.Parameters[0], out _)
                           .LoadArg(runtimeMethod.Parameters[1])
                           .Cast(typeof(TInstance), field.FieldType)
                           .Stfld(field);
                }
                else
                {
                    emitter.LoadArg(runtimeMethod.Parameters[1])
                           .Cast(typeof(TInstance), field.FieldType)
                           .Stsfld(field);
                }

                emitter.Ret();
            });
    }

    public static void SetValue<TInstance, TValue>(this FieldInfo field, ref TInstance instance, TValue value)
    {
        var setter = _cache.GetOrAdd(field, mad => CreateSetter<TInstance, TValue>(mad.Field!));
        setter(ref instance, value);
    }

    private static TDelegate AdaptDelegate<TDelegate>(MethodBase method)
        where TDelegate : Delegate 
    {
        var result = MethodAdapter.TryAdapt(method, typeof(TDelegate), out var adapted);
        result.ThrowIfFailed();
        if (adapted is not TDelegate typedDelegate)
            throw new InvalidOperationException();
        return typedDelegate;
    }

    private static Getter<TInstance, TValue> CreateGetter<TInstance, TValue>(PropertyInfo property)
    {
        // Do we have a getter?
        var propertyGetter = property.GetGetter();
        if (propertyGetter is null)
        {
            var backingField = property.GetBackingField();
            if (backingField is null)
            {
                throw new InvalidOperationException();
            }
            return CreateGetter<TInstance, TValue>(backingField);
        }
        return AdaptDelegate<Getter<TInstance, TValue>>(propertyGetter);
    }

    public static TValue GetValue<TInstance, TValue>(this PropertyInfo property, ref TInstance instance)
    {
        var getter = _cache.GetOrAdd(property, mad => CreateGetter<TInstance, TValue>(mad.Property!));
        return getter(ref instance);
    }

    private static Setter<TInstance, TValue> CreateSetter<TInstance, TValue>(PropertyInfo property)
    {
        // Do we have a getter?
        var propertySetter = property.GetSetter();
        if (propertySetter is null)
        {
            var backingField = property.GetBackingField();
            if (backingField is null)
            {
                throw new InvalidOperationException();
            }
            return CreateSetter<TInstance, TValue>(backingField);
        }
        return AdaptDelegate<Setter<TInstance, TValue>>(propertySetter);
    }

    public static void SetValue<TInstance, TValue>(this PropertyInfo property, ref TInstance instance, TValue value)
    {
        var setter = _cache.GetOrAdd(property, mad => CreateSetter<TInstance, TValue>(mad.Property!));
        setter(ref instance, value);
    }

    // TODO: EVENTS

    public static TInstance Construct<TInstance>(this ConstructorInfo constructor)
    {
        var ctor = _cache.GetOrAdd(constructor, mad => AdaptDelegate<Func<TInstance>>(mad.Constructor!));
        return ctor();
    }

    public static TInstance Construct<TInstance, TArg1>(this ConstructorInfo constructor, TArg1 arg1)
    {
        var ctor = _cache.GetOrAdd(constructor, mad => AdaptDelegate<Func<TArg1, TInstance>>(mad.Constructor!));
        return ctor(arg1);
    }

    public static TInstance Construct<TInstance, TArg1, TArg2>(this ConstructorInfo constructor, TArg1 arg1, TArg2 arg2)
    {
        var ctor = _cache.GetOrAdd(constructor, mad => AdaptDelegate<Func<TArg1, TArg2, TInstance>>(mad.Constructor!));
        return ctor(arg1, arg2);
    }

    public static TInstance Construct<TInstance, TArg1, TArg2, TArg3>(this ConstructorInfo constructor, TArg1 arg1, TArg2 arg2, TArg3 arg3)
    {
        var ctor = _cache.GetOrAdd(constructor, mad => AdaptDelegate<Func<TArg1, TArg2, TArg3, TInstance>>(mad.Constructor!));
        return ctor(arg1, arg2, arg3);
    }

    private static Constructor<TInstance> CreateConstructor<TInstance>(ConstructorInfo ctor)
    {
        ArgumentNullException.ThrowIfNull(ctor);
        return RuntimeBuilder.CreateDelegate<Constructor<TInstance>>($"{typeof(TInstance)}.ctor(params)",
            method =>
            {
                method.Emitter
                      .LoadParams(method.Parameters[0], ctor.GetParameters())
                      .Newobj(ctor)
                      .Cast(ctor.DeclaringType!, method.ReturnType)
                      .Ret();
            });
    }

    public static TInstance Construct<TInstance>(this ConstructorInfo constructor, params object?[] args)
    {
        var ctor = _cache.GetOrAdd(constructor, mad => CreateConstructor<TInstance>(mad.Constructor!));
        return ctor(args);
    }

    // TODO: METHODS
}*/