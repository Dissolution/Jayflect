namespace Jay;

public readonly partial struct Result<T>
{
    public static implicit operator Result<T>(T? value) => new Result<T>(true, value, null);
    public static implicit operator Result<T>(Exception? exception) => new Result<T>(false, default(T), exception ?? new Exception(Result.DefaultErrorMessage));
    public static implicit operator bool(Result<T> result) => result._pass;
    public static explicit operator T?(Result<T> result) => result.GetValue();
    public static explicit operator Exception?(Result<T> result) => result._pass ? null : result._error ?? new Exception(Result.DefaultErrorMessage);
    public static implicit operator Result(Result<T> result) => new Result(result._pass, result._error);

    public static bool operator true(Result<T> result) => result._pass;
    public static bool operator false(Result<T> result) => !result._pass;
    public static bool operator !(Result<T> result) => !result._pass;
    
    public static bool operator ==(Result<T> x, Result<T> y) => x._pass == y._pass;
    public static bool operator ==(Result<T> x, Result y) => x._pass == y._pass;
    public static bool operator ==(Result<T> result, bool pass) => result._pass == pass;
    public static bool operator !=(Result<T> x, Result<T> y) => x._pass != y._pass;
    public static bool operator !=(Result<T> x, Result y) => x._pass != y._pass;
    public static bool operator !=(Result<T> result, bool pass) => result._pass != pass;

    public static bool operator |(Result<T> x, Result<T> y) => x._pass || y._pass;
    public static bool operator |(Result<T> x, Result y) => x._pass || y._pass;
    public static bool operator |(Result<T> result, bool pass) => pass || result._pass;
    public static bool operator &(Result<T> x, Result<T> y) => x._pass && y._pass;
    public static bool operator &(Result<T> x, Result y) => x._pass && y._pass;
    public static bool operator &(Result<T> result, bool pass) => pass && result._pass;
    public static bool operator ^(Result<T> x, Result<T> y) => x._pass ^ y._pass;
    public static bool operator ^(Result<T> x, Result y) => x._pass ^ y._pass;
    public static bool operator ^(Result<T> result, bool pass) => result._pass ^ pass;

    
    /// <summary>
    /// Returns a passing <see cref="Result{T}"/> with the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The passing value.</param>
    /// <returns>A passing <see cref="Result{T}"/>.</returns>
    public static Result<T> Pass(T? value)
    {
        return new Result<T>(true, value, null);
    }

    /// <summary>
    /// Returns a failing <see cref="Result{T}"/> with the given <paramref name="exception"/>.
    /// </summary>
    /// <param name="exception">The failing <see cref="_error"/>.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Fail(Exception? exception = null)
    {
        exception ??= new Exception(Result.DefaultErrorMessage);
        return new Result<T>(false, default(T), exception);
    }
    
    
    /// <inheritdoc cref="Jay.Result.Result.TryInvoke"/>
    public static Result TryInvoke(Func<T>? func, out T? value)
    {
        return Result.TryInvoke<T>(func, out value);
    }

    /// <summary>
    /// Try to execute the given <paramref name="func"/> and return a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="func">The function to try to execute.</param>
    /// <returns>
    /// A passing <see cref="Result{T}"/> containing <paramref name="func"/>'s return value or
    /// a failing <see cref="Result{T}"/> containing the captured <see cref="_error"/>.
    /// </returns>
    public static Result<T> TryInvoke(Func<T>? func)
    {
        if (func is null)
        {
            return new ArgumentNullException(nameof(func));
        }
        try
        {
            return func.Invoke();
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
    
    /// <inheritdoc cref="Jay.Result.Result.InvokeOrDefault"/>
    public static T InvokeOrDefault(Func<T>? func, T fallback)
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
}