namespace Jay.Collections.Pooling;

/// <summary>
/// A pool of <typeparamref name="T"/> instances
/// </summary>
/// <typeparam name="T">An instance <c>class</c></typeparam>
public interface IObjectPool<T> : IDisposable
    where T : class
{
    /// <summary>
    /// Gets the maximum number of items stored by this pool.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Gets the current number of items being stored in this pool.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Borrows a <typeparamref name="T"/> instance that should be <see cref="Return"/>ed
    /// </summary>
    T Borrow();

    /// <summary>
    /// Returns a <see cref="Borrow()"/>ed <typeparamref name="T"/> instance to the pool to be cleaned and re-used.
    /// </summary>
    void Return(T? instance);

    /// <summary>
    /// Borrows a <typeparamref name="T"/> <paramref name="instance"/>
    /// that will be returned when the returned <see cref="IDisposable"/> is disposed.
    /// </summary>
    /// <param name="instance">A <typeparamref name="T"/> instance,
    /// it will be returned to its origin <see cref="IObjectPool{T}"/> when disposed.</param>
    /// <returns>An <see cref="IDisposable"/> that will return the <paramref name="instance"/>.</returns>
    /// <remarks>
    /// <paramref name="instance"/> must not be used after this is disposed.
    /// </remarks>
    IPoolInstance<T> Borrow(out T instance)
    {
        instance = Borrow();
        return new PoolInstance<T>(this, instance);
    }

    /// <summary>
    /// Borrows a <typeparamref name="T"/> instance, performs <paramref name="instanceAction"/> on it, and returns it to this pool
    /// </summary>
    /// <param name="instanceAction">
    /// The <see cref="Action{T}"/> to perform on a temporary <typeparamref name="T"/> instance
    /// </param>
    void Borrow(Action<T> instanceAction)
    {
        var instance = Borrow();
        try
        {
            instanceAction(instance);
        }
        finally
        {
            Return(instance);
        }
    }

    /// <summary>
    /// Executes the given <paramref name="instanceFunc"/> on a borrowed <typeparamref name="T"/> instance and returns its <typeparamref name="TResult"/>
    /// </summary>
    /// <typeparam name="TReturn">The type of value returned from <paramref name="instanceFunc"/></typeparam>
    /// <param name="instanceFunc">The function to execute on a <typeparamref name="T"/> instance</param>
    /// <returns>The return value from <paramref name="instanceFunc"/></returns>
    TReturn Borrow<TReturn>(Func<T, TReturn> instanceFunc)
    {
        var instance = Borrow();
        try
        {
            return instanceFunc(instance);
        }
        finally
        {
            Return(instance);
        }
    }
}