/*using System.Reflection;
using System.Reflection.Emit;
using Jay.Validation;

namespace Jay.Reflection.Implementation;

public abstract class MemberImplementer
{
    protected readonly TypeBuilder _typeBuilder;
    protected readonly MemberBuilders _memberBuilders;

    protected MemberImplementer(TypeBuilder typeBuilder, MemberBuilders memberBuilders)
    {
        _typeBuilder = typeBuilder;
        _memberBuilders = memberBuilders;
    }
}

public abstract class MemberImplementer<TMember> : MemberImplementer
    where TMember : MemberInfo
{
    /// <inheritdoc />
    protected MemberImplementer(TypeBuilder typeBuilder, MemberBuilders memberBuilders) 
        : base(typeBuilder, memberBuilders)
    {
        
    }

    public abstract void Implement(TMember interfaceMember);
}

/// <summary>
/// 
/// </summary>
/// <see cref="https://stackoverflow.com/a/40116709"/>
public class EventImplementer : MemberImplementer<EventInfo>
{
    private static readonly MethodInfo _delegateCombineMethod;
    private static readonly MethodInfo _delegateRemoveMethod;

    static EventImplementer()
    {
        _delegateCombineMethod = typeof(Delegate)
            .GetMethod(nameof(Delegate.Combine),
                BindingFlags.Public | BindingFlags.Static,
                new Type[2] { typeof(Delegate), typeof(Delegate) })
            .ThrowIfNull("Could not find Delegate.Combine(Delegate,Delegate)");
        _delegateRemoveMethod = typeof(Delegate)
            .GetMethod(nameof(Delegate.Remove),
                BindingFlags.Public | BindingFlags.Static,
                new Type[2] { typeof(Delegate), typeof(Delegate) })
            .ThrowIfNull("Could not find Delegate.Remove(Delegate,Delegate)");
    }
    
    /// <inheritdoc />
    public EventImplementer(TypeBuilder typeBuilder, MemberBuilders memberBuilders) : base(typeBuilder, memberBuilders)
    {
    }

    /// <inheritdoc />
    public override void Implement(EventInfo interfaceEvent)
    {
        if (interfaceEvent.EventHandlerType is null)
            throw new ArgumentException("The given interface event doesn't have an event handler type", nameof(interfaceEvent));
        var eventName = interfaceEvent.Name;
        var eventHandlerType = interfaceEvent.EventHandlerType;
        // Backing field
        var fieldBuilder = _typeBuilder.DefineField(eventName, 
            eventHandlerType, 
            FieldAttributes.Private);
        
        // Event
        var eventInfo = _typeBuilder.DefineEvent(eventName,
            EventAttributes.None,
            eventHandlerType);
        // Event Add
        var eventAddMethodBuilder = _typeBuilder.DefineMethod($"add_{eventName}",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final |
                MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                CallingConventions.HasThis,
                typeof(void),
                new Type[1] { eventHandlerType })
            .Emit(emitter => emitter
                .Ldarg_0()
                .Ldarg_0()
                .Ldfld(fieldBuilder)
                .Ldarg_1()
                .Call(_delegateCombineMethod)
                .Castclass(eventHandlerType)
                .Stfld(fieldBuilder)
                .Ret());
        eventInfo.SetAddOnMethod(eventAddMethodBuilder);
        // Event Remove
        var eventRemoveMethodBuilder = _typeBuilder.DefineMethod($"remove_{eventName}",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final |
                MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                CallingConventions.HasThis,
                typeof(void),
                new Type[1] { eventHandlerType })
            .Emit(emitter => emitter
                .Ldarg_0()
                .Ldarg_0()
                .Ldfld(fieldBuilder)
                .Ldarg_1()
                .Call(_delegateRemoveMethod)
                .Castclass(eventHandlerType)
                .Stfld(fieldBuilder)
                .Ret());
        eventInfo.SetRemoveOnMethod(eventRemoveMethodBuilder);
        // Event Raise !
        var invokeMethod = interfaceEvent.EventHandlerType.GetInvokeMethod();
        if (invokeMethod is null)
            throw new ArgumentException("Interface Event definition is malformed", nameof(interfaceEvent));
        var raiseParamTypes = invokeMethod.GetParameterTypes();
        var eventRaiseMethodBuilder = _typeBuilder.DefineMethod($"raise_{eventName}",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final |
                MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                CallingConventions.HasThis,
                typeof(void),
                raiseParamTypes)
            .Emit(emitter =>
                {
                    /* roughly:
                     * void raise_EventName(T0? arg0, ..., TN? argN)
                     * {
                     *    var copy = _eventField;
                     *    if (copy is null) return;
                     *    copy.Invoke(arg0, ..., argN)
                     * }
                     * #1#
                    emitter.DeclareLocal(eventHandlerType, out var copy)
                        .Ldarg(0)
                        .Ldfld(fieldBuilder)
                        .Stloc(copy)
                        .Ldloc(copy)
                        .Brfalse(out var lblRet)
                        .Ldloc(copy);
                    // Load the args, offset by 1 because HasThis
                    int i = 0;
                    while (i < raiseParamTypes.Length)
                    {
                        i++;
                        emitter.Ldarg(i);
                    }
                    // Invoke the delegate
                    emitter.Call(invokeMethod)
                        // Done
                        .MarkLabel(lblRet)
                        .Ret();
                }
            );
    }
}*/