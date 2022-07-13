/* =============================================================================
 * File:   Utf32Reader.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements a CodePointReader that operates on a UTF-32 encoded `Stream`.
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

using System.IO;

namespace HyperLib.IO;

/// <summary>
/// A <see cref="CodePointReader" /> that reads from a UTF-32 encoded stream.
/// </summary>
public class Utf32Reader : CodePointReader
{
    private readonly Stream _stream;
    private readonly bool _isLittleEndian;
    private readonly bool _throwOnSurrogates;
    private int _peek = -1;

    /// <summary>
    /// Construct a new <see cref="Utf32Reader" /> object from a stream with a specified endianness.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="isLittleEndian">
    /// <c>true</c> if the code units are in little endian;
    /// <c>false</c> if they are in big endian.
    /// </param>
    /// <param name="throwOnSurrogates">
    /// <c>true</c> to throw if a surrogate (U+D800..U+DFFF, inclusive) is encountered during a read;
    /// <c>false</c> to return the surrogate instead.
    /// </param>
    /// <exception cref="ArgumentNullException">If <paramref name="stream" /> is <c>null</c>.</exception>
    public Utf32Reader(Stream stream, bool isLittleEndian, bool throwOnSurrogates)
    {
        _stream = stream;
        _isLittleEndian = isLittleEndian;
        _throwOnSurrogates = throwOnSurrogates;
    }

    /// <inheritdoc />
    /// <exception cref="EndOfStreamException">If the EOF is reached in the middle of a code point.</exception>
    /// <exception cref="InvalidDataException">If a surrogate is encountered.</exception>
    /// <exception cref="InvalidDataException">If an encoded code point is greater than <c>U+10FFFF</c>.</exception>
    public override int Peek()
    {
        if (_peek > 0)
            return _peek;

        // nothing in the buffer, so read one and save it
        int c = Read();
        _peek = c;
        return c;
    }

    /// <inheritdoc />
    /// <exception cref="EndOfStreamException">If the EOF is reached in the middle of a code point.</exception>
    /// <exception cref="InvalidDataException">If a surrogate is encountered.</exception>
    /// <exception cref="InvalidDataException">If an encoded code point is greater than <c>U+10FFFF</c>.</exception>
    public override int Read()
    {
        if (_peek > 0)
        {
            int peek = _peek;
            _peek = -1;
            return peek;
        }

        int b0 = _stream.ReadByte();
        if (b0 is -1)
            return -1;

        int b1 = _stream.ReadByte();
        if (b1 is -1)
            throw new EndOfStreamException(EM.EndOfStream.InUtf32CodePoint);

        int b2 = _stream.ReadByte();
        if (b2 is -1)
            throw new EndOfStreamException(EM.EndOfStream.InUtf32CodePoint);

        int b3 = _stream.ReadByte();
        if (b3 is -1)
            throw new EndOfStreamException(EM.EndOfStream.InUtf32CodePoint);

        int c = _isLittleEndian
            ? (b3 << 24) | (b2 << 16) | (b1 << 8) | b0
            : (b0 << 24) | (b1 << 16) | (b2 << 8) | b3;

        if (_throwOnSurrogates && c is >= 0xD800 and <= 0xDFFF)
            throw new InvalidDataException(EM.InvalidData.Utf8Surrogate);
        if (c is < 0 or > 0x10FFFF)
            throw new InvalidDataException(EM.InvalidData.InvalidPlane);

        return c;
    }
}
