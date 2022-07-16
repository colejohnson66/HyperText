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

/// <summary>
/// A token emitted by the <see cref="HtmlTokenizer" />.
/// </summary>
public class Token
{
    private readonly int _character = 0;
    private readonly string? _comment = null;
    private readonly Doctype? _doctype = null;
    private readonly Tag? _tag = null;

    private Token(bool _)
    {
        Type = TokenType.EndOfFile;
    }

    private Token(int c)
    {
        Type = TokenType.Character;
        _character = c;
    }

    private Token(string comment)
    {
        Type = TokenType.Comment;
        _comment = comment;
    }

    private Token(Doctype dt)
    {
        Type = TokenType.Doctype;
        _doctype = dt;
    }

    private Token(Tag tag)
    {
        Type = TokenType.Tag;
        _tag = tag;
    }


    internal static Token NewCharacterToken(int c)
    {
        Debug.Assert(c is >= 0 and <= 0x10FFFF);
        return new(c);
    }

    internal static Token NewCommentToken(string str) => new(str);

    internal static Token NewDoctypeToken(Doctype dt) => new(dt);

    internal static Token NewEndOfFileToken() => new(false);

    internal static Token NewTagToken(Tag tag) => new(tag);

    public TokenType Type { get; }
    public int CharacterValue
    {
        get
        {
            if (Type is not TokenType.Character)
                throw new InvalidOperationException($"{nameof(Token)}.{nameof(Type)} is not {nameof(TokenType.Character)}.");
            return _character;
        }
    }
    public string CommentValue
    {
        get
        {
            if (Type is not TokenType.Comment)
                throw new InvalidOperationException($"{nameof(Token)}.{nameof(Type)} is not {nameof(TokenType.Comment)}.");

            Debug.Assert(_comment is not null);
            return _comment;
        }
    }
    public Doctype DoctypeValue
    {
        get
        {
            if (Type is not TokenType.Doctype)
                throw new InvalidOperationException($"{nameof(Token)}.{nameof(Type)} is not {nameof(TokenType.Doctype)}.");

            Debug.Assert(_doctype is not null);
            return _doctype!;
        }
    }
    public Tag TagValue
    {
        get
        {
            if (Type is not TokenType.Tag)
                throw new InvalidOperationException($"{nameof(Token)}.{nameof(Type)} is not {nameof(TokenType.Tag)}.");

            Debug.Assert(_tag is not null);
            return _tag;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (!Enum.IsDefined(Type))
            throw new InvalidOperationException($"{nameof(Token)} is in an invalid state; The type is unknown.");

        return Type switch
        {
            TokenType.Character => $"{nameof(Token)} {{ U+{CharacterValue:X4} }}",
            TokenType.Comment => $"{nameof(Token)} {{ <!-- {CommentValue} --> }}",
            TokenType.Doctype => $"{nameof(Token)} {{ {DoctypeValue} }}", // `Doctype` writes itself
            TokenType.EndOfFile => $"{nameof(Token)} {{ EOF }}",
            TokenType.Tag => $"{nameof(Token)} {{ {TagValue} }}", // `Tag` writes itself
            _ => throw new(), // unreachable
        };
    }
}
