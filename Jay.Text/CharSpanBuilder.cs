using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
// ReSharper disable UnusedParameter.Local

namespace Jay.Text;

/// <summary>Provides a handler used by the language compiler to process interpolated strings into <see cref="string"/> instances.</summary>
[InterpolatedStringHandler]
public ref struct CharSpanBuilder
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // becomes a constant when inputs are constant
    private static int GetStartingCapacity(int literalLength, int formattedCount) =>
        Math.Clamp(MINIMUM_CAPACITY, literalLength + (formattedCount * 16), MAXIMUM_CAPACITY);
    
    private const int MINIMUM_CAPACITY = 1024;
    private const int MAXIMUM_CAPACITY = 0x3FFFFFDF; // string.MaxLength < Array.MaxLength

    /// <summary>Array rented from the array pool and used to back <see cref="_chars"/>.</summary>
    private char[]? _charArray;
    /// <summary>The span to write into.</summary>
    private Span<char> _chars;
    /// <summary>Position at which to write the next character.</summary>
    private int _index;

    /// <summary>Gets a span of the written characters thus far.</summary>
    public Span<char> Written
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _chars.Slice(0, _index);
    }

    public Span<char> Available
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _chars.Slice(_index);
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _index;
        set => _index = Math.Clamp(0, value, Capacity);
    }

    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _chars.Length;
    }

    public CharSpanBuilder()
    {
        _chars = _charArray = ArrayPool<char>.Shared.Rent(MINIMUM_CAPACITY);
        _index = 0;
    }
    
    public CharSpanBuilder(Span<char> initialBuffer)
    {
        _chars = initialBuffer;
        _charArray = null;
        _index = 0;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public CharSpanBuilder(int literalLength, int formattedCount)
    {
        _chars = _charArray = ArrayPool<char>.Shared.Rent(GetStartingCapacity(literalLength, formattedCount));
        _index = 0;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public CharSpanBuilder(int literalLength, int formattedCount, Span<char> initialBuffer)
    {
        _chars = initialBuffer;
        _charArray = null;
        _index = 0;
    }

    #region Grow
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowThenCopy(scoped ReadOnlySpan<char> value)
    {
        GrowBy(value.Length);
        value.CopyTo(Available);
        _index += value.Length;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowBy(int additionalChars)
    {
        // This method is called when the remaining space (_chars.Length - _pos) is
        // insufficient to store a specific number of additional characters.  Thus, we
        // need to grow to at least that new total. GrowCore will handle growing by more
        // than that if possible.
        Debug.Assert(additionalChars > _chars.Length - _index);
        int newCapacity = Math.Clamp(MINIMUM_CAPACITY,
            Math.Max(_index + additionalChars, Capacity * 2),
            MAXIMUM_CAPACITY);
        GrowCore(newCapacity);
    }

    /// <summary>Grows the size of <see cref="_chars"/>.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)] // keep consumers as streamlined as possible
    private void GrowSome()
    {
        // This method is called when the remaining space in _chars isn't sufficient to continue
        // the operation.  Thus, we need at least one character beyond _chars.Length.  GrowCore
        // will handle growing by more than that if possible.
        int newCapacity = Math.Clamp(MINIMUM_CAPACITY, Capacity * 2, MAXIMUM_CAPACITY);
        GrowCore(newCapacity);
    }

    /// <summary>Grow the size of <see cref="_chars"/> to at least the specified <paramref name="minCapacity"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // but reuse this grow logic directly in both of the above grow routines
    private void GrowCore(int minCapacity)
    {
        char[] newArray = ArrayPool<char>.Shared.Rent(minCapacity);
        Written.CopyTo(newArray);

        char[]? toReturn = _charArray;
        _chars = _charArray = newArray;

        if (toReturn is not null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }
    #endregion
    
    #region Append
    private void AppendStringDirect(string text)
    {
        Debug.Assert(text is not null);
        if (!text.TryCopyTo(Available))
        {
            GrowThenCopy(text);
        }
    }

    /// <summary>Writes the specified string to the handler.</summary>
    /// <param name="text">The string to write.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLiteral(string text)
    {
        if (text.Length == 1)
        {
            int pos = _index;
            Span<char> chars = _chars;
            if ((uint)pos < (uint)chars.Length)
            {
                chars[pos] = text[0];
                _index = pos + 1;
            }
            else
            {
                GrowThenCopy(text);
            }
            return;
        }

        if (text.Length == 2)
        {
            int pos = _index;
            Span<char> chars = _chars;
            if ((uint)pos < chars.Length - 1)
            {
                chars[pos++] = text[0];
                chars[pos++] = text[1];
                _index = pos;
            }
            else
            {
                GrowThenCopy(text);
            }
            return;
        }

        AppendStringDirect(text);
    }

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AppendFormatted<T>(T? value)
    {
        string? str;
        if (value is IFormattable)
        {
            // If the value can format itself directly into our buffer, do so.
            if (value is ISpanFormattable)
            {
                int charsWritten;
                // constrained call avoiding boxing for value types
                while (!((ISpanFormattable)value).TryFormat(_chars.Slice(_index),
                           out charsWritten, default, default))
                {
                    GrowSome();
                }

                _index += charsWritten;
                return;
            }

            // constrained call avoiding boxing for value types
            str = ((IFormattable)value).ToString(default, default); 
        }
        else
        {
            str = value?.ToString();
        }

        if (str is not null)
        {
            AppendStringDirect(str);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AppendFormatted<T>(T value, string? format)
    {
        string? str;
        if (value is IFormattable)
        {
            // If the value can format itself directly into our buffer, do so.
            if (value is ISpanFormattable)
            {
                int charsWritten;
                // constrained call avoiding boxing for value types
                while (!((ISpanFormattable)value).TryFormat(_chars.Slice(_index),
                           out charsWritten,
                           format,
                           default)) 
                {
                    GrowSome();
                }

                _index += charsWritten;
                return;
            }

            // constrained call avoiding boxing for value types
            str = ((IFormattable)value).ToString(format, default);
        }
        else
        {
            str = value?.ToString();
        }

        if (str is not null)
        {
            AppendStringDirect(str);
        }
    }
    #endregion

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AppendFormatted(char ch)
    {
        int pos = _index;
        Span<char> chars = _chars;
        if ((uint)pos < (uint)chars.Length)
        {
            chars[pos] = ch;
            _index = pos + 1;
        }
        else
        {
            GrowThenCopy(new ReadOnlySpan<char>(in ch));
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AppendFormatted(ReadOnlySpan<char> value)
    {
        // Fast path for when the value fits in the current buffer
        if (value.TryCopyTo(Available))
        {
            _index += value.Length;
        }
        else
        {
            GrowThenCopy(value);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AppendFormatted(string? value)
    {
        if (value is not null)
            AppendLiteral(value);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AppendFormatted(object? obj) => AppendFormatted<object>(obj);
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AppendFormatted(object? value, string? format) => AppendFormatted<object?>(value, format);

    /// <summary>Clears the handler, returning any rented array to the pool.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // used only on a few hot paths
    public void Dispose()
    {
        char[]? toReturn = _charArray;
        this = default; // defensive clear
        if (toReturn is not null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) => false;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => 0;

    public string ToStringAndDispose()
    {
        string result = new string(Written);
        Dispose();
        return result;
    }

    public override string ToString() => new string(Written);
}