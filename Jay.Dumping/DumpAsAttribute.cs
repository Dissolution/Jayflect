namespace Jay.Dumping;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public sealed class DumpAsAttribute : Attribute
{
    public string? Dumped { get; }

    public DumpAsAttribute(string? dumped = null)
    {
        this.Dumped = dumped;
    }
}