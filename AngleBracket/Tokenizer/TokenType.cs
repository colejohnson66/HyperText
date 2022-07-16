/* =============================================================================
 * File:   TokenType.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Defines the various types of HTML tokens emitted by the tokenizer.
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

namespace AngleBracket.Tokenizer;

/// <summary>
/// The type of token that a <see cref="Token" /> object represents.
/// </summary>
public enum TokenType
{
    /// <summary>
    /// This <see cref="Token" /> represents a Unicode code point.
    /// The stored value is a 32 bit integer guaranteed to be in the range <c>0..0x10FFFF</c>, inclusive.
    /// </summary>
    Character,

    /// <summary>
    /// This <see cref="Token" /> represents an HTML comment.
    /// The stored value is a <see cref="string" />.
    /// </summary>
    Comment,

    /// <summary>
    /// This <see cref="Token" /> represents an HTML doctype.
    /// The stored value is a <see cref="Tokenizer.Doctype" /> object.
    /// </summary>
    Doctype,

    /// <summary>
    /// This <see cref="Token" /> represents the end of the input stream.
    /// No more <see cref="Token" />s will be emitted after one with this type.
    /// There is no stored value associated with this type.
    /// </summary>
    EndOfFile,

    /// <summary>
    /// This <see cref="Token" /> represents an HTML tag, both start and end (and self closing).
    /// The stored value is a <see cref="Tokenizer.Tag" /> object.
    /// </summary>
    Tag,
}
