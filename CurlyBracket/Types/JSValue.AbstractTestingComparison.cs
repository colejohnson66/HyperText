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
    public abstract JSValue RequireObjectCoercible();

    /// <summary>
    /// Implements the <c>IsArray</c> abstract operation.
    /// </summary>
    /// <returns>A boolean indicating if this object can be used as array.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isarray
    /// </remarks>
    public abstract bool IsArray();

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
    /// <c>true</c> to calculate <c>this &lt; other</c>; <c>false</c> for <c>other &lt; this</c>.
    /// </param>
    /// <returns>
    /// A boolean indicating if this object is less than or greater than (depending on <paramref name="leftFirst" />)
    ///   the other.
    /// <c>null</c> will be returned if this object or the other is <c>NaN</c>.
    /// </returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-islessthan
    /// </remarks>
    public abstract bool? IsLessThan(JSValue other, bool leftFirst);

    /// <summary>
    /// Implements the <c>IsLooselyEqual</c> abstract operation.
    /// This is called the "abstract equality comparison" is ECMAScript 12.0 (2021) and below.
    /// </summary>
    /// <param name="other">The other object to compare to this one.</param>
    /// <returns>A boolean indicating if this object is loosely equal (<c>==</c>) to the other.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-islooselyequal
    /// </remarks>
    public abstract bool IsLooselyEqual(JSValue other);

    /// <summary>
    /// Implements the <c>IsStrictlyEqual</c> abstract operation.
    /// This is called the "strict equality comparison" is ECMAScript 12.0 (2021) and below.
    /// </summary>
    /// <param name="other">The other object to compare to this one.</param>
    /// <returns>A boolean indicating if this object is strictly equal (<c>===</c>) to the other.</returns>
    /// <remarks>
    /// https://tc39.es/ecma262/#sec-isstrictlyequal
    /// </remarks>
    public abstract bool IsStrictlyEqual(JSValue other);
}
