namespace Jayflect;

public static partial class Reflect
{
    public static class Flags
    {
        public const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic |
                                        BindingFlags.Static | BindingFlags.Instance |
                                        BindingFlags.IgnoreCase;

        public const BindingFlags Public = BindingFlags.Public |
                                           BindingFlags.Static | BindingFlags.Instance |
                                           BindingFlags.IgnoreCase;

        public const BindingFlags NonPublic = BindingFlags.NonPublic |
                                              BindingFlags.Static | BindingFlags.Instance |
                                              BindingFlags.IgnoreCase;

        public const BindingFlags Static = BindingFlags.Public | BindingFlags.NonPublic |
                                           BindingFlags.Static |
                                           BindingFlags.IgnoreCase;

        public const BindingFlags Instance = BindingFlags.Public | BindingFlags.NonPublic |
                                             BindingFlags.Instance |
                                             BindingFlags.IgnoreCase;

        public const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;

        public const BindingFlags NonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.IgnoreCase;

        public const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        public const BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase;
    }

    private static HashSet<Type>? _allTypes;

    public static IReadOnlySet<Type> AllExportedTypes
    {
        get
        {
            if (_allTypes is null)
            {
                var allTypes = new HashSet<Type>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        foreach (var type in assembly.GetExportedTypes())
                        {
                            allTypes.Add(type);
                        }
                    }
                    catch
                    {
                        // Ignore this assembly
                    }
                }
                _allTypes = allTypes;
            }
            return _allTypes;
        }
    }
}