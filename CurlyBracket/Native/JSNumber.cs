/* =============================================================================
 * File:   JSNumber.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * <TODO>
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

using CurlyBracket.Engine;

namespace CurlyBracket.Native;

public class JSNumber : JSValue
{
    public JSNumber() : base(JSType.Number)
    {
        throw new NotImplementedException();
    }

    #region Abstract Type Conversions

    public override Result<JSValue, TypeError> ToPrimitive(JSType? preferredType = null)
    {
        throw new NotImplementedException();
    }

    public override Result<JSValue, TypeError> OrdinaryToPrimitive(JSType hint)
    {
        throw new NotImplementedException();
    }

    public override JSBoolean ToBoolean()
    {
        throw new NotImplementedException();
    }

    public override Result<JSValue, TypeError> ToNumeric()
    {
        throw new NotImplementedException();
    }

    public override Result<JSNumber, TypeError> ToNumber()
    {
        throw new NotImplementedException();
    }

    public override Result<JSValue, TypeError> StringToNumber()
    {
        throw new NotImplementedException();
    }

    public override Result<JSNumber, TypeError> ToIntegerOrInfinity()
    {
        throw new NotImplementedException();
    }

    public override Result<JSNumber, TypeError> ToInt32()
    {
        throw new NotImplementedException();
    }

    public override Result<JSNumber, TypeError> ToUInt32()
    {
        throw new NotImplementedException();
    }

    public override Result<JSNumber, TypeError> ToInt16()
    {
        throw new NotImplementedException();
    }

    public override Result<JSNumber, TypeError> ToUInt16()
    {
        throw new NotImplementedException();
    }

    public override Result<JSNumber, TypeError> ToInt8()
    {
        throw new NotImplementedException();
    }

    public override Result<JSNumber, TypeError> ToUInt8()
    {
        throw new NotImplementedException();
    }

    public override Result<JSNumber, TypeError> ToUInt8Clamp()
    {
        throw new NotImplementedException();
    }

    public override Result<JSBigInt, TypeError> ToBigInt()
    {
        throw new NotImplementedException();
    }

    public override Result<JSValue, TypeError> StringToBigInt()
    {
        throw new NotImplementedException();
    }

    public override Result<JSBigInt, TypeError> ToBigInt64()
    {
        throw new NotImplementedException();
    }

    public override Result<JSBigInt, TypeError> ToBigUInt64()
    {
        throw new NotImplementedException();
    }

    public override Result<JSString, TypeError> AbstractToString()
    {
        throw new NotImplementedException();
    }

    public override Result<JSObject, TypeError> ToObject()
    {
        throw new NotImplementedException();
    }

    public override Result<JSValue, TypeError> ToPropertyKey()
    {
        throw new NotImplementedException();
    }

    public override Result<JSValue, TypeError> ToLength()
    {
        throw new NotImplementedException();
    }

    public override Result<JSValue, TypeError> CanonicalNumericIndexString()
    {
        throw new NotImplementedException();
    }

    public override Result<JSValue, TypeError> ToIndex()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Abstract Testing/Comparison Operations


    public override Result<JSValue, TypeError> RequireObjectCoercible()
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

    public override bool IsExtensible()
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

    public override bool IsStringPrefix(JSString p)
    {
        throw new NotImplementedException();
    }

    public override bool IsStringWellFormedUnicode()
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
