using System.Reflection;
using Jay.Dumping.Interpolated;

namespace Jay.Dumping;

// This should never be added through scan, but if it does, it is the last ever added
[DumpOptions(Priority = int.MaxValue)]
internal sealed class DefaultDumper : Dumper<object>
{
    public override bool CanDump(Type type)
    {
        return true;
    }

    protected override void DumpImpl(ref DumpStringHandler dumpHandler, object value, DumpFormat format)
    {
        DumpTo<object>(ref dumpHandler, value, format);
    }

    public override void DumpObjTo(ref DumpStringHandler dumpHandler, object? obj, DumpFormat format = default)
    {
        DumpTo<object>(ref dumpHandler, obj, format);
    }
    public override void DumpTo(ref DumpStringHandler dumpHandler, object? value, DumpFormat format = default)
    {
        DumpTo<object>(ref dumpHandler, value, format);
    }

    public static void DumpTo<T>(ref DumpStringHandler dumpHandler, T? value, DumpFormat format = default)
    {
        if (DumpedNull(ref dumpHandler, value, format)) return;

        var valueType = value.GetType();
        
        if (valueType.IsEnum)
        {
           Debugger.Break();
        }
        
        if (value is IDumpable)
        {
            ((IDumpable)value).DumpTo(ref dumpHandler, format);
            return;
        }

        // Add more information for higher-level formats
        if (format.IsWithType)
        {
            dumpHandler.Write("(");
            dumpHandler.Dump(valueType, format);
            dumpHandler.Write(")");

            var properties = valueType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            // None to write?
            if (properties.Length == 0)
            {
                // Just write the value
                dumpHandler.Write(" ");
                dumpHandler.Write<T>(value);
                return;
            }
            
            // Write out members
            dumpHandler.Write(" {");

            foreach (var property in properties)
            {
                dumpHandler.Write(Environment.NewLine);
                dumpHandler.Write('\t');
                dumpHandler.Dump(property);
                dumpHandler.Write(": ");
                dumpHandler.Dump(property.GetValue(value), format);
            }
            dumpHandler.Write(Environment.NewLine);
            dumpHandler.Write('}');
        }
        else
        {
            // Do not call Dump further, at this point we're delegating behavior to DefaultInterpolatedStringHandler
            dumpHandler.Write<T>(value, format.GetCustomFormatString());
        }
    }
}