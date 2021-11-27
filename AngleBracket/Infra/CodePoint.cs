/* =============================================================================
 * File:   CodePoint.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the code point definitions as defined in the Infra standard from
 *   WHATWG.
 *
 * As of November 2021, this is located in section 4.5 "Code points":
 * <https://infra.spec.whatwg.org/#code-points>
 * =============================================================================
 * Copyright (c) 2021 Cole Tobin
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

using System.Text;

namespace AngleBracket.Infra;

public static class CodePoint
{
    public static bool IsSurrogate(Rune? r)
    {
        // A surrogate is a code point that is in the range U+D800 to U+DFFF,
        //   inclusive.
        return false; // all Runes are, by definition, not surrogates
    }
    public static bool IsSurrogate(int c)
    {
        // A surrogate is a code point that is in the range U+D800 to U+DFFF,
        //   inclusive.
        return c is >= 0xD800 and <= 0xDFFF;
    }

    public static bool IsScalarValue(Rune? r)
    {
        // A scalar value is a code point that is not a surrogate.
        return r != null; // all Runes are, by definition, scalars
    }

    public static bool IsNoncharacter(Rune? r)
    {
        // A noncharacter is a code point that is in the range U+FDD0 to U+FDEF,
        //   inclusive, or U+FFFE, U+FFFF, U+1FFFE, U+1FFFF, U+2FFFE, U+2FFFF,
        //   U+3FFFE, U+3FFFF, U+4FFFE, U+4FFFF, U+5FFFE, U+5FFFF, U+6FFFE,
        //   U+6FFFF, U+7FFFE, U+7FFFF, U+8FFFE, U+8FFFF, U+9FFFE, U+9FFFF,
        //   U+AFFFE, U+AFFFF, U+BFFFE, U+BFFFF, U+CFFFE, U+CFFFF, U+DFFFE,
        //   U+DFFFF, U+EFFFE, U+EFFFF, U+FFFFE, U+FFFFF, U+10FFFE, or U+10FFFF.

        // quick bail for these
        if (r == null)
            return false;
        if (r.Value.Value is >= 0xFDD0 and <= 0xFDEF)
            return true;

        // all others are of the form ((x << 16) | 0xFFFE) and ((x << 16) | 0xFFFF)
        //   (where x is 0 through 16 inclusive)
        int low16 = r.Value.Value & 0xFFFF; // all Runes are, by definition, in a valid plane
        return low16 == 0xFFFE || low16 == 0xFFFF;
    }
    public static bool IsNoncharacter(int c) => Rune.TryCreate(c, out Rune r) && IsNoncharacter(r);

    public static bool IsAsciiCodePoint(Rune? r)
    {
        // An ASCII code point is a code point in the range U+0000 NULL to
        //   U+007F DELETE, inclusive.
        return r != null && (r.Value.Value is <= 0x7F);
    }

    public static bool IsAsciiTabOrNewLine(Rune? r)
    {
        // An ASCII tab or newline is U+0009 TAB, U+000A LF, or U+000D CR.
        return r != null && (r.Value.Value is '\t' or '\n' or '\r');
    }

    public static bool IsAsciiWhitespace(Rune? r)
    {
        // ASCII whitespace is U+0009 TAB, U+000A LF, U+000C FF, U+000D CR, or
        //   U+0020 SPACE.
        return r != null && (r.Value.Value is '\t' or '\n' or '\f' or '\r' or ' ');
    }
    public static bool IsAsciiWhitespace(int c) => Rune.TryCreate(c, out Rune r) && IsAsciiWhitespace(r);

    public static bool IsC0Control(Rune? r)
    {
        // A C0 control is a code point in the range U+0000 NULL to
        //   U+001F INFORMATION SEPARATOR ONE, inclusive.
        return r != null && (r.Value.Value is <= 0x1F);
    }

    public static bool IsC0ControlOrSpace(Rune? r)
    {
        // A C0 control or space is a C0 control or U+0020 SPACE.
        return r != null && (r.Value.Value is <= 0x20);
    }

    public static bool IsControl(Rune? r)
    {
        // A control is a C0 control or a code point in the range U+007F DELETE
        //   to U+009F APPLICATION PROGRAM COMMAND, inclusive.
        return r != null && (r.Value.Value is <= 0x1F or (>= 0x7F and <= 0x9F));
    }
    public static bool IsControl(int c) => Rune.TryCreate(c, out Rune r) && IsControl(r);

    public static bool IsAsciiDigit(Rune? r)
    {
        // An ASCII digit is a code point in the range U+0030 (0) to U+0039 (9),
        //   inclusive.
        return r != null && (r.Value.Value is >= 0x30 and <= 0x39);
    }

    public static bool IsAsciiUpperHexDigit(Rune? r)
    {
        // An ASCII upper hex digit is an ASCII digit or a code point in the
        //   range U+0041 (A) to U+0046 (F), inclusive.
        return r != null && (r.Value.Value is (>= 0x30 and <= 0x39) or (>= 0x41 and <= 0x46));
    }

    public static bool IsAsciiLowerHexDigit(Rune? r)
    {
        // An ASCII lower hex digit is an ASCII digit or a code point in the
        //   range U+0061 (a) to U+0066 (f), inclusive.
        return r != null && (r.Value.Value is (>= 0x30 and <= 0x39) or (>= 0x61 and <= 0x66));
    }

    public static bool IsAsciiHexDigit(Rune? r)
    {
        // An ASCII hex digit is an ASCII upper hex digit or ASCII lower hex
        //   digit.
        return IsAsciiUpperHexDigit(r) || IsAsciiLowerHexDigit(r);
    }

    public static bool IsAsciiUpperAlpha(Rune? r)
    {
        // An ASCII upper alpha is a code point in the range U+0041 (A) to
        //   U+005A (Z), inclusive.
        return r != null && (r.Value.Value is >= 0x41 and <= 0x5A);
    }

    public static bool IsAsciiLowerAlpha(Rune? r)
    {
        // An ASCII lower alpha is a code point in the range U+0061 (a) to
        //   U+007A (z), inclusive.
        return r != null && (r.Value.Value is >= 0x61 and <= 0x7A);
    }

    public static bool IsAsciiAlpha(Rune? r)
    {
        // An ASCII alpha is an ASCII upper alpha or ASCII lower alpha.
        return IsAsciiUpperAlpha(r) || IsAsciiLowerAlpha(r);
    }

    public static bool IsAsciiAlphanumeric(Rune? r)
    {
        // An ASCII alphanumeric is an ASCII digit or ASCII alpha.
        return IsAsciiDigit(r) || IsAsciiAlpha(r);
    }
}
