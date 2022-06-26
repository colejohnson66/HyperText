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

using System.Diagnostics;

namespace CurlyBracket.Native;

public class JSBoolean : JSValue
{
    public static JSBoolean True { get; } = new(true);
    public static JSBoolean False { get; } = new(false);

    public JSBoolean(bool value)
        : base(JSType.Boolean)
    {
        Value = value;
    }

    public bool Value { get; }

    public override string ToString() =>
        $"JSBoolean {{ {(Value ? "true" : "false")} }}";

    #region Abstract Type Conversions

    public override JSValue ToPrimitive(JSType? preferredType = null) =>
        this;

    public override JSBoolean ToBoolean() =>
        this;

    public override JSValue ToNumeric() =>
        Value ? JSNumber.One : JSNumber.Zero;

    public override JSNumber ToNumber() =>
        Value ? JSNumber.One : JSNumber.Zero;

    public override JSNumber ToIntegerOrInfinity() =>
        Value ? JSNumber.One : JSNumber.Zero;

    public override JSNumber ToInt32() =>
        Value ? JSNumber.One : JSNumber.Zero;

    public override JSNumber ToUInt32() =>
        Value ? JSNumber.One : JSNumber.Zero;

    public override JSNumber ToInt16() =>
        Value ? JSNumber.One : JSNumber.Zero;

    public override JSNumber ToUInt16() =>
        Value ? JSNumber.One : JSNumber.Zero;

    public override JSNumber ToInt8() =>
        Value ? JSNumber.One : JSNumber.Zero;

    public override JSNumber ToUInt8() =>
        Value ? JSNumber.One : JSNumber.Zero;

    public override JSNumber ToUInt8Clamp() =>
        Value ? JSNumber.One : JSNumber.Zero;

    public override JSBigInt ToBigInt() =>
        Value ? JSBigInt.One : JSBigInt.Zero;

    public override JSBigInt ToBigInt64() =>
        Value ? JSBigInt.One : JSBigInt.Zero;

    public override JSBigInt ToBigUInt64() =>
        Value ? JSBigInt.One : JSBigInt.Zero;

    public override JSString AbstractToString() =>
        new(Value ? "true" : "false");

    public override JSObject ToObject()
    {
        throw new NotImplementedException();
    }

    public override JSValue ToPropertyKey() =>
        new JSString(Value ? "true" : "false");

    public override JSValue ToLength() =>
        Value ? JSNumber.One : JSNumber.Zero;

    public override JSValue ToIndex() =>
        ToIntegerOrInfinity();

    #endregion

    #region Abstract Testing/Comparison Operations


    public override JSValue RequireObjectCoercible() =>
        this;

    public override bool IsArray() =>
        false;

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
        Debug.Assert(other.Type is JSType.Boolean);
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
