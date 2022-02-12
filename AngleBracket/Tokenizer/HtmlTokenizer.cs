/* =============================================================================
 * File:   HtmlTokenizer.cs
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

using System.Linq;
using System.Text;
using AngleBracket.Parser;
using CodePoint.IO;

namespace AngleBracket.Tokenizer;

public partial class HtmlTokenizer : IDisposable
{
    private readonly RuneReader _input;

    public HtmlTokenizer(RuneReader input)
    {
        _input = input;
        _peekBuffer = new();
        InitStateMap();
    }

    private Rune? Peek()
    {
        if (_peekBuffer.Any())
            return _peekBuffer.Peek();

        // normalize out the carriage returns
        // <https://html.spec.whatwg.org/multipage/parsing.html#preprocessing-the-input-stream>
        Rune? r;
        do
        {
            r = _input.Read();
        } while (r?.Value == '\r');

        if (r.HasValue)
            _peekBuffer.Push(r.Value);
        return r;
    }

    private Rune? Read()
    {
        if (_peekBuffer.Any())
            return _peekBuffer.Pop();

        // normalize out the carriage returns
        // <https://html.spec.whatwg.org/multipage/parsing.html#preprocessing-the-input-stream>
        Rune? r;
        do
        {
            r = _input.Read();
        } while (r?.Value == '\r');
        return r;
    }

    private void PutBack(Rune? r)
    {
        Contract.Requires(r is not null); // no EOF
        _peekBuffer.Push(r!.Value);
    }

    private void AddParseError(ParseError error)
    {
        // TODO
    }

    #region IDisposable
    public void Dispose()
    {
        _input.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion
}
