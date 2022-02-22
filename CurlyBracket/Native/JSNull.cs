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

public class JSNull : JSValue, IEquatable<JSValue>, IEquatable<JSNull>
{
    public JSNull()
        : base(JSType.Null)
    { }

    public override bool Equals(object? obj) =>
        Equals(obj as JSNull);

    public bool Equals(JSValue? other) =>
        Equals(other as JSNull);

    public bool Equals(JSNull? other)
    {
        if (other is null)
            return false;

        return true;
    }

    public override int GetHashCode() =>
        base.GetHashCode();

    public override string ToString() =>
        $"JSNull";

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
        Result<JSValue, TypeError>.OK(JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToNumber() =>
        Result<JSNumber, TypeError>.OK(JSNumber.Zero);

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
        // "null"
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

    public override Result<JSValue, TypeError> ToIndex()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Abstract Testing/Comparison Operations


    public override Result<JSValue, TypeError> RequireObjectCoercible()
    {
        // TypeError
        throw new NotImplementedException();
    }

    public override Result<bool, TypeError> IsArray() =>
        Result<bool, TypeError>.OK(false);

    public override bool IsCallable() =>
        false;

    public override bool IsConstructor() =>
        false;

    public override bool IsExtensible()
    {
        throw new NotImplementedException();
    }

    public override bool IsIntegralNumber() =>
        false;

    public override bool IsPropertyKey() =>
        false;

    public override bool IsRegExp() =>
        false;

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
