/* =============================================================================
 * File:   JSType.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Enumerates the various "ECMAScript language types" as defined in section 6.1
 *   of ECMAScript 2021:
 * <https://262.ecma-international.org/12.0/#sec-ecmascript-language-types>
 * =============================================================================
 * Copyright (c) 2021 Cole Tobin
 *
 * This file is part of CurlyBracket.
 *
 * CurlyBracket is free software: you can redistribute it and/or modify it under
 *   the terms of the GNU General Public License as published by the Free
 *   Software Foundation, either version 3 of the License, or (at your option)
 *   any later version.
 *
 * CurlyBracket is distributed in the hope that it will be useful, but WITHOUT
 *   ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 *   FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License
 *   for more details.
 *
 * You should have received a copy of the GNU General Public License along with
 *   CurlyBracket. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

namespace CurlyBracket.Objects;

public enum JSType
{
    Undefined,
    Null,
    Boolean,
    String,
    Symbol,
    Number,
    BigInt,
    Object,
}
