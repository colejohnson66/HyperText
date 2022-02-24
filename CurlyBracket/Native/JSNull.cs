/* =============================================================================
 * File:   JSNull.cs
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

public class JSNull : JSValue
{
    public JSNull()
        : base(JSType.Null)
    { }

    public override string ToString() =>
        $"JSNull";

    #region Abstract Type Conversions

    public override Result<JSValue, TypeError> ToPrimitive(JSType? preferredType = null) =>
        Result<JSValue, TypeError>.OK(this);

    public override JSBoolean ToBoolean() =>
        JSBoolean.False;

    public override Result<JSValue, TypeError> ToNumeric() =>
        Result<JSValue, TypeError>.OK(JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToNumber() =>
        Result<JSNumber, TypeError>.OK(JSNumber.Zero);

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

    public override Result<JSBigInt, TypeError> ToBigInt() =>
        Result<JSBigInt, TypeError>.Error(new());

    public override Result<JSBigInt, TypeError> ToBigInt64() =>
        Result<JSBigInt, TypeError>.Error(new());

    public override Result<JSBigInt, TypeError> ToBigUInt64() =>
        Result<JSBigInt, TypeError>.Error(new());

    public override Result<JSString, TypeError> AbstractToString() =>
        Result<JSString, TypeError>.OK(new("null"));

    public override Result<JSObject, TypeError> ToObject() =>
        Result<JSObject, TypeError>.Error(new());

    public override Result<JSValue, TypeError> ToPropertyKey() =>
        Result<JSValue, TypeError>.OK(new JSString("null"));

    public override Result<JSValue, TypeError> ToLength() =>
        Result<JSValue, TypeError>.OK(JSNumber.Zero);

    public override Result<JSValue, TypeError> ToIndex() =>
        Result<JSValue, TypeError>.OK(JSNumber.Zero);

    #endregion

    #region Abstract Testing/Comparison Operations


    public override Result<JSValue, TypeError> RequireObjectCoercible() =>
        Result<JSValue, TypeError>.Error(new());

    public override Result<bool, TypeError> IsArray() =>
        Result<bool, TypeError>.OK(false);

    public override bool IsCallable() =>
        false;

    public override bool IsConstructor() =>
        false;

    public override bool IsIntegralNumber() =>
        false;

    public override bool IsPropertyKey() =>
        false;

    public override bool IsRegExp() =>
        false;

    public override bool SameValue(JSValue other)
    {
        if (other.Type is not JSType.Null)
            return false;
        return SameValueNonNumeric(other);
    }

    public override bool SameValueZero(JSValue other)
    {
        if (other.Type is not JSType.Null)
            return false;
        return SameValueNonNumeric(other);
    }

    public override bool SameValueNonNumeric(JSValue other)
    {
        Contract.Assert(other.Type is JSType.Null);
        return true;
    }

    public override bool IsLessThan(JSValue other, bool leftFirst)
    {
        throw new NotImplementedException();
    }

    public override bool IsLooselyEqual(JSValue other)
    {
        if (other.Type is JSType.Null)
            return IsStrictlyEqual(other);

        if (other.Type is JSType.Undefined)
            return true;

        return false;
    }

    public override bool IsStrictlyEqual(JSValue other)
    {
        if (other.Type is not JSType.Null)
            return false;
        return SameValueNonNumeric(other);
    }

    #endregion
}
