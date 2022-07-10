/* =============================================================================
 * File:   JSBoolean.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the ECMAScript boolean type.
 * <https://tc39.es/ecma262/#sec-ecmascript-language-types-boolean-type>
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
using System.Numerics;

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
        $"{nameof(JSBoolean)} {{ {(Value ? "true" : "false")} }}";

    #region Abstract Type Conversions

    public override JSValue ToPrimitive(JSType? preferredType = null) =>
        this;

    public override bool ToBoolean() =>
        Value;

    public override JSValue ToNumeric() =>
        Value ? JSNumber.One : JSNumber.Zero;

    public override JSNumber ToNumber() =>
        Value ? JSNumber.One : JSNumber.Zero;

    public override JSNumber ToIntegerOrInfinity() =>
        Value ? JSNumber.One : JSNumber.Zero;

    public override int ToInt32() =>
        Value ? 1 : 0;

    public override uint ToUInt32() =>
        Value ? 1u : 0u;

    public override short ToInt16() =>
        Value ? (short)1 : (short)0;

    public override ushort ToUInt16() =>
        Value ? (ushort)1 : (ushort)0;

    public override sbyte ToInt8() =>
        Value ? (sbyte)1 : (sbyte)0;

    public override byte ToUInt8() =>
        Value ? (byte)1 : (byte)0;

    public override byte ToUInt8Clamp() =>
        Value ? (byte)1 : (byte)0;

    public override BigInteger ToBigInt() =>
        Value ? BigInteger.One : BigInteger.Zero;

    public override long ToBigInt64() =>
        Value ? 1L : 0L;

    public override ulong ToBigUInt64() =>
        Value ? 1ul : 0ul;

    public override string AbstractToString() =>
        Value ? "true" : "false";

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

    public override bool SameValue(JSValue other) =>
        other.Type is JSType.Boolean && SameValueNonNumeric(other);

    public override bool SameValueZero(JSValue other) =>
        other.Type is JSType.Boolean && SameValueNonNumeric(other);

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

    public override bool IsStrictlyEqual(JSValue other) =>
        other.Type is JSType.Boolean && SameValueNonNumeric(other);

    #endregion
}
