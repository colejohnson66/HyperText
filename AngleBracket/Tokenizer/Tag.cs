/* =============================================================================
 * File:   Tag.cs
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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AngleBracket.Tokenizer;

public class Tag
{
    private readonly StringBuilder _name = new();
    private string? _nameCache;
    private readonly List<Attribute> _attributes = new();

    private Tag(bool isEndTag)
    {
        IsEndTag = isEndTag;
    }
    public static Tag NewStartTag() => new(false);
    public static Tag NewEndTag() => new(true);

    public string Name => _nameCache ??= _name.ToString();
    public bool IsSelfClosing { get; private set; }
    public bool IsEndTag { get; }
    public ReadOnlyCollection<Attribute> Attributes => new(_attributes);

    public void AppendName(int c)
    {
        Debug.Assert(c is >= 0 and <= 0x10FFFF);
        _name.Append(new Rune(c).ToString());
        _nameCache = null;
    }
    public void SetSelfClosingFlag() => IsSelfClosing = true;
    public Attribute NewAttribute()
    {
        Attribute attr = new();
        _attributes.Add(attr);
        return attr;
    }

    /// <summary>
    /// Checks this tag's list of attributes for duplicate attributes.
    /// A duplicate attribute is one with a name that is the same as an earlier attribute's name.
    /// If any are found, the first one is kept, and all others are dropped.
    /// </summary>
    /// <returns>
    /// <c>true</c> if any attributes were removed; otherwise <c>false</c>.
    /// </returns>
    public bool CheckAndCorrectDuplicateAttributes()
    {
        List<string> seenAttrNames = new(_attributes.Count);
        bool anyRemoved = false;
        foreach (Attribute thisAttr in _attributes)
        {
            if (seenAttrNames.Contains(thisAttr.Name))
            {
                anyRemoved = _attributes.Remove(thisAttr);
                Debug.Assert(anyRemoved);
            }
            else
            {
                seenAttrNames.Add(thisAttr.Name);
            }
        }
        return anyRemoved;
    }

    public Attribute? FindAttribute(string name) =>
        _attributes.FirstOrDefault(attr => attr.Name == name);

    public override string ToString()
    {
        StringBuilder ret = new("<");

        if (IsEndTag)
            ret.Append('/');

        ret.Append(Name);

        foreach (Attribute attr in _attributes)
        {
            ret.Append(
                attr.Value is ""
                    ? $" {attr.Name}"
                    : $" {attr.Name}=\"{attr.Value}\"");
        }

        if (IsSelfClosing)
            ret.Append(" /");

        ret.Append('>');
        return ret.ToString();
    }
}
