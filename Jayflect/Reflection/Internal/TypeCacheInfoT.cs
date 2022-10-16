using Jayflect;

namespace Jay.Reflection.Internal;

internal class TypeCacheInfo<T> : TypeCacheInfo
{
    public TypeCacheInfo()
        : base(typeof(T),
            RuntimeHelpers.IsReferenceOrContainsReferences<T>(),
            () => default(T),
            Result.InvokeOrDefault<int?>(() => Danger.SizeOf<T>(), null))
    {
    }

    public new T? GetDefault() => default(T);
    public new T GetUninitialized() => (T)RuntimeHelpers.GetUninitializedObject(Type);
}