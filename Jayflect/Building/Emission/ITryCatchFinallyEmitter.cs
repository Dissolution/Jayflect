
namespace Jayflect.Building.Emission;

public interface ITryCatchFinallyEmitter<out TEmitter> 
    where TEmitter : IFluentILEmitter<TEmitter>
{
    TEmitter EndTry { get; }
    
    ITryCatchFinallyEmitter<TEmitter> Catch<TException>(Action<TEmitter> catchBlock)
        where TException : Exception 
        => Catch(typeof(TException), catchBlock);

    ITryCatchFinallyEmitter<TEmitter> Catch(Type exceptionType, Action<TEmitter> catchBlock);

    TEmitter Finally(Action<TEmitter> finallyBlock);
}