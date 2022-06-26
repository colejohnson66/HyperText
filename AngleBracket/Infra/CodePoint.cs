/* =============================================================================
 * File:   CodePoint.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the code point definitions as defined in the Infra standard from
 *   WHATWG.
 *
 * As of the 6 May 2022 version, this is located in section 4.5 "Code points":
 *   <https://infra.spec.whatwg.org/#code-points>
 * =============================================================================
 * Copyright (c) 2021-2022 Cole Tobin
 *
 * This file is part of AngleBracket.
 *
 * AngleBracket is free software: you can redistribute it and/or modify it under
 *   the terms of the GNU Lesser General Public License as published by the Free
 *   Software Foundation, either version 3 of the License, or (at your option)
 *   any later version.
 *
 * AngleBracket is distributed in the hope that it will be useful, but WITHOUT
 *   ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 *   FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License
 *   for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 *   along with AngleBracket. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AngleBracket.Infra;

[PublicAPI]
[SuppressMessage("ReSharper", "CommentTypo")]
public static class CodePoint
{
    /// <summary>Determine if <paramref name="r" /> is a surrogate.</summary>
    /// <remarks>
    /// A surrogate is a code point that is in the range <c>U+D800</c> to <c>U+DFFF</c>, inclusive.
    /// https://infra.spec.whatwg.org/#surrogate
    /// </remarks>
    public static bool IsSurrogate(Rune? r) =>
        false; // all Runes are, by definition, not surrogates

    /// <summary>Determine if <paramref name="c" /> is a surrogate.</summary>
    /// <remarks>
    /// A surrogate is a code point that is in the range <c>U+D800</c> to <c>U+DFFF</c>, inclusive.
    /// https://infra.spec.whatwg.org/#surrogate
    /// </remarks>
    public static bool IsSurrogate(int c) =>
        c is >= 0xD800 and <= 0xDFFF;


    /// <summary>Determine if <paramref name="r" /> is a scalar value.</summary>
    /// <remarks>
    /// A scalar value is a code point that is not a surrogate.
    /// https://infra.spec.whatwg.org/#scalar-value
    /// </remarks>
    public static bool IsScalarValue(Rune? r) =>
        r.HasValue; // all Runes are, by definition, scalars (if not null)

    /// <summary>Determine if <paramref name="c" /> is a scalar value.</summary>
    /// <remarks>
    /// A scalar value is a code point that is not a surrogate.
    /// https://infra.spec.whatwg.org/#scalar-value
    /// </remarks>
    public static bool IsScalarValue(int c)
    {
        Debug.Assert(c >= 0);
        return !IsSurrogate(c);
    }


    /// <summary>Determine if <paramref name="r" /> is a noncharacter.</summary>
    /// <remarks>
    /// A noncharacter is a code point that is in the range <c>FDD0</c> to <c>U+FDEF</c>, inclusive, or <c>FFFE</c>,
    ///   <c>FFFF</c>, <c>1FFFE</c>, <c>1FFFF</c>, <c>2FFFE</c>, <c>2FFFF</c>, <c>3FFFE</c>, <c>3FFFF</c>, <c>4FFFE</c>,
    ///   <c>4FFFF</c>, <c>5FFFE</c>, <c>5FFFF</c>, <c>6FFFE</c>, <c>6FFFF</c>, <c>7FFFE</c>, <c>7FFFF</c>,
    ///   <c>8FFFE</c>, <c>8FFFF</c>, <c>9FFFE</c>, <c>9FFFF</c>, <c>AFFFE</c>, <c>AFFFF</c>, <c>BFFFE</c>,
    ///   <c>BFFFF</c>, <c>CFFFE</c>, <c>CFFFF</c>, <c>DFFFE</c>, <c>DFFFF</c>, <c>EFFFE</c>, <c>EFFFF</c>,
    ///   <c>FFFFE</c>, <c>FFFFF</c>, <c>10FFFE</c>, or <c>10FFFF.</c>
    /// https://infra.spec.whatwg.org/#noncharacter
    /// </remarks>
    public static bool IsNoncharacter(Rune? r)
    {
        // quick bail for these
        if (r == null)
            return false;
        if (r.Value.Value is >= 0xFDD0 and <= 0xFDEF)
            return true;

        // all others are of the form ((x << 16) | 0xFFFE) and ((x << 16) | 0xFFFF)
        //   (where x is 0 through 16 inclusive)
        int low16 = r.Value.Value & 0xFFFF; // all Runes are, by definition, in a valid plane
        return low16 is 0xFFFE or 0xFFFF;
    }

    /// <summary>Determine if <paramref name="c" /> is a noncharacter.</summary>
    /// <remarks>
    /// A noncharacter is a code point that is in the range <c>FDD0</c> to <c>U+FDEF</c>, inclusive, or <c>FFFE</c>,
    ///   <c>FFFF</c>, <c>1FFFE</c>, <c>1FFFF</c>, <c>2FFFE</c>, <c>2FFFF</c>, <c>3FFFE</c>, <c>3FFFF</c>, <c>4FFFE</c>,
    ///   <c>4FFFF</c>, <c>5FFFE</c>, <c>5FFFF</c>, <c>6FFFE</c>, <c>6FFFF</c>, <c>7FFFE</c>, <c>7FFFF</c>,
    ///   <c>8FFFE</c>, <c>8FFFF</c>, <c>9FFFE</c>, <c>9FFFF</c>, <c>AFFFE</c>, <c>AFFFF</c>, <c>BFFFE</c>,
    ///   <c>BFFFF</c>, <c>CFFFE</c>, <c>CFFFF</c>, <c>DFFFE</c>, <c>DFFFF</c>, <c>EFFFE</c>, <c>EFFFF</c>,
    ///   <c>FFFFE</c>, <c>FFFFF</c>, <c>10FFFE</c>, or <c>10FFFF.</c>
    /// https://infra.spec.whatwg.org/#noncharacter
    /// </remarks>
    public static bool IsNoncharacter(int c)
    {
        Debug.Assert(c >= 0);
        if (c is >= 0xFDD0 and <= 0xFDEF)
            return true;

        // all others are of the form ((x << 16) | 0xFFFE) and ((x << 16) | 0xFFFF)
        //   (where x is 0 through 16 inclusive)
        int high16 = c >> 16;
        int low16 = c & 0xFFFF;
        return high16 <= 0x10 && low16 is 0xFFFE or 0xFFFF;
    }


    /// <summary>Determine if <paramref name="r" /> is an ASCII code point.</summary>
    /// <remarks>
    /// An ASCII code point is a code point in the range <c>U+0000 NULL</c> to <c>U+007F DELETE</c>, inclusive.
    /// https://infra.spec.whatwg.org/#ascii-code-point
    /// </remarks>
    public static bool IsAsciiCodePoint(Rune? r) =>
        r?.Value is <= 0x7F;

    /// <summary>Determine if <paramref name="c" /> is an ASCII code point.</summary>
    /// <remarks>
    /// An ASCII code point is a code point in the range <c>U+0000 NULL</c> to <c>U+007F DELETE</c>, inclusive.
    /// https://infra.spec.whatwg.org/#ascii-code-point
    /// </remarks>
    public static bool IsAsciiCodePoint(int c)
    {
        Debug.Assert(c >= 0);
        return c <= 0x7F;
    }


    /// <summary>Determine if <paramref name="r" /> is an ASCII tab or newline.</summary>
    /// <remarks>
    /// An ASCII tab or newline is <c>U+0009 TAB</c>, <c>U+000A LF</c>, or <c>U+000D CR</c>.
    /// https://infra.spec.whatwg.org/#ascii-tab-or-newline
    /// </remarks>
    public static bool IsAsciiTabOrNewLine(Rune? r) =>
        r?.Value is '\t' or '\n' or '\r';

    /// <summary>Determine if <paramref name="c" /> is an ASCII tab or newline.</summary>
    /// <remarks>
    /// An ASCII tab or newline is <c>U+0009 TAB</c>, <c>U+000A LF</c>, or <c>U+000D CR</c>.
    /// https://infra.spec.whatwg.org/#ascii-tab-or-newline
    /// </remarks>
    public static bool IsAsciiTabOrNewLine(int c)
    {
        Debug.Assert(c >= 0);
        return c is '\t' or '\n' or '\r';
    }


    /// <summary>Determine if <paramref name="r" /> is ASCII whitespace.</summary>
    /// <remarks>
    /// ASCII whitespace is <c>U+0009 TAB</c>, <c>U+000A LF</c>, <c>U+000C FF</c>, <c>U+000D CR</c>, or
    ///   <c>U+0020 SPACE</c>.
    /// https://infra.spec.whatwg.org/#ascii-whitespace
    /// </remarks>
    public static bool IsAsciiWhitespace(Rune? r) =>
        r?.Value is '\t' or '\n' or '\f' or '\r' or ' ';

    /// <summary>Determine if <paramref name="c" /> is ASCII whitespace.</summary>
    /// <remarks>
    /// ASCII whitespace is <c>U+0009 TAB</c>, <c>U+000A LF</c>, <c>U+000C FF</c>, <c>U+000D CR</c>, or
    ///   <c>U+0020 SPACE</c>.
    /// https://infra.spec.whatwg.org/#ascii-whitespace
    /// </remarks>
    public static bool IsAsciiWhitespace(int c)
    {
        Debug.Assert(c >= 0);
        return c is '\t' or '\n' or '\f' or '\r' or ' ';
    }


    /// <summary>Determine if <paramref name="r" /> is a C0 control.</summary>
    /// <remarks>
    /// A C0 control is a code point in the range <c>U+0000 NULL</c> to <c>U+001F INFORMATION SEPARATOR ONE</c>,
    ///   inclusive.
    /// https://infra.spec.whatwg.org/#c0-control
    /// </remarks>
    public static bool IsC0Control(Rune? r) =>
        r?.Value is <= 0x1F;

    /// <summary>Determine if <paramref name="c" /> is a C0 control.</summary>
    /// <remarks>
    /// A C0 control is a code point in the range <c>U+0000 NULL</c> to <c>U+001F INFORMATION SEPARATOR ONE</c>,
    ///   inclusive.
    /// https://infra.spec.whatwg.org/#c0-control
    /// </remarks>
    public static bool IsC0Control(int c)
    {
        Debug.Assert(c >= 0);
        return c <= 0x1F;
    }


    /// <summary>Determine if <paramref name="r" /> is a C0 control or space.</summary>
    /// <remarks>
    /// A C0 control or space is a C0 control or <c>U+0020 SPACE</c>.
    /// https://infra.spec.whatwg.org/#c0-control-or-space
    /// </remarks>
    public static bool IsC0ControlOrSpace(Rune? r) =>
        r?.Value is <= 0x20;

    /// <summary>Determine if <paramref name="c" /> is a C0 control or space.</summary>
    /// <remarks>
    /// A C0 control or space is a C0 control or <c>U+0020 SPACE</c>.
    /// https://infra.spec.whatwg.org/#c0-control-or-space
    /// </remarks>
    public static bool IsC0ControlOrSpace(int c)
    {
        Debug.Assert(c >= 0);
        return c <= 0x20;
    }


    /// <summary>Determine if <paramref name="r" /> is a control.</summary>
    /// <remarks>
    /// A control is a C0 control or a code point in the range <c>U+007F DELETE</c> to <c>U+009F APPLICATION PROGRAM
    ///   COMMAND</c>, inclusive.
    /// https://infra.spec.whatwg.org/#control
    /// </remarks>
    public static bool IsControl(Rune? r) =>
        r?.Value is <= 0x1F or (>= 0x7F and <= 0x9F);

    /// <summary>Determine if <paramref name="c" /> is a control.</summary>
    /// <remarks>
    /// A control is a C0 control or a code point in the range <c>U+007F DELETE</c> to <c>U+009F APPLICATION PROGRAM
    ///   COMMAND</c>, inclusive.
    /// https://infra.spec.whatwg.org/#control
    /// </remarks>
    public static bool IsControl(int c)
    {
        Debug.Assert(c >= 0);
        return c is <= 0x1F or (>= 0x7F and <= 0x9F);
    }


    /// <summary>Determine if <paramref name="r" /> is an ASCII digit.</summary>
    /// <remarks>
    /// An ASCII digit is a code point in the range <c>U+0030 (0)</c> to <c>U+0039 (9)</c>, inclusive.
    /// https://infra.spec.whatwg.org/#ascii-digit
    /// </remarks>
    public static bool IsAsciiDigit(Rune? r) =>
        r?.Value is >= '0' and <= '9';

    /// <summary>Determine if <paramref name="c" /> is an ASCII digit.</summary>
    /// <remarks>
    /// An ASCII digit is a code point in the range <c>U+0030 (0)</c> to <c>U+0039 (9)</c>, inclusive.
    /// https://infra.spec.whatwg.org/#ascii-digit
    /// </remarks>
    public static bool IsAsciiDigit(int c)
    {
        Debug.Assert(c >= 0);
        return c is >= '0' and <= '9';
    }


    /// <summary>Determine if <paramref name="r" /> is an ASCII upper hex digit.</summary>
    /// <remarks>
    /// An ASCII upper hex digit is an ASCII digit or a code point in the range <c>U+0041 (A)</c> to <c>U+0046 (F)</c>,
    ///   inclusive.
    /// https://infra.spec.whatwg.org/#ascii-upper-hex-digit
    /// </remarks>
    public static bool IsAsciiUpperHexDigit(Rune? r) =>
        r?.Value is (>= '0' and <= '9') or (>= 'A' and <= 'F');

    /// <summary>Determine if <paramref name="c" /> is an ASCII upper hex digit.</summary>
    /// <remarks>
    /// An ASCII upper hex digit is an ASCII digit or a code point in the range <c>U+0041 (A)</c> to <c>U+0046 (F)</c>,
    ///   inclusive.
    /// https://infra.spec.whatwg.org/#ascii-upper-hex-digit
    /// </remarks>
    public static bool IsAsciiUpperHexDigit(int c)
    {
        Debug.Assert(c >= 0);
        return c is (>= '0' and <= '9') or (>= 'A' and <= 'F');
    }


    /// <summary>Determine if <paramref name="r" /> is an ASCII lower hex digit.</summary>
    /// <remarks>
    /// An ASCII lower hex digit is an ASCII digit or a code point in the range <c>U+0061 (a)</c> to <c>U+00666 (f)</c>,
    ///   inclusive.
    /// https://infra.spec.whatwg.org/#ascii-lower-hex-digit
    /// </remarks>
    public static bool IsAsciiLowerHexDigit(Rune? r) =>
        r?.Value is (>= '0' and <= '9') or (>= 'a' and <= 'f');

    /// <summary>Determine if <paramref name="c" /> is an ASCII lower hex digit.</summary>
    /// <remarks>
    /// An ASCII lower hex digit is an ASCII digit or a code point in the range <c>U+0061 (a)</c> to <c>U+00666 (f)</c>,
    ///   inclusive.
    /// https://infra.spec.whatwg.org/#ascii-lower-hex-digit
    /// </remarks>
    public static bool IsAsciiLowerHexDigit(int c)
    {
        Debug.Assert(c >= 0);
        return c is (>= '0' and <= '9') or (>= 'a' and <= 'f');
    }


    /// <summary>Determine if <paramref name="r" /> is an ASCII hex digit.</summary>
    /// <remarks>
    /// An ASCII hex digit is an ASCII upper hex digit or ASCII lower hex digit.
    /// https://infra.spec.whatwg.org/#ascii-hex-digit
    /// </remarks>
    public static bool IsAsciiHexDigit(Rune? r) =>
        r?.Value is (>= '0' and <= '9') or (>= 'A' and <= 'F') or (>= 'a' and <= 'f');

    /// <summary>Determine if <paramref name="c" /> is an ASCII hex digit.</summary>
    /// <remarks>
    /// An ASCII hex digit is an ASCII upper hex digit or ASCII lower hex digit.
    /// https://infra.spec.whatwg.org/#ascii-hex-digit
    /// </remarks>
    public static bool IsAsciiHexDigit(int c)
    {
        Debug.Assert(c >= 0);
        return c is (>= '0' and <= '9') or (>= 'A' and <= 'F') or (>= 'a' and <= 'f');
    }


    /// <summary>Determine if <paramref name="r" /> is an ASCII upper alpha.</summary>
    /// <remarks>
    /// An ASCII upper alpha is a code point in the range <c>U+0041 (A)</c> to <c>U+005A (Z)</c>, inclusive.
    /// https://infra.spec.whatwg.org/#ascii-upper-alpha
    /// </remarks>
    public static bool IsAsciiUpperAlpha(Rune? r) =>
        r?.Value is >= 'A' and <= 'Z';

    /// <summary>Determine if <paramref name="c" /> is an ASCII upper alpha.</summary>
    /// <remarks>
    /// An ASCII upper alpha is a code point in the range <c>U+0041 (A)</c> to <c>U+005A (Z)</c>, inclusive.
    /// https://infra.spec.whatwg.org/#ascii-upper-alpha
    /// </remarks>
    public static bool IsAsciiUpperAlpha(int c)
    {
        Debug.Assert(c >= 0);
        return c is >= 'A' and <= 'Z';
    }


    /// <summary>Determine if <paramref name="r" /> is an ASCII lower alpha.</summary>
    /// <remarks>
    /// An ASCII lower alpha is a code point in the range <c>U+0061 (a)</c> to <c>U+006A (z)</c>, inclusive.
    /// https://infra.spec.whatwg.org/#ascii-lower-alpha
    /// </remarks>
    public static bool IsAsciiLowerAlpha(Rune? r) =>
        r?.Value is >= 'a' and <= 'z';

    /// <summary>Determine if <paramref name="c" /> is an ASCII lower alpha.</summary>
    /// <remarks>
    /// An ASCII lower alpha is a code point in the range <c>U+0061 (a)</c> to <c>U+006A (z)</c>, inclusive.
    /// https://infra.spec.whatwg.org/#ascii-lower-alpha
    /// </remarks>
    public static bool IsAsciiLowerAlpha(int c)
    {
        Debug.Assert(c >= 0);
        return c is >= 'a' and <= 'z';
    }


    /// <summary>Determine if <paramref name="r" /> is an ASCII alpha.</summary>
    /// <remarks>
    /// An ASCII alpha is an ASCII upper alpha or ASCII lower alpha.
    /// https://infra.spec.whatwg.org/#ascii-alpha
    /// </remarks>
    public static bool IsAsciiAlpha(Rune? r) =>
        r?.Value is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z');

    /// <summary>Determine if <paramref name="c" /> is an ASCII alpha.</summary>
    /// <remarks>
    /// An ASCII alpha is an ASCII upper alpha or ASCII lower alpha.
    /// https://infra.spec.whatwg.org/#ascii-alpha
    /// </remarks>
    public static bool IsAsciiAlpha(int c)
    {
        Debug.Assert(c >= 0);
        return c is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z');
    }


    /// <summary>Determine if <paramref name="r" /> is an ASCII alphanumeric.</summary>
    /// <remarks>
    /// An ASCII alphanumeric is an ASCII digit or ASCII alpha.
    /// https://infra.spec.whatwg.org/#ascii-alphanumeric
    /// </remarks>
    public static bool IsAsciiAlphanumeric(Rune? r) =>
        r?.Value is (>= '0' and <= '9') or (>= 'A' and <= 'Z') or (>= 'a' and <= 'z');

    /// <summary>Determine if <paramref name="c" /> is an ASCII alphanumeric.</summary>
    /// <remarks>
    /// An ASCII alphanumeric is an ASCII digit or ASCII alpha.
    /// https://infra.spec.whatwg.org/#ascii-alphanumeric
    /// </remarks>
    public static bool IsAsciiAlphanumeric(int c)
    {
        Debug.Assert(c >= 0);
        return c is (>= '0' and <= '9') or (>= 'A' and <= 'Z') or (>= 'a' and <= 'z');
    }
}
