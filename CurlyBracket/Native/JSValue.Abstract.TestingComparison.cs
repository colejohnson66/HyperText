/* =============================================================================
 * File:   JSValue.cs
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

using CurlyBracket.Engine;

namespace CurlyBracket.Native;

public abstract partial class JSValue
{
    /// <summary>
    /// Implements the <c>RequireObjectCoercible</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-requireobjectcoercible
    /// </remarks>
    public abstract Result<JSValue, TypeError> RequireObjectCoercible();

    /// <summary>
    /// Implements the <c>IsArray</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isarray
    /// </remarks>
    public abstract Result<bool, TypeError> IsArray();

    /// <summary>
    /// Implements the <c>IsCallable</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-iscallable
    /// </remarks>
    public abstract bool IsCallable();

    /// <summary>
    /// Implements the <c>IsConstructor</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isconstructor
    /// </remarks>
    public abstract bool IsConstructor();

    /// <summary>
    /// Implements the <c>IsExtensible</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isextensible-o
    /// </remarks>
    public abstract bool IsExtensible();

    /// <summary>
    /// Implements the <c>IsIntegralNumber</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isintegralnumber
    /// </remarks>
    public abstract bool IsIntegralNumber();

    /// <summary>
    /// Implements the <c>IsPropertyKey</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-ispropertykey
    /// </remarks>
    public abstract bool IsPropertyKey();

    /// <summary>
    /// Implements the <c>IsRegExp</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isregexp
    /// </remarks>
    public abstract bool IsRegExp();

    /// <summary>
    /// Implements the <c>IsStringPrefix</c> abstract operation.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isstringprefix
    /// </remarks>
    public abstract bool IsStringPrefix(JSString p);

    /// <summary>
    /// Implements the <c>IsStringWellFormedUnicode</c> abstract operation.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isstringwellformedunicode
    /// </remarks>
    public abstract bool IsStringWellFormedUnicode();

    /// <summary>
    /// Implements the <c>SameValue</c> abstract operation.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-samevalue
    /// </remarks>
    public abstract bool SameValue(JSValue other);

    /// <summary>
    /// Implements the <c>SameValueZero</c> abstract operation.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-samevaluezero
    /// </remarks>
    public abstract bool SameValueZero(JSValue other);

    /// <summary>
    /// Implements the <c>SameValueNonNumeric</c> abstract operation.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-samevaluenonnumeric
    /// </remarks>
    public abstract bool SameValueNonNumeric(JSValue other);

    /// <summary>
    /// Implements the <c>IsLessThan</c> abstract operation.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="leftFirst"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-islessthan
    /// </remarks>
    public abstract bool IsLessThan(JSValue other, bool leftFirst);

    /// <summary>
    /// Implements the <c>IsLooselyEqual</c> abstract operation.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-islooselyequal
    /// </remarks>
    public abstract bool IsLooselyEqual(JSValue other);

    /// <summary>
    /// Implements the <c>IsStrictlyEqual</c> abstract operation.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isstrictlyequal
    /// </remarks>
    public abstract bool IsStrictlyEqual(JSValue other);
}
