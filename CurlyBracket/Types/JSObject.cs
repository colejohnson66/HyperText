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

using CurlyBracket.Runtime.Properties;
using DotNext;
using HyperLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace CurlyBracket.Types;

/// <summary>
/// Represents the object type in ECMAScript.
/// </summary>
[PublicAPI]
public class JSObject : JSValue
{
    private bool _initialized;
    private Dictionary<string, Property>? _properties;
    private Dictionary<JSSymbol, Property>? _symbols;
    private JSValue _prototype = JSValue.Null;

    /// <summary>
    /// Construct a new <see cref="JSObject" /> object.
    /// </summary>
    public JSObject()
        : base(JSType.Object)
    { }

    // TODO: engine reference

    /// <summary>
    /// Get this object's prototype object, or <see cref="JSNull">null</see>.
    /// </summary>
    public JSValue Prototype => GetPrototypeOf();

    /// <summary>
    /// Get or set a boolean indicating if this object is "extensible"; i.e. if properties can be added to it.
    /// </summary>
    public virtual bool Extensible { get; protected set; }

    #region Ordinary Object Internal Methods and Slots

    /// <summary>
    /// Gets this object's prototype.
    /// </summary>
    /// <returns>The prototype of this object, or <see cref="JSNull">null</see>.</returns>
    /// <remarks>
    /// This implements the <c>GetPrototypeOf</c> abstract operation:
    /// https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-getprototypeof
    /// </remarks>
    public JSValue GetPrototypeOf() =>
        _prototype;

    /// <summary>
    /// Set this object's prototype.
    /// </summary>
    /// <param name="value">The new prototype for this object, or <see cref="JSNull">null</see>.</param>
    /// <returns></returns>
    /// <remarks>
    /// This implements the <c>SetPrototypeOf</c> abstract operation:
    /// https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-setprototypeof-v
    /// </remarks>
    public bool SetPrototypeOf(JSValue value)
    {
        if (value.Type is not (JSType.Object or JSType.Null))
            throw new ArgumentException(null, nameof(value));

        if (value.SameValue(_prototype))
            return true;

        if (!Extensible)
            return false;

        // validate prototype chain
        JSValue p = value;
        while (true)
        {
            if (p.Type is JSType.Null)
                break;
            if (p.SameValue(this))
                return false;
            p = ((JSObject)p)._prototype;
        }

        _prototype = value;
        return true;
    }

    /// <summary>
    /// Get a boolean indicating if this object is "extensible"; i.e. if properties can be added to it.
    /// </summary>
    /// <returns>The value of <see cref="Extensible" />.</returns>
    /// <remarks>
    /// This implements the <c>IsExtensible</c> abstract operation:
    /// https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-isextensible
    /// </remarks>
    public bool IsExtensible() =>
        Extensible;

    /// <summary>
    /// Prevent this object from being "extensible".
    /// </summary>
    /// <returns><c>true</c>.</returns>
    /// <remarks>
    /// This implements the <c>PreventExtensions</c> abstract operation:
    /// https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-preventextensions
    /// </remarks>
    public bool PreventExtensions()
    {
        Extensible = false;
        return true;
    }

    /// <summary>
    /// Get a property from this object.
    /// </summary>
    /// <param name="property">The key of the property to get the value of.</param>
    /// <returns>
    /// The property with the key, <paramref name="property" />, or <c>null</c> if one does not exist.
    /// </returns>
    /// <remarks>
    /// This implements the <c>GetOwnProperty</c> abstract operation:
    /// https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-getownproperty-p
    /// </remarks>
    public Property? GetOwnProperty(JSValue property)
    {
        if (_properties is null)
            return null;

        Result<JSValue> propertyAsKey = property.ToPropertyKey();
        if (!propertyAsKey.IsSuccessful)
            return null;
        Result<string> propertyAsKeyStr = propertyAsKey.Value.AbstractToString();
        if (!propertyAsKeyStr.IsSuccessful)
            return null;
        string key = propertyAsKeyStr.Value;

        if (!_properties.TryGetValue(key, out Property? x))
            return null;

        // TODO: steps 4 and up
        throw new NotImplementedException();
    }

    public Result<bool> DefineOwnProperty(JSValue property, Property descriptor) =>
        throw new NotImplementedException();

    public Result<bool> HasProperty(JSValue property) =>
        throw new NotImplementedException();

    public Result<JSValue> Get(JSValue property, JSValue receiver) =>
        throw new NotImplementedException();

    public Result<bool> Set(JSValue property, JSValue value, JSValue receiver) =>
        throw new NotImplementedException();

    public Result<bool> Delete(JSValue property) =>
        throw new NotImplementedException();

    public List<JSValue> OwnPropertyKeys() =>
        throw new NotImplementedException();

    public static JSValue OrdinaryObjectCreate(JSValue proto) => // TODO: additionalInternalSlotsList
        throw new NotImplementedException();

    public static Result<JSValue> GetPrototypeFromConstructor(JSValue constructor) => // TODO: intrinsicDefaultProto
        throw new NotImplementedException();

    public Result<Unit> RequireInternalSlot(JSValue internalSlot) =>
        throw new NotImplementedException();

    #endregion

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
