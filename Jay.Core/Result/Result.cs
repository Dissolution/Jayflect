using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Jay;

/// <summary>
/// Represents the result of an operation as a Pass or a Failure with <see cref="Exception"/> information.
/// </summary>
public readonly partial struct Result : IEquatable<Result>
{
    // _pass is the field (rather than _fail) because default(Result) should be a failure
    /* These fields were chosen specifically.
     * default(Result) should be the same as default(bool), which is false.
     * Hence, the default values of all fields should the exactly the same as a failure.
     */
    
    internal readonly bool _pass;
    internal readonly Exception? _error;

    /// <remarks>
    /// We never let a consumer create a Result directly. They must use the implicit casts or the methods.
    /// </remarks>
    internal Result(bool pass, Exception? error)
    {
        _pass = pass;
        _error = error;
    }

    /// <summary>
    /// Throws the attached <see cref="Exception"/> if this is a failed <see cref="Result"/>
    /// </summary>
    /// <exception cref="Exception">The attached exception.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ThrowIfFailed()
    {
        if (!_pass)
        {
            throw (_error ?? new Exception(DefaultErrorMessage));
        }
    }

    /// <summary>
    /// Is this a failed <see cref="Result"/>?
    /// </summary>
    /// <param name="error">If this is a failed <see cref="Result"/>, the attached <see cref="Exception"/>; otherwise <see langword="null"/></param>
    /// <returns>true if this is a failed result; otherwise, false</returns>
    public bool IsFailure([NotNullWhen(true)] out Exception? error)
    {
        if (_pass)
        {
            error = null;
            return false;
        }

        error = (_error ?? new Exception(DefaultErrorMessage));
        return true;
    }

    /// <summary>
    /// Returns a <see cref="Result{T}"/> that is this <see cref="Result"/> with a <paramref name="value"/>
    /// </summary>
    public Result<T> WithValue<T>(T? value)
    {
        return new Result<T>(_pass, value, _error);
    }
    
    /// <inheritdoc cref="IEquatable{T}"/>
    public bool Equals(Result result) => result._pass == _pass;

    /// <inheritdoc cref="IEquatable{T}"/>
    public bool Equals(bool pass) => pass == _pass;

    public override bool Equals(object? obj)
    {
        if (obj is Result result)
            return Equals(result);
        if (obj is bool pass)
            return Equals(pass);
        return false;
    }

    public override int GetHashCode() => throw new InvalidOperationException();

    public override string ToString()
    {
        if (_pass)
            return bool.TrueString;
        if (_error is null)
            return bool.FalseString;
        return $"{_error.GetType().Name}: {_error.Message}";
    }
}