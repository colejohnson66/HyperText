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

public class Doctype
{
    private StringBuilder _name = new();
    // private bool _quirks;
    private StringBuilder? _public = null;
    private StringBuilder? _system = null;

    public Doctype()
    { }

    public string Name => _name.ToString();
    public bool QuirksMode { get; private set; }
    public string? PublicIdentifier => _public?.ToString();
    public string? SystemIdentifier => _system?.ToString();

    public void AppendName(char ch) => _name.Append(ch);
    public void SetQuirksFlag() => QuirksMode = true;
    public void SetPublicIdentifierToEmptyString() => _public = new();
    public void AppendPublicIdentifier(char ch)
    {
        Contract.Requires<ArgumentNullException>(_public != null);
        _public!.Append(ch);
    }
    public void SetSystemIdentifierToEmptyString() => _system = new();
    public void AppendSystemIdentifier(char ch)
    {
        Contract.Requires<ArgumentNullException>(_system != null);
        _system!.Append(ch);
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

        if (PublicIdentifier != null)
            ret.Append($", Public {{ '{PublicIdentifier}' }}");

        if (SystemIdentifier != null)
            ret.Append($", System {{ '{SystemIdentifier}' }}");

        ret.Append(" }");
        return ret.ToString();
    }
}
