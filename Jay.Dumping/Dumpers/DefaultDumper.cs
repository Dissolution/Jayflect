using System.Reflection;
using Jay.Dumping.Extensions;

namespace Jay.Dumping;

internal sealed class DefaultDumper : IDumper<object>, IDumper
{
    void IDumper.DumpTo(ref DumpStringHandler dumpHandler, object? value, DumpFormat format) 
        => Dump<object>(ref dumpHandler, value, format);
    void IDumper<object>.DumpTo(ref DumpStringHandler stringHandler, object? value, DumpFormat format) 
        => Dump<object>(ref stringHandler, value, format);

    public static void Dump<T>(ref DumpStringHandler stringHandler, T? value, DumpFormat format = default)
    {
        if (Dumper.DumpedNull(ref stringHandler, value, format)) return;

        if (value is IDumpable)
        {
            ((IDumpable)value).DumpTo(ref stringHandler, format);
            return;
        }

        // Add more information for higher-level formats
        if (format >= DumpFormat.Inspect)
        {
            var valueType = value.GetType();
            
            stringHandler.Write("(");
            stringHandler.Dump(valueType, format);
            stringHandler.Write(")");

            IReadOnlyList<MemberInfo> members;

            // Which members are we including?
            if (format == DumpFormat.Inspect)
            {
                members = valueType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            }
            else
            {
                members = valueType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            
            // None to write?
            if (members.Count == 0)
            {
                // Just write the value
                stringHandler.Write(" ");
                stringHandler.AppendFormatted<T>(value, format.GetCustomFormatString());
                return;
            }
            
            // Write out members
            stringHandler.Write(" {");

            foreach (var member in members)
            {
                stringHandler.Write(Environment.NewLine);
                stringHandler.Write('\t');
                stringHandler.Dump(member);
                stringHandler.Write(": ");
                object? memberValue;
                if (member is PropertyInfo property)
                    memberValue = property.GetValue(value);
                else if (member is FieldInfo field)
                    memberValue = field.GetValue(value);
                else
                    memberValue = "???";
                stringHandler.Dump(memberValue, format);
            }
            stringHandler.Write(Environment.NewLine);
            stringHandler.Write('}');
        }
        else
        {
            // Do not call Dump further, at this point we're delegating behavior to DefaultInterpolatedStringHandler
            stringHandler.Write<T>(value, format.GetCustomFormatString());
        }
    }
}