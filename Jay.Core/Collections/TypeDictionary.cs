using System.Diagnostics.CodeAnalysis;

namespace Jay.Collections;

/// <summary>
/// A <see cref="Dictionary{TKey,TValue}"/> of <see cref="Type"/> and <typeparamref name="TValue"/>
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class TypeDictionary<TValue> : Dictionary<Type, TValue>
{
    public TypeDictionary()
        : base() { }
    public TypeDictionary(int capacity)
        : base(capacity) { }

    /// <summary>
    /// Determines whether the <see cref="ConcurrentTypeDictionary{TValue}"/> contains a key of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The key <see cref="Type"/> to check for</typeparam>
    /// <returns></returns>
    public bool ContainsKey<T>() => base.ContainsKey(typeof(T));

    public bool TryGetValue<T>([MaybeNullWhen(false)] out TValue value) => base.TryGetValue(typeof(T), out value);

    public TValue GetOrAdd<T>(TValue addValue)
    {
        if (TryGetValue<T>(out var existingValue))
            return existingValue;
        Add(typeof(T), addValue);
        return addValue;
    }
    public TValue GetOrAdd<T>(Func<Type, TValue> addValue)
    {
        if (TryGetValue<T>(out var existingValue))
            return existingValue;
        var newValue = addValue(typeof(T));
        Add(typeof(T), newValue);
        return newValue;
    }

    public TValue AddOrUpdate<T>(TValue addValue, Func<TValue, TValue> updateValue)
    {
        if (TryGetValue<T>(out var existingValue))
        {
            var newValue = updateValue(existingValue);
            Set<T>(newValue);
            return newValue;
        }
        else
        {
            Add(typeof(T), addValue);
            return addValue;
        }
    }
    public TValue AddOrUpdate<T>(TValue addValue, Func<Type, TValue, TValue> updateValue)
    {
        if (TryGetValue<T>(out var existingValue))
        {
            var newValue = updateValue(typeof(T), existingValue);
            Set<T>(newValue);
            return newValue;
        }
        else
        {
            Add(typeof(T), addValue);
            return addValue;
        }
    }
    public TValue AddOrUpdate<T>(Func<Type, TValue> addValue, Func<TValue, TValue> updateValue)
    {
        TValue newValue;
        if (TryGetValue<T>(out var existingValue))
        {
            newValue = updateValue(existingValue);
            Set<T>(newValue);
        }
        else
        {
            newValue = addValue(typeof(T));
            Add(typeof(T), newValue);
        }
        return newValue;
    }
    public TValue AddOrUpdate<T>(Func<Type, TValue> addValue, Func<Type, TValue, TValue> updateValue)
    {
        TValue newValue;
        if (TryGetValue<T>(out var existingValue))
        {
            newValue = updateValue(typeof(T), existingValue);
            Set<T>(newValue);
        }
        else
        {
            newValue = addValue(typeof(T));
            Add(typeof(T), newValue);
        }
        return newValue;
    }

    public TValue Set<T>(TValue value)
    {
        this[typeof(T)] = value;
        return value;
    }

    public TValue Set(Type type, TValue value)
    {
        this[type] = value;
        return value;
    }
}