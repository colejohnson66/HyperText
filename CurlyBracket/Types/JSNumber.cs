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
using HyperLib;
using System.Diagnostics;
using System.Numerics;

namespace CurlyBracket.Types;

/// <summary>
/// Represents the number type in ECMAScript.
/// </summary>
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
    public override JSValue ToPrimitive(JSType? preferredType = null) =>
        this;

    /// <inheritdoc />
    public override bool ToBoolean() =>
        Value is not (0 or double.NaN);

    /// <inheritdoc />
    public override JSNumeric ToNumeric() =>
        this;

    /// <inheritdoc />
    public override double ToNumber() =>
        Value;

    /// <inheritdoc />
    public override double ToIntegerOrInfinity()
    {
        double number = Value;
        if (number is double.NaN or 0)
            return 0;
        if (double.IsInfinity(number))
            return number;
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override int ToInt32()
    {
        double number = Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override uint ToUInt32()
    {
        double number = Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override short ToInt16()
    {
        double number = Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override ushort ToUInt16()
    {
        double number = Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override sbyte ToInt8()
    {
        double number = Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override byte ToUInt8()
    {
        double number = Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override byte ToUInt8Clamp()
    {
        double number = Value;
        if (number is double.NaN or <= 0)
            return 0;
        if (number >= 255)
            return 255;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override BigInteger ToBigInt() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override long ToBigInt64() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override ulong ToBigUInt64() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override string AbstractToString() => throw new NotImplementedException(); // `Number::toString`

    /// <inheritdoc />
    public override JSObject ToObject() => throw new NotImplementedException();

    /// <inheritdoc />
    public override JSValue ToPropertyKey() =>
        new JSString(AbstractToString());

    /// <inheritdoc />
    public override ulong ToLength()
    {
        double len = ToIntegerOrInfinity();
        if (len <= 0)
            return 0;
        return (ulong)Math.Min(len, MAX_SAFE_INTEGER);
    }

    /// <inheritdoc />
    public override ulong ToIndex()
    {
        double integerIndex = ToIntegerOrInfinity();
        if (double.IsNegative(integerIndex))
            throw new RangeErrorException();
        ulong index = new JSNumber(integerIndex).ToLength();

        // if SameValue(integerIndex, index) is false, throw a RangeError exception
        // else return index
        throw new NotImplementedException();
    }

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

    /// <inheritdoc />
    public override bool? IsLessThan(JSValue other, bool leftFirst)
    {
        (JSValue px, JSValue py) = leftFirst
            ? (this, other.ToPrimitive(JSType.Number))
            : (other.ToPrimitive(JSType.Number), (JSValue)this); // cast needed to satisfy the compiler

        JSNumeric nx = px.ToNumeric();
        JSNumeric ny = py.ToNumeric();
        if (nx.Type == ny.Type)
        {
            if (nx.Type is JSType.Number)
                throw new NotImplementedException(); // TODO: `Number::lessThan(nx, ny)`
            Debug.Assert(nx.Type is JSType.BigInt);
            throw new NotImplementedException(); // TODO: `BigInt::lessThan(nx, ny)`
        }

        // ReSharper disable once RedundantIfElseBlock
        if (nx.Type is JSType.BigInt)
        {
            Debug.Assert(ny.Type is JSType.Number);
            JSNumber ny2 = (JSNumber)ny;
            return ny2.Value switch
            {
                double.NaN => null,
                double.PositiveInfinity => true,
                double.NegativeInfinity => false,
                _ => throw new NotImplementedException(), // R(nx) < R(ny)
            };
        }
        else
        {
            Debug.Assert(nx.Type is JSType.Number);
            Debug.Assert(ny.Type is JSType.BigInt);
            JSNumber nx2 = (JSNumber)nx;
            return nx2.Value switch
            {
                double.NaN => null,
                double.NegativeInfinity => true,
                double.PositiveInfinity => false,
                _ => throw new NotImplementedException(), // R(nx) < R(ny)
            };
        }
    }

    /// <inheritdoc />
    public override bool IsLooselyEqual(JSValue other) =>
        other.Type switch
        {
            JSType.Number => IsStrictlyEqual(other),
            JSType.String => IsStrictlyEqual(new JSNumber(other.ToNumber())),
            JSType.Object => IsLooselyEqual(other.ToPrimitive()),
            _ => false,
        };

    /// <inheritdoc />
    public override bool IsStrictlyEqual(JSValue other)
    {
        if (other.Type is not JSType.Number)
            return false;
        throw new NotImplementedException(); // return `Number::equal(x, y)`
    }

    #endregion
}
