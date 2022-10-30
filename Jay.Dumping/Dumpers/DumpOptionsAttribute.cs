namespace Jay.Dumping;

/// <summary>
/// Adjusts the priority for a concrete <see cref="IDumper"/> instance to be matched,
/// with a lower priority matching before a higher.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DumpOptionsAttribute : Attribute
{
    public int Priority { get; init; } = 0;

    public bool IsDefaultDumper { get; init; } = false;
    
    public DumpOptionsAttribute()
    {
        
    }
    
    public DumpOptionsAttribute(int priority, bool isDefaultDumper = false)
    {
        this.Priority = priority;
        this.IsDefaultDumper = isDefaultDumper;
    }
}