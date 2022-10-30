using System.Diagnostics;
using Jay.Dumping;
using Jay.Dumping.Extensions;

namespace Jayflect.Dumping;

public sealed class TypeDumper : Dumper<Type>
{
    // Simple type aliases
    private readonly List<(Type Type, string Dumped)> _typeDumpCache = new()
    {
        (typeof(bool), "bool"),
        (typeof(char), "char"),
        (typeof(sbyte), "sbyte"),
        (typeof(byte), "byte"),
        (typeof(short), "short"),
        (typeof(ushort), "ushort"),
        (typeof(int), "int"),
        (typeof(uint), "uint"),
        (typeof(long), "long"),
        (typeof(ulong), "ulong"),
        (typeof(float), "float"),
        (typeof(double), "double"),
        (typeof(decimal), "decimal"),
        (typeof(string), "string"),
        (typeof(object), "object"),
        (typeof(void), "void"),
    };

    protected override void DumpImpl(ref DumpStringHandler stringHandler, [NotNull] Type type, DumpFormat format)
    {
        Type? underType;

        // Enum is always just Name
        if (type.IsEnum)
        {
            stringHandler.Write(type.Name);
            return;
        }

        // Nullable<T>?
        underType = Nullable.GetUnderlyingType(type);
        if (underType is not null && format < DumpFormat.All)
        {
            // Shortcut to dumping base type, followed by ?
            DumpImpl(ref stringHandler, underType, format);
            stringHandler.Write('?');
            return;
        }

        // un-detailed fast cache check
        if (format < DumpFormat.All)
        {
            foreach (var pair in _typeDumpCache)
            {
                if (pair.Type == type)
                {
                    stringHandler.Write(pair.Dumped);
                    return;
                }
            }
        }

        // Shortcuts

        if (type.IsPointer)
        {
            // $"{type}*"
            underType = type.GetElementType();
            Debug.Assert(underType != null);
            DumpImpl(ref stringHandler, underType, format);
            stringHandler.Write('*');
            return;
        }

        if (type.IsByRef || type.IsByRefLike)
        {
            underType = type.GetElementType();
            Debug.Assert(underType != null);
            stringHandler.Write("ref ");
            DumpImpl(ref stringHandler, underType, format);
            return;
        }

        if (type.IsArray)
        {
            underType = type.GetElementType();
            Debug.Assert(underType != null);
            DumpImpl(ref stringHandler, underType, format);
            stringHandler.Write("[]");
            return;
        }

        // Nested Type?
        if (format > DumpFormat.View && (type.IsNested && !type.IsGenericParameter))
        {
            DumpImpl(ref stringHandler, type.DeclaringType!, format);
            stringHandler.Write('.');
        }

        // If non-generic
        if (!type.IsGenericType)
        {
            // Just write the type name and we're done
            stringHandler.Write(type.Name);
            return;
        }

        // Start processing type name
        ReadOnlySpan<char> typeName = type.Name;

        // I'm a parameter?
        if (type.IsGenericParameter)
        {
            var constraints = type.GetGenericParameterConstraints();
            if (constraints.Length > 0)
            {
                stringHandler.Write(" : ");
                Debugger.Break();
            }

            Debugger.Break();
        }

        // Add our types!
        var i = typeName.IndexOf('`');
        stringHandler.Write(i >= 0 ? typeName[..i] : typeName);
        stringHandler.Write('<');
        stringHandler.DumpDelimited<Type>(", ", type.GetGenericArguments(), format);
        stringHandler.Write('>');
    }
}