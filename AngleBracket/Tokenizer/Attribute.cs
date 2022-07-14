/* =============================================================================
 * File:   Attribute.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * <TODO>
 * =============================================================================
 * Copyright (c) 2021-2022 Cole Tobin
 *
 * This file is part of AngleBracket.
 *
 * AngleBracket is free software: you can redistribute it and/or modify it under
 *   the terms of the GNU General Public License as published by the Free
 *   Software Foundation, either version 3 of the License, or (at your option)
 *   any later version.
 *
 * AngleBracket is distributed in the hope that it will be useful, but WITHOUT
 *   ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 *   FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License
 *   for more details.
 *
 * You should have received a copy of the GNU General Public License along with
 *   AngleBracket. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System.Diagnostics;
using System.Text;

namespace AngleBracket.Tokenizer;

public class Attribute
{
    private readonly StringBuilder _name = new();
    private string? _nameCache;
    public string Name => _nameCache ??= _name.ToString();

    private readonly StringBuilder _value = new();
    private string? _valueCache;
    public string Value => _valueCache ??= _value.ToString();

    public void AppendName(int c)
    {
        Debug.Assert(c is >= 0 and <= 0x10FFFF);
        _name.Append(new Rune(c).ToString());
        _nameCache = null;
    }

    public void AppendValue(int c)
    {
        Debug.Assert(c is >= 0 and <= 0x10FFFF);
        _value.Append(new Rune(c).ToString());
        _valueCache = null;
    }

    public override string ToString() =>
        Value == ""
            ? $"{nameof(Attribute)} {{ '{Name}' }}"
            : $"{nameof(Attribute)} {{ '{Name}' = '{Value}' }}";
}
