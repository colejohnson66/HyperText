/* =============================================================================
 * File:   HtmlTokenizer.cs
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

using System.IO;
using System.Linq;
using AngleBracket.Parser;

namespace AngleBracket.Tokenizer;

public partial class HtmlTokenizer : IDisposable
{
    private readonly TextReader _input;

    public HtmlTokenizer(TextReader input)
    {
        _input = input;
        _peekBuffer = new();
    }

    private int Peek()
    {
        if (_peekBuffer.Any())
            return _peekBuffer.Peek();

        // normalize out the carriage returns
        // <https://html.spec.whatwg.org/multipage/parsing.html#preprocessing-the-input-stream>
        int c;
        while ((c = _input.Read()) == '\r')
        { }
        _peekBuffer.Push(c);
        return c;
    }

    private int Read()
    {
        if (_peekBuffer.Any())
            return _peekBuffer.Pop();

        // normalize out the carriage returns
        // <https://html.spec.whatwg.org/multipage/parsing.html#preprocessing-the-input-stream>
        int c;
        while ((c = _input.Read()) == '\r')
        { }
        return c;
    }

    private void PutBack(int c)
    {
        Contract.Requires(c >= 0); // no EOF
        _peekBuffer.Push(c);
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
