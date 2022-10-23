using Jay.Validation;
using Jayflect.Building;
using Jayflect.Building.Emission;
using Jayflect.Caching;
using Jayflect.Extensions;

namespace Jayflect;


public static partial class Reflect
{

    private static SetValue<TInstance, TValue> CreateSetValue<TInstance, TValue>(FieldInfo field)
    {
        return RuntimeBuilder.CreateDelegate<SetValue<TInstance, TValue>>($"set_{field.Name}",
            builder => builder.Emitter
                .EmitLoadInstance(builder.Parameters[0], field)
                .EmitLoadParameter(builder.Parameters[1], field.FieldType)
                .Stfld(field)
                .Ret());
    }
    
    private static GetValue<TInstance, TValue> CreateGetValue<TInstance, TValue>(FieldInfo field)
    {
        return RuntimeBuilder.CreateDelegate<GetValue<TInstance, TValue>>($"get_{field.Name}",
            builder => builder.Emitter
                .EmitLoadInstance(builder.Parameters[0], field)
                .Ldfld(field)
                .EmitCast(field.FieldType, builder.ReturnType)
                .Ret());
    }

    public static void SetValue<TInstance, TValue>(this FieldInfo fieldInfo,
        ref TInstance instance, TValue value)
    {
        var setValue = MemberDelegateCache.GetOrAdd(fieldInfo, CreateSetValue<TInstance, TValue>);
        setValue(ref instance, value);
    }
    public static TValue GetValue<TInstance, TValue>(this FieldInfo fieldInfo,
        ref TInstance instance)
    {
        var getValue = MemberDelegateCache.GetOrAdd(fieldInfo, CreateGetValue<TInstance, TValue>);
        return getValue(ref instance);
    }

    private static SetValue<TInstance, TValue> CreateSetValue<TInstance, TValue>(PropertyInfo property)
    {
        // Find setter
        var setter = property.GetSetter();
        if (setter is null)
        {
            var backingField = property.GetBackingField();
            if (backingField is not null)
                return CreateSetValue<TInstance, TValue>(backingField);
            throw new AdapterException($"Cannot find a Setter");
        }

        return RuntimeMethodAdapter.Adapt<SetValue<TInstance, TValue>>(setter);
    }
    
    private static GetValue<TInstance, TValue> CreateGetValue<TInstance, TValue>(PropertyInfo property)
    {
        // Find getter
        var getter = property.GetGetter();
        if (getter is null)
        {
            var backingField = property.GetBackingField();
            if (backingField is not null)
                return CreateGetValue<TInstance, TValue>(backingField);
            throw new AdapterException($"Cannot find a Getter");
        }

        return RuntimeMethodAdapter.Adapt<GetValue<TInstance, TValue>>(getter);
    }
    
    public static void SetValue<TInstance, TValue>(this PropertyInfo propertyInfo,
        ref TInstance instance, TValue value)
    {
        var setValue = MemberDelegateCache.GetOrAdd(propertyInfo, CreateSetValue<TInstance, TValue>);
        setValue(ref instance, value);
    }
    
    public static TValue GetValue<TInstance, TValue>(this PropertyInfo propertyInfo,
        ref TInstance instance)
    {
        var getValue = MemberDelegateCache.GetOrAdd(propertyInfo, CreateGetValue<TInstance, TValue>);
        return getValue(ref instance);
    }

    private static AddHandler<TInstance, THandler> CreateAddHandler<TInstance, THandler>(
        EventInfo eventInfo)
        where THandler : Delegate
    {
        // Find Adder
        var adder = eventInfo.GetAdder();
        if (adder is null)
        {
            var backingField = eventInfo.GetBackingField();
            if (backingField is not null)
            {
                throw new NotImplementedException();
            }
            throw new AdapterException($"Cannot find an Adder");
        }

        return RuntimeMethodAdapter.Adapt<AddHandler<TInstance, THandler>>(adder);
    }

    public static void AddHandler<TInstance, THandler>(this EventInfo eventInfo,
        ref TInstance instance,
        THandler eventHandler)
        where THandler : Delegate
    {
        var addHandler = MemberDelegateCache.GetOrAdd(eventInfo, CreateAddHandler<TInstance, THandler>);
        addHandler(ref instance, eventHandler);
    }
    
    private static RemoveHandler<TInstance, THandler> CreateRemoveHandler<TInstance, THandler>(
        EventInfo eventInfo)
        where THandler : Delegate
    {
        // Find Remover
        var remover = eventInfo.GetRemover();
        if (remover is null)
        {
            var backingField = eventInfo.GetBackingField();
            if (backingField is not null)
            {
                throw new NotImplementedException();
            }
            throw new AdapterException($"Cannot find a Remover");
        }

        return RuntimeMethodAdapter.Adapt<RemoveHandler<TInstance, THandler>>(remover);
    }

    public static void RemoveHandler<TInstance, THandler>(this EventInfo eventInfo,
        ref TInstance instance,
        THandler eventHandler)
        where THandler : Delegate
    {
        var removeHandler = MemberDelegateCache.GetOrAdd(eventInfo, CreateRemoveHandler<TInstance, THandler>);
        removeHandler(ref instance, eventHandler);
    }
    
    private static RaiseHandler<TInstance> CreateRaiseHandler<TInstance>(EventInfo eventInfo)
    {
        // Find Raiser
        var adder = eventInfo.GetRaiser();
        if (adder is not null)
        {
            // We somehow have one!
            return RuntimeMethodAdapter.Adapt<RaiseHandler<TInstance>>(adder);
        }
        var backingField = eventInfo.GetBackingField();
        if (backingField is not null)
        {
            var eventHandlerType = backingField.FieldType;
            var invokeMethod = eventHandlerType.GetInvokeMethod()
                .ValidateNotNull();
            return RuntimeBuilder.CreateDelegate<RaiseHandler<TInstance>>($"raise_{eventInfo.Name}",
                builder =>
                {
                   // The event field is a Delegate with the signature of the event
                   // e.g. EventHandler<T> is (object?,EventArgs<T>)
                   // We can access its parts!
                   var emitter = builder.Emitter;

                   emitter.EmitLoadInstance(builder.Parameters[0], backingField)
                       .Ldfld(backingField)
                       .DeclareLocal(eventHandlerType, out var eventHandler)
                       .Stloc(eventHandler)
                       .Ldloc(eventHandler)
                       .Brfalse(out var finished)
                       .Ldloc(eventHandler)
                       .Call(MemberCache.Methods.Delegate_GetInvocationList)
                       .DeclareLocal<Delegate[]>(out var delegates)
                       .Stloc(delegates)
                       .Ldloc(delegates)
                       .Ldlen()
                       .DeclareLocal<int>(out var len)
                       .Stloc(len)
                       .DeclareLocal<int>(out var i)
                       .Ldc_I4(0)
                       .Stloc(i)
                       .Br(out var whileCheck)
                       .DefineAndMarkLabel(out var doStart)
                       .Ldloc(delegates)
                       .Ldloc(i)
                       .Ldelem<Delegate>();
                   EmitterExtensions.EmitLoadParams(emitter, builder.Parameters[1],
                       invokeMethod.GetParameters());
                   emitter.Call(invokeMethod);
                   if (invokeMethod.ReturnType != typeof(void))
                       emitter.Pop();
                   emitter.Ldloc(i)
                       .Ldc_I4_1()
                       .Add()
                       .Stloc(i)
                       .MarkLabel(whileCheck)
                       .Ldloc(i)
                       .Ldloc(len)
                       .Blt(doStart)
                   .MarkLabel(finished)
                       .Ret();
                });
        }
        throw new AdapterException($"Cannot find a way to raise this event");
        
    }

    public static void RaiseHandler<TInstance>(this EventInfo eventInfo,
        ref TInstance instance,
        params object?[] eventArgs)
    {
        var raiseHandler = MemberDelegateCache.GetOrAdd(eventInfo, CreateRaiseHandler<TInstance>);
        raiseHandler(ref instance, eventArgs);
    }

    private static Construct<TInstance> CreateConstruct<TInstance>(ConstructorInfo ctor)
    {
        return RuntimeBuilder.CreateDelegate<Construct<TInstance>>($"construct_{ctor.OwnerType().Name}",
            builder =>
            {
                var emitter = builder.Emitter;
                emitter.EmitLoadParams(builder.Parameters[0], ctor.GetParameters())
                    .Newobj(ctor)
                    .EmitCast(ctor.DeclaringType!, typeof(TInstance))
                    .Ret();
            });
    }

    public static TInstance Construct<TInstance>(this ConstructorInfo constructorInfo,
        params object?[] args)
    {
        var construct = MemberDelegateCache.GetOrAdd(constructorInfo, CreateConstruct<TInstance>);
        return construct(args);
    }

    private static Invoke<TInstance, TReturn> CreateInvoke<TInstance, TReturn>(MethodBase method)
    {
        return RuntimeMethodAdapter.Adapt<Invoke<TInstance, TReturn>>(method);
    }

    public static TReturn Invoke<TInstance, TReturn>(this MethodBase method,
        ref TInstance instance,
        params object?[] args)
    {
        var invoke = MemberDelegateCache.GetOrAdd(method, CreateInvoke<TInstance, TReturn>);
        return invoke(ref instance, args);
    }
    
    public static TReturn Invoke<TInstance, T1, TReturn>(this MethodBase method,
        ref TInstance instance,
        T1 arg1)
    {
        var invoke = MemberDelegateCache.GetOrAdd(method, 
            RuntimeMethodAdapter.Adapt<Invoke<TInstance, T1, TReturn>>);
        return invoke(ref instance, arg1);
    }
    
    public static TReturn Invoke<TInstance, T1, T2, TReturn>(this MethodBase method,
        ref TInstance instance,
        T1 arg1, T2 arg2)
    {
        var invoke = MemberDelegateCache.GetOrAdd(method, 
            RuntimeMethodAdapter.Adapt<Invoke<TInstance, T1, T2, TReturn>>);
        return invoke(ref instance, arg1, arg2);
    }
    
    public static TReturn Invoke<TInstance, T1, T2, T3, TReturn>(this MethodBase method,
        ref TInstance instance,
        T1 arg1, T2 arg2, T3 arg3)
    {
        var invoke = MemberDelegateCache.GetOrAdd(method, 
            RuntimeMethodAdapter.Adapt<Invoke<TInstance, T1, T2, T3, TReturn>>);
        return invoke(ref instance, arg1, arg2, arg3);
    }
    
    public static TReturn Invoke<TInstance, T1, T2, T3, T4, TReturn>(this MethodBase method,
        ref TInstance instance,
        T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        var invoke = MemberDelegateCache.GetOrAdd(method, 
            RuntimeMethodAdapter.Adapt<Invoke<TInstance, T1, T2, T3, T4, TReturn>>);
        return invoke(ref instance, arg1, arg2, arg3, arg4);
    }
    
    public static TReturn Invoke<TInstance, T1, T2, T3, T4, T5, TReturn>(this MethodBase method,
        ref TInstance instance,
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        var invoke = MemberDelegateCache.GetOrAdd(method, 
            RuntimeMethodAdapter.Adapt<Invoke<TInstance, T1, T2, T3, T4, T5, TReturn>>);
        return invoke(ref instance, arg1, arg2, arg3, arg4, arg5);
    }
}