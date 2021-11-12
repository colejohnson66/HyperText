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
 *   the terms of the GNU Lesser General Public License as published by the Free
 *   Software Foundation, either version 3 of the License, or (at your option)
 *   any later version.
 *
 * AngleBracket is distributed in the hope that it will be useful, but WITHOUT
 *   ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 *   FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License
 *   for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 *   along with AngleBracket. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System.Text;

namespace AngleBracket.Tokenizer;

public class Attribute
{
    private readonly StringBuilder _name = new();
    private readonly StringBuilder _value = new();

    public Attribute()
    { }

    public string Name => _name.ToString();
    public string Value => _value.ToString();

    public void AppendName(char ch) => _name.Append(ch);
    public void AppendValue(char ch) => _value.Append(ch);

    public override string ToString()
    {
        if (Value == "")
            return $"Attribute {{ '{Name}' }}";
        return $"Attribute {{ '{Name}' = '{Value}' }}";
    }
}
