using System.Diagnostics.CodeAnalysis;

namespace Jay;

/// <remarks>
/// This is the <see langword="const"/> and <see langword="static"/> part of <see cref="Result"/>
///
/// As a rule, when we create a failed Result we want to have an Exception at that point.
/// Even if we have to create one, we at least capture a proper stack trace.
/// This is the overhead of Result.Fail
///
/// We want the end user to use Result almost exactly as if it was bool.
/// </remarks>
/// 
public readonly partial struct Result
{
    internal const string DefaultErrorMessage = "Operation Failed";
    
    public static implicit operator Result(bool pass) => pass ? Pass : new Result(false, new Exception(DefaultErrorMessage));
    public static implicit operator Result(Exception? exception) => new Result(false, exception ?? new Exception(DefaultErrorMessage));
    public static implicit operator bool(Result result) => result._pass;
    public static explicit operator Exception?(Result result) => result._pass ? null : result._error ?? new Exception(DefaultErrorMessage);

    public static bool operator true(Result result) => result._pass;
    public static bool operator false(Result result) => !result._pass;
    public static bool operator !(Result result) => !result._pass;
    
    public static bool operator ==(Result x, Result y) => x._pass == y._pass;
    public static bool operator ==(Result result, bool pass) => result._pass == pass;
    public static bool operator !=(Result x, Result y) => x._pass != y._pass;
    public static bool operator !=(Result result, bool pass) => result._pass != pass;

    public static bool operator |(Result x, Result y) => x._pass || y._pass;
    public static bool operator |(Result result, bool pass) => pass || result._pass;
    public static bool operator &(Result x, Result y) => x._pass && y._pass;
    public static bool operator &(Result result, bool pass) => pass && result._pass;
    public static bool operator ^(Result x, Result y) => x._pass ^ y._pass;
    public static bool operator ^(Result result, bool pass) => result._pass ^ pass;

    /// <summary>
    /// A successful <see cref="Result"/>
    /// </summary>
    public static readonly Result Pass = new Result(true, null);

    /// <summary>
    /// Creates a failed <see cref="Result"/> with attached <paramref name="exception"/>
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static Result Fail(Exception? exception = default)
    {
        exception ??= new Exception(DefaultErrorMessage);
        return new Result(false, exception);
    }
    
    /// <summary>
    /// Tries to invoke the <paramref name="action"/> and returns a <see cref="Result"/>
    /// </summary>
    /// <param name="action">The <see cref="Action"/> to <see cref="Action.Invoke"/></param>
    /// <returns>
    /// A successful <see cref="Result"/> if the <paramref name="action"/> invokes without throwing an <see cref="Exception"/>.
    /// A failed <see cref="Result"/> with the caught <see cref="Exception"/> attached if not.
    /// </returns>
    public static Result TryInvoke(Action? action)
    {
        if (action is null)
        {
            return new ArgumentNullException(nameof(action));
        }
        try
        {
            action.Invoke();
            return Pass;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <summary>
    /// Tries to invoke the <paramref name="func"/>, setting <paramref name="output"/> and returning a <see cref="Result"/>
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of <paramref name="output"/> the <paramref name="func"/> produces</typeparam>
    /// <param name="func">The <see cref="Func{T}"/> to <see cref="Func{T}.Invoke"/></param>
    /// <param name="output">The result of invoking <paramref name="func"/> or <see langword="default{TResult}"/> on failure.</param>
    /// <returns>
    /// A successful <see cref="Result"/> if the <paramref name="func"/> invokes without throwing an <see cref="Exception"/>.
    /// A failed <see cref="Result"/> with the caught <see cref="Exception"/> attached if not.
    /// </returns>
    public static Result TryInvoke<T>(Func<T>? func, [MaybeNullWhen(false)] out T output)
    {
        if (func is null)
        {
            output = default;
            return new ArgumentNullException(nameof(func));
        }
        try
        {
            output = func.Invoke();
            return Pass;
        }
        catch (Exception ex)
        {
            output = default;
            return ex;
        }
    }
    
    /// <inheritdoc cref="Result{T}"/>
    public static Result<T> TryInvoke<T>(Func<T>? func) => Result<T>.TryInvoke(func);

    
    /*public static T? InvokeOrDefault<T>(Func<T> func)
    {
        try
        {
            return func();
        }
        catch
        {
            return default;
        }
    }*/

    public delegate Result OutResult<T>(out T value);

    public static T Invoke<T>(OutResult<T> outResult)
    {
        outResult(out T value).ThrowIfFailed();
        return value;
    }
    
    /// <summary>
    /// Invokes the <paramref name="func"/> and returns its result.
    /// If the <paramref name="func"/> throws an <see cref="Exception"/>, <paramref name="fallback"/> is returned instead.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of <paramref name="output"/> the <paramref name="func"/> produces</typeparam>
    /// <param name="func">The <see cref="Func{T}"/> to <see cref="Func{T}.Invoke"/></param>
    /// <param name="fallback">The value to return if invocation fails.</param>
    /// <returns><paramref name="func"/>'s result or <paramref name="fallback"/></returns>
    [return: NotNullIfNotNull(nameof(fallback))]
    public static T? InvokeOrDefault<T>(Func<T>? func, T? fallback = default)
    {
        if (func is null)
        {
            return fallback;
        }
        try
        {
            return func();
        }
        catch
        {
            return fallback;
        }
    }
    
    /// <summary>
    /// Tries to dispose of <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of <paramref name="value"/> to dispose</typeparam>
    /// <param name="value">The value to dispose.</param>
    public static void Dispose<T>(T? value)
    {
        // Avoids boxing for disposable structs
        if (value is IDisposable)
        {
            try
            {
                ((IDisposable)value).Dispose();
            }
            catch // (Exception ex)
            {
                /* POKEMON */
            }
        }
    }

    public static async ValueTask DisposeAsync<T>(T? value)
    {
        if (value is IAsyncDisposable asyncDisposable)
        {
            try
            {
                await asyncDisposable.DisposeAsync();
            }
            catch // (Exception ex)
            {
                /* POKEMON */
            }
        }
        // Avoids boxing for disposable structs
        else if (value is IDisposable)
        {
            try
            {
                ((IDisposable)value).Dispose();
            }
            catch // (Exception ex)
            {
                /* POKEMON */
            }
        }
    }

}