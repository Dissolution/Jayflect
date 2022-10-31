using System.Diagnostics;
using Jay.Dumping;
using Jay.Dumping.Interpolated;

namespace Jayflect.Dumping;

public sealed class TypeDumper : Dumper<Type>
{
    // Simple type aliases
    private readonly List<(Type Type, string DumpString)> _typeDumpCache = new()
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

    private void DumpType(ref DumpStringHandler dumpHandler, Type type)
    {
         Type? underType;

        // Enum is always just Name
        if (type.IsEnum)
        {
            dumpHandler.Write(type.Name);
            return;
        }

        // Nullable<T>?
        underType = Nullable.GetUnderlyingType(type);
        if (underType is not null)
        {
            // Dump base type followed by question mark
            DumpType(ref dumpHandler, underType);
            dumpHandler.Write('?');
            return;
        }

        // un-detailed fast cache check
        foreach (var pair in _typeDumpCache)
        {
            if (pair.Type == type)
            {
                dumpHandler.Write(pair.DumpString);
                return;
            }
        }

        // Shortcuts

        if (type.IsPointer)
        {
            // $"{type}*"
            underType = type.GetElementType();
            Debug.Assert(underType != null);
            DumpType(ref dumpHandler, underType);
            dumpHandler.Write('*');
            return;
        }

        if (type.IsByRef)
        {
            underType = type.GetElementType();
            Debug.Assert(underType != null);
            dumpHandler.Write("ref ");
            DumpType(ref dumpHandler, underType);
            return;
        }

        if (type.IsArray)
        {
            underType = type.GetElementType();
            Debug.Assert(underType != null);
            DumpType(ref dumpHandler, underType);
            dumpHandler.Write("[]");
            return;
        }

        // Nested Type?
        if (type.IsNested && !type.IsGenericParameter)
        {
            DumpType(ref dumpHandler, type.DeclaringType!);
            dumpHandler.Write('.');
        }

        // If non-generic
        if (!type.IsGenericType)
        {
            // Just write the type name and we're done
            dumpHandler.Write(type.Name);
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
                dumpHandler.Write(" : ");
                Debugger.Break();
            }

            Debugger.Break();
        }
        
        // Name is often something like NAME`2 for NAME<,>, so we want to strip that off
        var i = typeName.IndexOf('`');
        dumpHandler.Write(i >= 0 ? typeName[..i] : typeName);
        
        // Add our generic types
        dumpHandler.Write('<');
        var argTypes = type.GetGenericArguments();
        for (i = 0; i < argTypes.Length; i++)
        {
            if (i > 0) dumpHandler.Write(", ");
            DumpType(ref dumpHandler, argTypes[i]);
        }
        dumpHandler.Write('>');
    }
    
    protected override void DumpImpl(ref DumpStringHandler stringHandler, [DisallowNull] Type type, DumpFormat format)
    {
        if (format.IsWithType)
        {
            stringHandler.Write("typeof(");
        }

        DumpType(ref stringHandler, type);
        
        if (format.IsWithType)
        {
            stringHandler.Write(")");
        }
    }
}