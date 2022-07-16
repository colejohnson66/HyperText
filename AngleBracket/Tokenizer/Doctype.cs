/* =============================================================================
 * File:   Doctype.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Represents an HTML DOCTYPE token emitted by the tokenizer.
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
/// Represents an HTML DOCTYPE.
/// </summary>
public class Doctype
{
    private readonly StringBuilder _name = new();
    private StringBuilder? _public = null;
    private StringBuilder? _system = null;

    private string? _nameCache = null;
    private string? _publicCache = null;
    private string? _systemCache = null;


    /// <summary>Get the DOCTYPE's name.</summary>
    public string Name => _nameCache ??= _name.ToString();
    /// <summary>Get a boolean indicating if this DOCTYPE indicates "quirks mode".</summary>
    public bool QuirksMode { get; private set; }
    /// <summary>Get the DOCTYPE's public identifier if it exists.</summary>
    public string? PublicIdentifier => _publicCache ??= _public?.ToString();
    /// <summary>Get the DOCTYPE's system identifier if it exists.</summary>
    public string? SystemIdentifier => _systemCache ??= _system?.ToString();


    /// <summary>
    /// Append a code point to the DOCTYPE's name.
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


    /// <summary>Change the DOCTYPE's public identifier from nothing to an empty string.</summary>
    /// <exception cref="InvalidOperationException">If this method is called twice.</exception>
    public void SetPublicIdentifierToEmptyString()
    {
        if (_public is not null)
            throw new InvalidOperationException();
        _public = new();
    }

    /// <summary>
    /// Append a code point to the DOCTYPE's public identifier.
    /// </summary>
    /// <param name="c">The code point to add.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="c" /> is not a valid Unicode code point.
    /// </exception>
    /// <exception cref="InvalidOperationException">If <see cref="PublicIdentifier" /> is <c>null</c>.</exception>
    public void AppendPublicIdentifier(int c)
    {
        if (_public is null)
            throw new InvalidOperationException();
        if (c is < 0 or > 0x10FFFF)
            throw new ArgumentOutOfRangeException(EM.ArgumentOutOfRange.ArgumentMustBeValidUnicode);

        if (Rune.IsValid(c))
            _public.Append((char)c);
        else
            _public.Append(new Rune(c).ToString());
        _publicCache = null;
    }


    /// <summary>Change the DOCTYPE's system identifier from nothing to an empty string.</summary>
    /// <exception cref="InvalidOperationException">If this method is called twice.</exception>
    public void SetSystemIdentifierToEmptyString() => _system = new();

    /// <summary>
    /// Append a code point to the DOCTYPE's system identifier.
    /// </summary>
    /// <param name="c">The code point to add.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="c" /> is not a valid Unicode code point.
    /// </exception>
    /// <exception cref="InvalidOperationException">If <see cref="SystemIdentifier" /> is <c>null</c>.</exception>
    public void AppendSystemIdentifier(int c)
    {
        if (_system is null)
            throw new InvalidOperationException();
        if (c is < 0 or > 0x10FFFF)
            throw new ArgumentOutOfRangeException(EM.ArgumentOutOfRange.ArgumentMustBeValidUnicode);

        if (Rune.IsValid(c))
            _system.Append((char)c);
        else
            _system.Append(new Rune(c).ToString());
        _systemCache = null;
    }


    /// <summary>Set the DOCTYPE's "quirks mode" flag.</summary>
    public void SetQuirksFlag() => QuirksMode = true;


    /// <inheritdoc />
    public override string ToString()
    {
        if (QuirksMode)
            return $"<!DOCTYPE {Name}>, Quirks=True";

        StringBuilder ret = new($"<!DOCTYPE {Name}");

        // only "PUBLIC" or "SYSTEM" allowed, not both at the same time
        if (PublicIdentifier is not null)
        {
            ret.Append($" PUBLIC \"{PublicIdentifier}\"");
            if (SystemIdentifier is not null)
                ret.Append($" \"{SystemIdentifier}\">");
            return ret.ToString();
        }

        if (SystemIdentifier is not null)
        {
            ret.Append($" SYSTEM \"{SystemIdentifier}\">");
            return ret.ToString();
        }

        ret.Append('>');
        return ret.ToString();
    }
}
