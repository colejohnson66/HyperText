/* =============================================================================
 * File:   EM.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Contains various exception messages (hence, "EM") used by the various
 *   HyperText components.
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

using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace HyperLib;

// TODO: bring these into a resource file loaded dynamically
// TODO: auto-generate this file

/// <summary>
/// Various exception messages used by HyperText's various components.
/// </summary>
[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
public static class EM
{
    /// <summary>Exception messages for <see cref="ArgumentException" /> objects.</summary>
    public static class Argument
    {
        /// <summary>The index and count parameters reach past the end of the buffer.</summary>
        public static string IndexAndCountReachPastBufferEnd = "The index and count parameters reach past the end of the buffer.";
    }

    /// <summary>Exception messages for <see cref="ArgumentOutOfRangeException" /> objects.</summary>
    public static class ArgumentOutOfRange
    {
        /// <summary>Argument must be non-negative.</summary>
        public static string ArgumentMustBeNonNegative = "Argument must be non-negative.";
        /// <summary>Argument must be a valid Unicode code point.</summary>
        public static string ArgumentMustBeValidUnicode = "Argument must be a valid Unicode code point.";
    }

    /// <summary>Exception messages for <see cref="EndOfStreamException" /> objects.</summary>
    public static class EndOfStream
    {
        /// <summary>The EOF was reached in the middle of a UTF-8 encoded code point.</summary>
        public static string InUtf8CodePoint = "The EOF was reached in the middle of a UTF-8 encoded code point.";
        /// <summary>The EOF was reached in the middle of a UTF-16 code unit.</summary>
        public static string InUtf16CodeUnit = "The EOF was reached in the middle of a UTF-16 code unit.";
        /// <summary>The EOF was reached in the middle of a UTF-16 surrogate pair.</summary>
        public static string InUtf16SurrogatePair = "The EOF was reached in the middle of a UTF-16 surrogate pair.";
        /// <summary>The EOF was reached in the middle of a UTF-32 encoded code point.</summary>
        public static string InUtf32CodePoint = "The EOF was reached in the middle of a UTF-32 encoded code point.";
    }

    /// <summary>Exception messages for <see cref="InvalidDataException" /> objects.</summary>
    public static class InvalidData
    {
        /// <summary>A code point is in an invalid plane.</summary>
        public static string InvalidPlane = "A code point is in an invalid plane.";
        /// <summary>An invalid "first byte" of a UTF-8 was encountered.</summary>
        public static string Utf8InvalidByte0 = "An invalid first byte of a UTF-8 encoded code point was encountered.";
        /// <summary>An invalid UTF-8 continuation byte was encountered.</summary>
        public static string Utf8InvalidContinuation = "An invalid UTF-8 continuation byte was encountered.";
        /// <summary>An "overlong" UTF-8 encoded code point was encountered.</summary>
        public static string Utf8Overlong = "An \"overlong\" UTF-8 encoded code point was encountered.";
        /// <summary>A UTF-8 encoded surrogate code point was encountered.</summary>
        public static string Utf8Surrogate = "A UTF-8 encoded surrogate code point was encountered.";
        /// <summary>A UTF-16 encoded code point contained an unpaired surrogate.</summary>
        public static string Utf16UnpairedSurrogate = "A UTF-16 encoded code point contained an unpaired surrogate.";
    }
}
