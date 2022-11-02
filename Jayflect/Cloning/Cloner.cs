using System.Collections;
using System.Diagnostics;
using Jay.Collections;
using Jay.Extensions;
using Jay.Utilities;
using Jayflect.Building;
using Jayflect.Building.Emission;
using Jayflect.Caching;
using Jayflect.Searching;

namespace Jayflect.Cloning;

/// <summary>
/// Deep-clones the given <paramref name="value"/>
/// </summary>
/// <typeparam name="T">The <see cref="Type"/> of value to deep clone</typeparam>
[return: NotNullIfNotNull(nameof(value))]
public delegate T DeepClone<T>(T value);

    public class ArrayWrapper : IEnumerable
    {
        private readonly Array _array;
        
        public int Rank { get; }
        public int[] LowerBounds { get; }
        public int[] UpperBounds { get; }

        public Type ElementType => _array.GetType().GetElementType()!;
        
        public int[] GetLengths()
        {
            var lengths = new int[Rank];
            for (var r = 0; r < Rank; r++)
            {
                lengths[r] = UpperBounds[r] - LowerBounds[r];
            }
            return lengths;
        }
        
        public object? GetValue(int[] indices)
        {
            return _array.GetValue(indices);
        }
        public void SetValue(int[] indices, object? value)
        {
            _array.SetValue(value, indices);
        }
        
        public ArrayWrapper(Array array)
        {
            _array = array;
            this.Rank = array.Rank;
            this.LowerBounds = new int[Rank];
            this.UpperBounds = new int[Rank];
            for (var r = 0; r < Rank; r++)
            {
                this.LowerBounds[r] = array.GetLowerBound(r);
                this.UpperBounds[r] = array.GetUpperBound(r);
            }
        }

        public ArrayEnumerator GetEnumerator()
        {
            return new ArrayEnumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    public sealed class ArrayEnumerator : IEnumerator
    {
        private readonly ArrayWrapper _arrayEnumerable;
        private int[]? _indices;
        private object? _current;

        public int[] Indices => _indices ?? throw new InvalidOperationException();

        public object? Current => _current;

        public ArrayEnumerator(ArrayWrapper arrayEnumerable)
        {
            _arrayEnumerable = arrayEnumerable;
        }

        private bool TryIncrementIndex(int rank)
        {
            // Are we trying to imcrement a non-existent rank? can't!
            if (rank > _arrayEnumerable.Rank) return false;
            
            int nextIndex = _indices![rank] + 1;
            // Will we go over upper bound?
            if (nextIndex > _arrayEnumerable.UpperBounds[rank])
            {
                // Increment the next rank
                if (!TryIncrementIndex(rank + 1)) 
                    return false;
                
                // Reset my rank back to its lowest bound
                _indices[rank] = _arrayEnumerable.LowerBounds[rank];
            }
            else
            {
                // Increment my index
                _indices[rank] = nextIndex;
            }
            return true;
        }
        
        public bool MoveNext()
        {
            if (_indices is null)
            {
                _indices = new int[_arrayEnumerable.Rank];
                Fast.Copy<int>(_arrayEnumerable.LowerBounds, _indices);
            }

            if (TryIncrementIndex(0))
            {
                _current = _arrayEnumerable.GetValue(_indices);
                return true;
            }
            
            _current = null;
            return false;
        }
        
        public void Reset()
        {
            _indices = null;
            _current = null;
        }

        public override string ToString()
        {
            if (_indices is null)
                return "Enumeration has not started";
            return Dump($"{_indices}: {_current:V}");
        }
    }

public static class Cloner
{
    private static readonly ConcurrentTypeDictionary<Delegate> _deepCloneCache = new();
    private static readonly ConcurrentTypeDictionary<DeepClone<object>> _objectCloneCache = new();

    static Cloner()
    {
        _deepCloneCache[typeof(string)] = FastClone<string>;
        _deepCloneCache[typeof(object)] = ObjectDeepClone;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T FastClone<T>(T value) => value;

    public static T[] DeepClone1DArray<T>(T[] array)
    {
        int len = array.Length;
        T[] clone = new T[len];
        var deepClone = GetDeepClone<T>();
        for (int i = 0; i < len; i++)
        {
            clone[i] = deepClone(array[i]);
        }
        return clone;
    }
    public static T[,] DeepClone2DArray<T>(T[,] array)
    {
        int arrayLen0 = array.GetLength(0);
        int arrayLen1 = array.GetLength(1);
        T[,] clone = new T[arrayLen0,arrayLen1];
        var deepClone = GetDeepClone<T>();
        for (var x = 0; x < array.GetLength(0); x++)
        {
            for (var y = 0; y < array.GetLength(1); y++)
            {
                clone[x, y] = deepClone(array[x, y]);
            }
        }
        return clone;
    }
    
    [return: NotNullIfNotNull(nameof(array))]
    public static Array DeepCloneArray(Array array)
    {
        var arrayWrapper = new ArrayWrapper(array);
        Array clone = Array.CreateInstance(arrayWrapper.ElementType, arrayWrapper.GetLengths(), arrayWrapper.LowerBounds);
        var cloner = GetObjectDeepClone(arrayWrapper.ElementType);
        var cloneWrapper = new ArrayWrapper(clone);
        var e = arrayWrapper.GetEnumerator();
        while (e.MoveNext())
        {
            int[] index = e.Indices;
            cloneWrapper.SetValue(index, cloner(e.Current!));
        }
        return clone;
    }

    
    
    private static Delegate CreateDeepClone(Type type)
    {
        var builder = RuntimeBuilder.CreateRuntimeDelegateBuilder(
            typeof(DeepClone<>).MakeGenericType(type),
            Dump($"clone_{type}"));
        var emitter = builder.Emitter;

        /*
        // Null check for non-value types
        if (!type.IsValueType)
        {
            emitter.Ldarg(0)
                .Ldind(type)
                .Brfalse(out var notNull)
                .Ldnull()
                .Ret()
                .MarkLabel(notNull);
        }
        */

        // unmanaged or string we just dup + return
        if (type == typeof(string) || type.IsUnmanaged())
        {
            emitter.Ldarga(0).Ldind(type).Ret();
        }
        // Special Array handling
        else if (type.IsArray)
        {
            emitter.Ldarg(0)
                .EmitCast(type, typeof(Array))
                .Call(MemberSearch.FindMethod(typeof(Cloner), new(nameof(DeepCloneArray), Visibility.Public | Visibility.Static, typeof(Array), typeof(Array))))
                .EmitCast(typeof(Array), type)
                .Ret();
        }
        // Everything else has some sort of reference down the chain
        else
        {
            // Create a raw value
            emitter.DeclareLocal(type, out var copy);

            if (type.IsValueType)
            {
                // init the copy, we'll clone the fields
                emitter.Ldloca(copy)
                    .Initobj(type);

                // copy each instance field
                var fields = type.GetFields(Reflect.Flags.Instance);
                foreach (var field in fields)
                {
                    if (field.FieldType.Implements<FieldInfo>())
                        Debugger.Break();

                    emitter.Ldloca(copy)
                        .Ldarg(0)
                        .Ldfld(field)
                        .Call(GetDeepCloneMethod(field.FieldType))
                        .Stfld(field);
                }
            }
            else
            {
                // Uninitialized object
                // We don't want to call the constructor, that may have side effects
                emitter.LoadType(type)
                    .Call(MemberCache.Methods.RuntimeHelpers_GetUninitializedObject)
                    .Unbox_Any(type)
                    .Stloc(copy);

                // copy each instance field
                var fields = type.GetFields(Reflect.Flags.Instance);
                foreach (var field in fields)
                {
                    var fieldDeepClone = GetDeepCloneMethod(field.FieldType);
                    emitter.Ldloc(copy)
                        .Ldarg(0)
                        .Ldfld(field)
                        .Call(fieldDeepClone)
                        .Stfld(field);
                }
            }

            // Load our clone and return!
            emitter.Ldloc(copy)
                .Ret();
        }

        return builder.CreateDelegate();
    }


    internal static DeepClone<T> GetDeepClone<T>()
    {
        return (_deepCloneCache.GetOrAdd<T>(CreateDeepClone) as DeepClone<T>)!;
    }

    [return: NotNullIfNotNull(nameof(value))]
    public static T DeepClone<T>(this T value)
    {
        if (value is null) return default!;
        return GetDeepClone<T>().Invoke(value);
    }
    
    private static MethodInfo GetDeepCloneMethod(Type type)
    {
        return MemberSearch.FindMethod(typeof(Cloner), new(
            nameof(DeepClone), Visibility.Public | Visibility.Static))
            .MakeGenericMethod(type);
    }
        
    private static DeepClone<object> CreateObjectClone(Type type)
    {
        if (type == typeof(object)) // prevent recursion
            return FastClone<object>!;
        
        return RuntimeBuilder.CreateDelegate<DeepClone<object>>(
            Dump($"clone_object_{type}"),
            emitter => emitter
                .Ldarg(0)
                .Unbox_Any(type)
                .Call(GetDeepCloneMethod(type))
                .Box(type)
                .Ret());
    }

    internal static DeepClone<object> GetObjectDeepClone(Type objectType)
    {
        return _objectCloneCache.GetOrAdd(objectType, CreateObjectClone);
    }
    
    [return: NotNullIfNotNull(nameof(obj))]
    public static object? ObjectDeepClone(this object? obj)
    {
        if (obj is null) return null;
        var type = obj.GetType();
        return GetObjectDeepClone(type).Invoke(obj);
    }
}