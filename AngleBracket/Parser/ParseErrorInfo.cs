/* =============================================================================
 * File:   ParseErrorInfo.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Represents a ParseError and any associated information used by the tokenizer
 *   caller.
 * =============================================================================
 * Copyright (c) 2022 Cole Tobin
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

namespace AngleBracket.Parser;

/// <summary>
/// A <see cref="ParseError" /> and the associated character index.
/// </summary>
/// <param name="Error">The actual <see cref="ParseError" />.</param>
/// <param name="InputCharOffset">The character index in the input stream that this error was encountered at.</param>
public record ParseErrorInfo(
    ParseError Error,
    int InputCharOffset);
