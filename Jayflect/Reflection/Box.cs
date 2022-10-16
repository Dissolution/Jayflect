using System.Diagnostics;
using Jay.Comparision;

namespace Jay.Reflection;

public readonly struct Box : IEquatable<Box>, 
                             IComparable<Box>, IComparable,
                             IFormattable
{
    public static bool operator ==(Box a, Box b) => a.Equals(b);
    public static bool operator ==(Box box, object? obj) => box.Equals(obj);
    public static bool operator ==(object? obj, Box box) => box.Equals(obj);
    
    public static bool operator !=(Box a, Box b) => !a.Equals(b);
    public static bool operator !=(Box box, object? obj) => !box.Equals(obj);
    public static bool operator !=(object? obj, Box box) => !box.Equals(obj);
    
    public static bool operator <(Box a, Box b) => a.CompareTo(b) < 0;
    public static bool operator <(Box box, object? obj) => box.CompareTo(obj) < 0;
    public static bool operator <(object? obj, Box box) => box.CompareTo(obj) > 0;
    
    public static bool operator <=(Box a, Box b) => a.CompareTo(b) <= 0;
    public static bool operator <=(Box box, object? obj) => box.CompareTo(obj) <= 0;
    public static bool operator <=(object? obj, Box box) => box.CompareTo(obj) >= 0;
    
    public static bool operator >(Box a, Box b) => a.CompareTo(b) > 0;
    public static bool operator >(Box box, object? obj) => box.CompareTo(obj) > 0;
    public static bool operator >(object? obj, Box box) => box.CompareTo(obj) < 0;
    
    public static bool operator >=(Box a, Box b) => a.CompareTo(b) >= 0;
    public static bool operator >=(Box box, object? obj) => box.CompareTo(obj) >= 0;
    public static bool operator >=(object? obj, Box box) => box.CompareTo(obj) <= 0;

    /// <summary>
    /// Wraps the given <see cref="object"/> <paramref name="value"/> in a <see cref="Box"/>.
    /// </summary>
    /// <param name="value">An <see cref="object"/>? (can be <see langword="null"/>) to be Boxed.</param>
    /// <returns>A <see cref="Box"/> containing <paramref name="value"/>.</returns>
    public static Box Wrap(object? value)
    {
        return new Box(value, value?.GetType());
    }

    /// <summary>
    /// Wraps the given <typeparamref name="T"/> <paramref name="value"/> in a <see cref="Box"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of <paramref name="value"/> to be Boxed.</typeparam>
    /// <param name="value">A <typeparamref name="T"/>? value (can be <see langword="null"/>) to be Boxed.</param>
    /// <returns>A <see cref="Box"/> containing <paramref name="value"/>.</returns>
    public static Box Wrap<T>(T? value)
    {
        return new Box(value, typeof(T));
    }

    private readonly object? _obj;
    private readonly Type? _objType;

    public bool ContainsNull
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _obj is null;
    }

    public bool IsNull
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _objType is null;
    }

    private Box(object? obj, Type? type)
    {
        Debug.Assert(type is not null || obj is null);
        _obj = obj;
        _objType = type;
    }

    /// <summary>
    /// Can the boxed value be a <typeparamref name="T"/> value?
    /// <para>
    /// note: any <c>class</c> and <see cref="string"/> can be <c>null</c>.
    /// </para>
    /// </summary>
    public bool CanBe<T>()
    {
        if (_obj is T) return true;
        if (_obj is null) return typeof(T).CanContainNull();
        return false;
    }

    public bool CanBe(Type? type)
    {
        if (type is null) return _obj is null;
        if (_obj is null) return type.CanContainNull();
        return _objType!.Implements(type);
    }
    
    public bool Is<T>()
    {
        return _obj is T;
    }

    public bool Is(Type? type)
    {
        if (type is null) return _obj is null;
        return _objType!.Implements(type);
    }

    public bool Is<T>([NotNullWhen(true)] out T? value)
    {
        if (_obj is T)
        {
            value = (T)_obj;
            return true;
        }
        value = default;
        return false;
    }

    public bool Is(Type type, [NotNullWhen(true)] out object? value)
    {
        if (_objType.Implements(type))
        {
            value = _obj!;
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }
    public int CompareTo(Box box)
    {
        if (IsNull) return box.IsNull ? 0 : -1;
        if (box.IsNull) return 1;
        if (box._objType != _objType)
            return 0;
        return ComparerCache.Compare(_objType!, _obj, box._obj);
    }

    public int CompareTo(object? obj)
    {
        if (IsNull) return obj is null ? 0 : -1;
        if (obj is null) return 1;
        if (_objType != obj.GetType()) return 0;
        return ComparerCache.Compare(_objType!, _obj, obj);
    }

    public bool Equals(Box box)
    {
        if (IsNull) return box.IsNull;
        return _objType == box._objType &&
               ComparerCache.Equals(_objType!, _obj, box._obj);
    }

    public override bool Equals(object? obj)
    {
        if (IsNull) return obj is null;
        if (obj is null) return false;
        return _objType == obj.GetType() &&
               ComparerCache.Equals(_objType!, _obj, obj);
    }

    public override int GetHashCode()
    {
        if (_obj is null) return 0;
        return ComparerCache.GetHashCode(_objType!, _obj);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (_obj is IFormattable formattable)
        {
            return formattable.ToString(format, formatProvider);
        }
        return _obj?.ToString() ?? "";
    }

    public override string ToString()
    {
        return _obj?.ToString() ?? "";
    }
}