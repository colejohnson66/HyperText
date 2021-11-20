/* =============================================================================
 * File:   Attribute.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * <TODO>
 * =============================================================================
 * Copyright (c) 2021 Cole Tobin
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

using System.Text;
using AngleBracket.Text;

namespace AngleBracket.Tokenizer;

public class Attribute
{
    private readonly List<Rune> _name = new();
    private readonly List<Rune> _value = new();

    public Attribute()
    { }

    public string Name => RuneHelpers.ConvertToString(_name);
    public string Value => RuneHelpers.ConvertToString(_value);

    public void AppendName(char c) => _name.Add(new(c));
    public void AppendName(int c) => _name.Add(new(c));
    public void AppendName(Rune r) => _name.Add(r);
    public void AppendValue(char c) => _value.Add(new(c));
    public void AppendValue(int c) => _value.Add(new(c));
    public void AppendValue(Rune r) => _value.Add(r);

    public override string ToString()
    {
        if (Value == "")
            return $"Attribute {{ '{Name}' }}";
        return $"Attribute {{ '{Name}' = '{Value}' }}";
    }
}
