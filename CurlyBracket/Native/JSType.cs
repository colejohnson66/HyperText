/* =============================================================================
 * File:   JSType.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Lists the "Language types" section of ECMAScript.
 * <https://tc39.es/ecma262/#sec-ecmascript-language-types>
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

using System.Numerics;

namespace CurlyBracket.Native;

#pragma warning disable CA1720 // Identifier contains type name

/// <summary>
/// Specifies the type of a <see cref="JSValue" /> object.
/// </summary>
/// <remarks>
/// https://tc39.es/ecma262/#sec-ecmascript-language-types
/// </remarks>
public enum JSType
{
    /// <summary>
    /// The ECMAScript "Undefined" type.
    /// </summary>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-ecmascript-language-types-undefined-type
    /// </remarks>
    Undefined,

    /// <summary>
    /// The ECMAScript "Null" type.
    /// </summary>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-ecmascript-language-types-null-type
    /// </remarks>
    Null,

    /// <summary>
    /// The ECMAScript "Boolean" type.
    /// Internally, a <see cref="JSBoolean" /> is a wrapper around a <see cref="bool" />.
    /// </summary>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-ecmascript-language-types-boolean-type
    /// </remarks>
    Boolean,

    /// <summary>
    /// The ECMAScript "String" type.
    /// Internally, a <see cref="JSString" /> is a wrapper around a <see cref="string" />.
    /// </summary>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-ecmascript-language-types-string-type
    /// </remarks>
    String,

    /// <summary>
    /// The ECMAScript "Symbol" type.
    /// </summary>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-ecmascript-language-types-symbol-type
    /// </remarks>
    Symbol,

    /// <summary>
    /// The ECMAScript "Number" type.
    /// This includes floating-point numbers and integers.
    /// Internally, a <see cref="JSNumber" /> is a wrapper around a <see cref="double" />.
    /// </summary>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-ecmascript-language-types-number-type
    /// </remarks>
    Number,

    /// <summary>
    /// The ECMAScript "BigInt" type.
    /// Internally, a <see cref="JSBigInt" /> is a wrapper around a <see cref="BigInteger" />.
    /// </summary>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-ecmascript-language-types-bigint-type
    /// </remarks>
    BigInt,

    /// <summary>
    /// The ECMAScript "Object" type.
    /// </summary>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-object-type
    /// </remarks>
    Object,
}
