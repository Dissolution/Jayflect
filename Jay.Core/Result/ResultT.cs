using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Jay;

/// <summary>
///
/// Use this by returning a <typeparamref name="T"/> value or an <see cref="Exception"/>
/// </summary>
public readonly partial struct Result<T> : IEquatable<Result<T>>,
                                           IEquatable<T?>,
                                           // Acts like option<T>
                                           IEnumerable<T?>
{
    /* Just as Result, these fields were chosen for the same criteria. */
    internal readonly bool _pass;
    internal readonly T? _value;
    internal readonly Exception? _error;

    internal Result(bool pass, T? value, Exception? error)
    {
        _pass = pass;
        _value = value;
        _error = error;
    }

    internal T? GetValue()
    {
        if (_pass)
            return _value;
        throw new InvalidOperationException("A failed Result has no Value");
    }

    /// <summary>
    /// Throws the attached <see cref="Exception"/> if this is a failed <see cref="Result{T}"/>
    /// </summary>
    /// <exception cref="Exception">The attached exception.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ThrowIfFailed()
    {
        if (!_pass)
        {
            throw (_error ?? new Exception(Result.DefaultErrorMessage));
        }
    }

    /// <summary>
    /// Throws the attached <see cref="Exception"/> if this is a failed <see cref="Result{T}"/>,
    /// otherwise retrieves <paramref name="value"/>.
    /// </summary>
    /// <exception cref="Exception">The attached exception.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ThrowIfFailed(out T value)
    {
        if (!_pass)
        {
            throw (_error ?? new Exception(Result.DefaultErrorMessage));
        }
        value = _value!;
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

        error = (_error ?? new Exception(Result.DefaultErrorMessage));
        return true;
    }
    
    /// <summary>
    /// Try to get the contained <paramref name="value"/>
    /// </summary>
    /// <param name="value">The contained value or <see langword="default{T}"> if this is a failed <see cref="Result{T}"/></param>
    /// <returns>The <see cref="Result"/> of retreiving <paramref name="value"/></returns>
    public Result TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return new Result(_pass, _error);
    }

    /// <inheritdoc cref="IEquatable{T}"/>
    public bool Equals(Result<T> result)
    {
        if (_pass)
        {
            return result._pass && EqualityComparer<T>.Default.Equals(_value, result._value);
        }
        return result._pass == false;
    }

    /// <inheritdoc cref="IEquatable{T}"/>
    public bool Equals(Result result) => result._pass == _pass;

    /// <inheritdoc cref="IEquatable{T}"/>
    public bool Equals(bool pass) => pass == _pass;

    /// <inheritdoc cref="IEquatable{T}"/>
    public bool Equals(T? value) => _pass && EqualityComparer<T>.Default.Equals(_value, value);

    public IEnumerator<T?> GetEnumerator()
    {
        if (_pass)
        {
            yield return _value;
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override bool Equals(object? obj)
    {
        if (obj is Result<T> resultT)
            return Equals(resultT);
        if (obj is Result result)
            return Equals(result);
        if (obj is bool pass)
            return Equals(pass);
        if (obj is T value)
            return Equals(value);
        return false;
    }

    public override int GetHashCode() => throw new InvalidOperationException();

    public override string ToString()
    {
        if (_pass)
            return _value?.ToString() ?? bool.TrueString;
        if (_error is null)
            return bool.FalseString;
        return $"{_error.GetType().Name}: {_error.Message}";
    }
}