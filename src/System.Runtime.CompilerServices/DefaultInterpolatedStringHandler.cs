// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Provides a handler used by the language compiler to process interpolated strings into <see cref="string"/> instances.
/// </summary>
[InterpolatedStringHandler]
public ref struct DefaultInterpolatedStringHandler
{
    /// <summary>
    /// Expected average length of formatted data used for an individual interpolation expression result.
    /// </summary>
    private const int GuessedLengthPerHole = 11;

    /// <summary>
    /// Minimum size array to rent from the pool.
    /// </summary>
    private const int MinimumArrayPoolLength = 256;

    /// <summary>
    /// Optional provider to pass to <see cref="IFormattable.ToString(string, IFormatProvider)"/> calls.
    /// </summary>
    private readonly IFormatProvider? _provider;

    /// <summary>
    /// Array rented from the array pool and used to back <see cref="_chars"/>.
    /// </summary>
    private char[]? _arrayToReturnToPool;

    /// <summary>
    /// The <see cref="Span{T}"/> to write into.
    /// </summary>
    private Span<char> _chars;

    /// <summary>
    /// Position at which to write the next character.
    /// </summary>
    private int _pos;

    /// <summary>
    /// Whether <see cref="_provider"/> provides an <see cref="ICustomFormatter"/>.
    /// </summary>
    private readonly bool _hasCustomFormatter;

    /// <summary>
    /// Creates a handler used to translate an interpolated string into a <see cref="string"/>.
    /// </summary>
    /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
    /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
    /// <remarks>This is intended to be called only by compiler-generated code. Arguments are not validated as they'd otherwise be for members intended to be used directly.</remarks>
    public DefaultInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        _provider = null;
        _chars = _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(GetDefaultLength(literalLength, formattedCount));
        _pos = 0;
        _hasCustomFormatter = false;
    }

    /// <summary>
    /// Creates a handler used to translate an interpolated string into a <see cref="string"/>.
    /// </summary>
    /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
    /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <remarks>This is intended to be called only by compiler-generated code. Arguments are not validated as they'd otherwise be for members intended to be used directly.</remarks>
    public DefaultInterpolatedStringHandler(int literalLength, int formattedCount, IFormatProvider? provider)
    {
        _provider = provider;
        _chars = _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(GetDefaultLength(literalLength, formattedCount));
        _pos = 0;
        _hasCustomFormatter = provider is not null && HasCustomFormatter(provider);
    }

    /// <summary>
    /// Creates a handler used to translate an interpolated string into a <see cref="string"/>.
    /// </summary>
    /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
    /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="initialBuffer">A buffer temporarily transferred to the handler for use as part of its formatting.  Contents may be overwritten.</param>
    /// <remarks>This is intended to be called only by compiler-generated code. Arguments are not validated as they'd otherwise be for members intended to be used directly.</remarks>
    public DefaultInterpolatedStringHandler(int literalLength, int formattedCount, IFormatProvider? provider, Span<char> initialBuffer)
    {
        _provider = provider;
        _chars = initialBuffer;
        _arrayToReturnToPool = null;
        _pos = 0;
        _hasCustomFormatter = provider is not null && HasCustomFormatter(provider);
    }

    /// <summary>
    /// Derives a default length with which to seed the handler.
    /// </summary>
    /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
    /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetDefaultLength(int literalLength, int formattedCount)
    {
        return Math.Max(MinimumArrayPoolLength, literalLength + (formattedCount * GuessedLengthPerHole));
    }

    /// <summary>
    /// Gets the built <see cref="string"/>.
    /// </summary>
    /// <returns>The built string.</returns>
    public override string ToString() => Text.ToString();

    /// <summary>
    /// Gets the built <see cref="string"/> and clears the handler.
    /// </summary>
    /// <returns>The built string.</returns>
    public string ToStringAndClear()
    {
        string result = Text.ToString();

        Clear();

        return result;
    }

    /// <summary>
    /// Clears the handler, returning any rented array to the pool.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Clear()
    {
        char[]? toReturn = _arrayToReturnToPool;

        this = default;

        if (toReturn is not null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    /// <summary>
    /// Gets a span of the written characters thus far.
    /// </summary>
    private ReadOnlySpan<char> Text => _chars.Slice(0, _pos);

    /// <summary>
    /// Writes the specified string to the handler.
    /// </summary>
    /// <param name="value">The string to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void AppendLiteral(string value)
    {
        if (value.Length == 1)
        {
            Span<char> chars = _chars;
            int pos = _pos;

            if ((uint)pos < (uint)chars.Length)
            {
                chars[pos] = value[0];

                _pos = pos + 1;
            }
            else
            {
                GrowThenCopyString(value);
            }

            return;
        }

        if (value.Length == 2)
        {
            Span<char> chars = _chars;
            int pos = _pos;

            if ((uint)pos < chars.Length - 1)
            {
                fixed (char* p = value)
                {
                    Unsafe.WriteUnaligned(
                        ref Unsafe.As<char, byte>(ref Unsafe.Add(ref MemoryMarshal.GetReference(chars), pos)),
                        Unsafe.ReadUnaligned<int>(ref *(byte*)p));

                    _pos = pos + 2;
                }
            }
            else
            {
                GrowThenCopyString(value);
            }

            return;
        }

        AppendStringDirect(value);
    }

    /// <summary>
    /// Writes the specified string to the handler.
    /// </summary>
    /// <param name="value">The string to write.</param>
    private void AppendStringDirect(string value)
    {
        if (value.AsSpan().TryCopyTo(_chars.Slice(_pos)))
        {
            _pos += value.Length;
        }
        else
        {
            GrowThenCopyString(value);
        }
    }

    /// <summary>
    /// Writes the specified value to the handler.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    public void AppendFormatted<T>(T value)
    {
        // If there's a custom formatter, always use it
        if (_hasCustomFormatter)
        {
            AppendCustomFormatter(value, format: null);

            return;
        }


        string? s;

        if (value is IFormattable)
        {
            s = ((IFormattable)value).ToString(format: null, _provider);
        }
        else
        {
            s = value?.ToString();
        }

        if (s is not null)
        {
            AppendStringDirect(s);
        }
    }
    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <param name="format">The format string.</param>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    public void AppendFormatted<T>(T value, string? format)
    {
        // If there's a custom formatter, always use it
        if (_hasCustomFormatter)
        {
            AppendCustomFormatter(value, format);

            return;
        }

        string? s;

        if (value is IFormattable)
        {
            s = ((IFormattable)value).ToString(format, _provider);
        }
        else
        {
            s = value?.ToString();
        }

        if (s is not null)
        {
            AppendStringDirect(s);
        }
    }

    /// <summary>
    /// Writes the specified value to the handler.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value. If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    public void AppendFormatted<T>(T value, int alignment)
    {
        int startingPos = _pos;

        AppendFormatted(value);

        if (alignment != 0)
        {
            AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
        }
    }

    /// <summary>
    /// Writes the specified value to the handler.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="format">The format string.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value. If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    public void AppendFormatted<T>(T value, int alignment, string? format)
    {
        int startingPos = _pos;

        AppendFormatted(value, format);

        if (alignment != 0)
        {
            AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
        }
    }

    /// <summary>
    /// Writes the specified <see cref="bool"/> value to the handler.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void AppendFormatted(bool value)
    {
        int charsWritten;

        while (!value.TryFormat(_chars.Slice(_pos), out charsWritten))
        {
            Grow();
        }

        _pos += charsWritten;
    }

    /// <summary>
    /// Writes the specified <see cref="int"/> value to the handler.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void AppendFormatted(int value)
    {
        if (value >= 0)
        {
            AppendFormatted((uint)value);
        }
        else
        {
            AppendFormatted<int>(value);
        }
    }

    /// <summary>
    /// Writes the specified <see cref="uint"/> value to the handler.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void AppendFormatted(uint value)
    {
        int charsWritten;

        while (!value.TryFormat(_chars.Slice(_pos), out charsWritten))
        {
            Grow();
        }

        _pos += charsWritten;
    }

    /// <summary>
    /// Writes the specified character span to the handler.
    /// </summary>
    /// <param name="value">The span to write.</param>
    public void AppendFormatted(ReadOnlySpan<char> value)
    {
        if (value.TryCopyTo(_chars.Slice(_pos)))
        {
            _pos += value.Length;
        }
        else
        {
            GrowThenCopySpan(value);
        }
    }

    /// <summary>
    /// Writes the specified string of chars to the handler.
    /// </summary>
    /// <param name="value">The span to write.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value. If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    /// <param name="format">The format string.</param>
    public void AppendFormatted(ReadOnlySpan<char> value, int alignment = 0, string? format = null)
    {
        bool leftAlign = false;

        if (alignment < 0)
        {
            leftAlign = true;
            alignment = -alignment;
        }

        int paddingRequired = alignment - value.Length;

        if (paddingRequired <= 0)
        {
            AppendFormatted(value);

            return;
        }

        // Write the value along with the appropriate padding
        EnsureCapacityForAdditionalChars(value.Length + paddingRequired);

        if (leftAlign)
        {
            value.CopyTo(_chars.Slice(_pos));

            _pos += value.Length;
            _chars.Slice(_pos, paddingRequired).Fill(' ');
            _pos += paddingRequired;
        }
        else
        {
            _chars.Slice(_pos, paddingRequired).Fill(' ');
            _pos += paddingRequired;

            value.CopyTo(_chars.Slice(_pos));

            _pos += value.Length;
        }
    }

    /// <summary>
    /// Writes the specified value to the handler.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void AppendFormatted(string? value)
    {
        // Fast-path for no custom formatter and a non-null string that fits in the current destination buffer
        if (!_hasCustomFormatter &&
            value is not null &&
            value.AsSpan().TryCopyTo(_chars.Slice(_pos)))
        {
            _pos += value.Length;
        }
        else
        {
            AppendFormattedSlow(value);
        }
    }

    /// <summary>
    /// Writes the specified value to the handler.
    /// </summary>
    /// <param name="value">The value to write.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AppendFormattedSlow(string? value)
    {
        if (_hasCustomFormatter)
        {
            AppendCustomFormatter(value, format: null);
        }
        else if (value is not null)
        {
            EnsureCapacityForAdditionalChars(value.Length);

            value.AsSpan().CopyTo(_chars.Slice(_pos));

            _pos += value.Length;
        }
    }

    /// <summary>
    /// Writes the specified value to the handler.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value. If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    /// <param name="format">The format string.</param>
    public void AppendFormatted(string? value, int alignment = 0, string? format = null)
    {
        AppendFormatted<string?>(value, alignment, format);
    }

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    /// <param name="format">The format string.</param>
    public void AppendFormatted(object? value, int alignment = 0, string? format = null)
    {
        AppendFormatted<object?>(value, alignment, format);
    }

    /// <summary>
    /// Gets whether the provider provides a custom formatter.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HasCustomFormatter(IFormatProvider provider)
    {
        return
            provider.GetType() != typeof(CultureInfo) &&
            provider.GetFormat(typeof(ICustomFormatter)) != null;
    }

    /// <summary>
    /// Formats the value using the custom formatter from the provider.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="format">The format string.</param>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AppendCustomFormatter<T>(T value, string? format)
    {
        ICustomFormatter? formatter = (ICustomFormatter?)_provider!.GetFormat(typeof(ICustomFormatter));

        if (formatter is not null && formatter.Format(format, value, _provider) is string customFormatted)
        {
            AppendStringDirect(customFormatted);
        }
    }

    /// <summary>
    /// Handles adding any padding required for aligning a formatted value in an interpolation expression.</summary>
    /// <param name="startingPos">The position at which the written value started.</param>
    /// <param name="alignment">Non-zero minimum number of characters that should be written for this value. If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    private void AppendOrInsertAlignmentIfNeeded(int startingPos, int alignment)
    {
        int charsWritten = _pos - startingPos;
        bool leftAlign = false;

        if (alignment < 0)
        {
            leftAlign = true;
            alignment = -alignment;
        }

        int paddingNeeded = alignment - charsWritten;

        if (paddingNeeded > 0)
        {
            EnsureCapacityForAdditionalChars(paddingNeeded);

            if (leftAlign)
            {
                _chars.Slice(_pos, paddingNeeded).Fill(' ');
            }
            else
            {
                _chars.Slice(startingPos, charsWritten).CopyTo(_chars.Slice(startingPos + paddingNeeded));
                _chars.Slice(startingPos, paddingNeeded).Fill(' ');
            }

            _pos += paddingNeeded;
        }
    }

    /// <summary>
    /// Ensures <see cref="_chars"/> has the capacity to store <paramref name="additionalChars"/> beyond <see cref="_pos"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacityForAdditionalChars(int additionalChars)
    {
        if (_chars.Length - _pos < additionalChars)
        {
            Grow(additionalChars);
        }
    }

    /// <summary>
    /// Fallback for fast path in <see cref="AppendStringDirect"/> when there's not enough space in the destination.
    /// </summary>
    /// <param name="value">The string to write.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowThenCopyString(string value)
    {
        Grow(value.Length);

        value.AsSpan().CopyTo(_chars.Slice(_pos));

        _pos += value.Length;
    }

    /// <summary>
    /// Fallback for <see cref="AppendFormatted(ReadOnlySpan{char})"/> for when not enough space exists in the current buffer.
    /// </summary>
    /// <param name="value">The span to write.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowThenCopySpan(ReadOnlySpan<char> value)
    {
        Grow(value.Length);

        value.CopyTo(_chars.Slice(_pos));

        _pos += value.Length;
    }

    /// <summary>
    /// Grows <see cref="_chars"/> to have the capacity to store at least <paramref name="additionalChars"/> beyond <see cref="_pos"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalChars)
    {
        GrowCore((uint)_pos + (uint)additionalChars);
    }

    /// <summary>
    /// Grows the size of <see cref="_chars"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)] // keep consumers as streamlined as possible
    private void Grow()
    {
        GrowCore((uint)_chars.Length + 1);
    }

    /// <summary>
    /// Grow the size of <see cref="_chars"/> to at least the specified <paramref name="requiredMinCapacity"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GrowCore(uint requiredMinCapacity)
    {
        uint newCapacity = Math.Max(requiredMinCapacity, Math.Min((uint)_chars.Length * 2, 0x3FFFFFDF));
        int arraySize = (int)Math.Min(Math.Max(newCapacity, MinimumArrayPoolLength), int.MaxValue);
        char[] newArray = ArrayPool<char>.Shared.Rent(arraySize);

        _chars.Slice(0, _pos).CopyTo(newArray);

        char[]? toReturn = _arrayToReturnToPool;

        _chars = _arrayToReturnToPool = newArray;

        if (toReturn is not null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }
}