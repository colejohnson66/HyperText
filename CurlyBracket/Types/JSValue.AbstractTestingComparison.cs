/* =============================================================================
 * File:   JSValue.AbstractTestingComparison.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Defines the "Testing and Comparison Operations" section of ECMAScript.
 * <https://tc39.es/ecma262/#sec-testing-and-comparison-operations>
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

using DotNext;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace CurlyBracket.Types;

public abstract partial class JSValue
{
    /// <summary>
    /// Implements the <c>RequireObjectCoercible</c> abstract operation.
    /// </summary>
    /// <returns>This object if it can be converted to an object through <see cref="ToObject" />.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-requireobjectcoercible
    /// </remarks>
    public abstract Result<JSValue> RequireObjectCoercible();

    /// <summary>
    /// Implements the <c>IsArray</c> abstract operation.
    /// </summary>
    /// <returns>A boolean indicating if this object can be used as array.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isarray
    /// </remarks>
    public abstract Result<bool> IsArray();

    /// <summary>
    /// Implements the <c>IsCallable</c> abstract operation.
    /// </summary>
    /// <returns>A boolean indicating if this object has the internal <c>[[Call]]</c> method.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-iscallable
    /// </remarks>
    public abstract bool IsCallable();

    /// <summary>
    /// Implements the <c>IsConstructor</c> abstract operation.
    /// </summary>
    /// <returns>A boolean indicating if this object has the internal <c>[[Construct]]</c> method.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isconstructor
    /// </remarks>
    public abstract bool IsConstructor();

    /// <summary>
    /// Implements the <c>IsIntegralNumber</c> abstract operation.
    /// </summary>
    /// <returns>A boolean indicating if this object is a finite integer.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isintegralnumber
    /// </remarks>
    public abstract bool IsIntegralNumber();

    /// <summary>
    /// Implements the <c>IsPropertyKey</c> abstract operation.
    /// </summary>
    /// <returns>A boolean indicating if this object can be used as a property key.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-ispropertykey
    /// </remarks>
    public abstract bool IsPropertyKey();

    /// <summary>
    /// Implements the <c>IsRegExp</c> abstract operation.
    /// </summary>
    /// <returns>A boolean indicating if this object can be used as a regular expression.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isregexp
    /// </remarks>
    public abstract bool IsRegExp();

    /// <summary>
    /// Implements the <c>SameValue</c> abstract operation.
    /// </summary>
    /// <param name="other">The other object to compare to this one.</param>
    /// <returns>A boolean indicating if this object is the same as the other.</returns>
    /// <remarks>
    /// <see cref="JSNumber" /> NaNs are considered equal.
    /// In addition, zeros are considered unequal.
    /// https://tc39.es/ecma262/#sec-samevalue
    /// </remarks>
    public abstract bool SameValue(JSValue other);

    /// <summary>
    /// Implements the <c>SameValueZero</c> abstract operation.
    /// </summary>
    /// <param name="other">The other object to compare to this one.</param>
    /// <returns>A boolean indicating if this object is the same as the other.</returns>
    /// <remarks>
    /// <see cref="JSNumber" /> NaNs are considered equal.
    /// https://tc39.es/ecma262/#sec-samevaluezero
    /// </remarks>
    public abstract bool SameValueZero(JSValue other);

    /// <summary>
    /// Implements the <c>SameValueNonNumeric</c> abstract operation.
    /// </summary>
    /// <param name="other">The other object to compare to this one.</param>
    /// <returns>A boolean indicating if this object is the same as the other.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-samevaluenonnumeric
    /// </remarks>
    public abstract bool SameValueNonNumeric(JSValue other);

    /// <summary>
    /// Implements the <c>IsLessThan</c> abstract operation.
    /// This is called the "abstract relational comparison" is ECMAScript 12.0 (2021) and below.
    /// </summary>
    /// <param name="other">The other object to compare to this one.</param>
    /// <param name="leftFirst">
    ///     <c>true</c> to calculate <c>this &lt; other</c>; <c>false</c> for <c>other &lt; this</c>.
    /// </param>
    /// <returns>
    /// A boolean indicating if this object is less than or greater than (depending on <paramref name="leftFirst" />)
    ///   the other.
    /// <c>null</c> will be returned in place of <c>undefined</c>.
    /// </returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-islessthan
    /// </remarks>
    public Result<bool?> IsLessThan(JSValue other, bool leftFirst)
    {
        // steps 1 and 2
        Result<JSValue> pxTemp;
        Result<JSValue> pyTemp;
        if (leftFirst)
        {
            // step 1
            pxTemp = ToPrimitive();
            if (!pxTemp.IsSuccessful)
                return new(pxTemp.Error);

            pyTemp = other.ToPrimitive();
            if (!pyTemp.IsSuccessful)
                return new(pyTemp.Error);
        }
        else
        {
            // step 2
            pxTemp = other.ToPrimitive();
            if (!pxTemp.IsSuccessful)
                return new(pxTemp.Error);

            pyTemp = ToPrimitive();
            if (!pyTemp.IsSuccessful)
                return new(pyTemp.Error);
        }
        JSValue px = pxTemp.Value;
        JSValue py = pyTemp.Value;

        // step 3
        if (px.Type is JSType.String && py.Type is JSType.String)
        {
            string px2 = (JSString)px;
            string py2 = (JSString)py;

            // TODO: this is inefficient; possibly O(3n)?
            if (py2.StartsWith(px2)) // step 3(a)
                return false;
            if (px2.StartsWith(py2)) // step 3(b)
                return true;

            // `StartsWith` calls above handle empty strings
            // steps 3(c), 3(d), and 3(e)
            char m = '\0';
            char n = '\0';
            for (int k = 0; k < px2.Length && k < py2.Length; k++)
            {
                m = px2[k];
                n = px2[k];
                if (m != n)
                    break;
            }
            return m < n;
        }

        // step 4(a)
        if (px.Type is JSType.BigInt && py.Type is JSType.String)
        {
            // ny = StringToBigInt(py)
            // if ny is undef, return undef
            // return BigInt::lessThan(px, ny)
            throw new NotImplementedException();
        }

        // step 4(b)
        if (px.Type is JSType.String && py.Type is JSType.BigInt)
        {
            // nx = StringToBigInt(px)
            // if nx is undef, return undef
            // return BigInt::lessThan(nx, py)
            throw new NotImplementedException();
        }

        // steps 4(c), 4(d), and 4(e)
        Result<JSNumeric> nxTemp = px.ToNumeric();
        Result<JSNumeric> nyTemp = py.ToNumeric();
        if (!nxTemp.IsSuccessful)
            return new(nxTemp.Error);
        if (!nyTemp.IsSuccessful)
            return new(nyTemp.Error);
        JSNumeric nx = nxTemp.Value;
        JSNumeric ny = nyTemp.Value;

        // step 4(f)
        if (nx.Type == ny.Type)
        {
            if (nx.Type is JSType.Number) // step 4(f)(i)
                throw new NotImplementedException(); // TODO: `Number::lessThan(nx, ny)`
            Debug.Assert(nx.Type is JSType.BigInt); // step 4(f)(i)(1)
            throw new NotImplementedException(); // step 4(f)(i)(2); TODO: `BigInt::lessThan(nx, ny)`
        }

        // ReSharper disable once RedundantIfElseBlock
        if (nx.Type is JSType.BigInt)
        {
            // step 4g
            Debug.Assert(ny.Type is JSType.Number);

            // steps 4(h), 4(i), 4(j), and 4(k)
            JSNumber ny2 = (JSNumber)ny;
            return ny2.Value switch
            {
                double.NaN => null,
                double.PositiveInfinity => true,
                double.NegativeInfinity => false,
                _ => throw new NotImplementedException(), // R(nx) < R(ny)
            };
        }
        else
        {
            // step 4g
            Debug.Assert(nx.Type is JSType.Number);
            Debug.Assert(ny.Type is JSType.BigInt);

            // steps 4(h), 4(i), 4(j), and 4(k)
            JSNumber nx2 = (JSNumber)nx;
            return nx2.Value switch
            {
                double.NaN => null,
                double.NegativeInfinity => true,
                double.PositiveInfinity => false,
                _ => throw new NotImplementedException(), // R(nx) < R(ny)
            };
        }
    }

    /// <summary>
    /// Implements the <c>IsLooselyEqual</c> abstract operation.
    /// This is called the "abstract equality comparison" is ECMAScript 12.0 (2021) and below.
    /// </summary>
    /// <param name="other">The other object to compare to this one.</param>
    /// <returns>A boolean indicating if this object is loosely equal (<c>==</c>) to the other.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-islooselyequal
    /// </remarks>
    // ReSharper disable once CommentTypo
    // NOTE: virtual because `[[IsHTMLDDA]]` needs to change semantics
    [SuppressMessage("ReSharper", "TailRecursiveCall")]
    public virtual Result<bool> IsLooselyEqual(JSValue other)
    {
        // ReSharper disable once InlineTemporaryVariable
        JSType typeX = Type;
        JSType typeY = other.Type;

        // step 1
        if (typeX == typeY)
            return IsStrictlyEqual(other);

        // step 2
        if (typeX is JSType.Null && typeY is JSType.Undefined)
            return true;

        // step 3
        if (typeX is JSType.Undefined && typeY is JSType.Null)
            return false;

        // ReSharper disable once CommentTypo
        // step 4 is the changes made by `[[IsHTMLDDA]]` slot

        // step 5
        if (typeX is JSType.Number && typeY is JSType.String)
            return IsLooselyEqual((JSNumber)other.ToNumber().Value);

        // step 6
        if (typeX is JSType.String && typeY is JSType.Number)
            return ((JSNumber)other.ToNumber().Value).IsLooselyEqual(this);

        // step 7
        if (typeX is JSType.BigInt && typeY is JSType.String)
        {
            // n = StringToBigInt(other)   // step 7a
            // if n is undef, return false // step 7b
            // return !IsLooselyEqual(n)   // step 7c
            throw new NotImplementedException();
        }

        // step 8
        if (typeX is JSType.String && typeY is JSType.BigInt)
            return other.IsLooselyEqual(this);

        // step 9
        if (typeX is JSType.Boolean)
            return ((JSNumber)ToNumber().Value).IsLooselyEqual(other);

        // step 10
        if (typeY is JSType.Boolean)
            return IsLooselyEqual((JSNumber)other.ToNumber().Value);

        // step 11
        if (typeX is JSType.String or JSType.Number or JSType.BigInt or JSType.Symbol && typeY is JSType.Object)
        {
            Result<JSValue> primValue = other.ToPrimitive();
            return !primValue.IsSuccessful
                ? new(primValue.Error)
                : IsLooselyEqual(primValue.Value);
        }

        // step 12
        if (typeX is JSType.Object && typeY is JSType.String or JSType.Number or JSType.BigInt or JSType.Symbol)
        {
            Result<JSValue> primValue = ToPrimitive();
            return !primValue.IsSuccessful
                ? new(primValue.Error)
                : primValue.Value.IsLooselyEqual(other);
        }

        // step 13
        if (typeX is JSType.BigInt && typeY is JSType.Number)
        {
            BigInteger x = (JSBigInt)this;
            double y = (JSNumber)other;
            if (y is double.NaN or double.PositiveInfinity or double.NegativeInfinity)
                return false;
            throw new NotImplementedException(); // return x == y
        }
        if (typeX is JSType.Number && typeY is JSType.BigInt)
        {
            double x = (JSNumber)this;
            BigInteger y = (JSBigInt)other;
            if (x is double.NaN or double.PositiveInfinity or double.NegativeInfinity)
                return false;
            throw new NotImplementedException(); // return x == y
        }

        // step 14
        return false;
    }

    /// <summary>
    /// Implements the <c>IsStrictlyEqual</c> abstract operation.
    /// This is called the "strict equality comparison" is ECMAScript 12.0 (2021) and below.
    /// </summary>
    /// <param name="other">The other object to compare to this one.</param>
    /// <returns>A boolean indicating if this object is strictly equal (<c>===</c>) to the other.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isstrictlyequal
    /// </remarks>
    public bool IsStrictlyEqual(JSValue other)
    {
        if (Type != other.Type)
            return false;

        if (Type is JSType.Number)
        {
            double x = (JSNumber)this;
            double y = (JSNumber)other;
            // ReSharper disable once CompareOfFloatsByEqualityOperator - intentional
            return x == y;
        }

        if (Type is JSType.BigInt)
        {
            BigInteger x = (JSBigInt)this;
            BigInteger y = (JSBigInt)other;
            return x == y;
        }

        return SameValueNonNumeric(other);
    }
}
