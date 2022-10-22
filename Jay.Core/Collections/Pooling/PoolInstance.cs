namespace Jay.Collections.Pooling;

internal class PoolInstance<T> : IPoolInstance<T>
    where T : class
{
    protected readonly IObjectPool<T> _pool;
    protected T? _instance;

    public T Instance => _instance ?? throw new ObjectDisposedException(GetType().Name);

    public PoolInstance(IObjectPool<T> pool, T instance)
    {
        _pool = pool;
        _instance = instance;
    }

    public void Dispose()
    {
        T? instance = Interlocked.Exchange(ref _instance, null);
        _pool.Return(instance);
    }

    public override bool Equals(object? obj) => throw new InvalidOperationException();

    public override int GetHashCode() => throw new InvalidOperationException();

    public override string ToString() => $"Returns {_instance} back to its origin ObjectPool";
}