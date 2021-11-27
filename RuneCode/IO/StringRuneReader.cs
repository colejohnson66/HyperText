/* =============================================================================
 * File:   StringRuneReader.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements a RuneReader that operates on a C# string.
 * =============================================================================
 * Copyright (c) 2021 Cole Tobin
 *
 * This file is part of RuneCode.
 *
 * RuneCode is free software: you can redistribute it and/or modify it under the
 *   terms of the GNU General Public License as published by the Free Software
 *   Foundation, either version 3 of the License, or (at your option) any later
 *   version.
 *
 * RuneCode is distributed in the hope that it will be useful, but WITHOUT ANY
 *   WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 *   FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
 *   details.
 *
 * You should have received a copy of the GNU General Public License along with
 *   RuneCode. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System.Text;

namespace RuneCode.IO;

public class StringRuneReader : RuneReader
{
    private readonly string _input;
    private int _index;

    public StringRuneReader(string input)
    {
        _input = input;
        _index = 0;
    }

    internal (Rune?, int count) PeekInternal()
    {
        if (_index == _input.Length)
            return (null, 0);

        char c = _input[_index];

        if (Rune.TryCreate(c, out Rune r))
            return (r, 1);

        char c2 = _input[_index + 1];
        if (Rune.TryCreate(c, c2, out r))
            return (r, 2);

        throw new ArgumentException(
            $"Invalid Unicode scalar in the input stream at index {_index}.");
    }

    public override Rune? Peek() => PeekInternal().Item1;

    public override Rune? Read()
    {
        (Rune? r, int count) = PeekInternal();
        _index += count;
        return r;
    }
}
