using System.Diagnostics;
using System.Reflection;
using Jay.Validation;

namespace Jay.Reflection.Internal;

internal class TypeCacheInfo
{
    private static readonly MethodInfo _isReferenceMethod;

    static TypeCacheInfo()
    {
        _isReferenceMethod = typeof(RuntimeHelpers)
                             .GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences),
                                 BindingFlags.Public | BindingFlags.Static,
                                 Type.EmptyTypes)
                             .ThrowIfNull($"Unable to find {nameof(RuntimeHelpers)}.{nameof(RuntimeHelpers.IsReferenceOrContainsReferences)}");

    }

    private readonly Func<object?> _getDefault;

    public Type Type { get; protected set; }
    public bool IsUnmanaged => !IsReferenceOrContainsReferences;
    public bool IsReferenceOrContainsReferences { get; protected set; }
    public int? Size { get; }

    protected TypeCacheInfo(Type type,
                            bool isReference,
                            Func<object?> getDefault,
                            int? size)
    {
        this.Type = type;
        this.IsReferenceOrContainsReferences = isReference;
        _getDefault = getDefault;
        this.Size = size;
    }

    public TypeCacheInfo(Type type)
    {
        this.Type = type;
        this.IsReferenceOrContainsReferences = (bool)_isReferenceMethod
                                                     .MakeGenericMethod(type)
                                                     .Invoke(null, null)!;
        if (type.IsClass || type.IsInterface)
        {
            _getDefault = () => null;
            Size = null;
        }
        else
        {
            Debug.Assert(type.IsValueType);
            _getDefault = () => Activator.CreateInstance(type);
            Size = typeof(TypeCache<>)
                   .MakeGenericType(Type)
                   .GetProperty(nameof(TypeCache<int>.Size),
                       BindingFlags.Public | BindingFlags.Static)
                   .ThrowIfNull()
                   .GetValue<Types.Static, int?>(ref Types.Static.Instance);
        }
    }

    public object? GetDefault() => _getDefault();

    public object GetUninitialized()
    {
        return RuntimeHelpers.GetUninitializedObject(Type);
    }
}