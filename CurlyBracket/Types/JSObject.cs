/* =============================================================================
 * File:   JSObject.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the ECMAScript object type.
 * <https://tc39.es/ecma262/#sec-object-type>
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
/// Represents the object type in ECMAScript.
/// </summary>
[PublicAPI]
public class JSObject : JSValue
{
    /// <summary>
    /// Construct a new <see cref="JSObject" /> object.
    /// </summary>
    public JSObject()
        : base(JSType.Object)
    { }

    #region Abstract Type Conversions

    /// <inheritdoc />
    public override Result<JSValue> ToPrimitive(ToPrimitiveType preferredType = ToPrimitiveType.Default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override bool ToBoolean() =>
        true;

    /// <inheritdoc />
    public override Result<JSNumeric> ToNumeric()
    {
        Result<JSValue> primValueTemp = ToPrimitive(ToPrimitiveType.Number);
        if (!primValueTemp.IsSuccessful)
            return new(primValueTemp.Error);

        JSValue primValue = primValueTemp.Value;
        if (primValue.Type is JSType.BigInt)
            return (JSBigInt)primValue;

        return primValue.ToNumber().Convert(result => (JSNumeric)(JSNumber)result);
    }

    /// <inheritdoc />
    public override Result<double> ToNumber()
    {
        Result<JSValue> primValue = ToPrimitive(ToPrimitiveType.Number);
        return !primValue.IsSuccessful
            ? new(primValue.Error)
            : primValue.Value.ToNumber();
    }


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
    public override Result<BigInteger> ToBigInt() =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override Result<long> ToBigInt64() =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override Result<ulong> ToBigUInt64() =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override Result<string> AbstractToString()
    {
        Result<JSValue> primValue = ToPrimitive(ToPrimitiveType.String);
        return !primValue.IsSuccessful
            ? new(primValue.Error)
            : primValue.Value.AbstractToString();
    }

    /// <inheritdoc />
    public override Result<JSObject> ToObject() =>
        this;

    /// <inheritdoc />
    public override Result<JSValue> ToPropertyKey()
    {
        Result<JSValue> keyTemp = ToPrimitive(ToPrimitiveType.String);
        JSValue key = keyTemp.Value;
        return key.Type is JSType.Symbol
            ? key
            : (JSString)key.AbstractToString().Value; // will never throw
    }

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
    public override Result<bool> IsArray()
    {
        // if this is Array exotic obj, return true
        // if this is Proxy exotic obj:
        //   if [[ProxyHandler]] is null, throw TypeError
        //   return [[ProxyTarget]].IsArray()
        // return false

        // ReSharper disable once ArrangeMethodOrOperatorBody
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool IsCallable() =>
        throw new NotImplementedException(); // return HasInternal([[Call]])

    /// <inheritdoc />
    public override bool IsConstructor() =>
        throw new NotImplementedException(); // return HasInternal([[Construct]])

    // TODO: IsExtensible(o): return o.[[IsExtensible]]()

    /// <inheritdoc />
    public override bool IsIntegralNumber() =>
        false;

    /// <inheritdoc />
    public override bool IsPropertyKey() =>
        false;

    /// <inheritdoc />
    public override bool IsRegExp()
    {
        // matcher = Get(@@match)
        // if matcher is not undef, return matcher.ToBoolean()
        // return Has([[RegExpMatcher]])

        // ReSharper disable once ArrangeMethodOrOperatorBody
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool SameValue(JSValue other) =>
        other.Type is JSType.Object && SameValueNonNumeric(other);

    /// <inheritdoc />
    public override bool SameValueZero(JSValue other) =>
        other.Type is JSType.Object && SameValueNonNumeric(other);

    /// <inheritdoc />
    public override bool SameValueNonNumeric(JSValue other)
    {
        Debug.Assert(other.Type is JSType.Object);
        throw new NotImplementedException(); // return true if x and y are the same object
    }

    #endregion
}
