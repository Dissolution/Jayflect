using System.Reflection;
using Jay.Collections;

namespace Jay.Reflection.Building;

public static class ExceptionBuilder
{
    internal delegate TException CommonExceptionConstructor<out TException>(string? message = null, Exception? innerException = null)
        where TException : Exception;

    private static readonly ConcurrentTypeDictionary<Delegate> _ctorCache;

    static ExceptionBuilder()
    {
        _ctorCache = new ConcurrentTypeDictionary<Delegate>();
    }
  
    private static CommonExceptionConstructor<TException> CreateCtor<TException>(Type exceptionType)
        where TException : Exception
    {
        var dm = RuntimeBuilder.CreateRuntimeMethod<CommonExceptionConstructor<TException>>($"ctor_{typeof(TException).Name}");
        var emitter = dm.Emitter;
        ConstructorInfo? ctor;
        ctor = exceptionType.GetConstructor(Reflect.InstanceFlags, new Type[2] { typeof(string), typeof(Exception) });
        if (ctor is not null)
        {
            emitter.Ldarg(0)
                   .Ldarg(1)
                   .Newobj(ctor)
                   .Ret();
            return dm.CreateDelegate();
        }

        ctor = exceptionType.GetConstructor(Reflect.InstanceFlags, new Type[1] { typeof(string) });
        if (ctor is not null)
        {
            emitter.Ldarg(0)
                   .Newobj(ctor)
                   .Ret();
            return dm.CreateDelegate();
        }

        ctor = exceptionType.GetConstructor(Reflect.InstanceFlags, new Type[1] { typeof(Exception) });
        if (ctor is not null)
        {
            emitter.Ldarg(1)
                   .Newobj(ctor)
                   .Ret();
            return dm.CreateDelegate();
        }

        ctor = exceptionType.GetConstructor(Reflect.InstanceFlags, Type.EmptyTypes);
        if (ctor is not null)
        {
            emitter.Newobj(ctor)
                   .Ret();
            return dm.CreateDelegate();
        }

        emitter.LoadUninitialized(exceptionType)
               .Ret();
        return dm.CreateDelegate();
    }


    internal static CommonExceptionConstructor<TException> GetCommonConstructor<TException>()
        where TException : Exception
    {
        return (_ctorCache.GetOrAdd<TException>(CreateCtor<TException>) as CommonExceptionConstructor<TException>)!;
    }

    public static TException CreateException<TException>(ref DefaultInterpolatedStringHandler message,
                                                         Exception? innerException = null)
        where TException : Exception
    {
        return GetCommonConstructor<TException>()(message.ToStringAndClear(), innerException);
    }
}