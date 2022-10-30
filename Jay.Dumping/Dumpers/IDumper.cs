namespace Jay.Dumping;

public interface IDumper
{
    /// <summary>
    /// Can this <see cref="IDumper"/> instance dump values of the given <paramref name="type"/>?
    /// </summary>
    bool CanDump(Type type);
    
    /// <summary>
    /// Dumps an <see cref="object"/> to a <paramref name="dumpHandler"/> with optional <see cref="format"/>
    /// </summary>
    /// <exception cref=""></exception>
    void DumpTo(ref DumpStringHandler dumpHandler, object? obj, DumpFormat format = default);
}