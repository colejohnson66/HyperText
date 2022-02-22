/* =============================================================================
 * File:   JSBoolean.cs
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

public class JSBoolean : JSValue, IEquatable<JSValue>, IEquatable<JSBoolean>
{
    public static JSBoolean True { get; } = new(true);
    public static JSBoolean False { get; } = new(false);

    public JSBoolean(bool value)
        : base(JSType.Boolean)
    {
        Value = value;
    }

    public bool Value { get; }

    public override bool Equals(object? obj) =>
        Equals(obj as JSBoolean);

    public bool Equals(JSValue? other) =>
        Equals(other as JSBoolean);

    public bool Equals(JSBoolean? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Value == other.Value;
    }

    public override int GetHashCode() =>
        Value.GetHashCode();

    public override string ToString() =>
        $"JSBoolean {{ {(Value ? "true" : "false")} }}";

    #region Abstract Type Conversions

    public override Result<JSValue, TypeError> ToPrimitive(JSType? preferredType = null) =>
        Result<JSValue, TypeError>.OK(this);

    public override Result<JSValue, TypeError> OrdinaryToPrimitive(JSType hint)
    {
        throw new NotImplementedException();
    }

    public override JSBoolean ToBoolean() =>
        this;

    public override Result<JSValue, TypeError> ToNumeric() =>
        Result<JSValue, TypeError>.OK(Value ? JSNumber.One : JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToNumber() =>
        Result<JSNumber, TypeError>.OK(Value ? JSNumber.One : JSNumber.Zero);

    public override Result<JSValue, TypeError> StringToNumber()
    {
        throw new NotImplementedException();
    }

    public override Result<JSNumber, TypeError> ToIntegerOrInfinity() =>
        Result<JSNumber, TypeError>.OK(Value ? JSNumber.One : JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToInt32() =>
        Result<JSNumber, TypeError>.OK(Value ? JSNumber.One : JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToUInt32() =>
        Result<JSNumber, TypeError>.OK(Value ? JSNumber.One : JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToInt16() =>
        Result<JSNumber, TypeError>.OK(Value ? JSNumber.One : JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToUInt16() =>
        Result<JSNumber, TypeError>.OK(Value ? JSNumber.One : JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToInt8() =>
        Result<JSNumber, TypeError>.OK(Value ? JSNumber.One : JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToUInt8() =>
        Result<JSNumber, TypeError>.OK(Value ? JSNumber.One : JSNumber.Zero);

    public override Result<JSNumber, TypeError> ToUInt8Clamp() =>
        Result<JSNumber, TypeError>.OK(Value ? JSNumber.One : JSNumber.Zero);

    public override Result<JSBigInt, TypeError> ToBigInt()
    {
        // 1n if true; 0n if false
        throw new NotImplementedException();
    }

    public override Result<JSValue, TypeError> StringToBigInt()
    {
        throw new NotImplementedException();
    }

    public override Result<JSBigInt, TypeError> ToBigInt64()
    {
        // 1n if true; 0n if false
        throw new NotImplementedException();
    }

    public override Result<JSBigInt, TypeError> ToBigUInt64()
    {
        // 1n if true; 0n if false
        throw new NotImplementedException();
    }

    public override Result<JSString, TypeError> AbstractToString()
    {
        // "true" if true; "false" if false
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

    public override Result<JSValue, TypeError> ToLength() =>
        Result<JSValue, TypeError>.OK(Value ? JSNumber.One : JSNumber.Zero);

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


    public override Result<JSValue, TypeError> RequireObjectCoercible() =>
        Result<JSValue, TypeError>.OK(this);

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
        if (other.Type is not JSType.Boolean)
            return false;
        return SameValueNonNumeric(other);
    }

    public override bool SameValueZero(JSValue other)
    {
        if (other.Type is not JSType.Boolean)
            return false;
        return SameValueNonNumeric(other);
    }

    public override bool SameValueNonNumeric(JSValue other)
    {
        Contract.Assert(other.Type is JSType.Boolean);
        return Value == ((JSBoolean)other).Value;
    }

    public override bool IsLessThan(JSValue other, bool leftFirst)
    {
        throw new NotImplementedException();
    }

    public override bool IsLooselyEqual(JSValue other)
    {
        if (other.Type is JSType.Boolean)
            return IsStrictlyEqual(other);
        // If Type(x) is Boolean, return IsLooselyEqual(! ToNumber(x), y).
        throw new NotImplementedException();
    }

    public override bool IsStrictlyEqual(JSValue other)
    {
        if (other.Type is not JSType.Boolean)
            return false;
        return SameValueNonNumeric(other);
    }

    #endregion
}
