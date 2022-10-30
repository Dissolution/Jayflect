using Jayflect.Building.Adaption;

namespace Jayflect;

public delegate void SetValue<TInstance, in TValue>([Instance] ref TInstance instance, TValue value);

public delegate TValue GetValue<TInstance, out TValue>([Instance] ref TInstance instance);

public delegate void AddHandler<TInstance, in THandler>([Instance] ref TInstance instance, THandler eventHandler)
    where THandler : Delegate;

public delegate void RemoveHandler<TInstance, in THandler>([Instance] ref TInstance instance, THandler eventHandler)
    where THandler : Delegate;

public delegate void RaiseHandler<TInstance>([Instance] ref TInstance instance, params object?[] eventArgs);

public delegate TInstance Construct<out TInstance>(params object?[] args);
public delegate TInstance Construct<out TInstance, in T1>(T1 arg1);
public delegate TInstance Construct<out TInstance, in T1, in T2>(T1 arg1, T2 arg2);
public delegate TInstance Construct<out TInstance, in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
public delegate TInstance Construct<out TInstance, in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
public delegate TInstance Construct<out TInstance, in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

public delegate TReturn Invoke<TInstance, out TReturn>([Instance] ref TInstance instance, params object?[] args);
public delegate TReturn Invoke<TInstance, in T1, out TReturn>([Instance] ref TInstance instance, T1 arg1);
public delegate TReturn Invoke<TInstance, in T1, in T2, out TReturn>([Instance] ref TInstance instance, T1 arg1, T2 arg2);
public delegate TReturn Invoke<TInstance, in T1, in T2, in T3, out TReturn>([Instance] ref TInstance instance, T1 arg1, T2 arg2, T3 arg3);
public delegate TReturn Invoke<TInstance, in T1, in T2, in T3, in T4, out TReturn>([Instance] ref TInstance instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
public delegate TReturn Invoke<TInstance, in T1, in T2, in T3, in T4, in T5, out TReturn>([Instance] ref TInstance instance,T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);