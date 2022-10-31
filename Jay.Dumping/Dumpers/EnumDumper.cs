using System.Collections.Concurrent;
using System.Reflection;
using Jay.Collections;
using Jay.Dumping.Interpolated;

namespace Jay.Dumping;

public sealed class EnumDumper : Dumper<Enum>, IDumper<Enum>, IDumper
{
    private static readonly ConcurrentTypeDictionary<ConcurrentDictionary<Enum, string>> _enumNames = new();
    
    private static ConcurrentDictionary<Enum, string> CreateEnumNames(Type enumType)
    {
        var enumNames = new ConcurrentDictionary<Enum, string>();
        var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var field in fields)
        {
            string name = field.Name;
            string? dump = field.GetCustomAttribute<DumpAsAttribute>()?.Dumped;
            Enum value = (field.GetValue(null) as Enum)!;
            enumNames.TryAdd(value, dump ?? name);
        }
        return enumNames;
    }

    public override bool CanDump(Type type) => type.IsEnum;

    protected override void DumpImpl(ref DumpStringHandler dumpHandler, [DisallowNull] Enum @enum, DumpFormat format)
    {
        var enumType = @enum.GetType();
        // We only want the type name, nothing else
        dumpHandler.Write(enumType.Name);
        dumpHandler.Write('.');
        var enumNames = _enumNames.GetOrAdd(enumType, CreateEnumNames);
        string name = enumNames.GetOrAdd(@enum, e => Enum.GetName(enumType, e) ?? e.ToString());
        dumpHandler.Write(name);
    }
}