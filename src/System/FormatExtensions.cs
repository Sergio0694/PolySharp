// ------------------------------------------------------
// Copyright (C) Microsoft. All rights reserved.
// ------------------------------------------------------

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System;

/// <summary>
/// Extensions to port formatting logic for primitive types.
/// </summary>
public static class FormatExtensions
{
    /// <summary>
    /// Tries to format the <see cref="bool"/> value into the provided span of characters.
    /// </summary>
    /// <param name="destination">When this method returns, this instance's value formatted as a span of characters.</param>
    /// <param name="charsWritten">When this method returns, the number of characters that were written in <paramref name="destination"/>.</param>
    /// <returns>Whether the formatting was successful.</returns>
    public static bool TryFormat(this bool value, Span<char> destination, out int charsWritten)
    {
        if (value)
        {
            if ((uint)destination.Length > 3)
            {
                destination[0] = 'T';
                destination[1] = 'r';
                destination[2] = 'u';
                destination[3] = 'e';
                charsWritten = 4;

                return true;
            }
        }
        else
        {
            if ((uint)destination.Length > 4)
            {
                destination[0] = 'F';
                destination[1] = 'a';
                destination[2] = 'l';
                destination[3] = 's';
                destination[4] = 'e';
                charsWritten = 5;

                return true;
            }
        }

        charsWritten = 0;

        return false;
    }

    /// <summary>
    /// Tries to format the <see cref="uint"/> value into the provided span of characters.
    /// </summary>
    /// <param name="destination">When this method returns, this instance's value formatted as a span of characters.</param>
    /// <param name="charsWritten">When this method returns, the number of characters that were written in <paramref name="destination"/>.</param>
    /// <returns>Whether the formatting was successful.</returns>
    public static unsafe bool TryFormat(this uint value, Span<char> destination, out int charsWritten)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int CountDigits(uint value)
        {
            int digits = 1;

            if (value >= 100000)
            {
                value /= 100000;
                digits += 5;
            }

            if (value < 10)
            {
                // No-op
            }
            else if (value < 100)
            {
                digits++;
            }
            else if (value < 1000)
            {
                digits += 2;
            }
            else if (value < 10000)
            {
                digits += 3;
            }
            else
            {
                digits += 4;
            }

            return digits;
        }

        int bufferLength = CountDigits(value);

        if (bufferLength > destination.Length)
        {
            charsWritten = 0;

            return false;
        }

        charsWritten = bufferLength;

        fixed (char* buffer = &MemoryMarshal.GetReference(destination))
        {
            char* p = buffer + bufferLength;

            do
            {
                uint quotient = value / 10;
                uint remainder = value - (quotient * 10);

                value = quotient;

                *--p = (char)(remainder + '0');
            }
            while (value != 0);
        }

        return true;
    }
}
