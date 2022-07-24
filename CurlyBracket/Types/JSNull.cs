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
using System.Diagnostics;
using System.Numerics;

namespace CurlyBracket.Types;

/// <summary>
/// Represents the "null" type in ECMAScript.
/// </summary>
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
    public override JSValue ToPrimitive(JSType? preferredType = null) =>
        this;

    /// <inheritdoc />
    public override bool ToBoolean() =>
        false;

    /// <inheritdoc />
    public override JSNumeric ToNumeric() =>
        JSNumber.Zero;

    /// <inheritdoc />
    public override double ToNumber() =>
        0;

    /// <inheritdoc />
    public override double ToIntegerOrInfinity() =>
        0;

    /// <inheritdoc />
    public override int ToInt32() =>
        0;

    /// <inheritdoc />
    public override uint ToUInt32() =>
        0;

    /// <inheritdoc />
    public override short ToInt16() =>
        0;

    /// <inheritdoc />
    public override ushort ToUInt16() =>
        0;

    /// <inheritdoc />
    public override sbyte ToInt8() =>
        0;

    /// <inheritdoc />
    public override byte ToUInt8() =>
        0;

    /// <inheritdoc />
    public override byte ToUInt8Clamp() =>
        0;

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
    public override string AbstractToString() =>
        "null";

    /// <inheritdoc />
    public override JSObject ToObject() =>
        throw new TypeErrorException();

    /// <inheritdoc />
    public override JSValue ToPropertyKey() =>
        new JSString("null"); // from `AbstractToString`

    /// <inheritdoc />
    public override ulong ToLength() =>
        0;

    /// <inheritdoc />
    public override ulong ToIndex() =>
        0;

    #endregion

    #region Abstract Testing/Comparisons

    /// <inheritdoc />
    public override JSValue RequireObjectCoercible() =>
        throw new TypeErrorException();

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
            JSType.Undefined => true, // only undefined is loosely equal to null
            JSType.Null => true,
            _ => false,
        };

    /// <inheritdoc />
    public override bool IsStrictlyEqual(JSValue other) =>
        other.Type is JSType.Null;

    #endregion
}
