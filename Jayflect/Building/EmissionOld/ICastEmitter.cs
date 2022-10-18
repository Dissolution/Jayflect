using System.Collections.Concurrent;

namespace Jay.Reflection.Building.Emission;

public interface ICastEmitter<E>
    where E : ICastEmitter<E>
{
    bool CanEmitCast(Type inType, Type outType);

    E EmitCast(Type inType, Type outType);

    E EmitCast<TIn, TOut>() => EmitCast(typeof(TIn), typeof(TOut));

    E EmitCastTo<TOut>(Type inType) => EmitCast(inType, typeof(TOut));

    E EmitCastFrom<TIn>(Type outType) => EmitCast(typeof(TIn), outType);
}

internal static class CastEmitter
{
    private static readonly ConcurrentDictionary<(Type InType, Type OutType), Action<IILGeneratorEmitter>> _emissionCache;

    static CastEmitter()
    {
        _emissionCache = new ConcurrentDictionary<(Type InType, Type OutType), Action<IILGeneratorEmitter>>();
    }

    public static bool CanEmitCast(Type inType, Type outType)
    {
        throw new NotImplementedException();
    }
}