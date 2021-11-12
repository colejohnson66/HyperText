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

namespace AngleBracket.Infra;

public static class CodePoint
{
    private static bool IsInRange(this int cp, int lower, int upper) => cp >= lower && cp <= upper;

    /// <summary>
    /// Determines if a codepoint is a surrogate as defined by WHATWG
    /// </summary>
    /// <param name="cp">The codepoint to check</param>
    /// <returns><c>true</c> if <paramref cref="cp" /> is a surrogate; otherwise <c>false</c></returns>
    public static bool IsSurrogate(int cp)
    {
        // A surrogate is a code point that is in the range U+D800 to U+DFFF,
        //   inclusive.
        return cp.IsInRange(0xD800, 0xDFFF);
    }

    public static bool IsScalarValue(int cp)
    {
        // A scalar value is a code point that is not a surrogate.
        return cp.IsInRange(0, 0xD7FF) || cp.IsInRange(0xE000, 0x10FFFF);
    }

    public static bool IsNoncharacter(int cp)
    {
        // A noncharacter is a code point that is in the range U+FDD0 to U+FDEF,
        //   inclusive, or U+FFFE, U+FFFF, U+1FFFE, U+1FFFF, U+2FFFE, U+2FFFF,
        //   U+3FFFE, U+3FFFF, U+4FFFE, U+4FFFF, U+5FFFE, U+5FFFF, U+6FFFE,
        //   U+6FFFF, U+7FFFE, U+7FFFF, U+8FFFE, U+8FFFF, U+9FFFE, U+9FFFF,
        //   U+AFFFE, U+AFFFF, U+BFFFE, U+BFFFF, U+CFFFE, U+CFFFF, U+DFFFE,
        //   U+DFFFF, U+EFFFE, U+EFFFF, U+FFFFE, U+FFFFF, U+10FFFE, or U+10FFFF.

        // quick bail for these
        if (cp.IsInRange(0xFDD0, 0xFDEF))
            return true;

        // all others are of the form ((x << 16) | 0xFFFE) and ((x << 16) | 0xFFFF)
        //   (where x is 0 through 16 inclusive)
        int high16 = cp >> 16;
        int low16 = cp & 0xFFFF;
        return high16.IsInRange(0, 16) && (low16 == 0xFFFE || low16 == 0xFFFF);
    }

    public static bool IsAsciiCodePoint(int cp)
    {
        // An ASCII code point is a code point in the range U+0000 NULL to
        //   U+007F DELETE, inclusive.
        return cp.IsInRange(0, 0x7F);
    }

    public static bool IsAsciiTabOrNewLine(int cp)
    {
        // An ASCII tab or newline is U+0009 TAB, U+000A LF, or U+000D CR.
        return cp == '\t' || cp == '\n' || cp == '\r';
    }

    public static bool IsAsciiWhitespace(int cp)
    {
        // ASCII whitespace is U+0009 TAB, U+000A LF, U+000C FF, U+000D CR, or
        //   U+0020 SPACE.
        return cp == '\t' || cp == '\n' || cp == '\f' || cp == '\r' || cp == ' ';
    }

    public static bool IsC0Control(int cp)
    {
        // A C0 control is a code point in the range U+0000 NULL to
        //   U+001F INFORMATION SEPARATOR ONE, inclusive.
        return cp.IsInRange(0, 0x1F);
    }

    public static bool IsC0ControlOrSpace(int cp)
    {
        // A C0 control or space is a C0 control or U+0020 SPACE.
        return cp.IsInRange(0, 0x20);
    }

    public static bool IsControl(int cp)
    {
        // A control is a C0 control or a code point in the range U+007F DELETE
        //   to U+009F APPLICATION PROGRAM COMMAND, inclusive.
        return IsC0Control(cp) || cp.IsInRange(0x7F, 0x9F);
    }

    public static bool IsAsciiDigit(int cp)
    {
        // An ASCII digit is a code point in the range U+0030 (0) to U+0039 (9),
        //   inclusive.
        return cp.IsInRange('0', '9');
    }

    public static bool IsAsciiUpperHexDigit(int cp)
    {
        // An ASCII upper hex digit is an ASCII digit or a code point in the
        //   range U+0041 (A) to U+0046 (F), inclusive.
        return cp.IsInRange('A', 'F');
    }

    public static bool IsAsciiLowerHexDigit(int cp)
    {
        // An ASCII lower hex digit is an ASCII digit or a code point in the
        //   range U+0061 (a) to U+0066 (f), inclusive.
        return cp.IsInRange('a', 'f');
    }

    public static bool IsAsciiHexDigit(int cp)
    {
        // An ASCII hex digit is an ASCII upper hex digit or ASCII lower hex
        //   digit.
        return IsAsciiUpperHexDigit(cp) || IsAsciiLowerHexDigit(cp);
    }

    public static bool IsAsciiUpperAlpha(int cp)
    {
        // An ASCII upper alpha is a code point in the range U+0041 (A) to
        //   U+005A (Z), inclusive.
        return cp.IsInRange('A', 'Z');
    }

    public static bool IsAsciiLowerAlpha(int cp)
    {
        // An ASCII lower alpha is a code point in the range U+0061 (a) to
        //   U+007A (z), inclusive.
        return cp.IsInRange('a', 'z');
    }

    public static bool IsAsciiAlpha(int cp)
    {
        // An ASCII alpha is an ASCII upper alpha or ASCII lower alpha.
        return IsAsciiUpperAlpha(cp) || IsAsciiLowerAlpha(cp);
    }

    public static bool IsAsciiAlphanumeric(int cp)
    {
        // An ASCII alphanumeric is an ASCII digit or ASCII alpha.
        return IsAsciiDigit(cp) || IsAsciiAlpha(cp);
    }
}
