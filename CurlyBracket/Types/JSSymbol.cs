/* =============================================================================
 * File:   JSSymbol.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the ECMAScript symbol type.
 * <https://tc39.es/ecma262/#sec-ecmascript-language-types-symbol-type>
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
using System.Numerics;

namespace CurlyBracket.Types;

/// <summary>
/// Represents the symbol type in ECMAScript.
/// </summary>
[PublicAPI]
public class JSSymbol : JSValue
{
    /// <summary>
    /// Construct a new <see cref="JSSymbol" /> with a specified value.
    /// </summary>
    /// <param name="value"></param>
    public JSSymbol(string value)
        : base(JSType.Symbol)
    {
        Value = value;
    }

    /// <summary>
    /// The value of this object.
    /// </summary>
    public string Value { get; }

    #region Abstract Type Conversions

    /// <inheritdoc />
    public override Result<JSValue> ToPrimitive(ToPrimitiveType preferredType = ToPrimitiveType.Default) =>
        this;

    /// <inheritdoc />
    public override bool ToBoolean() =>
        true;

    /// <inheritdoc />
    public override Result<JSNumeric> ToNumeric() =>
        new(new TypeErrorException()); // from `ToNumber()`

    /// <inheritdoc />
    public override Result<double> ToNumber() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<double> ToIntegerOrInfinity() =>
        new(new TypeErrorException()); // from `ToNumber()`

    /// <inheritdoc />
    public override Result<int> ToInt32() =>
        new(new TypeErrorException()); // from `ToNumber()`

    /// <inheritdoc />
    public override Result<uint> ToUInt32() =>
        new(new TypeErrorException()); // from `ToNumber()`

    /// <inheritdoc />
    public override Result<short> ToInt16() =>
        new(new TypeErrorException()); // from `ToNumber()`

    /// <inheritdoc />
    public override Result<ushort> ToUInt16() =>
        new(new TypeErrorException()); // from `ToNumber()`

    /// <inheritdoc />
    public override Result<sbyte> ToInt8() =>
        new(new TypeErrorException()); // from `ToNumber()`

    /// <inheritdoc />
    public override Result<byte> ToUInt8() =>
        new(new TypeErrorException()); // from `ToNumber()`

    /// <inheritdoc />
    public override Result<byte> ToUInt8Clamp() =>
        new(new TypeErrorException()); // from `ToNumber()`

    /// <inheritdoc />
    public override Result<BigInteger> ToBigInt() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<long> ToBigInt64() =>
        new(new TypeErrorException()); // from `ToBigInt()`

    /// <inheritdoc />
    public override Result<ulong> ToBigUInt64() =>
        new(new TypeErrorException()); // from `ToBigInt()`

    /// <inheritdoc />
    public override Result<string> AbstractToString() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<JSObject> ToObject() =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override Result<JSValue> ToPropertyKey() =>
        this;

    /// <inheritdoc />
    public override Result<ulong> ToLength() =>
        new(new TypeErrorException()); // from `ToNumber()`

    /// <inheritdoc />
    public override Result<ulong> ToIndex() =>
        new(new TypeErrorException()); // from `ToNumber()`

    #endregion

    #region Abstract Testing/Comparisons

    /// <inheritdoc />
    public override Result<JSValue> RequireObjectCoercible() =>
        this;

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
        true;

    /// <inheritdoc />
    public override bool IsRegExp() =>
        false;

    /// <inheritdoc />
    public override bool SameValue(JSValue other) =>
        other.Type is JSType.Symbol && SameValueNonNumeric(other);

    /// <inheritdoc />
    public override bool SameValueZero(JSValue other) =>
        other.Type is JSType.Symbol && SameValueNonNumeric(other);

    /// <inheritdoc />
    public override bool SameValueNonNumeric(JSValue other)
    {
        if (other.Type is not JSType.Symbol)
            return false;
        throw new NotImplementedException(); // return true if this and other are same symbol value
    }

    #endregion
}
