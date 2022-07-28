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
using DotNext;
using HyperLib;
using System.Diagnostics;
using System.Numerics;

namespace CurlyBracket.Types;

/// <summary>
/// Represents the number type in ECMAScript.
/// </summary>
[PublicAPI]
public class JSNumber : JSNumeric
{
    internal const double MAX_SAFE_INTEGER = 9007199254740991;
    internal const double MIN_SAFE_INTEGER = -9007199254740991;

    /// <summary>A static instance of a <see cref="JSNumber" /> type representing negative one.</summary>
    public static JSNumber MinusOne { get; } = new(-1);
    /// <summary>A static instance of a <see cref="JSNumber" /> type representing negative zero.</summary>
    public static JSNumber MinusZero { get; } = new( // the compiler silently treats `-0` as `0`; this fixes that
        BitConverter.Int64BitsToDouble(unchecked((long)0x8000_0000_0000_0000ul)));
    /// <summary>A static instance of a <see cref="JSNumber" /> type representing zero.</summary>
    public static JSNumber Zero { get; } = new(0);
    /// <summary>A static instance of a <see cref="JSNumber" /> type representing one.</summary>
    public static JSNumber One { get; } = new(1);
    /// <summary>A static instance of a <see cref="JSNumber" /> type representing NaN.</summary>
    public static JSNumber NaN { get; } = new(double.NaN);
    /// <summary>A static instance of a <see cref="JSNumber" /> type representing the "maximum safe integer".</summary>
    /// <remarks>
    /// This represents the ECMAScript constant <c>Number.MAX_SAFE_INTEGER</c>;
    /// It is the largest integer value that can be stored exactly.
    /// Floating point values greater than this have a precision greater than one.
    /// </remarks>
    public static JSNumber MaxSafeInteger { get; } = new(MAX_SAFE_INTEGER);
    /// <summary>A static instance of a <see cref="JSNumber" /> type representing the "minimum safe integer".</summary>
    /// <remarks>
    /// This represents the ECMAScript constant <c>Number.MIN_SAFE_INTEGER</c>;
    /// It is the smallest integer value that can be stored exactly.
    /// Floating point values less than this have a precision greater than one.
    /// </remarks>
    public static JSNumber MinSafeInteger { get; } = new(MIN_SAFE_INTEGER);

    /// <summary>
    /// Construct a new <see cref="JSNumber" /> object with a specified value.
    /// </summary>
    /// <param name="value">The value to set this object to.</param>
    public JSNumber(double value)
        : base(JSType.Number)
    {
        Value = value;
    }

    /// <summary>
    /// The value of this object.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Implicitly extract the value of a <see cref="JSNumber" /> object.
    /// </summary>
    /// <param name="o">The <see cref="JSNumber" /> object to get the value of.</param>
    /// <returns>The value of the object.</returns>
    public static implicit operator double(JSNumber o) =>
        o.Value;

    /// <summary>
    /// Implicitly get a <see cref="JSNumber" /> object for a specified value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="JSNumber" /> object.</param>
    /// <returns>A new <see cref="JSNumber" /> object with the specified value.</returns>
    public static implicit operator JSNumber(double value) =>
        new(value);

    #region Abstract Type Conversions

    /// <inheritdoc />
    public override Result<JSValue> ToPrimitive(JSType? preferredType = null) =>
        this;

    /// <inheritdoc />
    public override bool ToBoolean() =>
        Value is not (0 or double.NaN);

    /// <inheritdoc />
    public override Result<JSNumeric> ToNumeric() =>
        this;

    /// <inheritdoc />
    public override Result<double> ToNumber() =>
        Value;

    /// <inheritdoc />
    public override Result<double> ToIntegerOrInfinity()
    {
        double number = Value;
        if (number is double.NaN or 0)
            return 0;
        if (double.IsInfinity(number))
            return number;
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<int> ToInt32()
    {
        double number = Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<uint> ToUInt32()
    {
        double number = Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<short> ToInt16()
    {
        double number = Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<ushort> ToUInt16()
    {
        double number = Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<sbyte> ToInt8()
    {
        double number = Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<byte> ToUInt8()
    {
        double number = Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<byte> ToUInt8Clamp()
    {
        double number = Value;
        if (number is double.NaN or <= 0)
            return 0;
        if (number >= 255)
            return 255;

        throw new NotImplementedException();
    }

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
    public override Result<string> AbstractToString() => throw new NotImplementedException(); // `Number::toString`

    /// <inheritdoc />
    public override Result<JSObject> ToObject() => throw new NotImplementedException();

    /// <inheritdoc />
    public override Result<JSValue> ToPropertyKey() =>
        new JSString(AbstractToString().Value); // will never throw for numbers

    /// <inheritdoc />
    public override Result<ulong> ToLength()
    {
        Result<double> len = ToIntegerOrInfinity();
        if (!len.IsSuccessful)
            return new(len.Error);
        if (len.Value <= 0)
            return 0;
        return (ulong)Math.Min(len.Value, MAX_SAFE_INTEGER);
    }

    /// <inheritdoc />
    public override Result<ulong> ToIndex()
    {
        Result<double> integerIndex = ToIntegerOrInfinity();
        if (!integerIndex.IsSuccessful)
            return new(integerIndex.Error);
        if (double.IsNegative(integerIndex.Value))
            throw new RangeErrorException();
        ulong index = new JSNumber(integerIndex.Value).ToLength().Value; // will never throw for numbers

        // if SameValue(integerIndex, index) is false, throw a RangeError exception
        // else return index
        throw new NotImplementedException();
    }

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
    public override bool IsIntegralNumber()
    {
        if (Value is double.NaN || double.IsInfinity(Value))
            return false;

        double abs = Math.Abs(Value);
        // ReSharper disable once CompareOfFloatsByEqualityOperator - spec requires it
        return Math.Floor(abs) == abs;
    }

    /// <inheritdoc />
    public override bool IsPropertyKey() =>
        false;

    /// <inheritdoc />
    public override bool IsRegExp() =>
        false;

    /// <inheritdoc />
    public override bool SameValue(JSValue other)
    {
        if (other.Type is not JSType.Number)
            return false;
        throw new NotImplementedException(); // return `Number::sameValue(other)`
    }

    /// <inheritdoc />
    public override bool SameValueZero(JSValue other)
    {
        if (other.Type is not JSType.Number)
            return false;
        throw new NotImplementedException(); // return `Number::sameValueZero(other)`
    }

    /// <inheritdoc />
    public override bool SameValueNonNumeric(JSValue other)
    {
        Debug.Assert(other.Type is JSType.Number);
        throw new UnreachableException(); // should never be called for numeric values
    }

    #endregion
}
