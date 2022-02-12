/* =============================================================================
 * File:   Token.cs
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

using System.Text;

namespace AngleBracket.Tokenizer;

public class Token
{
    // private TokenType _type;
    private readonly dynamic? _value = null;

    private Token(TokenType type, dynamic? value = null)
    {
        Type = type;
        _value = value;
    }
    public static Token NewCharacterToken(Rune r) => new(TokenType.Character, r);
    public static Token NewCommentToken(string str) => new(TokenType.Comment, str);
    public static Token NewDoctypeToken(Doctype dt) => new(TokenType.Doctype, dt);
    public static Token NewEndOfFileToken() => new(TokenType.EndOfFile);
    public static Token NewTagToken(Tag tag) => new(TokenType.Tag, tag);

    public TokenType Type { get; private set; }
    public Rune CharacterValue
    {
        get
        {
            Contract.Assert(Type is TokenType.Character);
            return _value!;
        }
    }
    public string CommentValue
    {
        get
        {
            Contract.Assert(Type is TokenType.Comment);
            return _value!;
        }
    }
    public Doctype DoctypeValue
    {
        get
        {
            Contract.Assert(Type is TokenType.Doctype);
            return _value!;
        }
    }
    public Tag TagValue
    {
        get
        {
            Contract.Assert(Type is TokenType.Tag);
            return _value!;
        }
    }

    public override string ToString()
    {
        Contract.Assert(
            Type is TokenType.Character or TokenType.Comment or TokenType.Doctype or TokenType.EndOfFile or TokenType.Tag,
            $"{nameof(Token)} object is in an invalid state. The type ({Type}) is unknown.");

        return Type switch
        {
            TokenType.Character => $"Token {{ Character {{ {_value} }} }}",
            TokenType.Comment => $"Token {{ Comment {{ {_value} }} }}",
            TokenType.Doctype => $"Token {{ Doctype {{ {_value} }} }}",
            TokenType.EndOfFile => $"Token {{ EndOfFile }}",
            TokenType.Tag => $"Token {{ Tag {{ {_value} }} }}",
            _ => throw new(), // unreachable
        };
    }
}
