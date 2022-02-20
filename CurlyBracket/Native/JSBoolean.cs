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

namespace CurlyBracket.Native;

public class JSBoolean : JSValue, IEquatable<JSValue>, IEquatable<JSBoolean>
{
    public static JSBoolean True { get; } = new(true);
    public static JSBoolean False { get; } = new(false);

    public JSBoolean(bool value)
        : base(JSType.Boolean)
    {
        Value = value;
    }

    public bool Value { get; }

    public override bool Equals(object? obj) =>
        Equals(obj as JSBoolean);

    public bool Equals(JSValue? other) =>
        Equals(other as JSBoolean);

    public bool Equals(JSBoolean? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Value == other.Value;
    }

    public override int GetHashCode() =>
        Value.GetHashCode();

    public override string ToString() =>
        $"JSBoolean {{ {(Value ? "true" : "false")} }}";
}
