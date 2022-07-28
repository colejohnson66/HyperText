/* =============================================================================
 * File:   Unit.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 * Defines the "unit" type; an object with no variables or properties, and the
 *   only methods are comparison ones and the ones inherited from `object`.
 * This is akin to Rust's `()` type (an empty tuple).
 * =============================================================================
 * Copyright (c) 2022 Cole Tobin
 *
 * This file is part of HyperLib.
 *
 * HyperLib is free software: you can redistribute it and/or modify it under the
 *   terms of the GNU General Public License as published by the Free Software
 *   Foundation, either version 3 of the License, or (at your option) any later
 *   version.
 *
 * HyperLib is distributed in the hope that it will be useful, but WITHOUT ANY
 *   WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 *   FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
 *   details.
 *
 * You should have received a copy of the GNU General Public License along with
 *   HyperLib. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System.Diagnostics.CodeAnalysis;

namespace HyperLib;

/// <summary>
/// Represents an object that has no value.
/// </summary>
public struct Unit
    : IEquatable<Unit>
{
    /// <summary>
    /// Get a new <see cref="Unit" /> object.
    /// </summary>
    public static Unit Default => new();

    /// <summary>
    /// Determine if two <see cref="Unit" /> objects are equal.
    /// As all <see cref="Unit" />s are identical, this will always return <c>true</c>.
    /// </summary>
    /// <param name="left">The first <see cref="Unit" /> instance to compare.</param>
    /// <param name="right">The second <see cref="Unit" /> instance to compare.</param>
    /// <returns><c>true</c></returns>
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public static bool operator ==(Unit left, Unit right) =>
        true;

    /// <summary>
    /// Determine if two <see cref="Unit" /> objects are unequal.
    /// As all <see cref="Unit" />s are identical, this will always return <c>false</c>.
    /// </summary>
    /// <param name="left">The first <see cref="Unit" /> instance to compare.</param>
    /// <param name="right">The second <see cref="Unit" /> instance to compare.</param>
    /// <returns><c>false</c></returns>
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public static bool operator !=(Unit left, Unit right) =>
        false;

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is Unit;

    /// <inheritdoc />
    public override string ToString() =>
        "()";

    /// <inheritdoc />
    public bool Equals(Unit other) =>
        true;

    /// <inheritdoc />
    public override int GetHashCode() =>
        0;
}
