/* =============================================================================
 * File:   CodePointReader.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements a clone of the abstract `TextReader` class from Microsoft, but
 *   operating on Unicode code points instead of C# chars.
 * =============================================================================
 * Copyright (c) 2022 Cole Tobin
 *
 * This file is part of HyperLib.
 *
 * HyperLib is free software: you can redistribute it and/or modify it under the
 *   terms of the GNU General Public License as published by the Free Software
 *   Foundation, either version 3 of the License, or (at your option) any later
 *   version.
 *
 * HyperLib is distributed in the hope that it will be useful, but WITHOUT ANY
 *   WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 *   FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
 *   details.
 *
 * You should have received a copy of the GNU General Public License along with
 *   HyperLib. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System.Collections.Generic;
using System.IO;

namespace HyperLib.IO;

/// <summary>
/// A reader than can read Unicode code points.
/// </summary>
/// <remarks>
/// This is similar to the Microsoft provided <see cref="TextReader" />, but with a change to work on Unicode code
///   points ("runes") instead of <see cref="char" />s.
/// </remarks>
[PublicAPI]
public abstract class CodePointReader : IDisposable
{
    /// <summary>
    /// Close the reader, releasing all resources associated with it.
    /// </summary>
    public virtual void Close()
    {
        Dispose();
    }

    /// <summary>
    /// Peek the next Unicode code point in the stream.
    /// </summary>
    /// <returns>The next code point, or <c>-1</c> if the EOF is reached.</returns>
    public abstract int Peek();

    /// <summary>
    /// Read/consume the next Unicode code point in the stream.
    /// </summary>
    /// <returns>The next code point, or <c>-1</c> if the EOF is reached.</returns>
    public abstract int Read();

    /// <summary>
    /// Read multiple Unicode code points from the stream, and write them into the provided buffer.
    /// </summary>
    /// <param name="buffer">The buffer to write the read code points into.</param>
    /// <returns>
    /// The number of code points read.
    /// If the EOF is reached, this will be less than <paramref name="buffer" />'s length, and any values in the buffer
    ///   after this returned index will be untouched.
    /// </returns>
    public virtual int Read(Span<int> buffer)
    {
        int i = 0;
        while (i < buffer.Length)
        {
            int c = Read();
            if (c is -1)
                break;
            buffer[i] = c;
            i++;
        }

        return i;
    }

    /// <summary>
    /// Read a specified amount of Unicode code points from the stream, and write them into the provided buffer
    ///   beginning at the specified index.
    /// </summary>
    /// <param name="buffer">The buffer to write the read code points into.</param>
    /// <param name="index">The index in the buffer to begin writing.</param>
    /// <param name="count">The maximum number of code points to read.</param>
    /// <returns>
    /// The number of code points read.
    /// If the EOF is reached, this will be less than <paramref name="count" />.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// If <paramref name="index" /> and <paramref name="count" /> reach past the end of <paramref name="buffer" />.
    /// </exception>
    /// <exception cref="ArgumentNullException">If <paramref name="buffer" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index" /> is negative.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="count" /> is negative.</exception>
    public virtual int Read(int[]? buffer, int index, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), index, EM.ArgumentOutOfRange.ArgumentMustBeNonNegative);
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, EM.ArgumentOutOfRange.ArgumentMustBeNonNegative);
        if (index + count > buffer.Length)
            throw new ArgumentException(EM.Argument.IndexAndCountReachPastBufferEnd);

        int i = 0;
        while (i < count)
        {
            int c = Read();
            if (c is -1)
                break;
            buffer[index + i] = c;
            i++;
        }

        return i;
    }


    /// <summary>
    /// Read a specified amount of Unicode code points from the stream, and write them into the provided buffer
    ///   beginning at the specified index.
    /// This method will block until either: no more code points can be read, or the specified count is reached.
    /// </summary>
    /// <param name="buffer">The buffer to write the read code points into.</param>
    /// <param name="index">The index in the buffer to begin writing.</param>
    /// <param name="count">The maximum number of code points to read.</param>
    /// <returns>
    /// The number of code points read.
    /// If a call to <see cref="Read(int[], int, int)" /> returns zero at any point, this value will be less than
    ///   <paramref name="count" />.
    /// </returns>
    /// <remarks>
    /// This method works by making repeated calls to <see cref="Read(int[], int, int)" /> with different index and
    ///   count values.
    /// If any call returns zero, this method will exit prematurely.
    /// If an implementer wishes to wait between successive calls to <see cref="Read(int[], int, int)" />, they must do
    ///   so themselves.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// If <paramref name="index" /> and <paramref name="count" /> reach past the end of <paramref name="buffer" />.
    /// </exception>
    /// <exception cref="ArgumentNullException">If <paramref name="buffer" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index" /> is negative.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="count" /> is negative.</exception>
    public virtual int ReadBlock(int[]? buffer, int index, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), index, EM.ArgumentOutOfRange.ArgumentMustBeNonNegative);
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, EM.ArgumentOutOfRange.ArgumentMustBeNonNegative);
        if (index + count > buffer.Length)
            throw new ArgumentException(EM.Argument.IndexAndCountReachPastBufferEnd);

        int i = 0;
        while (true)
        {
            int n = Read(buffer, index + i, count - i);
            i += n;
            if (n == 0)
                break;
        }

        return i;
    }

    /// <summary>
    /// Read Unicode code points up to an end of line character (either <c>'\r'</c> or <c>'\n'</c>).
    /// The end of line character(s) are consumed, but not returned.
    /// </summary>
    /// <returns>
    /// The code points up to, but not including, the end of line character(s).
    /// If the EOF is reached before reading a single code point, <c>null</c> is returned instead.
    /// </returns>
    public virtual List<int>? ReadLine()
    {
        List<int> buffer = new();
        while (true)
        {
            int c = Read();

            if (c is -1)
                return buffer.Count is 0 ? null : buffer;

            if (c is '\r')
            {
                if (Peek() is '\n')
                    Read();
                return buffer;
            }

            if (c is '\n')
                return buffer;

            buffer.Add(c);
        }
    }

    /// <summary>
    /// Read Unicode code points up to the end of the stream.
    /// </summary>
    /// <returns>The code points up to the end of the stream.</returns>
    public virtual List<int> ReadToEnd()
    {
        List<int> buffer = new();

        while (true)
        {
            int c = Read();
            if (c is -1)
                break;

            buffer.Add(c);
        }

        return buffer;
    }

    /// <summary>
    /// Release all resources associated with this <see cref="CodePointReader" /> object.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Release all unmanaged resources associated with this <see cref="CodePointReader" /> object, and optionally
    ///   release the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release both managed and unmanaged resources, or <c>false</c> to release only the unmanaged
    ///   resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    { }
}
