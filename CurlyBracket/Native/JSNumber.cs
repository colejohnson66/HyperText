/* =============================================================================
 * File:   JSNumber.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the ECMAScript number type.
 * <https://tc39.es/ecma262/#sec-ecmascript-language-types-number-type>
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
using System.Diagnostics;
using System.Numerics;

namespace CurlyBracket.Native;

public class JSNumber : JSValue
{
    public static JSNumber NaN { get; } = new(double.NaN);
    public static JSNumber Infinity { get; } = new(double.PositiveInfinity);
    public static JSNumber NegativeInfinity { get; } = new(double.NegativeInfinity);
    public static JSNumber Zero { get; } = new(0);
    public static JSNumber NegativeZero { get; } = new(-0);
    public static JSNumber One { get; } = new(1);
    public static JSNumber NegativeOne { get; } = new(-1);
    public const double MAX_SAFE_INTEGER = 9007199254740991;
    public static JSNumber MaxSafeInteger { get; } = new(9007199254740991);
    public static JSNumber MinSafeInteger { get; } = new(-9007199254740991);

    public JSNumber(double value)
        : base(JSType.Number)
    {
        Value = value;
    }

    public double Value { get; }

    public override string ToString() =>
        $"{nameof(JSNumber)} {{ {Value} }}";

    #region Abstract Type Conversions

    public override JSValue ToPrimitive(JSType? preferredType = null) =>
        this;

    public override bool ToBoolean() =>
        Value is not (0 or double.NaN);

    public override JSValue ToNumeric() =>
        this;

    public override JSNumber ToNumber() =>
        this;

    public override JSNumber ToIntegerOrInfinity()
    {
        if (Value is 0 or double.NaN)
            return Zero;
        if (double.IsPositiveInfinity(Value))
            return Infinity;
        if (double.IsNegativeInfinity(Value))
            return NegativeInfinity;

        // equivalent to Math.Sign(Value) * Math.Floor(Math.Abs(Value))
        return new(Math.Truncate(Value));
    }

    public override int ToInt32()
    {
        throw new NotImplementedException();
    }

    public override uint ToUInt32()
    {
        throw new NotImplementedException();
    }

    public override short ToInt16()
    {
        throw new NotImplementedException();
    }

    public override ushort ToUInt16()
    {
        throw new NotImplementedException();
    }

    public override sbyte ToInt8()
    {
        throw new NotImplementedException();
    }

    public override byte ToUInt8()
    {
        throw new NotImplementedException();
    }

    public override byte ToUInt8Clamp()
    {
        throw new NotImplementedException();
    }

    public override BigInteger ToBigInt() =>
        throw new TypeErrorException();

    public override long ToBigInt64() =>
        throw new TypeErrorException();

    public override ulong ToBigUInt64() =>
        throw new TypeErrorException();

    public override string AbstractToString()
    {
        // Number::ToString(this)
        throw new NotImplementedException();
    }

    public override JSObject ToObject()
    {
        throw new NotImplementedException();
    }

    public override JSValue ToPropertyKey() =>
        new JSString(AbstractToString());

    public override JSValue ToLength()
    {
        JSNumber len = ToIntegerOrInfinity();
        return len.Value < 0 ? Zero : new(Math.Min(len.Value, MaxSafeInteger.Value));
    }

    public override JSValue ToIndex()
    {
        JSNumber integer = ToIntegerOrInfinity();
        JSValue clamped = ToLength();
        if (!integer.SameValue(clamped))
            throw new RangeErrorException();
        Debug.Assert(integer.Value is >= 0 and <= MAX_SAFE_INTEGER);
        return integer;
    }

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

    public override bool IsIntegralNumber()
    {
        if (Value is 0 or double.NaN)
            return false;
        double abs = Math.Abs(Value);
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return Math.Floor(abs) == abs;
    }

    public override bool IsPropertyKey() =>
        false;

    public override bool IsRegExp() =>
        false;

    public override bool SameValue(JSValue other)
    {
        if (other.Type is not JSType.Number)
            return false;
        // return Number::SameValue(this, other);
        throw new NotImplementedException();
    }

    public override bool SameValueZero(JSValue other)
    {
        if (other.Type is not JSType.Number)
            return false;
        // return Number::SameValueZero(this, other);
        throw new NotImplementedException();
    }

    // not implemented for JSNumber or JSBigInt
    public override bool SameValueNonNumeric(JSValue other) =>
        throw new InvalidOperationException();

    public override bool IsLessThan(JSValue other, bool leftFirst)
    {
        throw new NotImplementedException();
    }

    public override bool IsLooselyEqual(JSValue other) =>
        other.Type switch
        {
            JSType.Number => IsStrictlyEqual(other),
            JSType.String => IsStrictlyEqual(other.ToNumber()),
            JSType.Object => IsLooselyEqual(other.ToPrimitive()),
            JSType.BigInt when double.IsNaN(Value) || double.IsPositiveInfinity(Value) || double.IsNegativeInfinity(Value) => false,
            JSType.BigInt => throw new NotImplementedException(), // Value == other.Value
            _ => false,
        };

    public override bool IsStrictlyEqual(JSValue other)
    {
        if (other.Type is not JSType.Number)
            return false;
        // return Number::Equal(this, other);
        throw new NotImplementedException();
    }

    #endregion
}
