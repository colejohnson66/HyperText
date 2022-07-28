/* =============================================================================
 * File:   JSBigInt.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the ECMAScript BigInt type.
 * <https://tc39.es/ecma262/#sec-ecmascript-language-types-bigint-type>
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
using HyperLib;
using System.Diagnostics;
using System.Numerics;

namespace CurlyBracket.Types;

/// <summary>
/// Represents the big integer (bigint) type in ECMAScript.
/// </summary>
[PublicAPI]
public class JSBigInt : JSNumeric
{
    /// <summary>A static instance of a <see cref="JSBigInt" /> type representing negative one.</summary>
    public static JSBigInt MinusOne { get; } = new(BigInteger.MinusOne);
    /// <summary>A static instance of a <see cref="JSBigInt" /> type representing zero.</summary>
    public static JSBigInt Zero { get; } = new(BigInteger.Zero);
    /// <summary>A static instance of a <see cref="JSBigInt" /> type representing one.</summary>
    public static JSBigInt One { get; } = new(BigInteger.One);

    /// <summary>
    /// Construct a new <see cref="JSBigInt" /> object with a specified value.
    /// </summary>
    /// <param name="value">The value to set this object to.</param>
    public JSBigInt(BigInteger value)
        : base(JSType.BigInt)
    {
        Value = value;
    }

    /// <summary>
    /// The value of this object.
    /// </summary>
    public BigInteger Value { get; }

    /// <summary>
    /// Implicitly extract the value of a <see cref="JSBigInt" /> object.
    /// </summary>
    /// <param name="o">The <see cref="JSBigInt" /> object to get the value of.</param>
    /// <returns>The value of the object.</returns>
    public static implicit operator BigInteger(JSBigInt o) =>
        o.Value;

    /// <summary>
    /// Implicitly get a <see cref="JSBigInt" /> object for a specified value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="JSBigInt" /> object.</param>
    /// <returns>A new <see cref="JSBigInt" /> object with the specified value.</returns>
    public static implicit operator JSBigInt(BigInteger value) =>
        new(value);

    #region Abstract Type Conversions

    /// <inheritdoc />
    public override Result<JSValue> ToPrimitive(ToPrimitiveType preferredType = ToPrimitiveType.Default) =>
        this;

    /// <inheritdoc />
    public override bool ToBoolean() =>
        !Value.IsZero;

    /// <inheritdoc />
    public override Result<JSNumeric> ToNumeric() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<double> ToNumber() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<double> ToIntegerOrInfinity() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<int> ToInt32() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<uint> ToUInt32() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<short> ToInt16() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<ushort> ToUInt16() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<sbyte> ToInt8() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<byte> ToUInt8() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<byte> ToUInt8Clamp() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<BigInteger> ToBigInt() =>
        Value;

    /// <inheritdoc />
    public override Result<long> ToBigInt64() => throw new NotImplementedException();

    /// <inheritdoc />
    public override Result<ulong> ToBigUInt64() => throw new NotImplementedException();

    /// <inheritdoc />
    public override Result<string> AbstractToString() => throw new NotImplementedException();

    /// <inheritdoc />
    public override Result<JSObject> ToObject() => throw new NotImplementedException();

    /// <inheritdoc />
    public override Result<JSValue> ToPropertyKey() =>
        new JSString(AbstractToString().Value); // will never throw for bigint

    /// <inheritdoc />
    public override Result<ulong> ToLength() =>
        new(new TypeErrorException());

    /// <inheritdoc />
    public override Result<ulong> ToIndex() =>
        new(new TypeErrorException());

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
        false; // `IsIntegralNumber` only matters for numbers, not big integers

    /// <inheritdoc />
    public override bool IsPropertyKey() =>
        false;

    /// <inheritdoc />
    public override bool IsRegExp() =>
        false;

    /// <inheritdoc />
    public override bool SameValue(JSValue other)
    {
        if (other.Type is not JSType.BigInt)
            return false;
        throw new NotImplementedException(); // return `BigInt::sameValue(other)`
    }

    /// <inheritdoc />
    public override bool SameValueZero(JSValue other)
    {
        if (other.Type is not JSType.BigInt)
            return false;
        throw new NotImplementedException(); // return `BigInt::sameValueZero(other)`
    }

    /// <inheritdoc />
    public override bool SameValueNonNumeric(JSValue other)
    {
        Debug.Assert(other.Type is JSType.BigInt);
        throw new UnreachableException(); // should never be called for numeric values
    }

    #endregion
}
