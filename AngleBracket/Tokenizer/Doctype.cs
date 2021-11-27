/* =============================================================================
 * File:   Doctype.cs
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

public class Doctype
{
    private readonly List<Rune> _name = new();
    // private bool _quirks;
    private List<Rune>? _public = null;
    private List<Rune>? _system = null;

    public Doctype()
    { }

    public string Name => RuneHelpers.ConvertToString(_name);
    public bool QuirksMode { get; private set; }
    public string? PublicIdentifier => _public is null ? null : RuneHelpers.ConvertToString(_public);
    public string? SystemIdentifier => _system is null ? null : RuneHelpers.ConvertToString(_system);

    public void AppendName(Rune r) => _name.Add(r);
    public void SetQuirksFlag() => QuirksMode = true;
    public void SetPublicIdentifierToEmptyString() => _public = new();
    public void AppendPublicIdentifier(Rune r)
    {
        Contract.Requires<ArgumentNullException>(_public != null);
        _public!.Add(r);
    }
    public void SetSystemIdentifierToEmptyString() => _system = new();
    public void AppendSystemIdentifier(Rune r)
    {
        Contract.Requires<ArgumentNullException>(_system != null);
        _system!.Add(r);
    }

    public override string ToString()
    {
        StringBuilder ret = new("Doctype { ");

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
