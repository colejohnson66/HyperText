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

using CurlyBracket.Runtime;

namespace CurlyBracket.Native;

public class JSNull : JSValue
{
    public JSNull()
        : base(JSType.Null)
    { }

    public override string ToString() =>
        $"JSNull";

    #region Abstract Type Conversions

    public override JSValue ToPrimitive(JSType? preferredType = null) =>
        this;

    public override JSBoolean ToBoolean() =>
        JSBoolean.False;

    public override JSValue ToNumeric() =>
        JSNumber.Zero;

    public override JSNumber ToNumber() =>
        JSNumber.Zero;

    public override JSNumber ToIntegerOrInfinity() =>
        JSNumber.Zero;

    public override JSNumber ToInt32() =>
        JSNumber.Zero;

    public override JSNumber ToUInt32() =>
        JSNumber.Zero;

    public override JSNumber ToInt16() =>
        JSNumber.Zero;

    public override JSNumber ToUInt16() =>
        JSNumber.Zero;

    public override JSNumber ToInt8() =>
        JSNumber.Zero;

    public override JSNumber ToUInt8() =>
        JSNumber.Zero;

    public override JSNumber ToUInt8Clamp() =>
        JSNumber.Zero;

    public override JSBigInt ToBigInt() =>
        throw new TypeErrorException();

    public override JSBigInt ToBigInt64() =>
        throw new TypeErrorException();

    public override JSBigInt ToBigUInt64() =>
        throw new TypeErrorException();

    public override JSString AbstractToString() =>
        new("null");

    public override JSObject ToObject() =>
        throw new TypeErrorException();

    public override JSValue ToPropertyKey() =>
        new JSString("null");

    public override JSValue ToLength() =>
        JSNumber.Zero;

    public override JSValue ToIndex() =>
        JSNumber.Zero;

    #endregion

    #region Abstract Testing/Comparison Operations


    public override JSValue RequireObjectCoercible() =>
        throw new TypeErrorException();

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
        JSValue px = leftFirst ? this : other;
        JSValue py = leftFirst ? other : this;

        JSValue nx = px.ToNumeric();
        JSValue ny = py.ToNumeric();
        // if (nx.Type == ny.Type)
        //     return nx.Type is JSType.Number ? Number::LessThan(nx, ny) : BigInt::LessThan(nx, ny);

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
