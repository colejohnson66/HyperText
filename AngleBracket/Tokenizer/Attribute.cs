/* =============================================================================
 * File:   Attribute.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Represents an HTML attribute used in HTML tag tokens that are emitted by the
 *   tokenizer.
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
using System.Text;

namespace AngleBracket.Tokenizer;

/// <summary>
/// Represents an HTML attribute.
/// </summary>
public class Attribute
{
    private readonly StringBuilder _name = new();
    private readonly StringBuilder _value = new();

    private string? _nameCache;
    private string? _valueCache;


    /// <summary>Get the attribute's name.</summary>
    public string Name => _nameCache ??= _name.ToString();
    /// <summary>Get the attribute's value.</summary>
    /// <remarks>Attributes with no value will return an empty string.</remarks>
    public string Value => _valueCache ??= _value.ToString();


    /// <summary>
    /// Append a code point to the attribute's name.
    /// </summary>
    /// <param name="c">The code point to add.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="c" /> is not a valid Unicode code point.
    /// </exception>
    public void AppendToName(int c)
    {
        if (c is < 0 or > 0x10FFFF)
            throw new ArgumentOutOfRangeException(EM.ArgumentOutOfRange.ArgumentMustBeValidUnicode);

        if (Rune.IsValid(c))
            _name.Append((char)c);
        else
            _name.Append(new Rune(c).ToString());
        _nameCache = null;
    }

    /// <summary>
    /// Append a code point to the attribute's value.
    /// </summary>
    /// <param name="c">The code point to add.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="c" /> is not a valid Unicode code point.
    /// </exception>
    public void AppendValue(int c)
    {
        if (c is < 0 or > 0x10FFFF)
            throw new ArgumentOutOfRangeException(EM.ArgumentOutOfRange.ArgumentMustBeValidUnicode);

        if (Rune.IsValid(c))
            _value.Append((char)c);
        else
            _value.Append(new Rune(c).ToString());
        _valueCache = null;
    }


    /// <inheritdoc />
    public override string ToString() =>
        Value is ""
            ? $"{nameof(Attribute)} {{ '{Name}' }}"
            : $"{nameof(Attribute)} {{ '{Name}' = '{Value}' }}";
}
