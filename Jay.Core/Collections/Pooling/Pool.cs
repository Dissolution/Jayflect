using Jay.Utilities;

namespace Jay.Collections.Pooling;

public static class Pool
{
    /// <summary>
    /// The default capacity any pool should start with
    /// </summary>
    internal static readonly int DefaultCapacity = Environment.ProcessorCount * 2;
    
    /// <summary>
    /// The maximum capacity for any pool
    /// </summary>
    public static readonly int MaxCapacity = Array.MaxLength;

    /// <summary>
    /// Creates a new <see cref="ObjectPool{T}"/> for classes with default constructors.
    /// </summary>
    /// <typeparam name="T">A class with a default constructor.</typeparam>
    /// <param name="clean">An optional action to perform on a <typeparamref name="T"/> when it is returned.</param>
    /// <param name="dispose">An optional action to perform on a <typeparamref name="T"/> if it is disposed.</param>
    /// <returns>A new <see cref="ObjectPool{T}"/> instance.</returns>
    public static IObjectPool<T> Create<T>(Action<T>? clean = null, 
                                          Action<T>? dispose = null,
                                          Constraints.IsNew<T> _ = default)
        where T : class, new()
    {
        return new ObjectPool<T>(() => new T(), clean, dispose);
    }

    /// <summary>
    /// Creates a new <see cref="ObjectPool{T}"/> for <see cref="IDisposable"/> classes.
    /// </summary>
    /// <typeparam name="T">An <see cref="IDisposable"/> class.</typeparam>
    /// <param name="factory">A function to create a new <typeparamref name="T"/> instance.</param>
    /// <param name="clean">An optional action to perform on a <typeparamref name="T"/> when it is returned.</param>
    /// <returns>A new <see cref="ObjectPool{T}"/> instance.</returns>
    public static IObjectPool<T> Create<T>(Func<T> factory,
                                          Action<T>? clean = null,
                                          Constraints.IsDisposable<T> _ = default)
        where T : class, IDisposable
    {
        return new ObjectPool<T>(factory, clean, t => t.Dispose());
    }

    /// <summary>
    /// Creates a new <see cref="ObjectPool{T}"/> for <see cref="IDisposable"/> classes with a default constructor.
    /// </summary>
    /// <typeparam name="T">An <see cref="IDisposable"/> class with a default constructor.</typeparam>
    /// <param name="clean">An optional action to perform on a <typeparamref name="T"/> when it is returned.</param>
    /// <returns>A new <see cref="ObjectPool{T}"/> instance.</returns>
    public static IObjectPool<T> Create<T>(Action<T>? clean = null,
                                          Constraints.IsNewDisposable<T> _ = default)
        where T : class, IDisposable, new()
    {
        return new ObjectPool<T>(() => new T(), clean, t => t.Dispose());
    }

    /// <summary>
    /// Creates a new <see cref="ObjectPool{T}"/> for classes.
    /// </summary>
    /// <typeparam name="T">A class.</typeparam>
    /// <param name="factory">A function to create a new <typeparamref name="T"/> instance.</param>
    /// <param name="clean">An optional action to perform on a <typeparamref name="T"/> when it is returned.</param>
    /// <param name="dispose">An optional action to perform on a <typeparamref name="T"/> if it is disposed.</param>
    /// <returns>A new <see cref="ObjectPool{T}"/> instance.</returns>
    public static IObjectPool<T> Create<T>(Func<T> factory,
                                          Action<T>? clean = null,
                                          Action<T>? dispose = null)
        where T : class
    {
        return new ObjectPool<T>(factory, clean, dispose);
    }
}