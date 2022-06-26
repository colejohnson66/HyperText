/* =============================================================================
 * File:   JSString.cs
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
using System.Diagnostics;

namespace CurlyBracket.Native;

public class JSString : JSValue
{
    public JSString(string value)
        : base(JSType.String)
    {
        Value = value;
    }

    public string Value { get; }

    public override string ToString() =>
        $"JSString {{ {Value} }}";

    #region Abstract Type Conversions

    public override JSValue ToPrimitive(JSType? preferredType = null) =>
        this;

    public override JSBoolean ToBoolean() =>
        Value.Length is 0 ? JSBoolean.False : JSBoolean.True;

    public override JSValue ToNumeric() =>
        StringToNumber();

    public override JSNumber ToNumber() =>
        StringToNumber();

    /// <summary>
    /// Implements the <c>StringToNumber</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-stringtonumber
    /// </remarks>
    public JSNumber StringToNumber()
    {
        throw new NotImplementedException();
    }

    public override JSNumber ToIntegerOrInfinity() =>
        ToNumber().ToIntegerOrInfinity();

    public override JSNumber ToInt32() =>
        ToNumber().ToInt32();

    public override JSNumber ToUInt32() =>
        ToNumber().ToUInt32();

    public override JSNumber ToInt16() =>
        ToNumber().ToInt16();

    public override JSNumber ToUInt16() =>
        ToNumber().ToUInt16();

    public override JSNumber ToInt8() =>
        ToNumber().ToInt8();

    public override JSNumber ToUInt8() =>
        ToNumber().ToUInt8();

    public override JSNumber ToUInt8Clamp() =>
        ToNumber().ToUInt8Clamp();

    public override JSBigInt ToBigInt()
    {
        JSValue n = StringToBigInt();
        if (n.Type is JSType.Undefined)
            throw new SyntaxErrorException();
        return (JSBigInt)n;
    }

    /// <summary>
    /// Implements the <c>StringToBigInt</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-stringtobigint
    /// </remarks>
    public JSValue StringToBigInt()
    {
        throw new NotImplementedException();
    }

    public override JSBigInt ToBigInt64()
    {
        throw new NotImplementedException();
    }

    public override JSBigInt ToBigUInt64()
    {
        throw new NotImplementedException();
    }

    public override JSString AbstractToString() =>
        this;

    public override JSObject ToObject()
    {
        throw new NotImplementedException();
    }

    public override JSValue ToPropertyKey() =>
        this;

    public override JSValue ToLength() =>
        ToNumber().ToLength();

    /// <summary>
    /// Implements the <c>CanonicalNumericIndexString</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-canonicalnumericindexstring
    /// </remarks>
    public JSValue CanonicalNumericIndexString()
    {
        throw new NotImplementedException();
    }

    public override JSValue ToIndex() =>
        ToNumber().ToIndex();

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
        true;

    public override bool IsRegExp() =>
        false;

    /// <summary>
    /// Implements the <c>IsStringPrefix</c> abstract operation.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isstringprefix
    /// </remarks>
    public bool IsStringPrefix(JSString p)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implements the <c>IsStringWellFormedUnicode</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isstringwellformedunicode
    /// </remarks>
    public bool IsStringWellFormedUnicode()
    {
        throw new NotImplementedException();
    }

    public override bool SameValue(JSValue other)
    {
        if (other.Type is not JSType.String)
            return false;
        return SameValueNonNumeric(other);
    }

    public override bool SameValueZero(JSValue other)
    {
        if (other.Type is not JSType.String)
            return false;
        return SameValueNonNumeric(other);
    }

    public override bool SameValueNonNumeric(JSValue other)
    {
        Debug.Assert(other.Type is JSType.String);
        return Value == ((JSString)other).Value;
    }

    public override bool IsLessThan(JSValue other, bool leftFirst)
    {
        throw new NotImplementedException();
    }

    public override bool IsLooselyEqual(JSValue other)
    {
        if (other.Type is JSType.String)
            IsStrictlyEqual(other);

        if (other.Type is JSType.Number)
            ToNumber().IsStrictlyEqual(other);

        // if (other.Type is JSType.BigInt)
        //     IsLooselyEqual(other, this);

        if (other.Type is JSType.Object)
            return IsLooselyEqual(other.ToPrimitive());

        return false;
    }

    public override bool IsStrictlyEqual(JSValue other)
    {
        if (other.Type is not JSType.String)
            return false;
        return SameValueNonNumeric(other);
    }

    #endregion
}
