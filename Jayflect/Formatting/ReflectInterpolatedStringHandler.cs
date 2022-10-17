using System.Collections;
using System.Diagnostics;
using Jayflect.Extensions;

namespace Jayflect.Formatting;

internal static class ReflectFormatter
{
    private static readonly List<(Type Type, string Formatted)> _formattedTypeCache = new()
    {
        { (typeof(bool), "bool") },
        { (typeof(char), "char") },
        { (typeof(sbyte), "sbyte") },
        { (typeof(byte), "byte") },
        { (typeof(short), "short") },
        { (typeof(ushort), "ushort") },
        { (typeof(int), "int") },
        { (typeof(uint), "uint") },
        { (typeof(long), "long") },
        { (typeof(ulong), "ulong") },
        { (typeof(float), "float") },
        { (typeof(double), "double") },
        { (typeof(decimal), "decimal") },
        { (typeof(string), "string") },
        { (typeof(object), "object") },
        { (typeof(void), "void") },
    };

    /// <summary>
    /// 
    /// </summary>
    private enum Format
    {
        // DECLARATION ORDER MATTERS!!!
        None,
        Detailed,
        Reflect,
    }

    private static Format GetFormat(ReadOnlySpan<char> format)
    {
        if (format.Length == 0) return Format.None;
        if (format[0] is 'd' or 'D') return Format.Detailed;
        if (format[0] is 'r' or 'R') return Format.Reflect;
        throw new ArgumentException($"Invalid Format argument '{format}'", nameof(format));
    }

    private static bool WroteNull<T>(ref DefaultInterpolatedStringHandler stringHandler, [NotNullWhen(false)] T? value, ReadOnlySpan<char> format)
    {
        if (value is null)
        {
            if (GetFormat(format) >= Format.Detailed)
            {
                stringHandler.Write(' ');
                stringHandler.WriteType(typeof(T), format);
                stringHandler.Write(' ');
            }
            stringHandler.Write("null");
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Write(this ref DefaultInterpolatedStringHandler stringHandler, char ch)
    {
        stringHandler.AppendFormatted(new ReadOnlySpan<char>(in ch));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Write(this ref DefaultInterpolatedStringHandler stringHandler, string? text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            stringHandler.AppendLiteral(text);
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Write(this ref DefaultInterpolatedStringHandler stringHandler, ReadOnlySpan<char> text)
    {
        stringHandler.AppendFormatted(text);
    }

    private static void WriteType(this ref DefaultInterpolatedStringHandler stringHandler, Type? type, ReadOnlySpan<char> format = default)
    {
        if (WroteNull(ref stringHandler, type, format)) return;

        bool detailed = format.Length > 0 && format[0] is 'D' or 'd';
        Type? underType;

        // Enum is always just Name
        if (type.IsEnum)
        {
            stringHandler.Write(type.Name);
            return;
        }

        // un-detailed fast cache check
        if (!detailed)
        {
            foreach (var pair in _formattedTypeCache)
            {
                if (pair.Type == type)
                {
                    stringHandler.Write(pair.Formatted);
                    return;
                }
            }

            // Nullable<T>?
            underType = Nullable.GetUnderlyingType(type);
            if (underType is not null)
            {
                // Shortcut to dumping base type, followed by ?
                stringHandler.WriteType(underType, format);
                stringHandler.Write('?');
                return;
            }
        }

        // Shortcuts

        if (type.IsPointer)
        {
            // $"{type}*"
            underType = type.GetElementType();
            Debug.Assert(underType != null);
            stringHandler.WriteType(underType, format);
            stringHandler.Write('*');
            return;
        }

        if (type.IsByRef || type.IsByRefLike)
        {
            underType = type.GetElementType();
            Debug.Assert(underType != null);
            stringHandler.Write("ref ");
            stringHandler.WriteType(underType, format);
            return;
        }

        if (type.IsArray)
        {
            underType = type.GetElementType();
            Debug.Assert(underType != null);
            stringHandler.WriteType(underType, format);
            stringHandler.Write("[]");
            return;
        }

        // Nested Type?
        if (detailed && (type.IsNested && !type.IsGenericParameter))
        {
            stringHandler.WriteType(type.DeclaringType, format);
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
        var genericTypes = type.GetGenericArguments();
        var i = typeName.IndexOf('`');
        stringHandler.Write(i >= 0 ? typeName[..i] : typeName);
        stringHandler.Write('<');
        for (i = 0; i < genericTypes.Length; i++)
        {
            if (i > 0) stringHandler.Write('<');
            stringHandler.WriteType(genericTypes[i], format);
        }
        stringHandler.Write('>');
    }

    private static void WriteField(this ref DefaultInterpolatedStringHandler stringHandler, FieldInfo? field, ReadOnlySpan<char> format = default)
    {
        if (WroteNull(ref stringHandler, field, format)) return;
        stringHandler.WriteType(field.FieldType, format);
        stringHandler.Write(' ');
        if (GetFormat(format) >= Format.Detailed)
        {
            stringHandler.WriteType(field.OwnerType(), format);
            stringHandler.Write('.');
        }
        stringHandler.Write(field.Name);
    }

    private static void WriteProperty(this ref DefaultInterpolatedStringHandler stringHandler, PropertyInfo? property,
        ReadOnlySpan<char> format = default)
    {
        if (WroteNull(ref stringHandler, property, format)) return;
        var getVis = property.GetGetter().Visibility();
        var setVis = property.GetSetter().Visibility();
        Visibility highVis = getVis >= setVis ? getVis : setVis;
        stringHandler.WriteValue(highVis);
        stringHandler.Write(' ');
        stringHandler.WriteType(property.PropertyType, format);
        stringHandler.Write(' ');
        if (GetFormat(format) >= Format.Detailed)
        {
            stringHandler.WriteType(property.OwnerType(), format);
            stringHandler.Write('.');
        }

        stringHandler.Write(property.Name);
        stringHandler.Write(" {");
        if (getVis != Visibility.None)
        {
            if (getVis != highVis)
                stringHandler.WriteValue(getVis);
            stringHandler.Write(" get; ");
        }

        if (setVis != Visibility.None)
        {
            if (setVis != highVis)
                stringHandler.WriteValue(setVis);
            stringHandler.Write(" set; ");
        }
        stringHandler.Write('}');
    }

    private static void WriteEvent(this ref DefaultInterpolatedStringHandler stringHandler, EventInfo? eventInfo,
        ReadOnlySpan<char> format = default)
    {
        if (WroteNull(ref stringHandler, eventInfo, format)) return;
        stringHandler.WriteType(eventInfo.EventHandlerType, format);
        stringHandler.Write(' ');
        if (GetFormat(format) >= Format.Detailed)
        {
            stringHandler.WriteType(eventInfo.OwnerType(), format);
            stringHandler.Write('.');
        }
        stringHandler.Write(eventInfo.Name);
    }

    private static void WriteConstructor(this ref DefaultInterpolatedStringHandler stringHandler, ConstructorInfo? ctor,
        ReadOnlySpan<char> format = default)
    {
        if (WroteNull(ref stringHandler, ctor, format)) return;

        stringHandler.WriteType(ctor.DeclaringType!, format);
        stringHandler.Write(".ctor(");
        var parameters = ctor.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
        {
            if (i > 0) stringHandler.Write(',');
            stringHandler.WriteParameter(parameters[i], format);
        }
        stringHandler.Write(')');
    }

    private static void WriteMethod(this ref DefaultInterpolatedStringHandler stringHandler, MethodBase? method,
        ReadOnlySpan<char> format = default)
    {
        if (WroteNull(ref stringHandler, method, format)) return;
        stringHandler.WriteType(method.ReturnType(), format);
        stringHandler.Write(' ');
        if (GetFormat(format) >= Format.Detailed)
        {
            stringHandler.WriteType(method.OwnerType(), format);
            stringHandler.Write('.');
        }
        stringHandler.Write(method.Name);

        if (method.IsGenericMethod)
        {
            stringHandler.Write('<');
            var genericTypes = method.GetGenericArguments();
            for (var i = 0; i < genericTypes.Length; i++)
            {
                if (i > 0) stringHandler.Write(',');
                stringHandler.WriteType(genericTypes[i]);
            }
            stringHandler.Write('>');
        }
        stringHandler.Write('(');
        var parameters = method.GetParameters();
        for (var i = 0; i < parameters.Length; i++)
        {
            if (i > 0) stringHandler.Write(',');
            stringHandler.WriteParameter(parameters[i]);
        }
        stringHandler.Write(')');
    }

    private static void WriteParameter(this ref DefaultInterpolatedStringHandler stringHandler, ParameterInfo? parameter, ReadOnlySpan<char> format = default)
    {
        if (WroteNull(ref stringHandler, parameter, format)) return;
        stringHandler.WriteType(parameter.ParameterType, format);
        stringHandler.Write(' ');
        stringHandler.Write(parameter.Name);
        if (parameter.HasDefaultValue)
        {
            stringHandler.Write(" = ");
            stringHandler.WriteValue(parameter.DefaultValue);
        }
    }

    public static void WriteValue<T>(this ref DefaultInterpolatedStringHandler stringHandler, T? value, ReadOnlySpan<char> format = default)
    {
        switch (value)
        {
            case null:
                WroteNull(ref stringHandler, value, format);
                break;
            case Type type:
                stringHandler.WriteType(type, format);
                break;
            case FieldInfo fieldInfo:
                stringHandler.WriteField(fieldInfo, format);
                break;
            case PropertyInfo propertyInfo:
                stringHandler.WriteProperty(propertyInfo, format);
                break;
            case EventInfo eventInfo:
                stringHandler.WriteEvent(eventInfo, format);
                break;
            case ConstructorInfo ctorInfo:
                stringHandler.WriteConstructor(ctorInfo, format);
                break;
            case MethodInfo methodInfo:
                stringHandler.WriteMethod(methodInfo, format);
                break;
            case ParameterInfo parameterInfo:
                stringHandler.WriteParameter(parameterInfo, format);
                break;
            default:
                stringHandler.AppendFormatted<T>(value); // Do not pass our format
                break;
        }
    }

    public static string FormatValue<T>(T? value, ReadOnlySpan<char> format = default)
    {
        DefaultInterpolatedStringHandler stringHandler = default;
        WriteValue<T>(ref stringHandler, value, format);
        return stringHandler.ToStringAndClear();
    }
}

[InterpolatedStringHandler]
public ref struct ReflectInterpolatedStringHandler
{
    private DefaultInterpolatedStringHandler _stringHandler;

    public ReflectInterpolatedStringHandler(int literalLength, int formatCount)
    {
        _stringHandler = new(literalLength, formatCount);
    }

    public void AppendLiteral(string text)
    {
        // pass-through
        _stringHandler.AppendLiteral(text);
    }

    public void AppendFormatted<T>(T? value, string? format = null)
    {
        _stringHandler.WriteValue<T>(value, format);
    }

    public void AppendFormatted(string? text)
    {
        _stringHandler.AppendLiteral(text ?? "");
    }

    public void AppendFormatted(IEnumerable? enumerable, string? format = null)
    {
        if (enumerable is null) return;
        IEnumerator? enumerator = null;
        try
        {
            enumerator = enumerable.GetEnumerator();
            // At least one?
            if (enumerator.MoveNext())
            {
                // Append it
                _stringHandler.WriteValue<object>(enumerator.Current, format);
            }
            // The rest
            while (enumerator.MoveNext())
            {
                // Delimiter
                _stringHandler.AppendLiteral(",");
                // This value
                _stringHandler.WriteValue<object>(enumerator.Current, format);
            }
        }
        finally
        {
            if (enumerator is IDisposable disposable)
                disposable.Dispose();
        }
    }

    public string ToStringAndClear()
    {
        return _stringHandler.ToStringAndClear();
    }

    public override string ToString()
    {
        return _stringHandler.ToString();
    }
}