/* =============================================================================
 * File:   Doctype.cs
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

using AngleBracket.Text;
using System.Diagnostics;
using System.Text;

namespace AngleBracket.Tokenizer;

public class Doctype
{
    private readonly StringBuilder _name = new();
    private StringBuilder? _public = null;
    private StringBuilder? _system = null;

    // public Doctype()
    // { }

    public string Name => _name.ToString();
    public bool QuirksMode { get; private set; }
    public string? PublicIdentifier => _public?.ToString();
    public string? SystemIdentifier => _system?.ToString();

    public void AppendName(int c)
    {
        Debug.Assert(c is >= 0 and <= 0x10FFFF);
        _name.Append(new Rune(c).ToString());
    }
    public void SetQuirksFlag() => QuirksMode = true;
    public void SetPublicIdentifierToEmptyString() => _public = new();
    public void AppendPublicIdentifier(int c)
    {
        Debug.Assert(_public is not null);
        Debug.Assert(c is >= 0 and <= 0x10FFFF);
        _public.Append(new Rune(c).ToString());
    }
    public void SetSystemIdentifierToEmptyString() => _system = new();
    public void AppendSystemIdentifier(int c)
    {
        Debug.Assert(_system is not null);
        Debug.Assert(c is >= 0 and <= 0x10FFFF);
        _system.Append(new Rune(c).ToString());
    }

    public override string ToString()
    {
        StringBuilder ret = new($"{nameof(Doctype)} {{ ");

        ret.Append(Name);

        if (QuirksMode)
        {
            ret.Append(", Quirks }");
            return ret.ToString();
        }

        if (PublicIdentifier is not null)
            ret.Append($", Public {{ '{PublicIdentifier}' }}");

        if (SystemIdentifier is not null)
            ret.Append($", System {{ '{SystemIdentifier}' }}");

        ret.Append(" }");
        return ret.ToString();
    }
}
