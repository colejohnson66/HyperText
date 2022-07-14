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

using System.Diagnostics;

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
    public static Token NewCharacterToken(int c)
    {
        Debug.Assert(c is >= 0 and <= 0x10FFFF);
        return new(TokenType.Character, c);
    }
    public static Token NewCommentToken(string str) => new(TokenType.Comment, str);
    public static Token NewDoctypeToken(Doctype dt) => new(TokenType.Doctype, dt);
    public static Token NewEndOfFileToken() => new(TokenType.EndOfFile);
    public static Token NewTagToken(Tag tag) => new(TokenType.Tag, tag);

    public TokenType Type { get; }
    public int CharacterValue
    {
        get
        {
            if (Type is not TokenType.Character)
                throw new InvalidOperationException($"{nameof(Token)}.{nameof(Type)} is not {nameof(TokenType.Character)}.");
            return _value!;
        }
    }
    public string CommentValue
    {
        get
        {
            if (Type is not TokenType.Comment)
                throw new InvalidOperationException($"{nameof(Token)}.{nameof(Type)} is not {nameof(TokenType.Comment)}.");
            return _value!;
        }
    }
    public Doctype DoctypeValue
    {
        get
        {
            if (Type is not TokenType.Doctype)
                throw new InvalidOperationException($"{nameof(Token)}.{nameof(Type)} is not {nameof(TokenType.Doctype)}.");
            return _value!;
        }
    }
    public Tag TagValue
    {
        get
        {
            if (Type is not TokenType.Tag)
                throw new InvalidOperationException($"{nameof(Token)}.{nameof(Type)} is not {nameof(TokenType.Tag)}.");
            return _value!;
        }
    }

    public override string ToString()
    {
        if (!Enum.IsDefined(Type))
            throw new InvalidOperationException($"{nameof(Token)} is in an invalid state; The type is unknown.");

        return Type switch
        {
            TokenType.Character => $"{nameof(Token)} {{ U+{CharacterValue:X4} }}",
            TokenType.Comment => $"{nameof(Token)} {{ <!-- {CommentValue} --> }}",
            TokenType.Doctype => $"{nameof(Token)} {{ {DoctypeValue} }}", // `Doctype` writes its type
            TokenType.EndOfFile => $"{nameof(Token)} {{ EOF }}",
            TokenType.Tag => $"{nameof(Token)} {{ {TagValue} }}", // `Tag` writes its type
            _ => throw new(), // unreachable
        };
    }
}
