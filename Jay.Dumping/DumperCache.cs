using System.Reflection;
using Jay.Collections;
using Jay.Dumping.Extensions;
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
            {
                return dumper;
            }
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
                // Why isn't it?
                if (type.IsValueType)
                {
                    return new StructInterfaceDumper<T>(dumper);
                }
                throw new InvalidOperationException();
            }
        }
        // Can we use default?
        {
            if (_defaultDumper is IDumper<T> tDumper) return tDumper;
            // Why not?
            var t = type.Dump();
            Debugger.Break();
            throw new InvalidOperationException();
        }
    }
    
    public static IDumper GetDumper(Type type)
    {
        return _typeDumperCache.GetOrAdd(type, FindDumper);
    }

    public static IDumper<T> GetDumper<T>()
    {
        IDumper iDumper = _typeDumperCache.GetOrAdd(typeof(T), _ => FindDumper<T>());
        if (iDumper is IDumper<T> tDumper) return tDumper;
        Debugger.Break();
        throw new InvalidOperationException();
    }
}