using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Jay.Collections;

/// <summary>
/// A <see cref="ConcurrentDictionary{TKey,TValue}"/> of <see cref="Type"/> and <typeparamref name="TValue"/>
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class ConcurrentTypeDictionary<TValue> : ConcurrentDictionary<Type, TValue>
{
    public ConcurrentTypeDictionary()
        : base() { }
    public ConcurrentTypeDictionary(int capacity)
        : base(Environment.ProcessorCount, capacity) { }

    /// <summary>
    /// Determines whether the <see cref="ConcurrentTypeDictionary{TValue}"/> contains a key of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The key <see cref="Type"/> to check for</typeparam>
    /// <returns></returns>
    public bool ContainsKey<T>() => base.ContainsKey(typeof(T));

    public bool TryGetValue<T>([MaybeNullWhen(false)] out TValue value) => base.TryGetValue(typeof(T), out value);

    public TValue GetOrAdd<T>(TValue addValue)
    {
        return base.GetOrAdd(typeof(T), addValue);
    }
    public TValue GetOrAdd<T>(Func<Type, TValue> addValue)
    {
        return base.GetOrAdd(typeof(T), addValue);
    }

    public TValue AddOrUpdate<T>(TValue addValue, Func<TValue, TValue> updateValue)
    {
        return base.AddOrUpdate(typeof(T), addValue, (_, existing) => updateValue(existing));
    }
    public TValue AddOrUpdate<T>(TValue addValue, Func<Type, TValue, TValue> updateValue)
    {
        return base.AddOrUpdate(typeof(T), addValue, updateValue);
    }
    public TValue AddOrUpdate<T>(Func<Type, TValue> addValue, Func<TValue, TValue> updateValue)
    {
        return base.AddOrUpdate(typeof(T), addValue, (_, existing) => updateValue(existing));
    }
    public TValue AddOrUpdate<T>(Func<Type, TValue> addValue, Func<Type, TValue, TValue> updateValue)
    {
        return base.AddOrUpdate(typeof(T), addValue, updateValue);
    }

    public TValue Set<T>(TValue value)
    {
        return base.AddOrUpdate(typeof(T), value, (_, _) => value);
    }

    public TValue Set(Type type, TValue value)
    {
        return base.AddOrUpdate(type, value, (_, _) => value);
    }
}