/* =============================================================================
 * File:   JSValue.AbstractTypeConversions.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Defines the "Type Conversions" section of ECMAScript.
 * <https://tc39.es/ecma262/#sec-type-conversion>
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
using System.Numerics;

// ReSharper disable CommentTypo (for URLs)

namespace CurlyBracket.Types;

public abstract partial class JSValue
{
    /// <summary>
    /// Implements the <c>ToPrimitive</c> abstract operation.
    /// </summary>
    /// <param name="preferredType">The preferred type to convert an object type into.</param>
    /// <returns>This value converted to a non-object type.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toprimitive
    /// </remarks>
    public abstract Result<JSValue> ToPrimitive(ToPrimitiveType preferredType = ToPrimitiveType.Default);

    /// <summary>
    /// Implements the <c>ToBoolean</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a boolean.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toboolean
    /// </remarks>
    public abstract bool ToBoolean();

    /// <summary>
    /// Implements the <c>ToNumeric</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to either a <see cref="JSNumber" /> or <see cref="JSBigInt" />.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tonumeric
    /// </remarks>
    public abstract Result<JSNumeric> ToNumeric();

    /// <summary>
    /// Implements the <c>ToNumber</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a number.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tonumber
    /// </remarks>
    public abstract Result<double> ToNumber();

    /// <summary>
    /// Implements the <c>ToIntegerOrInfinity</c> abstract operation.
    /// </summary>
    /// <returns>This value converted either to infinity or an integer</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tointegerorinfinity
    /// </remarks>
    public abstract Result<double> ToIntegerOrInfinity();

    /// <summary>
    /// Implements the <c>ToInt32</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a 32 bit signed integer.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toint32
    /// </remarks>
    public abstract Result<int> ToInt32();

    /// <summary>
    /// Implements the <c>ToUInt32</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a 32 bit unsigned integer.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-touint32
    /// </remarks>
    public abstract Result<uint> ToUInt32();

    /// <summary>
    /// Implements the <c>ToInt16</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a 16 bit signed integer.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toint16
    /// </remarks>
    public abstract Result<short> ToInt16();

    /// <summary>
    /// Implements the <c>ToUInt16</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a 16 bit unsigned integer.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-touint16
    /// </remarks>
    public abstract Result<ushort> ToUInt16();

    /// <summary>
    /// Implements the <c>ToInt8</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a 8 bit signed integer.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toint8
    /// </remarks>
    public abstract Result<sbyte> ToInt8();

    /// <summary>
    /// Implements the <c>ToUInt8</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a 8 bit unsigned integer.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-touint8
    /// </remarks>
    public abstract Result<byte> ToUInt8();

    /// <summary>
    /// Implements the <c>ToUInt8Clamp</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a 8 bit unsigned integer by clamping it into range.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-touint8clamp
    /// </remarks>
    public abstract Result<byte> ToUInt8Clamp();

    /// <summary>
    /// Implements the <c>ToBigInt</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to an arbitrarily large integer.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tobigint
    /// </remarks>
    public abstract Result<BigInteger> ToBigInt();

    /// <summary>
    /// Implements the <c>ToBigInt64</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a 64 bit signed integer.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tobigint64
    /// </remarks>
    public abstract Result<long> ToBigInt64();

    /// <summary>
    /// Implements the <c>ToBigUInt64</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a 64 bit unsigned integer.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tobiguint64
    /// </remarks>
    public abstract Result<ulong> ToBigUInt64();

    /// <summary>
    /// Implements the <c>ToString</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a string.</returns>
    /// <remarks>
    /// Named with the "Abstract" prefix to avoid naming conflicts with <see cref="Object.ToString" />.
    /// https://tc39.es/ecma262/#sec-tostring
    /// </remarks>
    public abstract Result<string> AbstractToString();

    /// <summary>
    /// Implements the <c>ToObject</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to an object.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toobject
    /// </remarks>
    public abstract Result<JSObject> ToObject();

    /// <summary>
    /// Implements the <c>ToPropertyKey</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a property key.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-topropertykey
    /// </remarks>
    public abstract Result<JSValue> ToPropertyKey();

    /// <summary>
    /// Implements the <c>ToLength</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a number suitable for use as the length of an array-like object.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tolength
    /// </remarks>
    public abstract Result<ulong> ToLength();

    /// <summary>
    /// Implements the <c>ToIndex</c> abstract operation.
    /// </summary>
    /// <returns>This value converted to a non-negative integer.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toindex
    /// </remarks>
    public abstract Result<ulong> ToIndex();
}
