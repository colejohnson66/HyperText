/* =============================================================================
 * File:   Tag.cs
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

using System.Linq;
using System.Text;

namespace AngleBracket.Tokenizer;

public class Tag
{
    private readonly StringBuilder _name = new();
    // private bool _selfClosing;
    // private bool _isEndTag;
    private readonly List<Attribute> _attributes = new();

    private Tag(bool isEndTag)
    {
        IsEndTag = isEndTag;
    }
    public static Tag NewStartTag() => new(false);
    public static Tag NewEndTag() => new(true);

    public string Name => _name.ToString();
    public bool IsSelfClosing { get; private set; }
    public bool IsEndTag { get; }
    public List<Attribute> Attributes => _attributes;

    public void AppendName(char c) => _name.Append(c);
    public void AppendName(int c) => _name.Append(char.ConvertFromUtf32(c));
    public void SetSelfClosingFlag() => IsSelfClosing = true;
    public Attribute NewAttribute()
    {
        Attribute attr = new();
        _attributes.Add(attr);
        return attr;
    }

    public override string ToString()
    {
        StringBuilder ret = new("Tag { ");

        ret.Append(Name);

        if (IsSelfClosing)
            ret.Append(", SelfClosing");

        if (IsEndTag)
            ret.Append(", EndTag");

        if (Attributes.Any())
        {
            ret.Append(", Attributes { ");
            for (int i = 0; i < Attributes.Count; i++)
            {
                ret.Append(Attributes[i]);
                if (i != Attributes.Count - 1)
                    ret.Append(", ");
            }
            ret.Append(" }");
        }

        return ret.ToString();
    }
}
