/* =============================================================================
 * File:   JSBoolean.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the ECMAScript boolean type.
 * <https://tc39.es/ecma262/#sec-ecmascript-language-types-boolean-type>
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

using System.Diagnostics;
using System.Numerics;

namespace CurlyBracket.Types;

/// <summary>
/// Represents the boolean type in ECMAScript.
/// </summary>
public class JSBoolean : JSValue
{
    /// <summary>A static instance of a <see cref="JSBoolean" /> type representing <c>true</c>.</summary>
    public static JSBoolean True { get; } = new(true);
    /// <summary>A static instance of a <see cref="JSBoolean" /> type representing <c>false</c>.</summary>
    public static JSBoolean False { get; } = new(false);

    /// <summary>
    /// Construct a new <see cref="JSBoolean" /> object with a specified value.
    /// </summary>
    /// <param name="value">The value to set this object to.</param>
    public JSBoolean(bool value)
        : base(JSType.Boolean)
    {
        Value = value;
    }

    /// <summary>
    /// The value of this object.
    /// </summary>
    public bool Value { get; }

    /// <summary>
    /// Implicitly extract the value of a <see cref="JSBoolean" /> object.
    /// </summary>
    /// <param name="o">The <see cref="JSBoolean" /> object to get the value of.</param>
    /// <returns>The value of the object.</returns>
    public static implicit operator bool(JSBoolean o) =>
        o.Value;

    /// <summary>
    /// Implicitly get a <see cref="JSBoolean" /> object for a specified value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="JSBoolean" /> object.</param>
    /// <returns>Either the <see cref="True" /> object or the <see cref="False" /> object.</returns>
    public static implicit operator JSBoolean(bool value) =>
        value ? True : False;

    #region Abstract Type Conversions

    /// <inheritdoc />
    public override JSValue ToPrimitive(JSType? preferredType = null) =>
        this;

    /// <inheritdoc />
    public override bool ToBoolean() =>
        Value;

    /// <inheritdoc />
    public override JSNumeric ToNumeric() =>
        Value ? JSNumber.One : JSNumber.Zero;

    /// <inheritdoc />
    public override double ToNumber() =>
        Value ? 1 : 0;

    /// <inheritdoc />
    public override double ToIntegerOrInfinity() =>
        Value ? 1 : 0;

    /// <inheritdoc />
    public override int ToInt32() =>
        Value ? 1 : 0;

    /// <inheritdoc />
    public override uint ToUInt32() =>
        Value ? 1u : 0u;

    /// <inheritdoc />
    public override short ToInt16() =>
        (short)(Value ? 1 : 0);

    /// <inheritdoc />
    public override ushort ToUInt16() =>
        (ushort)(Value ? 1 : 0);

    /// <inheritdoc />
    public override sbyte ToInt8() =>
        (sbyte)(Value ? 1 : 0);

    /// <inheritdoc />
    public override byte ToUInt8() =>
        (byte)(Value ? 1 : 0);

    /// <inheritdoc />
    public override byte ToUInt8Clamp() =>
        (byte)(Value ? 1 : 0);

    /// <inheritdoc />
    public override BigInteger ToBigInt() =>
        Value ? BigInteger.One : BigInteger.Zero;

    /// <inheritdoc />
    public override long ToBigInt64() =>
        Value ? 1 : 0;

    /// <inheritdoc />
    public override ulong ToBigUInt64() =>
        Value ? 1u : 0u;

    /// <inheritdoc />
    public override string AbstractToString() =>
        Value ? "true" : "false";

    /// <inheritdoc />
    public override JSObject ToObject() => throw new NotImplementedException();

    /// <inheritdoc />
    public override JSValue ToPropertyKey() =>
        new JSString(Value ? "true" : "false"); // from `AbstractToString`

    /// <inheritdoc />
    public override ulong ToLength() =>
        Value ? 1u : 0u;

    /// <inheritdoc />
    public override ulong ToIndex() =>
        Value ? 1u : 0u;

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
        false;

    /// <inheritdoc />
    public override bool IsRegExp() =>
        false;

    /// <inheritdoc />
    public override bool SameValue(JSValue other) =>
        other.Type is JSType.Boolean && SameValueNonNumeric(other);

    /// <inheritdoc />
    public override bool SameValueZero(JSValue other) =>
        other.Type is JSType.Boolean && SameValueNonNumeric(other);

    /// <inheritdoc />
    public override bool SameValueNonNumeric(JSValue other)
    {
        Debug.Assert(other.Type is JSType.Boolean);
        return Value == (JSBoolean)other;
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
        other.Type is JSType.Boolean
            ? IsStrictlyEqual(other)
            : new JSNumber(ToNumber()).IsLooselyEqual(other);

    /// <inheritdoc />
    public override bool IsStrictlyEqual(JSValue other) =>
        other.Type is JSType.Boolean && Value == (JSBoolean)other;

    #endregion
}
