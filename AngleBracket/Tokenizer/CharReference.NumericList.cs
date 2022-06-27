/* =============================================================================
 * File:   CharReference.NumericList.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Contains the list of "numeric character references" as defined in the HTML
 *   standard from WHATWG.
 *
 * As of the 17 June 2022 version, this is located in section 13.2.5.80 "Numeric
 *   character reference end state":
 *   <https://html.spec.whatwg.org/multipage/parsing.html#numeric-character-reference-end-state>
 * =============================================================================
 * Copyright (c) 2021-2022 Cole Tobin
 *
 * This file is part of AngleBracket.
 *
 * AngleBracket is free software: you can redistribute it and/or modify it under
 *   the terms of the GNU General Public License as published by the Free
 *   Software Foundation, either version 3 of the License, or (at your option)
 *   any later version.
 *
 * AngleBracket is distributed in the hope that it will be useful, but WITHOUT
 *   ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 *   FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License
 *   for more details.
 *
 * You should have received a copy of the GNU General Public License along with
 *   AngleBracket. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System.Collections.ObjectModel;

namespace AngleBracket.Tokenizer;

public static partial class CharReference
{
    public static readonly ReadOnlyDictionary<int, int> NumericList = new(new Dictionary<int, int>
    {
        { 0x80, 0x20AC },
        { 0x82, 0x201A },
        { 0x83, 0x0192 },
        { 0x84, 0x201E },
        { 0x85, 0x2026 },
        { 0x86, 0x2020 },
        { 0x87, 0x2021 },
        { 0x88, 0x02C6 },
        { 0x89, 0x2030 },
        { 0x8A, 0x0160 },
        { 0x8B, 0x2039 },
        { 0x8C, 0x0152 },
        { 0x8E, 0x017D },
        { 0x91, 0x2018 },
        { 0x92, 0x2019 },
        { 0x93, 0x201C },
        { 0x94, 0x201D },
        { 0x95, 0x2022 },
        { 0x96, 0x2013 },
        { 0x97, 0x2014 },
        { 0x98, 0x02DC },
        { 0x99, 0x2122 },
        { 0x9A, 0x0161 },
        { 0x9B, 0x203A },
        { 0x9C, 0x0153 },
        { 0x9E, 0x017E },
        { 0x9F, 0x0178 },
    });
}
