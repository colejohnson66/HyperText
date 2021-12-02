/* =============================================================================
 * File:   JSValue.InternalMethods.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements the various "ordinary object internal methods and slots" as
 *   specified in sections 6.1.7 and 10.1 of ECMAScript 2021:
 * <https://262.ecma-international.org/12.0/#sec-object-internal-methods-and-internal-slots>
 * <https://262.ecma-international.org/12.0/#sec-ordinary-object-internal-methods-and-internal-slots>
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

using System.Collections.Generic;

namespace CurlyBracket.Objects;

public abstract partial class JSValue
{
    private Dictionary<string, JSValue> _properties = new();

    // TODO: JSObject | null
    public virtual JSValue GetPrototypeOf() => throw new NotImplementedException();

    public virtual bool SetPrototypeOf(JSValue v) => throw new NotImplementedException();

    public virtual bool IsExtensible() => throw new NotImplementedException();

    public virtual bool PreventExtensions() => throw new NotImplementedException();

    // TODO: undefined | PropDescriptor
    public virtual JSValue GetOwnProperty(JSValue p) => throw new NotImplementedException();

    public virtual bool DefineOwnProperty(JSValue p, JSValue desc) => throw new NotImplementedException();

    public virtual bool HasProperty(JSValue p) => throw new NotImplementedException();

    public virtual JSValue Get(JSValue p, JSValue receiver) => throw new NotImplementedException();

    public virtual bool Set(JSValue p, JSValue v, JSValue receiver) => throw new NotImplementedException();

    public virtual bool Delete(JSValue p) => throw new NotImplementedException();

    // TODO: PropertyKey[]
    public virtual JSValue OwnPropertyKeys() => throw new NotImplementedException();
}
