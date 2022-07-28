/* =============================================================================
 * File:   ToPrimitiveType.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Defines the possible values that can be passed to `JSValue.ToPrimitive` for
 *   the `preferredType` parameter.
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
/// Defines the possible values that can be passed to <see cref="JSValue.ToPrimitive" /> for the preferred type
///   parameter.
/// </summary>
[PublicAPI]
public enum ToPrimitiveType
{
    /// <summary>
    /// The "default" preferred type.
    /// Indicates to <see cref="JSValue.ToPrimitive" /> that there is no preference for what is returned.
    /// </summary>
    Default,

    /// <summary>
    /// Indicates to <see cref="JSValue.ToPrimitive" /> that a numeric type should be returned.
    /// </summary>
    Number,

    /// <summary>
    /// Indicates to <see cref="JSValue.ToPrimitive" /> that a <see cref="JSString" /> type should be returned.
    /// </summary>
    String,
}
