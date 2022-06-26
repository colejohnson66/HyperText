/* =============================================================================
 * File:   RuneReader.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements an abstract reader similar to .NET's `System.Text.TextReader`,
 *   but operating on "Runes" instead of chars. This ensures that all read
 *   characters are valid Unicode scalars.
 * =============================================================================
 * Copyright (c) 2021-2022 Cole Tobin
 *
 * This file is part of CodePoint.
 *
 * CodePoint is free software: you can redistribute it and/or modify it under
 *   the terms of the GNU General Public License as published by the Free
 *   Software Foundation, either version 3 of the License, or (at your option)
 *   any later version.
 *
 * CodePoint is distributed in the hope that it will be useful, but WITHOUT ANY
 *   WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 *   FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
 *   details.
 *
 * You should have received a copy of the GNU General Public License along with
 *   CodePoint. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System.Text;

namespace CodePoint.IO;

public abstract class RuneReader : IDisposable
{
    /// <summary>Returns the next <see cref="Rune" /> from the input stream without consuming it.</summary>
    /// <returns>The peeked <see cref="Rune" />, or <c>null</c> if the end of the stream is reached.</returns>
    public abstract Rune? Peek();

    /// <summary>Consumes and returns the next <see cref="Rune" /> from the input stream.</summary>
    /// <returns>The peeked <see cref="Rune" />, or <c>null</c> if the end of the stream is reached.</returns>
    public abstract Rune? Read();

    /// <summary>
    /// Attempts to consume <paramref name="count" /> number of <see cref="Rune" />s from from the input stream while
    ///   storing them in <paramref name="buffer" />, beginning at the offset <paramref name="index" />.
    /// If the end of the stream is reached, <paramref name="buffer" /> is unchanged from the current index on.
    /// </summary>
    /// <param name="buffer">
    /// The array to store the <see cref="Rune" />s into.
    /// Indexes less than <paramref name="index" /> are unchanged.
    /// </param>
    /// <param name="index">The offset into <paramref name="buffer" /> to begin reading into.</param>
    /// <param name="count">The amount of <see cref="Rune" />s to attempt to read.</param>
    /// <returns>
    /// The number of <see cref="Rune" />s read from the input stream.
    /// If the end of the stream is reached along the way, this value will be less than <paramref name="count" />.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// If <paramref name="index" /> or <paramref name="count" /> are negative.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// If <paramref name="index" /> plus <paramref name="count" /> is greater than the length of
    ///   <paramref name="buffer" />.
    /// </exception>
    /// <exception cref="ArgumentNullException">If <paramref name="buffer" /> is <c>null</c>.</exception>
    public virtual int Read(Rune?[]? buffer, int index, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (index < 0)
            throw new ArgumentException("Index must be non-negative.");
        if (count < 0)
            throw new ArgumentException("Count must be non-negative.");
        if (index + count > buffer.Length)
            throw new ArgumentException("Buffer must be large enough to store the result.");

        for (int i = 0; i < count; i++)
        {
            Rune? r = Read();
            if (r is null)
                return i;
            buffer[index + i] = r;
        }
        return count;
    }

    public virtual int Read(Span<Rune?> buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            Rune? r = Read();
            if (r == null)
                return i;
            buffer[i] = r;
        }
        return buffer.Length;
    }

    public virtual void Close() => Dispose();

    protected virtual void Dispose(bool disposing)
    { }

    #region IDisposable
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
