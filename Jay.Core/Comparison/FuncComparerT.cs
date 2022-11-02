namespace Jay.Comparison;

public sealed class FuncComparer<T> : Comparer<T>
{
    private readonly Func<T?, T?, int> _compare;

    public FuncComparer(Func<T?, T?, int> compare)
    {
        _compare = compare;
    }
    
    public override int Compare(T? x, T? y) => _compare(x, y);
}