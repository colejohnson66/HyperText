/* =============================================================================
 * File:   Utf16Reader.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements a CodePointReader that operates on a UTF-16 encoded `Stream`.
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
/// A <see cref="CodePointReader" /> that reads from a UTF-16 encoded stream.
/// </summary>
public class Utf16Reader : CodePointReader
{
    private readonly Stream _stream;
    private readonly bool _isLittleEndian;
    private int _peek = -1;

    /// <summary>
    /// Construct a new <see cref="Utf16Reader" /> object from a stream with a specified endianness.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="isLittleEndian">
    /// <c>true</c> if the code units are in little endian;
    /// <c>false</c> if they are in big endian.
    /// </param>
    /// <exception cref="ArgumentNullException">If <paramref name="stream" /> is <c>null</c>.</exception>
    public Utf16Reader(Stream stream, bool isLittleEndian)
    {
        _stream = stream;
        _isLittleEndian = isLittleEndian;
    }

    /// <inheritdoc />
    /// <exception cref="EndOfStreamException">If the EOF is reached in the middle of a code unit.</exception>
    /// <exception cref="EndOfStreamException">If the EOF is reached in the middle of a surrogate pair.</exception>
    /// <exception cref="InvalidDataException">If an unpaired surrogate is encountered.</exception>
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
    /// <exception cref="EndOfStreamException">If the EOF is reached in the middle of a code unit.</exception>
    /// <exception cref="EndOfStreamException">If the EOF is reached in the middle of a surrogate pair.</exception>
    /// <exception cref="InvalidDataException">If an unpaired surrogate is encountered.</exception>
    /// <exception cref="InvalidDataException">If an encoded code point is greater than <c>U+10FFFF</c>.</exception>
    public override int Read()
    {
        if (_peek > 0)
        {
            int peek = _peek;
            _peek = -1;
            return peek;
        }

        // ReSharper disable CommentTypo
        /* UTF-16 is an extension of UCS-2 to allow support for U+10000 and beyond.
         * A 16 bit word is read, and, if it is less than 0xD7FF or greater than 0xE000, it is a code point, and is
         *   returned.
         * If it is not in those ranges (i.e., it's between 0xD800 and 0xDFFF, inclusive), it is a surrogate code unit.
         * Surrogates MUST be encoded in pairs: a high surrogate and a low surrogate, in that order.
         * In addition, the high surrogate MUST be in the range 0xD800 and 0xDBFF, inclusive, and the low surrogate MUST
         *   be in the range 0xDC00 and 0xDFFF, inclusive.
         *
         * In visual form, the encoding/decoding of a code point into UTF-16 surrogates is so:
         *     CP = yyyy yyyy yy xx xxxx xxxx + 0x10000
         *          ├──────────┘ └──────────┤
         *    High  │  1101 10xx xxxx xxxx ─┘
         *    Low   └─ 1101 11yy yyyy yyyy
         *
         * In pseudo-code form, this means that a surrogate pair can be decoded as follows:
         *   // assuming we know a surrogate pair is to be read next
         *   high = ReadUInt16()
         *   low  = ReadUInt16()
         *   assert((high & 0xFC00) == 0xD800)
         *   assert((low  & 0xFC00) == 0xDC00)
         *   CP = 0x10000 + (((high & 0x3FF) << 10) | (low & 0x3FF))
         */
        // ReSharper restore CommentTypo

        int c0 = ReadWord();
        if (c0 is -1)
            return -1;

        if ((c0 & 0xFC00) != 0xD800)
            return c0; // not a surrogate

        int c1 = ReadWord();
        if (c1 is -1)
            throw new EndOfStreamException(EM.EndOfStream.InUtf16SurrogatePair);

        if ((c1 & 0xFC00) != 0xDC00)
            throw new InvalidDataException(EM.InvalidData.Utf16UnpairedSurrogate);

        int high = c0 & 0x3FF;
        int low = c1 & 0x3FF;
        int c = 0x10000 + ((high << 10) | low);

        if (c is < 0 or > 0x10FFFF)
            throw new InvalidDataException(EM.InvalidData.InvalidPlane);

        return c;
    }

    private int ReadWord()
    {
        int b0 = _stream.ReadByte();
        if (b0 is -1)
            return -1;

        int b1 = _stream.ReadByte();
        if (b1 is -1)
            throw new EndOfStreamException(EM.EndOfStream.InUtf16CodeUnit);

        return _isLittleEndian
            ? (b1 << 8) | b0
            : (b0 << 8) | b1;
    }
}
