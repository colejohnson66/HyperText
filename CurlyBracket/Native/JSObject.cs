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

using System.Numerics;

namespace CurlyBracket.Native;

public class JSObject : JSValue
{
    public JSObject() : base(JSType.Object)
    {
        throw new NotImplementedException();
    }

    #region Abstract Type Conversions

    /// <inheritdoc />
    public override JSValue ToPrimitive(JSType? preferredType = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>OrdinaryToPrimitive</c> abstract operation.
    /// </summary>
    /// <param name="hint"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-ordinarytoprimitive
    /// </remarks>
    public JSValue OrdinaryToPrimitive(JSType hint)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool ToBoolean()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override JSValue ToNumeric()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override JSNumber ToNumber()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override JSNumber ToIntegerOrInfinity()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override int ToInt32()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override uint ToUInt32()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override short ToInt16()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override ushort ToUInt16()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override sbyte ToInt8()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override byte ToUInt8()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override byte ToUInt8Clamp()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override BigInteger ToBigInt()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override long ToBigInt64()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override ulong ToBigUInt64()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override string AbstractToString()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override JSObject ToObject()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override JSValue ToPropertyKey()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override JSValue ToLength()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override JSValue ToIndex()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Abstract Testing/Comparison Operations

    /// <inheritdoc />
    public override JSValue RequireObjectCoercible()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool IsArray()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool IsCallable()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool IsConstructor()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>IsExtensible</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isextensible-o
    /// </remarks>
    public bool IsExtensible()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool IsIntegralNumber()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool IsPropertyKey()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool IsRegExp()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool SameValue(JSValue other)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool SameValueZero(JSValue other)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool SameValueNonNumeric(JSValue other)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool IsLessThan(JSValue other, bool leftFirst)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool IsLooselyEqual(JSValue other)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool IsStrictlyEqual(JSValue other)
    {
        throw new NotImplementedException();
    }

    #endregion
}
