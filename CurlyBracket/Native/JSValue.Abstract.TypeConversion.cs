/* =============================================================================
 * File:   JSValue.Abstract.TypeConversions.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the "Type conversions" section of ECMAScript.
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

using System;
using CurlyBracket.Engine;

namespace CurlyBracket.Native;

public abstract partial class JSValue
{
    /// <summary>
    /// Implements the <c>ToPrimitive</c> abstract operation.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="preferredType"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toprimitive
    /// </remarks>
    public static Result<JSValue, TypeError> ToPrimitive(JSValue input, JSType? preferredType = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>OrdinaryToPrimitive</c> abstract operation.
    /// </summary>
    /// <param name="o"></param>
    /// <param name="preferredType"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-ordinarytoprimitive
    /// </remarks>
    public static Result<JSValue, TypeError> OrdinaryToPrimitive(JSObject o, JSType hint)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToBoolean</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toboolean
    /// </remarks>
    public static JSBoolean ToBoolean(JSValue argument)
    {
        return argument switch
        {
            JSUndefined => JSBoolean.False,
            JSNull => JSBoolean.False,
            JSBoolean b => b,
            JSNumber n => throw new NotImplementedException(),
            JSString s => throw new NotImplementedException(),
            JSSymbol => JSBoolean.True,
            JSBigInt bi => throw new NotImplementedException(),
            JSObject => JSBoolean.True,
            _ => throw new InvalidOperationException($"Unexpected value type: {argument.Type}"),
        };
    }

    /// <summary>
    /// Implements the <c>ToNumeric</c> abstract operation.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tonumeric
    /// </remarks>
    public static Result<JSValue, TypeError> ToNumeric(JSValue value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToNumber</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tonumber
    /// </remarks>
    public static Result<JSNumber, TypeError> ToNumber(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>StringToNumber</c> abstract operation.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-stringtonumber
    /// </remarks>
    public static Result<JSValue, TypeError> StringToNumber(JSString str)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToIntegerOrInfinity</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tointegerorinfinity
    /// </remarks>
    public static Result<JSNumber, TypeError> ToIntegerOrInfinity(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToInt32</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toint32
    /// </remarks>
    public static Result<JSNumber, TypeError> ToInt32(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToUInt32</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-touint32
    /// </remarks>
    public static Result<JSNumber, TypeError> ToUInt32(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToInt16</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toint16
    /// </remarks>
    public static Result<JSNumber, TypeError> ToInt16(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToUInt16</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-touint16
    /// </remarks>
    public static Result<JSNumber, TypeError> ToUInt16(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToInt8</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toint8
    /// </remarks>
    public static Result<JSNumber, TypeError> ToInt8(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToUInt8</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-touint8
    /// </remarks>
    public static Result<JSNumber, TypeError> ToUInt8(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToUInt8Clamp</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-touint8clamp
    /// </remarks>
    public static Result<JSNumber, TypeError> ToUInt8Clamp(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToBigInt</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tobigint
    /// </remarks>
    public static Result<JSBigInt, TypeError> ToBigInt(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>StringToBigInt</c> abstract operation.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-stringtobigint
    /// </remarks>
    public static Result<JSValue, TypeError> StringToBigInt(JSString str)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToBigInt64</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tobigint64
    /// </remarks>
    public static Result<JSBigInt, TypeError> ToBigInt64(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToBigUInt64</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tobiguint64
    /// </remarks>
    public static Result<JSBigInt, TypeError> ToBigUInt64(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToString</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tostring
    /// </remarks>
    public static Result<JSString, TypeError> ToString(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToObject</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toobject
    /// </remarks>
    public static Result<JSObject, TypeError> ToObject(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToPropertyKey</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-topropertykey
    /// </remarks>
    public static Result<JSValue, TypeError> ToPropertyKey(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToLength</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-tolength
    /// </remarks>
    public static Result<JSValue, TypeError> ToLength(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>CanonicalNumericIndexString</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-canonicalnumericindexstring
    /// </remarks>
    public static Result<JSValue, TypeError> CanonicalNumericIndexString(JSValue argument)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>ToIndex</c> abstract operation.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-toindex
    /// </remarks>
    public static Result<JSValue, TypeError> ToIndex(JSValue argument)
    {
        throw new NotImplementedException();
    }
}
