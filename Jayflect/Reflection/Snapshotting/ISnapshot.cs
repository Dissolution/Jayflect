using Jay.Collections;

namespace Jay.Reflection.Snapshotting;

public sealed record PropertyChange<T>(T? OldValue, T? NewValue);

public static class Snapshot
{
    public static ISnapshot<T> Take<T>(ref T instance)
    {
        //return Snapshot<T>.Create(ref instance);
        throw new NotImplementedException();
    }
}

public interface ISnapshot<T>
{
    void UpdateFrom(in T instance);
    void ApplyTo(ref T instance);
    IReadOnlyDictionary<string, PropertyChange<object?>> ChangesTo(in T instance);
}

// internal sealed class Snapshot<T> : ISnapshot<T>
// {
//     private static readonly (Getter<T, object?> Getter, Setter<T, object?> Setter)[] _fieldGetSetMethods;
//     
//     static Snapshot()
//     {
//         var fields = typeof(T).GetFields(Reflect.InstanceFlags);
//         _fieldGetSetMethods = new (Getter<T, object?> Getter, Setter<T, object?> Setter)[fields.Length];
//         for (var i = 0; i < fields.Length; i++)
//         {
//             var field = fields[i];
//             _fieldGetSetMethods[i] = (field.CreateGetter<T, object?>(), field.CreateSetter<T, object?>());
//         }
//     }
//
//     public static Snapshot<T> Create(ref T instance)
//     {
//         List<(Setter<T, object?> Setter, object? Value)> setters = new();
//         
//         foreach (var (getter, setter) in _fieldGetSetMethods)
//         {
//             object? value = getter(ref instance);
//             setters.Add((setter, value));
//         }
//
//         return new Snapshot<T>(setters);
//     }
//
//     private readonly List<(Setter<T, object?> Setter, object? Value)> _setters;
//     
//     private Snapshot(List<(Setter<T, object?> Setter, object? Value)> fieldValues)
//     {
//         _setters = fieldValues;
//     }
//
//     /// <inheritdoc />
//     public void ApplyTo(ref T instance)
//     {
//         foreach (var (setter, value) in _setters)
//         {
//             setter(ref instance, value);
//         }
//     }
// }