/* =============================================================================
 * File:   JSNumeric.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * TODO
 * =============================================================================
 * Copyright (c) 2022 Cole Tobin
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

namespace CurlyBracket.Types;

/// <summary>
/// Represents a numeric ECMAScript value.
/// Valid subtypes are <see cref="JSBigInt" /> and <see cref="JSNumber" />.
/// </summary>
[PublicAPI]
public abstract class JSNumeric : JSValue
{
    /// <summary>
    /// Constructs a new <see cref="JSNumeric" /> with a specified type.
    /// </summary>
    /// <param name="type">The type of value this <see cref="JSNumeric" /> represents.</param>
    /// <exception cref="ArgumentException">
    /// If <paramref name="type" /> is not <see cref="JSType.BigInt" /> or <see cref="JSType.Number" />.
    /// </exception>
    protected JSNumeric(JSType type)
        : base(type)
    {
        if (type is not (JSType.BigInt or JSType.Number))
            throw new ArgumentException($"{nameof(JSNumeric)} values must only be big integers or numbers.", nameof(type));
    }
}
