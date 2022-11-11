namespace Jayflect;

[Flags]
public enum Visibility
{
    None = 0,
    Instance = 1 << 0,
    Static = 1 << 1,
    Private = 1 << 2,
    Protected = 1 << 3,
    Internal = 1 << 4,
    NonPublic = Private | Protected | Internal,
    Public = 1 << 5,
    Any = Instance | Static | Private | Protected | Internal | Public,
}

public static class VisibilityExtensions
{
    public static BindingFlags ToBindingFlags(this Visibility visibility)
    {
        BindingFlags bindingFlags = default;
        if (visibility.Has(Visibility.Instance))
            bindingFlags |= BindingFlags.Instance;
        if (visibility.Has(Visibility.Static))
            bindingFlags |= BindingFlags.Static;
        if (visibility.Has(Visibility.Public))
            bindingFlags |= BindingFlags.Public;
        if (visibility.Has(Visibility.NonPublic))
            bindingFlags |= BindingFlags.NonPublic;
        return bindingFlags;
    }

    public static Visibility ToVisibilityFlags(this BindingFlags bindingFlags)
    {
        Visibility visibility = default;
        if (bindingFlags.Has(BindingFlags.Instance))
            visibility |= Visibility.Instance;
        if (bindingFlags.Has(BindingFlags.Static))
            visibility |= Visibility.Static;
        if (bindingFlags.Has(BindingFlags.Public))
            visibility |= Visibility.Public;
        if (bindingFlags.Has(BindingFlags.NonPublic))
            visibility |= Visibility.NonPublic;
        return visibility;
    }
}