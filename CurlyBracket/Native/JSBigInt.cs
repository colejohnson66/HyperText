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
using System.Numerics;

namespace CurlyBracket.Native;

public class JSBigInt : JSValue
{
    public static JSBigInt Zero { get; } = new(BigInteger.Zero);
    public static JSBigInt One { get; } = new(BigInteger.One);
    public static JSBigInt NegativeOne { get; } = new(BigInteger.MinusOne);

    public JSBigInt(BigInteger value)
        : base(JSType.BigInt)
    {
        Value = value;
    }

    public BigInteger Value { get; }

    public override string ToString() =>
        $"{nameof(JSBigInt)} {{ {Value} }}";

    #region Abstract Type Conversions

    public override JSValue ToPrimitive(JSType? preferredType = null) =>
        this;

    public override bool ToBoolean() =>
        !Value.IsZero;

    public override JSValue ToNumeric() =>
        this;

    public override JSNumber ToNumber() =>
        throw new TypeErrorException();

    public override JSNumber ToIntegerOrInfinity() =>
        throw new TypeErrorException();

    public override int ToInt32() =>
        throw new TypeErrorException();

    public override uint ToUInt32() =>
        throw new TypeErrorException();

    public override short ToInt16() =>
        throw new TypeErrorException();

    public override ushort ToUInt16() =>
        throw new TypeErrorException();

    public override sbyte ToInt8() =>
        throw new TypeErrorException();

    public override byte ToUInt8() =>
        throw new TypeErrorException();

    public override byte ToUInt8Clamp() =>
        throw new TypeErrorException();

    public override BigInteger ToBigInt() =>
        Value;

    public override long ToBigInt64()
    {
        throw new NotImplementedException();
    }

    public override ulong ToBigUInt64()
    {
        throw new NotImplementedException();
    }

    public override string AbstractToString()
    {
        // return BigInt::ToString(Value);
        throw new NotImplementedException();
    }

    public override JSObject ToObject()
    {
        throw new NotImplementedException();
    }

    public override JSValue ToPropertyKey() =>
        new JSString(AbstractToString());

    public override JSValue ToLength() =>
        throw new TypeErrorException();

    public override JSValue ToIndex() =>
        throw new TypeErrorException();

    #endregion

    #region Abstract Testing/Comparison Operations

    public override JSValue RequireObjectCoercible() =>
        this;

    public override bool IsArray() =>
        false;

    public override bool IsCallable() =>
        false;

    public override bool IsConstructor() =>
        false;

    public override bool IsIntegralNumber() =>
        false;

    public override bool IsPropertyKey() =>
        false;

    public override bool IsRegExp() =>
        false;

    public override bool SameValue(JSValue other)
    {
        if (other.Type is not JSType.BigInt)
            return false;
        // return BigInt::SaveValue(this, other);
        throw new NotImplementedException();
    }

    public override bool SameValueZero(JSValue other)
    {
        if (other.Type is not JSType.BigInt)
            return false;
        // return BigInt::SaveValueZero(this, other);
        throw new NotImplementedException();
    }

    // not implemented for JSNumber or JSBigInt
    public override bool SameValueNonNumeric(JSValue other) =>
        throw new InvalidOperationException();

    public override bool IsLessThan(JSValue other, bool leftFirst)
    {
        throw new NotImplementedException();
    }

    public override bool IsLooselyEqual(JSValue other)
    {
        throw new NotImplementedException();
    }

    public override bool IsStrictlyEqual(JSValue other)
    {
        if (other.Type is not JSType.BigInt)
            return false;
        // return BigInt::Equal(this, other) == 0;
        throw new NotImplementedException();
    }

    #endregion
}
