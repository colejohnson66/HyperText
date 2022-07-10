/* =============================================================================
 * File:   JSValue.Abstract.TypeConversions.cs
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

using System.Numerics;

namespace CurlyBracket.Native;

public abstract partial class JSValue
{
    /// <summary>
    /// Implements the <c>ToPrimitive</c> abstract operation.
    /// </summary>
    /// <param name="preferredType"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toprimitive
    /// </remarks>
    public abstract JSValue ToPrimitive(JSType? preferredType = null);

    /// <summary>
    /// Implements the <c>ToBoolean</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toboolean
    /// </remarks>
    public abstract bool ToBoolean();

    /// <summary>
    /// Implements the <c>ToNumeric</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tonumeric
    /// </remarks>
    public abstract JSValue ToNumeric();

    /// <summary>
    /// Implements the <c>ToNumber</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tonumber
    /// </remarks>
    public abstract JSNumber ToNumber();

    /// <summary>
    /// Implements the <c>ToIntegerOrInfinity</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tointegerorinfinity
    /// </remarks>
    public abstract JSNumber ToIntegerOrInfinity();

    /// <summary>
    /// Implements the <c>ToInt32</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toint32
    /// </remarks>
    public abstract int ToInt32();

    /// <summary>
    /// Implements the <c>ToUInt32</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-touint32
    /// </remarks>
    public abstract uint ToUInt32();

    /// <summary>
    /// Implements the <c>ToInt16</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toint16
    /// </remarks>
    public abstract short ToInt16();

    /// <summary>
    /// Implements the <c>ToUInt16</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-touint16
    /// </remarks>
    public abstract ushort ToUInt16();

    /// <summary>
    /// Implements the <c>ToInt8</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toint8
    /// </remarks>
    public abstract sbyte ToInt8();

    /// <summary>
    /// Implements the <c>ToUInt8</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-touint8
    /// </remarks>
    public abstract byte ToUInt8();

    /// <summary>
    /// Implements the <c>ToUInt8Clamp</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-touint8clamp
    /// </remarks>
    public abstract byte ToUInt8Clamp();

    /// <summary>
    /// Implements the <c>ToBigInt</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tobigint
    /// </remarks>
    public abstract BigInteger ToBigInt();

    /// <summary>
    /// Implements the <c>ToBigInt64</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tobigint64
    /// </remarks>
    public abstract long ToBigInt64();

    /// <summary>
    /// Implements the <c>ToBigUInt64</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tobiguint64
    /// </remarks>
    public abstract ulong ToBigUInt64();

    /// <summary>
    /// Implements the <c>ToString</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Named with the "Abstract" prefix to avoid naming conflicts with <see cref="Object.ToString" />.
    /// https://tc39.es/ecma262/#sec-tostring
    /// </remarks>
    public abstract string AbstractToString();

    /// <summary>
    /// Implements the <c>ToObject</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toobject
    /// </remarks>
    public abstract JSObject ToObject();

    /// <summary>
    /// Implements the <c>ToPropertyKey</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-topropertykey
    /// </remarks>
    public abstract JSValue ToPropertyKey();

    /// <summary>
    /// Implements the <c>ToLength</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tolength
    /// </remarks>
    public abstract JSValue ToLength();

    /// <summary>
    /// Implements the <c>ToIndex</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toindex
    /// </remarks>
    public abstract JSValue ToIndex();
}
