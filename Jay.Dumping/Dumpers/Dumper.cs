namespace Jay.Dumping;

/// <summary>
/// A shared base class for <see cref="Dumper{T}"/> instances
/// </summary>
public abstract class Dumper
{
    /// <summary>
    /// Can this <see cref="Dumper"/> dump values of the given <paramref name="type"/>?
    /// </summary>
    public abstract bool CanDump(Type type);

    internal abstract void DumpObject(ref DefStringHandler stringHandler, [NotNull] object obj, DumpFormat dumpFormat);
}