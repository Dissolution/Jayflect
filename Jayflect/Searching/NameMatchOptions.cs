namespace Jayflect.Searching;

[Flags]
public enum NameMatchOptions
{
    Exact = 0,
    IgnoreCase = 1 << 0,
    StartsWith = 1 << 1,
    EndsWith = 1 << 2,
    Contains = 1 << 3,
}