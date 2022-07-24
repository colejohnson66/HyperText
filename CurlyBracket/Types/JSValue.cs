/* =============================================================================
 * File:   JSValue.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the base class of all ECMAScript values and objects.
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

using System.Diagnostics;
using System.Linq;

namespace CurlyBracket.Types;

/// <summary>
/// Represents the base type for all ECMAScript values and objects.
/// </summary>
/// <remarks>
/// This is akin to how all C# objects inherit from <see cref="object" />.
/// </remarks>
public abstract partial class JSValue
{
    /// <summary>A static instance of the <see cref="JSUndefined" /> type.</summary>
    public static JSUndefined Undefined => JSUndefined.Instance;
    /// <summary>A static instance of the <see cref="JSNull" /> type.</summary>
    public static JSNull Null => JSNull.Instance;

    /// <summary>
    /// Construct a new <see cref="JSValue" /> with a specified type.
    /// </summary>
    /// <param name="type">The type of value this <see cref="JSValue" /> represents.</param>
    protected JSValue(JSType type)
    {
        Debug.Assert(Enum.GetValues<JSType>().Contains(type));
        Type = type;
    }

    /// <summary>
    /// The type of value this <see cref="JSValue" /> object represents.
    /// </summary>
    public JSType Type { get; }
}
