
namespace Jayflect.Building.Emission;

/// <summary>
/// A fluent interface for building try/catch/finally blocks
/// </summary>
/// <typeparam name="TEmitter">The <see cref="IFluentILEmitter"/> to be resumed upon <see cref="Finally()"/></typeparam>
public interface ITryCatchFinallyBuilder<out TEmitter> 
    where TEmitter : IFluentILEmitter<TEmitter>
{
    EmitterLabel? EndTryLabel { get; }
    
    ITryCatchFinallyBuilder<TEmitter> Try(Action<TEmitter> tryBlock);
    
    /// <summary>
    /// Adds a <c>catch</c> <paramref name="catchBlock"/> and continues Try/Catch/Finally building
    /// </summary>
    /// <typeparam name="TException">The <see cref="Type"/> of <see cref="Exception"/>s to be caught</typeparam>
    /// <param name="catchBlock">The instructions to emit during this <c>catch</c> block</param>
    /// <returns>This <see cref="ITryCatchFinallyBuilder{TEmitter}"/></returns>
    ITryCatchFinallyBuilder<TEmitter> Catch<TException>(Action<TEmitter> catchBlock)
        where TException : Exception 
        => Catch(typeof(TException), catchBlock);

    /// <summary>
    /// Adds a <c>catch</c> <paramref name="catchBlock"/> and continues Try/Catch/Finally building
    /// </summary>
    /// <param name="exceptionType">The <see cref="Type"/> of <see cref="Exception"/>s to be caught</param>
    /// <param name="catchBlock">The instructions to emit during this <c>catch</c> block</param>
    /// <returns>This <see cref="ITryCatchFinallyBuilder{TEmitter}"/></returns>
    ITryCatchFinallyBuilder<TEmitter> Catch(Type exceptionType, Action<TEmitter> catchBlock);

    /// <summary>
    /// Ends this <c>catch</c> block
    /// </summary>
    TEmitter Finally();

    /// <summary>
    /// Adds a <c>finally</c> <paramref name="finallyBlock"/> and returns back to emission
    /// </summary>
    TEmitter Finally(Action<TEmitter>? finallyBlock);
}

public class TCFBuilder<TEmitter> : ITryCatchFinallyBuilder<TEmitter>
    where TEmitter : IFluentILEmitter<TEmitter>
{
    private readonly TEmitter _emitter;
    private EmitterLabel? _endTryLabel;

    public EmitterLabel? EndTryLabel => _endTryLabel;
    
    public TCFBuilder(TEmitter emitter)
    {
        _emitter = emitter;
    }

    public ITryCatchFinallyBuilder<TEmitter> Try(Action<TEmitter> tryBlock)
    {
        _emitter.BeginExceptionBlock(out _endTryLabel);
        tryBlock(_emitter);
        return this;
    }

    public ITryCatchFinallyBuilder<TEmitter> Catch(Type exceptionType, Action<TEmitter> catchBlock)
    {
        _emitter.BeginCatchBlock(exceptionType);
        catchBlock(_emitter);
        return this;
    }

    public TEmitter Finally() => _emitter;
    
    public TEmitter Finally(Action<TEmitter>? finallyBlock)
    {
        if (finallyBlock is not null)
        {
            _emitter.BeginFinallyBlock();
            finallyBlock(_emitter);
            _emitter.EndExceptionBlock();
        }
        return _emitter;
    }
}
