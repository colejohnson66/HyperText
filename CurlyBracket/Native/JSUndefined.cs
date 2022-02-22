/* =============================================================================
 * File:   JSUndefined.cs
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

public class JSUndefined : JSValue
{
    public JSUndefined() : base(JSType.Undefined)
    {
        throw new NotImplementedException();
    }

    #region Abstract Type Conversions

    public override Result<JSValue, TypeError> ToPrimitive(JSType? preferredType = null) =>
        Result<JSValue, TypeError>.OK(this);

    public override Result<JSValue, TypeError> OrdinaryToPrimitive(JSType hint)
    {
        throw new NotImplementedException();
    }

    public override JSBoolean ToBoolean() =>
        JSBoolean.False;

    public override Result<JSValue, TypeError> ToNumeric() =>
        Result<JSValue, TypeError>.OK(JSNumber.NaN);

    public override Result<JSNumber, TypeError> ToNumber() =>
        Result<JSNumber, TypeError>.OK(JSNumber.NaN);

    public override Result<JSValue, TypeError> StringToNumber()
    {
        throw new NotImplementedException();
    }

    public override Result<JSNumber, TypeError> ToIntegerOrInfinity() =>
        Result<JSNumber, TypeError>.OK(JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToInt32() =>
        Result<JSNumber, TypeError>.OK(JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToUInt32() =>
        Result<JSNumber, TypeError>.OK(JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToInt16() =>
        Result<JSNumber, TypeError>.OK(JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToUInt16() =>
        Result<JSNumber, TypeError>.OK(JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToInt8() =>
        Result<JSNumber, TypeError>.OK(JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToUInt8() =>
        Result<JSNumber, TypeError>.OK(JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToUInt8Clamp() =>
        Result<JSNumber, TypeError>.OK(JSNumber.Zero);

    public override Result<JSBigInt, TypeError> ToBigInt()
    {
        // TypeError
        throw new NotImplementedException();
    }

    public override Result<JSValue, TypeError> StringToBigInt()
    {
        throw new NotImplementedException();
    }

    public override Result<JSBigInt, TypeError> ToBigInt64()
    {
        // TypeError
        throw new NotImplementedException();
    }

    public override Result<JSBigInt, TypeError> ToBigUInt64()
    {
        // TypeError
        throw new NotImplementedException();
    }

    public override Result<JSString, TypeError> AbstractToString()
    {
        // "undefined"
        throw new NotImplementedException();
    }

    public override Result<JSObject, TypeError> ToObject()
    {
        // TypeError
        throw new NotImplementedException();
    }

    public override Result<JSValue, TypeError> ToPropertyKey()
    {
        throw new NotImplementedException();
    }

    public override Result<JSValue, TypeError> ToLength() =>
        Result<JSValue, TypeError>.OK(JSNumber.Zero);

    public override Result<JSValue, TypeError> CanonicalNumericIndexString()
    {
        throw new NotImplementedException();
    }

    public override Result<JSValue, TypeError> ToIndex() =>
        Result<JSValue, TypeError>.OK(JSNumber.Zero);

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
