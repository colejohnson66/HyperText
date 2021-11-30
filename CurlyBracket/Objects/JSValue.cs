/* =============================================================================
 * File:   JSValue.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * <TODO>
 * =============================================================================
 * Copyright (c) 2021 Cole Tobin
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

namespace CurlyBracket.Objects;

public abstract partial class JSValue
{
    public JSType Type { get; private set; }
    public dynamic? Value { get; private set; }

    /// <summary>
    /// Checks if this value is undefined.
    /// Equivalent to <c>value === undefined</c> in JavaScript.
    /// </summary>
    /// <returns><c>true</c> if this value is undefined; otherwise <c>false</c></returns>
    public bool IsUndefined() => Type == JSType.Undefined;

    /// <summary>
    /// Checks if this value is null.
    /// Equivalent to <c>value === null</c> in JavaScript.
    /// </summary>
    /// <returns><c>true</c> if this value is null; otherwise <c>false</c></returns>
    public bool IsNull() => Type == JSType.Null;

    /// <summary>
    /// Checks if this value is a boolean.
    /// Equivalent to <c>typeof value === 'boolean'</c> in JavaScript.
    /// </summary>
    /// <returns><c>true</c> if this value is a boolean; otherwise <c>false</c></returns>
    public bool IsBoolean() => Type == JSType.Boolean;

    /// <summary>
    /// Checks if this value is a string.
    /// Equivalent to <c>typeof value === 'string'</c> in JavaScript.
    /// </summary>
    /// <returns><c>true</c> if this value is a string; otherwise <c>false</c></returns>
    public bool IsString() => Type == JSType.String;

    /// <summary>
    /// Checks if this value is a symbol.
    /// Equivalent to <c>typeof value === 'symbol'</c> in JavaScript.
    /// </summary>
    /// <returns><c>true</c> if this value is a symbol; otherwise <c>false</c></returns>
    public bool IsSymbol() => Type == JSType.Symbol;

    /// <summary>
    /// Checks if this value is a number.
    /// Equivalent to <c>value === 'number'</c> in JavaScript.
    /// </summary>
    /// <returns><c>true</c> if this value is a number; otherwise <c>false</c></returns>
    public bool IsNumber() => Type == JSType.Number;

    /// <summary>
    /// Checks if this value is a BigInt.
    /// Equivalent to <c>value === 'bigint'</c> in JavaScript.
    /// </summary>
    /// <returns><c>true</c> if this value is a BigInt; otherwise <c>false</c></returns>
    public bool IsBigInt() => Type == JSType.BigInt;

    /// <summary>
    /// Checks if this value is a numeric value.
    /// </summary>
    /// <returns><c>true</c> if this value is a numeric value; otherwise <c>false</c></returns>
    public bool IsNumeric() => IsNumber() || IsBigInt();

    /// <summary>
    /// Checks if this value is an object.
    /// Equivalent to <c>value === 'object'</c> in JavaScript.
    /// </summary>
    /// <returns><c>true</c> if this value is a generic JS object; otherwise <c>false</c></returns>
    public bool IsObject() => Type == JSType.Object;

    /// <summary>
    /// Checks if this value is a function (either an invocable object or a built in function).
    /// Equivalent to <c>typeof value === 'function'</c> in JavaScript.
    /// </summary>
    /// <returns><c>true</c> if this value is a function; otherwise <c>false</c></returns>
    public bool IsFunction() => throw new NotImplementedException();
}
