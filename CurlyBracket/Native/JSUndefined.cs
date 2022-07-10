/* =============================================================================
 * File:   JSUndefined.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the ECMAScript undefined type.
 * <https://tc39.es/ecma262/#sec-ecmascript-language-types-undefined-type>
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
using System.Diagnostics;
using System.Numerics;

namespace CurlyBracket.Native;

public class JSUndefined : JSValue
{
    public JSUndefined()
        : base(JSType.Undefined)
    { }

    public override string ToString() =>
        nameof(JSUndefined);

    #region Abstract Type Conversions

    public override JSValue ToPrimitive(JSType? preferredType = null) =>
        this;

    public override bool ToBoolean() =>
        false;

    public override JSValue ToNumeric() =>
        JSNumber.NaN;

    public override JSNumber ToNumber() =>
        JSNumber.NaN;

    public override JSNumber ToIntegerOrInfinity() =>
        JSNumber.Zero;

    public override int ToInt32() =>
        0;

    public override uint ToUInt32() =>
        0;

    public override short ToInt16() =>
        0;

    public override ushort ToUInt16() =>
        0;

    public override sbyte ToInt8() =>
        0;

    public override byte ToUInt8() =>
        0;

    public override byte ToUInt8Clamp() =>
        0;

    public override BigInteger ToBigInt() =>
        throw new TypeErrorException();

    public override long ToBigInt64() =>
        throw new TypeErrorException();

    public override ulong ToBigUInt64() =>
        throw new TypeErrorException();

    public override string AbstractToString() =>
        "undefined";

    public override JSObject ToObject() =>
        throw new TypeErrorException();

    public override JSValue ToPropertyKey() =>
        new JSString("undefined");

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

    public override bool SameValue(JSValue other) =>
        other.Type is JSType.Undefined && SameValueNonNumeric(other);

    public override bool SameValueZero(JSValue other) =>
        other.Type is JSType.Undefined && SameValueNonNumeric(other);

    public override bool SameValueNonNumeric(JSValue other)
    {
        Debug.Assert(other.Type is JSType.Undefined);
        return true;
    }

    public override bool IsLessThan(JSValue other, bool leftFirst)
    {
        throw new NotImplementedException();
    }

    public override bool IsLooselyEqual(JSValue other) =>
        other.Type switch
        {
            JSType.Undefined => IsStrictlyEqual(other),
            JSType.Null => true,
            _ => false,
        };

    public override bool IsStrictlyEqual(JSValue other) =>
        other.Type is JSType.Undefined && SameValueNonNumeric(other);

    #endregion
}
