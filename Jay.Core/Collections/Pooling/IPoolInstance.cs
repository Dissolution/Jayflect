namespace Jay.Collections.Pooling;

/// <summary>
/// An <see cref="IDisposable"/> that returns a <typeparamref name="T"/> instance value to its source <see cref="IObjectPool{T}"/> when it is disposed.
/// </summary>
public interface IPoolInstance<out T> : IDisposable
    where T : class
{
    /// <summary>
    /// Gets the temporary instance value
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if this <see cref="IPoolInstance{T}"/> has been disposed
    /// </exception>
    T Instance { get; }
}