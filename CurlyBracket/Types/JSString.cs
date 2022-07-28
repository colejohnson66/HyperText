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

using DotNext;
using System.Diagnostics;
using System.Numerics;

namespace CurlyBracket.Types;

/// <summary>
/// Represents the string type in ECMAScript.
/// </summary>
[PublicAPI]
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
    public override Result<JSValue> ToPrimitive(JSType? preferredType = null) =>
        this;

    /// <inheritdoc />
    public override bool ToBoolean() =>
        Value.Length is not 0;

    /// <inheritdoc />
    public override Result<JSNumeric> ToNumeric() =>
        new JSNumber(ToNumber().Value); // will never throw

    /// <inheritdoc />
    public override Result<double> ToNumber() =>
        throw new NotImplementedException(); // `StringToNumber(this)`

    private JSNumber StringToNumber() =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override Result<double> ToIntegerOrInfinity()
    {
        Result<double> numberTemp = ToNumber();
        if (!numberTemp.IsSuccessful)
            return new(numberTemp.Error);

        double number = numberTemp.Value;
        if (number is double.NaN or 0)
            return 0;
        if (double.IsInfinity(number))
            return number;
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<int> ToInt32()
    {
        Result<double> numberTemp = ToNumber();
        if (!numberTemp.IsSuccessful)
            return new(numberTemp.Error);

        double number = numberTemp.Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<uint> ToUInt32()
    {
        Result<double> numberTemp = ToNumber();
        if (!numberTemp.IsSuccessful)
            return new(numberTemp.Error);

        double number = numberTemp.Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<short> ToInt16()
    {
        Result<double> numberTemp = ToNumber();
        if (!numberTemp.IsSuccessful)
            return new(numberTemp.Error);

        double number = numberTemp.Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<ushort> ToUInt16()
    {
        Result<double> numberTemp = ToNumber();
        if (!numberTemp.IsSuccessful)
            return new(numberTemp.Error);

        double number = numberTemp.Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<sbyte> ToInt8()
    {
        Result<double> numberTemp = ToNumber();
        if (!numberTemp.IsSuccessful)
            return new(numberTemp.Error);

        double number = numberTemp.Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<byte> ToUInt8()
    {
        Result<double> numberTemp = ToNumber();
        if (!numberTemp.IsSuccessful)
            return new(numberTemp.Error);

        double number = numberTemp.Value;
        if (number is double.NaN or 0 or double.PositiveInfinity or double.NegativeInfinity)
            return 0;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<byte> ToUInt8Clamp()
    {
        Result<double> numberTemp = ToNumber();
        if (!numberTemp.IsSuccessful)
            return new(numberTemp.Error);

        double number = numberTemp.Value;
        if (number is double.NaN or <= 0)
            return 0;
        if (number >= 255)
            return 255;

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<BigInteger> ToBigInt()
    {
        // n = StringToBigInt()
        // if n is undef, throw SyntaxErrorException
        // return n

        // ReSharper disable once ArrangeMethodOrOperatorBody
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Result<long> ToBigInt64() =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override Result<ulong> ToBigUInt64() =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override Result<string> AbstractToString() =>
        Value;

    /// <inheritdoc />
    public override Result<JSObject> ToObject() =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override Result<JSValue> ToPropertyKey() =>
        this;

    /// <inheritdoc />
    public override Result<ulong> ToLength() =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override Result<ulong> ToIndex() =>
        throw new NotImplementedException();

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

    #endregion
}
