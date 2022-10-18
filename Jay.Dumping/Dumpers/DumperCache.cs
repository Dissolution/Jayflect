using System.Collections.Concurrent;
using System.Reflection;
using Jay.Extensions;

namespace Jay.Dumping;

public static class DumperCache
{
    private static readonly List<Dumper> _dumpers;
    private static readonly ConcurrentDictionary<Type, Dumper> _typeDumperCache;

    public static int KnownDumpers => _dumpers.Concat(_typeDumperCache.Values).Distinct().Count();

    static DumperCache()
    {
        _dumpers = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetExportedTypes())
            .Where(type => type.Implements<Dumper>() && !type.IsAbstract)
            .Select(dumperType => (Activator.CreateInstance(dumperType) as Dumper)!)
            .ToList();
        _typeDumperCache = new()
        {
            // Add our specifics
            [typeof(object)] = new ObjectDumper(),
        };
    }
    
    private static Dumper FindDumper(Type type)
    {
        foreach (var dumper in _dumpers)
        {
            if (dumper.CanDump(type))
                return dumper;
        }
        return (typeof(DefaultDumper<>).MakeGenericType(type)
            .GetProperty(nameof(DefaultDumper<object?>.Instance), BindingFlags.Public | BindingFlags.Static)!
            .GetValue(null) as Dumper)!;
    }
    
    private static Dumper<T> FindDumper<T>(Type type)
    {
        foreach (var dumper in _dumpers)
        {
            if (dumper.CanDump(type))
                return (dumper as Dumper<T>)!;
        }
        return DefaultDumper<T>.Instance;
    }
    
    public static Dumper GetDumper(Type type) => _typeDumperCache.GetOrAdd(type, t => FindDumper(t));

    public static Dumper<T> GetDumper<T>() => (_typeDumperCache.GetOrAdd(typeof(T), type => FindDumper<T>(type)) as Dumper<T>)!;
}