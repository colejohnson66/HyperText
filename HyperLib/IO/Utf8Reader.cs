/* =============================================================================
 * File:   Utf8Reader.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements a CodePointReader that operates on a UTF-8 encoded `Stream`.
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
/// A <see cref="CodePointReader" /> that reads from a UTF-8 encoded stream.
/// </summary>
public class Utf8Reader : CodePointReader
{
    private readonly Stream _stream;
    private readonly bool _throwOnOverlong;
    private readonly bool _throwOnSurrogates;
    private int _peek = -1;

    /// <summary>
    /// Construct a new <see cref="Utf8Reader" /> object from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="stream" /> is <c>null</c>.</exception>
    public Utf8Reader(Stream? stream)
        : this(stream, true, true)
    { }

    // ReSharper disable once CommentTypo
    /// <summary>
    /// Construct a new <see cref="Utf8Reader" /> object from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="throwOnOverlong">
    /// <c>true</c> to throw if an "overlong encoding" is encountered during a read, such as an ASCII code point being
    ///   encoded in two or more bytes.
    /// <c>false</c> to return the invalidly encoded code point.
    /// However, if the first byte indicates more than four bytes (allowed by RFC 2279, but prohibited by RFC 3629), an
    ///   exception will still be thrown.
    /// </param>
    /// <param name="throwOnSurrogates">
    /// <c>true</c> to throw if a surrogate (U+D800..U+DFFF, inclusive) is encountered during a read;
    /// <c>false</c> to return the surrogate instead.
    /// </param>
    /// <exception cref="ArgumentNullException">If <paramref name="stream" /> is <c>null</c>.</exception>
    public Utf8Reader(Stream? stream, bool throwOnOverlong, bool throwOnSurrogates)
    {
        ArgumentNullException.ThrowIfNull(stream);

        _stream = stream;
        _throwOnOverlong = throwOnOverlong;
        _throwOnSurrogates = throwOnSurrogates;
    }

    /// <inheritdoc />
    /// <exception cref="EndOfStreamException">
    /// If the EOF is reached in the middle of a multibyte code point.
    /// </exception>
    /// <exception cref="InvalidDataException">If the first byte is invalid.</exception>
    /// <exception cref="InvalidDataException">If an invalid continuation byte is encountered.</exception>
    /// <exception cref="InvalidDataException">If an overlong code point is encountered.</exception>
    /// <exception cref="InvalidDataException">If a surrogate code point is encountered.</exception>
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
    /// <exception cref="EndOfStreamException">
    /// If the EOF is reached in the middle of a multibyte code point.
    /// </exception>
    /// <exception cref="InvalidDataException">If the first byte is invalid.</exception>
    /// <exception cref="InvalidDataException">If an invalid continuation byte is encountered.</exception>
    /// <exception cref="InvalidDataException">If an overlong code point is encountered.</exception>
    /// <exception cref="InvalidDataException">If a surrogate code point is encountered.</exception>
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
        /* UTF-8 is encoded as follows:
         * ┌──────────┬──────────┬───────────┬───────────┬───────────┬───────────┐
         * │ First CP │  Last CP │   Byte 1  │   Byte 2  │   Byte 3  │   Byte 4  │
         * ├──────────┼──────────┼───────────┼───────────┼───────────┼───────────┤
         * │  U+ 0000 │ U+  007F │ 0xxx xxxx │           │           │           │
         * ├──────────┼──────────┼───────────┼───────────┼───────────┼───────────┤
         * │  U+ 0080 │ U+  07FF │ 110x xxxx │ 10xx xxxx │           │           │
         * ├──────────┼──────────┼───────────┼───────────┼───────────┼───────────┤
         * │  U+ 0800 │ U+  FFFF │ 1110 xxxx │ 10xx xxxx │ 10xx xxxx │           │
         * ├──────────┼──────────┼───────────┼───────────┼───────────┼───────────┤
         * │  U+10000 │ U+10FFFF │ 1111 0xxx │ 10xx xxxx │ 10xx xxxx │ 10xx xxxx │
         * └──────────┴──────────┴───────────┴───────────┴───────────┴───────────┘
         *
         * Of note:
         *   - A code point must be encoded in the least number of bytes possible.
         *     I.E. a space (U+0020) MUST be encoded as a single byte, not as two, three, or four.
         *     A consequence of this is that no more than four bytes per code point are allowed.
         *   - Surrogates (U+D800..U+DFFF) are not allowed.
         * However, both can be ignored by flags in the constructor.
         */
        // ReSharper restore CommentTypo

        int b0 = _stream.ReadByte();
        if (b0 is -1)
            return -1;
        if ((b0 & 0x80) == 0)
            return b0; // one byte code point (ASCII)

        if ((b0 & 0xE0) == 0xC0)
        {
            // two bytes
            int c = (b0 & 0x1F) << 6;

            int b1 = _stream.ReadByte();
            if (b1 is -1)
                throw new EndOfStreamException(EM.EndOfStream.InUtf8CodePoint);
            if ((b1 & 0xC0) != 0x80)
                throw new InvalidDataException(EM.InvalidData.Utf8InvalidContinuation);

            c |= b1 & 0x3F;

            // two byte encoded code points can never be surrogates, so don't bother checking
            if (_throwOnOverlong && c < 0x80)
                throw new InvalidDataException(EM.InvalidData.Utf8Overlong);

            return c;
        }

        if ((b0 & 0xF0) == 0xE0)
        {
            // three bytes
            int c = (b0 & 0x0F) << 12;

            int b1 = _stream.ReadByte();
            int b2 = _stream.ReadByte();
            if (b1 is -1 || b2 is -1)
                throw new EndOfStreamException(EM.EndOfStream.InUtf8CodePoint);
            if ((b1 & 0xC0) != 0x80 || (b2 & 0xC0) != 0x80)
                throw new InvalidDataException(EM.InvalidData.Utf8InvalidContinuation);

            c |= (b1 & 0x3F) << 6;
            c |= b2 & 0x3F;

            if (_throwOnOverlong && c < 0x800)
                throw new InvalidDataException(EM.InvalidData.Utf8Overlong);
            if (_throwOnSurrogates && c is >= 0xD800 and <= 0xDFFF)
                throw new InvalidDataException(EM.InvalidData.Utf8Surrogate);

            return c;
        }

        if ((b0 & 0xF8) == 0xF0)
        {
            // four bytes
            int c = (b0 & 7) << 18;

            int b1 = _stream.ReadByte();
            int b2 = _stream.ReadByte();
            int b3 = _stream.ReadByte();
            if (b1 is -1 || b2 is -1 || b3 is -1)
                throw new EndOfStreamException(EM.EndOfStream.InUtf8CodePoint);
            if ((b1 & 0xC0) != 0x80 || (b2 & 0xC0) != 0x80 || (b3 & 0xC0) != 0x80)
                throw new InvalidDataException(EM.InvalidData.Utf8InvalidContinuation);

            c |= (b1 & 0x3F) << 12;
            c |= (b2 & 0x3F) << 6;
            c |= b3 & 0x3F;

            if (_throwOnOverlong && c < 0x10000)
                throw new InvalidDataException(EM.InvalidData.Utf8Overlong);
            if (_throwOnSurrogates && c is >= 0xD800 and <= 0xDFFF)
                throw new InvalidDataException(EM.InvalidData.Utf8Surrogate);

            if (c > 0x10FFFF)
                throw new InvalidDataException(EM.InvalidData.InvalidPlane);

            return c;
        }

        throw new InvalidDataException(EM.InvalidData.Utf8InvalidByte0);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _stream.Dispose();
    }
}
