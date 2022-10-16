namespace Jay.Reflection;

[Flags]
public enum NameMatchFlags
{
    Exact = 0,
    IgnoreCase = 1 << 0,
    BeginsWith = 1 << 1,
    EndsWith = 1 << 2,
    Contains = BeginsWith | EndsWith | 1 << 3,
}