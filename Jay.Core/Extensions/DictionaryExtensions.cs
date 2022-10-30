namespace Jay.Extensions;

public static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, 
        TKey key, TValue addValue) 
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var existingValue))
            return existingValue;
        dictionary[key] = addValue;
        return addValue;
    }
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, 
        TKey key, Func<TKey, TValue> addValue) 
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var existingValue))
            return existingValue;
        existingValue = addValue(key);
        dictionary[key] = existingValue;
        return existingValue;
    }

    public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, 
        TKey key, TValue addValue, Func<TValue, TValue> updateValue) 
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var existingValue))
        {
            var newValue = updateValue(existingValue);
            dictionary[key] = newValue;
            return newValue;
        }
        else
        {
            dictionary[key] = addValue;
            return addValue;
        }
    }
   
    public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, 
        TKey key, Func<TKey, TValue> addValue, Func<TValue, TValue> updateValue) 
        where TKey : notnull
    {
        TValue newValue;
        if (dictionary.TryGetValue(key, out var existingValue))
        {
            newValue = updateValue(existingValue);
        }
        else
        {
            newValue = addValue(key);
        }
        dictionary[key] = newValue;
        return newValue;
    }
    
    public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, 
        TKey key, Func<TValue> addValue, Func<TKey, TValue, TValue> updateValue) 
        where TKey : notnull
    {
        TValue newValue;
        if (dictionary.TryGetValue(key, out var existingValue))
        {
            newValue = updateValue(key, existingValue);
        }
        else
        {
            newValue = addValue();
        }
        dictionary[key] = newValue;
        return newValue;
    }
    
    public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, 
        TKey key, Func<TKey, TValue> addValue, Func<TKey, TValue, TValue> updateValue) 
        where TKey : notnull
    {
        TValue newValue;
        if (dictionary.TryGetValue(key, out var existingValue))
        {
            newValue = updateValue(key, existingValue);
        }
        else
        {
            newValue = addValue(key);
        }
        dictionary[key] = newValue;
        return newValue;
    }
}