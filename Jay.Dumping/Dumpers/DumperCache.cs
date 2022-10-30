using System.Collections.Concurrent;
using System.Reflection;
using Jay.Collections;
using Jay.Extensions;

namespace Jay.Dumping;

public static class DumperCache
{
    private static readonly List<IDumper> _dumpers;
    private static readonly ConcurrentTypeDictionary<IDumper> _typeDumperCache;
    private static readonly IDumper _defaultDumper;
    
    public static int KnownDumpers => _dumpers.Concat(_typeDumperCache.Values).Distinct().Count();

    static DumperCache()
    {
        // Find all public concrete instance IDumpers in all possible Assemblies
        _dumpers = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetExportedTypes())
            .Where(type => type.Implements<IDumper>() &&
                           type.IsPublic && !type.IsAbstract && !type.IsInterface)
            .OrderBy(type => type.GetCustomAttribute<DumpOptionsAttribute>()?.Priority ?? 0)
            .Select(dumperType => (Activator.CreateInstance(dumperType) as IDumper)!)
            .ToList();
        _typeDumperCache = new()
        {
            // Add our specifics
            [typeof(object)] = new ObjectDumper(),
        };

        // Did we find a higher priority Default dumper?
        _defaultDumper = _dumpers.FirstOrDefault(dumper 
                => dumper.GetType().GetCustomAttribute<DumpOptionsAttribute>()?.IsDefaultDumper == true,
            new DefaultDumper());
    }
    
    private static IDumper FindDumper(Type type)
    {
        foreach (var dumper in _dumpers)
        {
            if (dumper.CanDump(type))
                return dumper;
        }
        return _defaultDumper;
    }
    
    private static IDumper<T> FindDumper<T>()
    {
        var type = typeof(T);
        foreach (var dumper in _dumpers)
        {
            if (dumper.CanDump(type))
            {
                if (dumper is IDumper<T> tDumper) return tDumper;
                throw new InvalidOperationException();
            }
        }
        {
            if (_defaultDumper is IDumper<T> tDumper) return tDumper;
            throw new InvalidOperationException();
        }
    }
    
    public static IDumper GetDumper(Type type) => _typeDumperCache.GetOrAdd(type, t => FindDumper(t));

    public static IDumper<T> GetDumper<T>()
    {
        var dumper = _typeDumperCache.GetOrAdd(typeof(T), _ => FindDumper<T>());
        if (dumper is IDumper<T> tDumper) return tDumper;
        throw new InvalidOperationException();
    }
}