/* =============================================================================
 * File:   Tag.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Represents an HTML tag token emitted by the tokenizer.
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

using HyperLib;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AngleBracket.Tokenizer;

/// <summary>
/// Represents an HTML tag.
/// </summary>
public class Tag
{
    private readonly StringBuilder _name = new();
    private string? _nameCache;
    private readonly List<Attribute> _attributes = new();


    private Tag(bool isEndTag)
    {
        IsEndTag = isEndTag;
    }


    /// <summary>Construct a new start <see cref="Tag" />.</summary>
    public static Tag NewStartTag() => new(false);

    /// <summary>Construct a new end <see cref="Tag" />.</summary>
    public static Tag NewEndTag() => new(true);


    /// <summary>Get the tag's name.</summary>
    public string Name => _nameCache ??= _name.ToString();
    /// <summary>Get a boolean indicating if this tag is self closing.</summary>
    public bool IsSelfClosing { get; private set; }
    /// <summary>Get a boolean indicating if this tag is an end tag.</summary>
    public bool IsEndTag { get; }
    /// <summary>Get the <see cref="Attribute" />s contained in this tag.</summary>
    public ReadOnlyCollection<Attribute> Attributes => new(_attributes);


    /// <summary>
    /// Append a code point to the tag's name.
    /// </summary>
    /// <param name="c">The code point to add.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="c" /> is not a valid Unicode code point.
    /// </exception>
    public void AppendName(int c)
    {
        if (c is < 0 or > 0x10FFFF)
            throw new ArgumentOutOfRangeException(EM.ArgumentOutOfRange.ArgumentMustBeValidUnicode);

        if (Rune.IsValid(c))
            _name.Append((char)c);
        else
            _name.Append(new Rune(c).ToString());
        _nameCache = null;
    }

    /// <summary>Set the tag's self-closing flag.</summary>
    public void SetSelfClosingFlag() => IsSelfClosing = true;

    /// <summary>Create a new <see cref="Attribute" /> object and add it to <see cref="Attributes" />.</summary>
    /// <returns>The newly constructed <see cref="Attribute" />.</returns>
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

    /// <summary>Find an <see cref="Attribute" /> with a specified name.</summary>
    /// <param name="name">The name of the attribute to find.</param>
    /// <returns>The <see cref="Attribute" /> with a specified name, or <c>null</c> if one couldn't be found.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="name" /> is <c>null</c>.</exception>
    public Attribute? FindAttribute(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return _attributes.FirstOrDefault(attr => attr.Name == name);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder ret = new(IsEndTag ? $"</{Name}" : $"<{Name}");

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
