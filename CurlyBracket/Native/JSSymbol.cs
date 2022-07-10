/* =============================================================================
 * File:   JSSymbol.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the ECMAScript symbol type.
 * <https://tc39.es/ecma262/#sec-ecmascript-language-types-symbol-type>
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

public class JSSymbol : JSValue
{
    public JSSymbol(string description)
        : base(JSType.Symbol)
    {
        Description = description;
    }

    public string Description { get; }

    public override string ToString() =>
        $"{nameof(JSSymbol)} {{ {Description} }}";

    #region Abstract Type Conversions

    public override JSValue ToPrimitive(JSType? preferredType = null)
    {
        throw new NotImplementedException();
    }

    public override bool ToBoolean()
    {
        throw new NotImplementedException();
    }

    public override JSValue ToNumeric()
    {
        throw new NotImplementedException();
    }

    public override JSNumber ToNumber()
    {
        throw new NotImplementedException();
    }

    public override JSNumber ToIntegerOrInfinity()
    {
        throw new NotImplementedException();
    }

    public override int ToInt32()
    {
        throw new NotImplementedException();
    }

    public override uint ToUInt32()
    {
        throw new NotImplementedException();
    }

    public override short ToInt16()
    {
        throw new NotImplementedException();
    }

    public override ushort ToUInt16()
    {
        throw new NotImplementedException();
    }

    public override sbyte ToInt8()
    {
        throw new NotImplementedException();
    }

    public override byte ToUInt8()
    {
        throw new NotImplementedException();
    }

    public override byte ToUInt8Clamp()
    {
        throw new NotImplementedException();
    }

    public override BigInteger ToBigInt()
    {
        throw new NotImplementedException();
    }

    public override long ToBigInt64()
    {
        throw new NotImplementedException();
    }

    public override ulong ToBigUInt64()
    {
        throw new NotImplementedException();
    }

    public override string AbstractToString()
    {
        throw new NotImplementedException();
    }

    public override JSObject ToObject()
    {
        throw new NotImplementedException();
    }

    public override JSValue ToPropertyKey()
    {
        throw new NotImplementedException();
    }

    public override JSValue ToLength()
    {
        throw new NotImplementedException();
    }

    public override JSValue ToIndex()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Abstract Testing/Comparison Operations

    public override JSValue RequireObjectCoercible()
    {
        throw new NotImplementedException();
    }

    public override bool IsArray()
    {
        throw new NotImplementedException();
    }

    public override bool IsCallable()
    {
        throw new NotImplementedException();
    }

    public override bool IsConstructor()
    {
        throw new NotImplementedException();
    }

    public override bool IsIntegralNumber()
    {
        throw new NotImplementedException();
    }

    public override bool IsPropertyKey()
    {
        throw new NotImplementedException();
    }

    public override bool IsRegExp()
    {
        throw new NotImplementedException();
    }

    public override bool SameValue(JSValue other)
    {
        throw new NotImplementedException();
    }

    public override bool SameValueZero(JSValue other)
    {
        throw new NotImplementedException();
    }

    public override bool SameValueNonNumeric(JSValue other)
    {
        throw new NotImplementedException();
    }

    public override bool IsLessThan(JSValue other, bool leftFirst)
    {
        throw new NotImplementedException();
    }

    public override bool IsLooselyEqual(JSValue other)
    {
        throw new NotImplementedException();
    }

    public override bool IsStrictlyEqual(JSValue other)
    {
        throw new NotImplementedException();
    }

    #endregion
}
