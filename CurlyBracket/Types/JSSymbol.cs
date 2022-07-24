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
using System.Numerics;

namespace CurlyBracket.Types;

/// <summary>
/// Represents the symbol type in ECMAScript.
/// </summary>
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
    public override JSValue ToPrimitive(JSType? preferredType = null) =>
        this;

    /// <inheritdoc />
    public override bool ToBoolean() =>
        true;

    /// <inheritdoc />
    public override JSNumeric ToNumeric() =>
        throw new TypeErrorException(); // from `ToNumber()`

    /// <inheritdoc />
    public override double ToNumber() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override double ToIntegerOrInfinity() =>
        throw new TypeErrorException(); // from `ToNumber()`

    /// <inheritdoc />
    public override int ToInt32() =>
        throw new TypeErrorException(); // from `ToNumber()`

    /// <inheritdoc />
    public override uint ToUInt32() =>
        throw new TypeErrorException(); // from `ToNumber()`

    /// <inheritdoc />
    public override short ToInt16() =>
        throw new TypeErrorException(); // from `ToNumber()`

    /// <inheritdoc />
    public override ushort ToUInt16() =>
        throw new TypeErrorException(); // from `ToNumber()`

    /// <inheritdoc />
    public override sbyte ToInt8() =>
        throw new TypeErrorException(); // from `ToNumber()`

    /// <inheritdoc />
    public override byte ToUInt8() =>
        throw new TypeErrorException(); // from `ToNumber()`

    /// <inheritdoc />
    public override byte ToUInt8Clamp() =>
        throw new TypeErrorException(); // from `ToNumber()`

    /// <inheritdoc />
    public override BigInteger ToBigInt() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override long ToBigInt64() =>
        throw new TypeErrorException(); // from `ToBigInt()`

    /// <inheritdoc />
    public override ulong ToBigUInt64() =>
        throw new TypeErrorException(); // from `ToBigInt()`

    /// <inheritdoc />
    public override string AbstractToString() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override JSObject ToObject() =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override JSValue ToPropertyKey() =>
        this;

    /// <inheritdoc />
    public override ulong ToLength() =>
        throw new TypeErrorException(); // from `ToNumber()`

    /// <inheritdoc />
    public override ulong ToIndex() =>
        throw new TypeErrorException(); // from `ToNumber()`

    #endregion

    #region Abstract Testing/Comparisons

    /// <inheritdoc />
    public override JSValue RequireObjectCoercible() =>
        this;

    /// <inheritdoc />
    public override bool IsArray() =>
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

    /// <inheritdoc />
    public override bool? IsLessThan(JSValue other, bool leftFirst) =>
        // due to either `px` or `py` being this Symbol object, skipping to step 4d/4e would throw
        throw new TypeErrorException();

    /// <inheritdoc />
    public override bool IsLooselyEqual(JSValue other) =>
        other.Type is JSType.Symbol && SameValueNonNumeric(other);

    /// <inheritdoc />
    public override bool IsStrictlyEqual(JSValue other) =>
        other.Type is JSType.Symbol && SameValueNonNumeric(other);

    #endregion
}
