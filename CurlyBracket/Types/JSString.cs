/* =============================================================================
 * File:   JSString.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the ECMAScript string type.
 * <https://tc39.es/ecma262/#sec-ecmascript-language-types-string-type>
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
/// Represents the string type in ECMAScript.
/// </summary>
public class JSString : JSValue
{
    /// <summary>
    /// Construct a new <see cref="JSString" /> with a specified value.
    /// </summary>
    /// <param name="value">The value to set this object to.</param>
    public JSString(string value)
        : base(JSType.String)
    {
        Value = value;
    }

    /// <summary>
    /// The value of this object.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Implicitly extract the value of a <see cref="JSString" /> object.
    /// </summary>
    /// <param name="o">The <see cref="JSString" /> object to get the value of.</param>
    /// <returns>The value of the object.</returns>
    public static implicit operator string(JSString o) =>
        o.Value;

    /// <summary>
    /// Implicitly get a <see cref="JSString" /> object for a specified value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="JSString" /> object.</param>
    /// <returns>A new <see cref="JSString" /> object with the specified value.</returns>
    public static implicit operator JSString(string value) =>
        new(value);

    #region Abstract Type Conversions

    /// <inheritdoc />
    public override JSValue ToPrimitive(JSType? preferredType = null) =>
        this;

    /// <inheritdoc />
    public override bool ToBoolean() =>
        Value.Length is not 0;

    /// <inheritdoc />
    public override JSNumeric ToNumeric() =>
        new JSNumber(ToNumber());

    /// <inheritdoc />
    public override double ToNumber() =>
        throw new NotImplementedException(); // `StringToNumber(this)`


    /// <inheritdoc />
    public override double ToIntegerOrInfinity()
    {
        double number = ToNumber();
        if (number is double.NaN or 0)
            return 0;
        if (double.IsInfinity(number))
            return number;
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override int ToInt32()
    {
        double number = ToNumber();
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override uint ToUInt32()
    {
        double number = ToNumber();
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override short ToInt16()
    {
        double number = ToNumber();
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override ushort ToUInt16()
    {
        double number = ToNumber();
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override sbyte ToInt8()
    {
        double number = ToNumber();
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override byte ToUInt8()
    {
        double number = ToNumber();
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override byte ToUInt8Clamp()
    {
        double number = ToNumber();
        if (number is double.NaN or <= 0)
            return 0;
        if (number >= 255)
            return 255;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override BigInteger ToBigInt()
    {
        // n = StringToBigInt()
        // if n is undef, throw SyntaxErrorException
        // return n

        // ReSharper disable once ArrangeMethodOrOperatorBody
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override long ToBigInt64() =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override ulong ToBigUInt64() =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override string AbstractToString() =>
        Value;

    /// <inheritdoc />
    public override JSObject ToObject() =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override JSValue ToPropertyKey() =>
        this;

    /// <inheritdoc />
    public override ulong ToLength() =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override ulong ToIndex() =>
        throw new NotImplementedException();

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
        other.Type is JSType.String && SameValueNonNumeric(other);

    /// <inheritdoc />
    public override bool SameValueZero(JSValue other) =>
        other.Type is JSType.String && SameValueNonNumeric(other);

    /// <inheritdoc />
    public override bool SameValueNonNumeric(JSValue other)
    {
        Debug.Assert(other.Type is JSType.String);
        return Value.Equals((JSString)Value, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool? IsLessThan(JSValue other, bool leftFirst)
    {
        (JSValue px, JSValue py) = leftFirst
            ? (this, other.ToPrimitive(JSType.Number))
            : (other.ToPrimitive(JSType.Number), (JSValue)this); // cast needed to satisfy the compiler

        if (px.Type is JSType.String && py.Type is JSType.String)
        {
            string px2 = (JSString)px;
            string py2 = (JSString)py;

            // TODO: this is inefficient; possibly O(3n)
            if (py2.StartsWith(px2))
                return false;
            if (px2.StartsWith(py2))
                return true;

            // `StartsWith` calls above handle empty strings
            char m = '\0';
            char n = '\0';
            for (int k = 0; k < px2.Length && k < py2.Length; k++)
            {
                m = px2[k];
                n = px2[k];
                if (m != n)
                    break;
            }
            return m < n;
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
    public override bool IsLooselyEqual(JSValue other) =>
        other.Type switch
        {
            JSType.String => IsStrictlyEqual(other),
            JSType.BigInt => other.IsLooselyEqual(this),
            JSType.Object => IsLooselyEqual(other.ToPrimitive()),
            _ => false,
        };

    /// <inheritdoc />
    public override bool IsStrictlyEqual(JSValue other) =>
        other.Type is JSType.String && SameValueNonNumeric(other);

    #endregion
}
