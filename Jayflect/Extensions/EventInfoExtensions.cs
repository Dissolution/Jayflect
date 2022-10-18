namespace Jayflect.Extensions;

public static class EventInfoExtensions
{
    public static MethodInfo? GetAdder(this EventInfo? eventInfo)
    {
        if (eventInfo is null) return null;
        return eventInfo.GetAddMethod(false) ??
               eventInfo.GetAddMethod(true);
    }

    public static MethodInfo? GetRemover(this EventInfo? eventInfo)
    {
        if (eventInfo is null) return null;
        return eventInfo.GetRemoveMethod(false) ??
               eventInfo.GetRemoveMethod(true);
    }

    public static MethodInfo? GetRaiser(this EventInfo? eventInfo)
    {
        if (eventInfo is null) return null;
        return eventInfo.GetRaiseMethod(false) ??
               eventInfo.GetRaiseMethod(true);
    }

    public static Visibility Visibility(this EventInfo? eventInfo)
    {
        Visibility visibility = Jayflect.Visibility.None;
        if (eventInfo is null)
            return visibility;
        visibility |= eventInfo.GetAdder().Visibility();
        visibility |= eventInfo.GetRemover().Visibility();
        visibility |= eventInfo.GetRaiser().Visibility();
        return visibility;
    }

    public static bool IsStatic(this EventInfo? eventInfo)
    {
        if (eventInfo is null) return false;
        return eventInfo.GetAdder().IsStatic() ||
               eventInfo.GetRemover().IsStatic() ||
               eventInfo.GetRaiser().IsStatic();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventInfo"></param>
    /// <returns></returns>
    /// <see cref="https://stackoverflow.com/questions/9847424/is-the-backing-field-of-a-compiler-generated-event-always-guaranteed-to-use-the"/>
    /// <remarks>
    /// This is **NOT** guaranteed to consistently work if the compiler team changes their minds.
    /// </remarks>
    public static FieldInfo? GetBackingField(this EventInfo? eventInfo)
    {
        if (eventInfo is null) return null;
        
        BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;
        
        if (eventInfo.IsStatic())
        {
            flags |= BindingFlags.Static;
        }
        else
        {
            flags |= BindingFlags.Instance;
        }
        return eventInfo.DeclaringType?.GetField(eventInfo.Name, flags);
    }

    /*
    public static EventAdder<TInstance, THandler> CreateAdder<TInstance, THandler>(this EventInfo eventInfo)
        where THandler : Delegate
    {
        ArgumentNullException.ThrowIfNull(eventInfo);
        var adder = eventInfo.GetAdder();
        if (adder is null) throw new ReflectionException("Cannot");
        return adder.CreateDelegate<EventAdder<TInstance, THandler>>();
    }

    public static EventRemover<TInstance, THandler> CreateRemover<TInstance, THandler>(this EventInfo eventInfo)
        where THandler : Delegate
    {
        ArgumentNullException.ThrowIfNull(eventInfo);
        var remover = eventInfo.GetRemover();
        if (remover is null) throw new ReflectionException("Cannot");
        return remover.CreateDelegate<EventRemover<TInstance, THandler>>();
    }

    public static EventRaiser<TInstance, TEventArgs> CreateRaiser<TInstance, TEventArgs>(this EventInfo eventInfo)
        where TEventArgs : EventArgs
    {
        ArgumentNullException.ThrowIfNull(eventInfo);
        var raiser = eventInfo.GetRaiser();
        if (raiser is null)
        {
            /* We can do some hackery here
             * There is always a backing Delegate field that the event is interacting with
             * we can just load up that field's value as a MulticastDelegate
             * call GetInvocationList -> Delegate[]
             * and then fire off each one in turn, feeding them the instance as sender and the eventargs
            *//*
            var backingField = eventInfo.GetBackingField();
            if (backingField is null)
                throw new ReflectionException($"Unable to find {eventInfo}'s backing field for a Raiser");
            var handlerSig = MethodSig.Of(backingField.FieldType, out var invokeMethod);
            Debug.Assert(handlerSig.ParameterCount == 2);
            var senderParam = handlerSig.Parameters[0];
            Debug.Assert(senderParam.ParameterType == typeof(object));
            var eventArgsParam = handlerSig.Parameters[1];
            Debug.Assert(eventArgsParam.ParameterType.IsAssignableTo(typeof(EventArgs)));

            return RuntimeBuilder.CreateDelegate<EventRaiser<TInstance, TEventArgs>>(
             $"{typeof(TInstance)}.{eventInfo.Name}.Raiser",
             method =>
             {
                 var emitter = method.Emitter;
                 emitter.LoadInstanceFor(backingField, method.Parameters[0], out int offset);
                 Debug.Assert(offset == 1);

                 // Check for null backing field
                 emitter.Ldfld(backingField)
                        .DefineLabel(out var lblEnd)
                        .Brfalse(lblEnd);

                 //Locals
                 emitter.DeclareLocal(typeof(Delegate[]), out var delegates)
                        .DeclareLocal<object>(out var sender)
                        .DeclareLocal<int>(out var i);
                 if (backingField.IsStatic)
                 {
                     emitter.LoadType(typeof(TInstance))
                            .Stloc(sender);
                 }
                 else
                 {
                     emitter.LoadInstanceFor(backingField, method.Parameters[0], out offset);
                     emitter.Stloc(sender);
                 }

                 // Load and store our Delegate[] into a local variables
                 emitter.LoadInstanceFor(backingField, method.Parameters[0], out offset)
                        .Ldfld(backingField)
                        .Cast(backingField.FieldType, typeof(MulticastDelegate))
                        .Call(MethodInfoCache.MulticastDelegate_GetInvocationList)
                        .Stloc(delegates)

                        // For loop
                        .Ldc_I4_0()
                        .Stloc(i)
                        .DefineLabel(out var lblWhile)
                        .Br(lblWhile)

                        // Start of loop
                        .DefineLabel(out var lblStart).MarkLabel(lblStart)
                        // Load Delegate[]
                        .Ldloc(delegates)
                        .Ldloc(i)
                        // Load Delegate[i]
                        .Ldelem_Ref()
                        // As the appropriate Delegate Type (which is basically EventHandler or EventHandler<T>)
                        .Castclass(backingField.FieldType)
                        // Sender
                        .Ldloc(sender)
                        // Args
                        .LoadAs(method.Parameters[1], eventArgsParam.ParameterType)
                        // Call
                        .Call(invokeMethod)

                        // i++
                        .Ldloc(i)
                        .Ldc_I4_1()
                        .Add()
                        .Stloc(i)

                        // While
                        .MarkLabel(lblWhile)
                        .Ldloc(i)
                        .Ldloc(delegates)
                        .Ldlen()
                        .Conv_I4()
                        .Blt(lblStart)

                        // End
                        .MarkLabel(lblEnd)
                        .Ret();
             });
        }
        else
        {
            raiser.TryAdapt<EventRaiser<TInstance, TEventArgs>>(out var del).ThrowIfFailed();
            return del!;
        }
    }

    public static EventDisposer<TInstance> CreateDisposer<TInstance>(this EventInfo eventInfo)
    {
        ArgumentNullException.ThrowIfNull(eventInfo);
        // We're just going to set the backing field to null
        var backingField = eventInfo.GetBackingField();
        if (backingField is null)
            throw new ReflectionException($"Unable to find {eventInfo}'s backing field for a Raiser");
        return RuntimeBuilder.CreateDelegate<EventDisposer<TInstance>>($"{typeof(TInstance)}.{eventInfo.Name}.Dispose",
                                                                       method =>
                                                                       {
                                                                           var emitter = method.Emitter;
                                                                           emitter.LoadInstanceFor(backingField, method.Parameters[0], out int offset);
                                                                           Debug.Assert(offset == 1);
                                                                               // Store null in the backing field
                                                                               emitter.Ldnull()
                                                                               .Stfld(backingField)
                                                                               .Ret();
                                                                       });
    }
    */
}