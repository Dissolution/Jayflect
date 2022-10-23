using System.Diagnostics;
using Jay.Collections;

namespace Jayflect.Extensions;

public static class GenericExtensions
{
    private sealed class TypeComplexityComparer : IEqualityComparer<Type>, IComparer<Type>
    {
        private static int RootValue(Type type)
        {
            // struct < class < interface
            if (type.IsValueType) return 0;
            if (type.IsClass) return 1;
            if (type.IsInterface) return 2;
            throw new NotImplementedException();
        }
        
        private static int L1Compare(Type left, Type right)
        {
            return RootValue(left).CompareTo(RootValue(right));
        }

        private static int OpennessValue(Type type)
        {
            // sealed < 'normal' < abstract
            if (type.IsSealed) return 0;
            if (!type.IsAbstract) return 1;
            return 2;
        }
        
        private static int L2Compare(Type left, Type right)
        {
            return OpennessValue(left).CompareTo(OpennessValue(right));
        }

        private static int GenericValue(Type type)
        {
            // `IList` < `IList<int>` < `IList<>` < `IDictionary<string, string>` < `IDictionary<,>` < ... n type args
            if (!type.IsGenericType) return 0;
            var genericTypes = type.GenericTypeArguments;
            Debug.Assert(genericTypes.Length > 0);
            // return value starts at (2*n)-1, where n is the number of generic types
            // this is to allow 'growth' of the value for generic type definitions (to sort 'higher')
            // <>       - 1 or 2
            // <,>      - 3 or 4
            // <,,>     - 5 or 6
            // <n>      - (2 * n) - 1 or (2 * n)
            int value = (2 * genericTypes.Length) - 1;

            if (type.IsGenericTypeDefinition)
                value++;
            return value;
        }
        
        private static int L3Compare(Type left, Type right)
        {
            return GenericValue(left).CompareTo(GenericValue(right));
        }

        public static TypeComplexityComparer Instance { get; } = new();
        
        public int Compare(Type? left, Type? right)
        {
            if (ReferenceEquals(left, right)) return 0;
            if (left is null) return -1;
            if (right is null) return 1;
            int c = L1Compare(left, right);
            if (c != 0) return c;
            c = L2Compare(left, right);
            if (c != 0) return c;
            c = L3Compare(left, right);
            return c;
        }

        public bool Equals(Type? left, Type? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return RootValue(left) == RootValue(right) &&
                   OpennessValue(left) == OpennessValue(right) &&
                   GenericValue(left) == GenericValue(right);
        }
        
        public int GetHashCode(Type? type)
        {
            if (type is null) return 0;
            return HashCode.Combine(RootValue(type), OpennessValue(type), GenericValue(type));
        }
    }

    public static IReadOnlyList<Type> GetImplementingTypes<T>(this T? value)
    {
        if (value is null) return Array.Empty<Type>();
        var types = new HashSet<Type>();
        Type? type = value.GetType();
        // Add my interfaces
        foreach (var interfaceType in type.GetInterfaces())
        {
            types.Add(interfaceType);
        }
        // Add base types
        while (((type = type?.BaseType) is not null))
        {
            types.Add(type);
        }
        // return sorted!
        return types.OrderByDescending(t => t, TypeComplexityComparer.Instance)
            .ToList();
    }

    private static readonly ConcurrentTypeDictionary<Delegate> _unhookEventsDelegateCache = new();

    private static Action<T> GetUnhooker<T>()
    {
        return (_unhookEventsDelegateCache.GetOrAdd<T>(CreateEventUnhooker) as Action<T>)!;
    }
    
    private static Delegate CreateEventUnhooker(Type type)
    {
        var fields = type.GetEvents(Reflect.Flags.All)
            .Select(vent => vent.GetBackingField())
            .Where(field => field is not null)
            .ToList();
        throw new NotImplementedException();
        /*
        return RuntimeBuilder.CreateDelegate(typeof(Action<>).MakeGenericType(type),
            emitter =>
            {
                foreach (var field in fields)
                {
                    emitter.Ldarg_0()
                        .Ldnull()
                        .Stfld(field!);
                }
                emitter.Ret();
            });
            */
    }

    /// <summary>
    /// Unhook all <c>event</c>s on <c>this</c> <paramref name="instance"/>
    /// </summary>
    /// <remarks>
    /// Sets all underlying fields to null
    /// </remarks>
    public static void UnhookEvents<T>(this T instance)
    {
        GetUnhooker<T>()(instance);
    }
}