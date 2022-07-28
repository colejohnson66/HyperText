/* =============================================================================
 * File:   JSNull.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the ECMAScript null type.
 * <https://tc39.es/ecma262/#sec-ecmascript-language-types-null-type>
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

using CurlyBracket.Runtime;
using DotNext;
using System.Diagnostics;
using System.Numerics;

namespace CurlyBracket.Types;

/// <summary>
/// Represents the "null" type in ECMAScript.
/// </summary>
[PublicAPI]
public class JSNull : JSValue
{
    /// <summary>
    /// A static instance of the <see cref="JSNull" /> type.
    /// </summary>
    public static JSNull Instance { get; } = new();

    private JSNull()
        : base(JSType.Null)
    { }

    #region Abstract Type Conversions

    /// <inheritdoc />
    public override Result<JSValue> ToPrimitive(ToPrimitiveType preferredType = ToPrimitiveType.Default) =>
        this;

    /// <inheritdoc />
    public override bool ToBoolean() =>
        false;

    /// <inheritdoc />
    public override Result<JSNumeric> ToNumeric() =>
        JSNumber.Zero;

    /// <inheritdoc />
    public override Result<double> ToNumber() =>
        0;

    /// <inheritdoc />
    public override Result<double> ToIntegerOrInfinity() =>
        0;

    /// <inheritdoc />
    public override Result<int> ToInt32() =>
        0;

    /// <inheritdoc />
    public override Result<uint> ToUInt32() =>
        0;

    /// <inheritdoc />
    public override Result<short> ToInt16() =>
        0;

    /// <inheritdoc />
    public override Result<ushort> ToUInt16() =>
        0;

    /// <inheritdoc />
    public override Result<sbyte> ToInt8() =>
        0;

    /// <inheritdoc />
    public override Result<byte> ToUInt8() =>
        0;

    /// <inheritdoc />
    public override Result<byte> ToUInt8Clamp() =>
        0;

    /// <inheritdoc />
    public override Result<BigInteger> ToBigInt() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<long> ToBigInt64() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<ulong> ToBigUInt64() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<string> AbstractToString() =>
        "null";

    /// <inheritdoc />
    public override Result<JSObject> ToObject() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<JSValue> ToPropertyKey() =>
        new JSString("null"); // from `AbstractToString`

    /// <inheritdoc />
    public override Result<ulong> ToLength() =>
        0;

    /// <inheritdoc />
    public override Result<ulong> ToIndex() =>
        0;

    #endregion

    #region Abstract Testing/Comparisons

    /// <inheritdoc />
    public override Result<JSValue> RequireObjectCoercible() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<bool> IsArray() =>
        false;

    /// <inheritdoc />
    public override bool IsCallable() =>
        false;

    /// <inheritdoc />
    public override bool IsConstructor() =>
        false;

    /// <inheritdoc />
    public override bool IsIntegralNumber() =>
        false;

    /// <inheritdoc />
    public override bool IsPropertyKey() =>
        false;

    /// <inheritdoc />
    public override bool IsRegExp() =>
        false;

    /// <inheritdoc />
    public override bool SameValue(JSValue other) =>
        other.Type is JSType.Null;

    /// <inheritdoc />
    public override bool SameValueZero(JSValue other) =>
        other.Type is JSType.Null;

    /// <inheritdoc />
    public override bool SameValueNonNumeric(JSValue other)
    {
        Debug.Assert(other.Type is JSType.Null);
        return true;
    }

    #endregion
}
