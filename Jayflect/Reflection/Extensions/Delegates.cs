namespace Jay.Reflection;

public delegate TValue Getter<TInstance, out TValue>(ref TInstance instance);
public delegate void Setter<TInstance, in TValue>(ref TInstance instance, TValue value);

public delegate void EventAdder<TInstance, in THandler>(ref TInstance instance, THandler handler) where THandler : Delegate;
public delegate void EventRemover<TInstance, in THandler>(ref TInstance instance, THandler handler) where THandler : Delegate;
public delegate void EventRaiser<TInstance, in TEventArgs>(ref TInstance instance, TEventArgs args) where TEventArgs : EventArgs;
public delegate void EventDisposer<TInstance>(ref TInstance instance);

public delegate TInstance Constructor<out TInstance>(params object?[] args);
public delegate TReturn Invoker<TInstance, out TReturn>(ref TInstance instance, params object?[] args);
