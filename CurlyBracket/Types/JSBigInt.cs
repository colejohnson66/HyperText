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
using HyperLib;
using System.Diagnostics;
using System.Numerics;

namespace CurlyBracket.Types;

/// <summary>
/// Represents the big integer type in ECMAScript.
/// </summary>
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
    public override JSValue ToPrimitive(JSType? preferredType = null) =>
        this;

    /// <inheritdoc />
    public override bool ToBoolean() =>
        !Value.IsZero;

    /// <inheritdoc />
    public override JSNumeric ToNumeric() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override double ToNumber() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override double ToIntegerOrInfinity() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override int ToInt32() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override uint ToUInt32() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override short ToInt16() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override ushort ToUInt16() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override sbyte ToInt8() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override byte ToUInt8() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override byte ToUInt8Clamp() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override BigInteger ToBigInt() =>
        Value;

    /// <inheritdoc />
    public override long ToBigInt64() => throw new NotImplementedException();

    /// <inheritdoc />
    public override ulong ToBigUInt64() => throw new NotImplementedException();

    /// <inheritdoc />
    public override string AbstractToString() => throw new NotImplementedException();

    /// <inheritdoc />
    public override JSObject ToObject() => throw new NotImplementedException();

    /// <inheritdoc />
    public override JSValue ToPropertyKey() =>
        new JSString(AbstractToString());

    /// <inheritdoc />
    public override ulong ToLength() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override ulong ToIndex() =>
        throw new TypeErrorException();

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

    /// <inheritdoc />
    public override bool? IsLessThan(JSValue other, bool leftFirst)
    {
        (JSValue px, JSValue py) = leftFirst
            ? (this, other.ToPrimitive(JSType.Number))
            : (other.ToPrimitive(JSType.Number), (JSValue)this); // cast needed to satisfy the compiler

        if (px.Type is JSType.BigInt && py.Type is JSType.String)
        {
            // ny = StringToBigInt(py)
            // if ny is undef, return undef
            // return BigInt::lessThan(px, ny)
            throw new NotImplementedException();
        }
        if (px.Type is JSType.String && py.Type is JSType.BigInt)
        {
            // nx = StringToBigInt(px)
            // if nx is undef, return undef
            // return BigInt::lessThan(nx, py)
            throw new NotImplementedException();
        }

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
    public override bool IsLooselyEqual(JSValue other)
    {
        if (other.Type == JSType.BigInt)
            return IsStrictlyEqual(other);

        if (other.Type == JSType.String)
        {
            // n = StringToBigInt(other)
            // if n is undefined, return false
            // return IsLooselyEqual(n)
            throw new NotImplementedException();
        }

        // ReSharper disable once TailRecursiveCall
        if (other.Type is JSType.Object)
            return IsLooselyEqual(other.ToPrimitive());

        if (other.Type is JSType.Number)
        {
            JSNumber y = (JSNumber)other;
            if (y.Value is double.NaN || double.IsInfinity(y.Value))
                return false;
            throw new NotImplementedException(); // return R(x) == R(y)
        }

        return false;
    }

    /// <inheritdoc />
    public override bool IsStrictlyEqual(JSValue other)
    {
        if (other.Type is not JSType.BigInt)
            return false;
        throw new NotImplementedException(); // return `BigInt::equal(x, y)`
    }

    #endregion
}
