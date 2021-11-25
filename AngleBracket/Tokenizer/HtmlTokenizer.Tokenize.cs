/* =============================================================================
 * File:   HtmlTokenizer.Tokenizer.cs
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

using System.Linq;
using System.Text;
using static AngleBracket.Infra.CodePoint;
using AngleBracket.Parser;
using System.Globalization;
using System.Diagnostics;
using AngleBracket.Text;

namespace AngleBracket.Tokenizer;

public partial class HtmlTokenizer
{
    private const int EOF = -1;
    private static readonly Rune REPLACEMENT_CHARACTER = new(0xFFFD);

    private Action<int>[]? _stateMap;
    private readonly Queue<Token> _tokensToEmit = new();
    private readonly Stack<Rune> _peekBuffer;

    private TokenizerState _state = TokenizerState.Data;
    private TokenizerState? _returnState = null;
    private readonly List<Rune> _tempBuffer = new();
    private int _charRefCode = 0;

    private Attribute? _currentAttribute = null;
    private StringBuilder? _currentComment = null;
    private Doctype? _currentDoctype = null;
    private Tag? _currentTag = null;

    /// <summary>
    /// Initializes the internal state map.
    /// This can't be <c>static</c> due to the tokenizer function pointers not
    ///   being <c>static</c> themselves.
    /// Theoretically, the tokenizer functions could take a <c>this</c> pointer
    ///   equivalent, but this is what I chose.
    /// </summary>
    private void InitStateMap()
    {
        _stateMap = new Action<int>[(int)TokenizerState.__Count];
        _stateMap[(int)TokenizerState.Data] = Data;
        _stateMap[(int)TokenizerState.RCData] = RCData;
        _stateMap[(int)TokenizerState.RawText] = RawText;
        _stateMap[(int)TokenizerState.ScriptData] = ScriptData;
        _stateMap[(int)TokenizerState.Plaintext] = Plaintext;
        _stateMap[(int)TokenizerState.TagOpen] = TagOpen;
        _stateMap[(int)TokenizerState.EndTagOpen] = EndTagOpen;
        _stateMap[(int)TokenizerState.TagName] = TagName;
        _stateMap[(int)TokenizerState.RCDataLessThanSign] = RCDataLessThanSign;
        _stateMap[(int)TokenizerState.RCDataEndTagOpen] = RCDataEndTagOpen;
        _stateMap[(int)TokenizerState.RCDataEndTagName] = RCDataEndTagName;
        _stateMap[(int)TokenizerState.RawTextLessThanSign] = RawTextLessThanSign;
        _stateMap[(int)TokenizerState.RawTextEndTagOpen] = RawTextEndTagOpen;
        _stateMap[(int)TokenizerState.RawTextEndTagName] = RawTextEndTagName;
        _stateMap[(int)TokenizerState.ScriptDataLessThanSign] = ScriptDataLessThanSign;
        _stateMap[(int)TokenizerState.ScriptDataEndTagOpen] = ScriptDataEndTagOpen;
        _stateMap[(int)TokenizerState.ScriptDataEndTagName] = ScriptDataEndTagName;
        _stateMap[(int)TokenizerState.ScriptDataEscapeStart] = ScriptDataEscapeStart;
        _stateMap[(int)TokenizerState.ScriptDataEscapeStartDash] = ScriptDataEscapeStartDash;
        _stateMap[(int)TokenizerState.ScriptDataEscaped] = ScriptDataEscaped;
        _stateMap[(int)TokenizerState.ScriptDataEscapedDash] = ScriptDataEscapedDash;
        _stateMap[(int)TokenizerState.ScriptDataEscapedDashDash] = ScriptDataEscapedDashDash;
        _stateMap[(int)TokenizerState.ScriptDataEscapedLessThanSign] = ScriptDataEscapedLessThanSign;
        _stateMap[(int)TokenizerState.ScriptDataEscapedEndTagOpen] = ScriptDataEscapedEndTagOpen;
        _stateMap[(int)TokenizerState.ScriptDataEscapedEndTagName] = ScriptDataEscapedEndTagName;
        _stateMap[(int)TokenizerState.ScriptDataDoubleEscapeStart] = ScriptDataDoubleEscapeStart;
        _stateMap[(int)TokenizerState.ScriptDataDoubleEscaped] = ScriptDataDoubleEscaped;
        _stateMap[(int)TokenizerState.ScriptDataDoubleEscapedDash] = ScriptDataDoubleEscapedDash;
        _stateMap[(int)TokenizerState.ScriptDataDoubleEscapedDashDash] = ScriptDataDoubleEscapedDashDash;
        _stateMap[(int)TokenizerState.ScriptDataDoubleEscapedLessThanSign] = ScriptDataDoubleEscapedLessThanSign;
        _stateMap[(int)TokenizerState.ScriptDataDoubleEscapeEnd] = ScriptDataDoubleEscapeEnd;
        _stateMap[(int)TokenizerState.BeforeAttributeName] = BeforeAttributeName;
        _stateMap[(int)TokenizerState.AttributeName] = AttributeName;
        _stateMap[(int)TokenizerState.AfterAttributeName] = AfterAttributeName;
        _stateMap[(int)TokenizerState.BeforeAttributeValue] = BeforeAttributeValue;
        _stateMap[(int)TokenizerState.AttributeValueDoubleQuoted] = AttributeValueDoubleQuoted;
        _stateMap[(int)TokenizerState.AttributeValueSingleQuoted] = AttributeValueSingleQuoted;
        _stateMap[(int)TokenizerState.AttributeValueUnquoted] = AttributeValueUnquoted;
        _stateMap[(int)TokenizerState.AfterAttributeValueQuoted] = AfterAttributeValueQuoted;
        _stateMap[(int)TokenizerState.SelfClosingStartTag] = SelfClosingStartTag;
        _stateMap[(int)TokenizerState.BogusComment] = BogusComment;
        _stateMap[(int)TokenizerState.MarkupDeclarationOpen] = MarkupDeclarationOpen;
        _stateMap[(int)TokenizerState.CommentStart] = CommentStart;
        _stateMap[(int)TokenizerState.CommentStartDash] = CommentStartDash;
        _stateMap[(int)TokenizerState.Comment] = Comment;
        _stateMap[(int)TokenizerState.CommentLessThanSign] = CommentLessThanSign;
        _stateMap[(int)TokenizerState.CommentLessThanSignBang] = CommentLessThanSignBang;
        _stateMap[(int)TokenizerState.CommentLessThanSignBangDash] = CommentLessThanSignBangDash;
        _stateMap[(int)TokenizerState.CommentLessThanSignBangDashDash] = CommentLessThanSignBangDashDash;
        _stateMap[(int)TokenizerState.CommentEndDash] = CommentEndDash;
        _stateMap[(int)TokenizerState.CommentEnd] = CommentEnd;
        _stateMap[(int)TokenizerState.CommentEndBang] = CommentEndBang;
        _stateMap[(int)TokenizerState.Doctype] = Doctype;
        _stateMap[(int)TokenizerState.BeforeDoctypeName] = BeforeDoctypeName;
        _stateMap[(int)TokenizerState.DoctypeName] = DoctypeName;
        _stateMap[(int)TokenizerState.AfterDoctypeName] = AfterDoctypeName;
        _stateMap[(int)TokenizerState.AfterDoctypePublicKeyword] = AfterDoctypePublicKeyword;
        _stateMap[(int)TokenizerState.BeforeDoctypePublicIdentifier] = BeforeDoctypePublicIdentifier;
        _stateMap[(int)TokenizerState.DoctypePublicIdentifierDoubleQuoted] = DoctypePublicIdentifierDoubleQuoted;
        _stateMap[(int)TokenizerState.DoctypePublicIdentifierSingleQuoted] = DoctypePublicIdentifierSingleQuoted;
        _stateMap[(int)TokenizerState.AfterDoctypePublicIdentifier] = AfterDoctypePublicIdentifier;
        _stateMap[(int)TokenizerState.BetweenDoctypePublicAndSystemIdentifiers] = BetweenDoctypePublicAndSystemIdentifiers;
        _stateMap[(int)TokenizerState.AfterDoctypeSystemKeyword] = AfterDoctypeSystemKeyword;
        _stateMap[(int)TokenizerState.BeforeDoctypeSystemIdentifier] = BeforeDoctypeSystemIdentifier;
        _stateMap[(int)TokenizerState.DoctypeSystemIdentifierDoubleQuoted] = DoctypeSystemIdentifierDoubleQuoted;
        _stateMap[(int)TokenizerState.DoctypeSystemIdentifierSingleQuoted] = DoctypeSystemIdentifierSingleQuoted;
        _stateMap[(int)TokenizerState.AfterDoctypeSystemIdentifier] = AfterDoctypeSystemIdentifier;
        _stateMap[(int)TokenizerState.BogusDoctype] = BogusDoctype;
        _stateMap[(int)TokenizerState.CDataSection] = CDataSection;
        _stateMap[(int)TokenizerState.CDataSectionBracket] = CDataSectionBracket;
        _stateMap[(int)TokenizerState.CDataSectionEnd] = CDataSectionEnd;
        _stateMap[(int)TokenizerState.CharacterReference] = CharacterReference;
        _stateMap[(int)TokenizerState.NamedCharacterReference] = NamedCharacterReference;
        _stateMap[(int)TokenizerState.AmbiguousAmpersand] = AmbiguousAmpersand;
        _stateMap[(int)TokenizerState.NumericCharacterReference] = NumericCharacterReference;
        _stateMap[(int)TokenizerState.HexadecimalCharacterReferenceStart] = HexadecimalCharacterReferenceStart;
        _stateMap[(int)TokenizerState.DecimalCharacterReferenceStart] = DecimalCharacterReferenceStart;
        _stateMap[(int)TokenizerState.HexadecimalCharacterReference] = HexadecimalCharacterReference;
        _stateMap[(int)TokenizerState.DecimalCharacterReference] = DecimalCharacterReference;
        _stateMap[(int)TokenizerState.NumericCharacterReferenceEnd] = NumericCharacterReferenceEnd;
    }

#pragma warning disable CA1822
    // could be static, but to avoid having to prefix all calls with
    //   `HtmlTokenizer`, disable that warning
    private bool IsSpecialWhitespace(int c) => c != -1 && IsSpecialWhitespace(new Rune(c));
    private bool IsSpecialWhitespace(Rune r) => r.Value == '\t' || r.Value == '\n' || r.Value == '\f' || r.Value == ' ';
    private Rune ToAsciiLowercase(int c) => ToAsciiLowercase(new Rune(c));
    private Rune ToAsciiLowercase(Rune r) => new(r.Value + ('a' - 'A'));
#pragma warning restore CA1822

    private bool IsCurrentEndTagAnAppropriateOne()
    {
        throw new NotImplementedException();
    }

    private bool WasConsumedAsPartOfAnAttribute() =>
        _returnState == TokenizerState.AttributeValueDoubleQuoted ||
        _returnState == TokenizerState.AttributeValueSingleQuoted ||
        _returnState == TokenizerState.AttributeValueUnquoted;

    private void FlushCodePointsConsumedAsCharacterReference()
    {
        if (WasConsumedAsPartOfAnAttribute())
        {
            foreach (Rune r in _tempBuffer)
                _currentAttribute!.AppendValue(r);
            return;
        }

        foreach (Rune r in _tempBuffer)
            EmitCharacterToken(r);
    }

    private void EmitCharacterToken(int c) => EmitCharacterToken(new Rune(c));
    private void EmitCharacterToken(Rune r) => _tokensToEmit.Enqueue(Token.NewCharacterToken(r));
    private void EmitReplacementCharacterToken() => EmitCharacterToken(REPLACEMENT_CHARACTER);
    private void EmitCharacterTokensFromTemporaryBuffer()
    {
        foreach (Rune r in _tempBuffer)
            EmitCharacterToken(r);
        _tempBuffer.Clear();
    }
    private void EmitCommentToken()
    {
        Contract.Requires<NullReferenceException>(_currentComment != null);
        _tokensToEmit.Enqueue(Token.NewCommentToken(_currentComment!.ToString()));
        _currentComment = null;
    }
    private void EmitDoctypeToken()
    {
        Contract.Requires<NullReferenceException>(_currentDoctype != null);
        _tokensToEmit.Enqueue(Token.NewDoctypeToken(_currentDoctype!));
        _currentDoctype = null;
    }
    private void EmitEndOfFileToken() => _tokensToEmit.Enqueue(Token.NewEndOfFileToken());
    private void EmitTagToken()
    {
        Contract.Requires<NullReferenceException>(_currentTag != null);
        _tokensToEmit.Enqueue(Token.NewTagToken(_currentTag!));
        _currentAttribute = null;
        _currentTag = null;
    }

    private bool CompareTemporaryBuffer(string compareTo)
    {
        if (_tempBuffer.Count != compareTo.Length)
            return false;

        return RuneHelpers.ConvertToString(_tempBuffer) == compareTo;
    }

    /// <summary>
    /// Gets an enumerator that yields multiple <see cref="Token" /> objects
    /// </summary>
    /// <returns>The enumerator</returns>
    public IEnumerable<Token> GetToken()
    {
        InitStateMap();
        while (true)
        {
            while (_tokensToEmit.Any())
            {
                Token token = _tokensToEmit.Dequeue();
                yield return _tokensToEmit.Dequeue();
                if (token.Type == TokenType.EndOfFile)
                    yield break;

                int c = Read();
                Debug.Assert(CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.Surrogate);
                _stateMap![(int)_state](c); // SAFETY: only nullable to silence the compiler; never null outside constructor
            }
        }
    }

    private void Reconsume(TokenizerState newState, int c)
    {
        _state = newState;
        _stateMap![(int)newState](c); // SAFETY: only nullable to silence the compiler; never null outside constructor
    }

    private void Data(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#data-state>

        // Consume the next input character:
        if (c == '&')
        {
            // Set the return state to the data state.
            _returnState = TokenizerState.Data;
            // Switch to the character reference state.
            _state = TokenizerState.CharacterReference;
        }
        else if (c == '<')
        {
            // Switch to the tag open state.
            _state = TokenizerState.TagOpen;
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
        else if (c == EOF)
        {
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
    }

    private void RCData(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rcdata-state>

        // Consume the next input character:
        if (c == '&')
        {
            // Set the return state to the RCDATA state.
            _returnState = TokenizerState.RCData;
            // Switch to the character reference state.
            _state = TokenizerState.CharacterReference;
            return;
        }
        else if (c == '<')
        {
            // Switch to the RCDATA less-than sign state.
            _state = TokenizerState.RCDataLessThanSign;
            return;
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Emit a U+FFFD REPLACEMENT CHARACTER character token.
            EmitReplacementCharacterToken();
            return;
        }
        else if (c == EOF)
        {
            // Emit an end-of-file token.
            EmitEndOfFileToken();
            return;
        }
        else
        {
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
    }

    private void RawText(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rawtext-state>

        // Consume the next input character:
        if (c == '<')
        {
            // Switch to the RAWTEXT less-than sign state.
            _state = TokenizerState.RawTextLessThanSign;
            return;
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Emit a U+FFFD REPLACEMENT CHARACTER character token.
            EmitReplacementCharacterToken();
            return;
        }
        else if (c == EOF)
        {
            // Emit an end-of-file token.
            EmitEndOfFileToken();
            return;
        }
        else
        {
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
    }

    private void ScriptData(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-state>

        // Consume the next input character:
        if (c == '<')
        {
            // Switch to the script data less-than sign state.
            _state = TokenizerState.ScriptDataLessThanSign;
            return;
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Emit a U+FFFD REPLACEMENT CHARACTER character token.
            EmitReplacementCharacterToken();
            return;
        }
        else if (c == EOF)
        {
            // Emit an end-of-file token.
            EmitEndOfFileToken();
            return;
        }
        else
        {
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
    }

    private void Plaintext(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#plaintext-state>

        // Consume the next input character:
        if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Emit a U+FFFD REPLACEMENT CHARACTER character token.
            EmitReplacementCharacterToken();
            return;
        }
        else if (c == EOF)
        {
            // Emit an end-of-file token.
            EmitEndOfFileToken();
            return;
        }
        else
        {
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
    }

    private void TagOpen(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#tag-open-state>

        // Consume the next input character:
        if (c == '!')
        {
            // Switch to the markup declaration open state.
            _state = TokenizerState.MarkupDeclarationOpen;
        }
        else if (c == '/')
        {
            // Switch to the end tag open state.
            _state = TokenizerState.EndTagOpen;
        }
        else if (IsAsciiAlpha(c))
        {
            // Create a new start tag token, set its tag name to the empty string.
            _currentTag = Tag.NewStartTag();
            // Reconsume in the tag name state.
            Reconsume(TokenizerState.TagName, c);
        }
        else if (c == '?')
        {
            // This is an unexpected-question-mark-instead-of-tag-name parse error.
            AddParseError(ParseError.UnexpectedQuestionMarkInsteadOfTagName);
            // Create a comment token whose data is the empty string.
            _currentComment = new();
            // Reconsume in the bogus comment state.
            Reconsume(TokenizerState.BogusComment, c);
        }
        else if (c == EOF)
        {
            // This is an eof-before-tag-name parse error.
            AddParseError(ParseError.EofBeforeTagName);
            // Emit a U+003C LESS-THAN SIGN character token and an end-of-file token.
            EmitCharacterToken('<');
            EmitEndOfFileToken();
        }
        else
        {
            // This is an invalid-first-character-of-tag-name parse error.
            AddParseError(ParseError.InvalidFirstCharacterOfTagName);
            // Emit a U+003C LESS-THAN SIGN character token.
            EmitCharacterToken('<');
            // Reconsume in the data state.
            Reconsume(TokenizerState.Data, c);
        }
    }

    private void EndTagOpen(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#end-tag-open-state>

        // Consume the next input character:
        if (IsAsciiAlpha(c))
        {
            // Create a new end tag token, set its tag name to the empty string.
            _currentTag = Tag.NewEndTag();
            // Reconsume in the tag name state.
            Reconsume(TokenizerState.TagName, c);
        }
        else if (c == '>')
        {
            // This is a missing-end-tag-name parse error.
            AddParseError(ParseError.MissingEndTagName);
            // Switch to the data state.
            _state = TokenizerState.Data;
        }
        else if (c == EOF)
        {
            // This is an eof-before-tag-name parse error.
            AddParseError(ParseError.EofBeforeTagName);
            // Emit a U+003C LESS-THAN SIGN character token, a U+002F SOLIDUS
            //   character token and an end-of-file token.
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitEndOfFileToken();
        }
        else
        {
            // This is an invalid-first-character-of-tag-name parse error.
            AddParseError(ParseError.InvalidFirstCharacterOfTagName);
            // Create a comment token whose data is the empty string.
            _currentComment = new();
            // Reconsume in the bogus comment state.
            Reconsume(TokenizerState.BogusComment, c);
        }
    }

    private void TagName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#tag-name-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Switch to the before attribute name state.
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (c == '/')
        {
            // Switch to the self-closing start tag state.
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (c == '>')
        {
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current tag token.
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(c))
        {
            // Append the lowercase version of the current input character (add
            //   0x0020 to the character's code point) to the current tag
            //   token's tag name.
            _currentTag!.AppendName(ToAsciiLowercase(c));
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Append a U+FFFD REPLACEMENT CHARACTER character to the current
            //   tag token's tag name.
            _currentTag!.AppendName(REPLACEMENT_CHARACTER);
        }
        else if (c == EOF)
        {
            // This is an eof-in-tag parse error.
            AddParseError(ParseError.EofInTag);
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Append the current input character to the current tag token's tag
            //   name.
            _currentTag!.AppendName(c);
        }
    }

    private void RCDataLessThanSign(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rcdata-less-than-sign-state>

        // Consume the next input character:
        if (c == '/')
        {
            // Set the temporary buffer to the empty string.
            _tempBuffer.Clear();
            // Switch to the RCDATA end tag open state.
            _state = TokenizerState.RCDataEndTagOpen;
        }
        else
        {
            // Emit a U+003C LESS-THAN SIGN character token.
            EmitCharacterToken('<');
            // Reconsume in the RCDATA state.
            Reconsume(TokenizerState.RCData, c);
        }
    }

    private void RCDataEndTagOpen(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rcdata-end-tag-open-state>

        // Consume the next input character:
        if (IsAsciiAlpha(c))
        {
            // Create a new end tag token, set its tag name to the empty string.
            _currentTag = Tag.NewEndTag();
            // Reconsume in the RCDATA end tag name state.
            Reconsume(TokenizerState.RCDataEndTagName, c);
        }
    }

    private void RCDataEndTagName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rcdata-end-tag-name-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c) && IsCurrentEndTagAnAppropriateOne())
        {
            // If the current end tag token is an appropriate end tag token,
            //   then switch to the before attribute name state.
            // Otherwise, treat it as per the "anything else" entry below.
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (c == '/' && IsCurrentEndTagAnAppropriateOne())
        {
            // If the current end tag token is an appropriate end tag token,
            //   then switch to the self-closing start tag state.
            // Otherwise, treat it as per the "anything else" entry below.
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (c == '>' && IsCurrentEndTagAnAppropriateOne())
        {
            // If the current end tag token is an appropriate end tag token,
            //   then switch to the data state and emit the current tag token.
            // Otherwise, treat it as per the "anything else" entry below.
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(c))
        {
            // Append the lowercase version of the current input character (add
            //   0x0020 to the character's code point) to the current tag
            //   token's tag name.
            _currentTag!.AppendName(ToAsciiLowercase(c));
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(new Rune(c));
        }
        else if (IsAsciiLowerAlpha(c))
        {
            // Append the current input character to the current tag token's tag
            //   name.
            _currentTag!.AppendName(c);
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(new Rune(c));
        }
        else
        {
            // Emit a U+003C LESS-THAN SIGN character token, a U+002F SOLIDUS
            //   character token, and a character token for each of the
            //   characters in the temporary buffer (in the order they were
            //   added to the buffer).
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitCharacterTokensFromTemporaryBuffer();
            // Reconsume in the RCDATA state.
            Reconsume(TokenizerState.RCData, c);
        }
    }

    private void RawTextLessThanSign(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rawtext-less-than-sign-state>

        // Consume the next input character:
        if (c == '/')
        {
            // Set the temporary buffer to the empty string.
            _tempBuffer.Clear();
            // Switch to the RAWTEXT end tag open state.
            _state = TokenizerState.RawTextEndTagOpen;
        }
        else
        {
            // Emit a U+003C LESS-THAN SIGN character token.
            EmitCharacterToken('<');
            // Reconsume in the RAWTEXT state.
            Reconsume(TokenizerState.RawText, c);
        }
    }

    private void RawTextEndTagOpen(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rawtext-end-tag-open-state>

        // Consume the next input character:
        if (IsAsciiAlpha(c))
        {
            // Create a new end tag token, set its tag name to the empty string.
            _currentTag = Tag.NewEndTag();
            // Reconsume in the RAWTEXT end tag name state.
            Reconsume(TokenizerState.RawTextEndTagName, c);
        }
        else
        {
            // Emit a U+003C LESS-THAN SIGN character token and a U+002F SOLIDUS
            //   character token.
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            // Reconsume in the RAWTEXT state.
            Reconsume(TokenizerState.RawText, c);
        }
    }

    private void RawTextEndTagName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rawtext-end-tag-name-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c) && IsCurrentEndTagAnAppropriateOne())
        {
            // If the current end tag token is an appropriate end tag token,
            //   then switch to the before attribute name state.
            // Otherwise, treat it as per the "anything else" entry below.
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (c == '/' && IsCurrentEndTagAnAppropriateOne())
        {
            // If the current end tag token is an appropriate end tag token,
            //   then switch to the self-closing start tag state.
            // Otherwise, treat it as per the "anything else" entry below.
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (c == '>' && IsCurrentEndTagAnAppropriateOne())
        {
            // If the current end tag token is an appropriate end tag token,
            //   then switch to the data state and emit the current tag token.
            // Otherwise, treat it as per the "anything else" entry below.
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(c))
        {
            // Append the lowercase version of the current input character (add
            //   0x0020 to the character's code point) to the current tag
            //   token's tag name.
            _currentTag!.AppendName(ToAsciiLowercase(c));
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(new Rune(c));
        }
        else if (IsAsciiLowerAlpha(c))
        {
            // Append the current input character to the current tag token's tag
            //   name.
            _currentTag!.AppendName(c);
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(new Rune(c));
        }
        else
        {
            // Emit a U+003C LESS-THAN SIGN character token, a U+002F SOLIDUS
            //   character token, and a character token for each of the
            //   characters in the temporary buffer (in the order they were
            //   added to the buffer).
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitCharacterTokensFromTemporaryBuffer();
            // Reconsume in the RAWTEXT state.
            Reconsume(TokenizerState.RawText, c);
        }
    }

    private void ScriptDataLessThanSign(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-less-than-sign-state>

        // Consume the next input character:
        if (c == '/')
        {
            // Set the temporary buffer to the empty string.
            _tempBuffer.Clear();
            // Switch to the script data end tag open state.
            _state = TokenizerState.ScriptDataEndTagOpen;
        }
        else if (c == '!')
        {
            // Switch to the script data escape start state.
            _state = TokenizerState.ScriptDataEscapeStart;
            // Emit a U+003C LESS-THAN SIGN character token and a
            //   U+0021 EXCLAMATION MARK character token.
            EmitCharacterToken('<');
            EmitCharacterToken('!');
        }
        else
        {
            // Emit a U+003C LESS-THAN SIGN character token.
            EmitCharacterToken('<');
            // Reconsume in the script data state.
            Reconsume(TokenizerState.ScriptData, c);
        }
    }

    private void ScriptDataEndTagOpen(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-end-tag-open-state>

        // Consume the next input character:
        if (IsAsciiAlpha(c))
        {
            // Create a new end tag token, set its tag name to the empty string.
            _currentTag = Tag.NewEndTag();
            // Reconsume in the script data end tag name state.
            Reconsume(TokenizerState.ScriptDataEndTagName, c);
        }
        else
        {
            // Emit a U+003C LESS-THAN SIGN character token and a U+002F SOLIDUS
            //   character token.
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            // Reconsume in the script data state.
            Reconsume(TokenizerState.ScriptData, c);
        }
    }

    private void ScriptDataEndTagName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-end-tag-name-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c) && IsCurrentEndTagAnAppropriateOne())
        {
            // If the current end tag token is an appropriate end tag token,
            //   then switch to the before attribute name state.
            // Otherwise, treat it as per the "anything else" entry below.
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (c == '/' && IsCurrentEndTagAnAppropriateOne())
        {
            // If the current end tag token is an appropriate end tag token,
            //   then switch to the self-closing start tag state.
            // Otherwise, treat it as per the "anything else" entry below.
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (c == '>' && IsCurrentEndTagAnAppropriateOne())
        {
            // If the current end tag token is an appropriate end tag token,
            //   then switch to the data state and emit the current tag token.
            // Otherwise, treat it as per the "anything else" entry below.
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(c))
        {
            // Append the lowercase version of the current input character (add
            //   0x0020 to the character's code point) to the current tag
            //   token's tag name.
            _currentTag!.AppendName(ToAsciiLowercase(c));
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(new Rune(c));
        }
        else if (IsAsciiLowerAlpha(c))
        {
            // Append the current input character to the current tag token's tag
            //   name.
            _currentTag!.AppendName(c);
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(new Rune(c));
        }
        else
        {
            // Emit a U+003C LESS-THAN SIGN character token, a U+002F SOLIDUS
            //   character token, and a character token for each of the
            //   characters in the temporary buffer (in the order they were
            //   added to the buffer).
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitCharacterTokensFromTemporaryBuffer();
            // Reconsume in the script data state.
            Reconsume(TokenizerState.ScriptData, c);
        }
    }

    private void ScriptDataEscapeStart(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escape-start-state>

        // Consume the next input character:
        if (c == '-')
        {
            // Switch to the script data escape start dash state.
            _state = TokenizerState.ScriptDataEscapeStartDash;
            // Emit a U+002D HYPHEN-MINUS character token.
            EmitCharacterToken('-');
        }
        else
        {
            // Reconsume in the script data state.
            Reconsume(TokenizerState.ScriptData, c);
        }
    }

    private void ScriptDataEscapeStartDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escape-start-dash-state>

        // Consume the next input character:
        if (c == '-')
        {
            // Switch to the script data escaped dash dash state.
            _state = TokenizerState.ScriptDataEscapedDashDash;
            // Emit a U+002D HYPHEN-MINUS character token.
            EmitCharacterToken('-');
        }
        else
        {
            // Reconsume in the script data state.
            Reconsume(TokenizerState.ScriptData, c);
        }
    }

    private void ScriptDataEscaped(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-state>

        // Consume the next input character:
        if (c == '-')
        {
            // Switch to the script data escaped dash state.
            _state = TokenizerState.ScriptDataEscapedDash;
            // Emit a U+002D HYPHEN-MINUS character token.
            EmitCharacterToken('-');
        }
        else if (c == '<')
        {
            // Switch to the script data escaped less-than sign state.
            _state = TokenizerState.ScriptDataEscapedLessThanSign;
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Emit a U+FFFD REPLACEMENT CHARACTER character token.
            EmitReplacementCharacterToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-script-html-comment-like-text parse error.
            AddParseError(ParseError.EofInScriptHtmlCommentLikeText);
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
    }

    private void ScriptDataEscapedDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-dash-state>

        // Consume the next input character:
        if (c == '-')
        {
            // Switch to the script data escaped dash dash state.
            _state = TokenizerState.ScriptDataEscapedDashDash;
            // Emit a U+002D HYPHEN-MINUS character token.
            EmitCharacterToken('-');
        }
        else if (c == '<')
        {
            // Switch to the script data escaped less-than sign state.
            _state = TokenizerState.ScriptDataEscapedLessThanSign;
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Switch to the script data escaped state.
            _state = TokenizerState.ScriptDataEscaped;
            // Emit a U+FFFD REPLACEMENT CHARACTER character token.
            EmitReplacementCharacterToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-script-html-comment-like-text parse error.
            AddParseError(ParseError.EofInScriptHtmlCommentLikeText);
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Switch to the script data escaped state.
            _state = TokenizerState.ScriptDataEscaped;
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
    }

    private void ScriptDataEscapedDashDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-dash-dash-state>

        // Consume the next input character:
        if (c == '-')
        {
            // Emit a U+002D HYPHEN-MINUS character token.
            EmitCharacterToken('-');
        }
        else if (c == '<')
        {
            // Switch to the script data escaped less-than sign state.
            _state = TokenizerState.ScriptDataEscapedLessThanSign;
        }
        else if (c == '>')
        {
            // Switch to the script data state.
            _state = TokenizerState.ScriptData;
            // Emit a U+003E GREATER-THAN SIGN character token.
            EmitCharacterToken('>');
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Switch to the script data escaped state.
            _state = TokenizerState.ScriptDataEscaped;
            // Emit a U+FFFD REPLACEMENT CHARACTER character token.
            EmitReplacementCharacterToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-script-html-comment-like-text parse error.
            AddParseError(ParseError.EofInScriptHtmlCommentLikeText);
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Switch to the script data escaped state.
            _state = TokenizerState.ScriptDataEscaped;
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
    }

    private void ScriptDataEscapedLessThanSign(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-less-than-sign-state>

        // Consume the next input character:
        if (c == '/')
        {
            // Set the temporary buffer to the empty string.
            _tempBuffer.Clear();
            // Switch to the script data escaped end tag open state.
            _state = TokenizerState.ScriptDataEscapedEndTagOpen;
        }
        else if (IsAsciiAlpha(c))
        {
            // Set the temporary buffer to the empty string.
            _tempBuffer.Clear();
            // Emit a U+003C LESS-THAN SIGN character token.
            EmitCharacterToken('<');
            // Reconsume in the script data double escape start state.
            Reconsume(TokenizerState.ScriptDataDoubleEscapeStart, c);
        }
        else
        {
            // Emit a U+003C LESS-THAN SIGN character token.
            EmitCharacterToken('<');
            // Reconsume in the script data escaped state.
            Reconsume(TokenizerState.ScriptDataEscaped, c);
        }
    }

    private void ScriptDataEscapedEndTagOpen(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-end-tag-open-state>

        // Consume the next input character:
        if (IsAsciiAlpha(c))
        {
            // Create a new end tag token, set its tag name to the empty string.
            _currentTag = Tag.NewEndTag();
            // Reconsume in the script data escaped end tag name state.
            Reconsume(TokenizerState.ScriptDataEscapedEndTagName, c);
        }
        else
        {
            // Emit a U+003C LESS-THAN SIGN character token and a U+002F SOLIDUS
            //   character token.
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            // Reconsume in the script data escaped state.
            Reconsume(TokenizerState.ScriptDataEscaped, c);
        }
    }

    private void ScriptDataEscapedEndTagName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-end-tag-name-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c) && IsCurrentEndTagAnAppropriateOne())
        {
            // If the current end tag token is an appropriate end tag token,
            //   then switch to the before attribute name state.
            // Otherwise, treat it as per the "anything else" entry below.
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (c == '/' && IsCurrentEndTagAnAppropriateOne())
        {
            // If the current end tag token is an appropriate end tag token,
            //   then switch to the self-closing start tag state.
            // Otherwise, treat it as per the "anything else" entry below.
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (c == '>' && IsCurrentEndTagAnAppropriateOne())
        {
            // If the current end tag token is an appropriate end tag token,
            //   then switch to the data state and emit the current tag token.
            // Otherwise, treat it as per the "anything else" entry below.
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(c))
        {
            // Append the lowercase version of the current input character (add
            //   0x0020 to the character's code point) to the current tag
            //   token's tag name.
            _currentTag!.AppendName(ToAsciiLowercase(c));
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(new Rune(c));
        }
        else if (IsAsciiLowerAlpha(c))
        {
            // Append the current input character to the current tag token's tag
            //   name.
            _currentTag!.AppendName(c);
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(new Rune(c));
        }
        else
        {
            // Emit a U+003C LESS-THAN SIGN character token, a U+002F SOLIDUS
            //   character token, and a character token for each of the
            //   characters in the temporary buffer (in the order they were
            //   added to the buffer).
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitCharacterTokensFromTemporaryBuffer();
            // Reconsume in the script data escaped state.
            Reconsume(TokenizerState.ScriptDataEscaped, c);
        }
    }

    private void ScriptDataDoubleEscapeStart(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escape-start-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c) || c == '/' || c == '>')
        {
            if (CompareTemporaryBuffer("script"))
            {
                // If the temporary buffer is the string "script", then switch
                //   to the script data double escaped state.
                _state = TokenizerState.ScriptDataDoubleEscaped;
            }
            else
            {
                // Otherwise, switch to the script data escaped state.
                _state = TokenizerState.ScriptDataEscaped;
                // Emit the current input character as a character token.
                EmitCharacterToken(c);
            }
        }
        else if (IsAsciiUpperAlpha(c))
        {
            // Append the lowercase version of the current input character (add
            //   0x0020 to the character's code point) to the temporary buffer.
            _tempBuffer.Add(ToAsciiLowercase(c));
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
        else if (IsAsciiLowerAlpha(c))
        {
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(new Rune(c));
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
        else
        {
            // Reconsume in the script data escaped state.
            Reconsume(TokenizerState.ScriptDataEscaped, c);
        }
    }

    private void ScriptDataDoubleEscaped(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escaped-state>

        // Consume the next input character:
        if (c == '-')
        {
            // Switch to the script data double escaped dash state.
            _state = TokenizerState.ScriptDataDoubleEscapedDash;
            // Emit a U+002D HYPHEN-MINUS character token.
            EmitCharacterToken('-');
        }
        else if (c == '<')
        {
            // Switch to the script data double escaped less-than sign state.
            _state = TokenizerState.ScriptDataDoubleEscapedLessThanSign;
            // Emit a U+003C LESS-THAN SIGN character token.
            EmitCharacterToken('<');
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Emit a U+FFFD REPLACEMENT CHARACTER character token.
            EmitReplacementCharacterToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-script-html-comment-like-text parse error.
            AddParseError(ParseError.EofInScriptHtmlCommentLikeText);
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
    }

    private void ScriptDataDoubleEscapedDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escaped-dash-state>

        // Consume the next input character:
        if (c == '-')
        {
            // Switch to the script data double escaped dash dash state.
            _state = TokenizerState.ScriptDataDoubleEscapedDashDash;
            // Emit a U+002D HYPHEN-MINUS character token.
            EmitCharacterToken('-');
        }
        else if (c == '<')
        {
            // Switch to the script data double escaped less-than sign state.
            _state = TokenizerState.ScriptDataDoubleEscapedLessThanSign;
            // Emit a U+003C LESS-THAN SIGN character token.
            EmitCharacterToken('<');
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Switch to the script data double escaped state.
            _state = TokenizerState.ScriptDataDoubleEscaped;
            // Emit a U+FFFD REPLACEMENT CHARACTER character token.
            EmitReplacementCharacterToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-script-html-comment-like-text parse error.
            AddParseError(ParseError.EofInScriptHtmlCommentLikeText);
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Switch to the script data double escaped state.
            _state = TokenizerState.ScriptDataDoubleEscaped;
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
    }

    private void ScriptDataDoubleEscapedDashDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escaped-dash-dash-state>

        // Consume the next input character:
        if (c == '-')
        {
            // Emit a U+002D HYPHEN-MINUS character token.
            EmitCharacterToken('-');
        }
        else if (c == '<')
        {
            // Switch to the script data double escaped less-than sign state.
            _state = TokenizerState.ScriptDataDoubleEscapedLessThanSign;
            // Emit a U+003C LESS-THAN SIGN character token.
            EmitCharacterToken('<');
        }
        else if (c == '>')
        {
            // Switch to the script data state.
            _state = TokenizerState.ScriptData;
            // Emit a U+003E GREATER-THAN SIGN character token.
            EmitCharacterToken('>');
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Switch to the script data double escaped state.
            _state = TokenizerState.ScriptDataDoubleEscaped;
            // Emit a U+FFFD REPLACEMENT CHARACTER character token.
            EmitReplacementCharacterToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-script-html-comment-like-text parse error.
            AddParseError(ParseError.EofInScriptHtmlCommentLikeText);
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Switch to the script data double escaped state.
            _state = TokenizerState.ScriptDataDoubleEscaped;
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
    }

    private void ScriptDataDoubleEscapedLessThanSign(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escaped-less-than-sign-state>

        // Consume the next input character:
        if (c == '/')
        {
            // Set the temporary buffer to the empty string.
            _tempBuffer.Clear();
            // Switch to the script data double escape end state.
            _state = TokenizerState.ScriptDataDoubleEscapeEnd;
            // Emit a U+002F SOLIDUS character token.
            EmitCharacterToken('/');
        }
        else
        {
            // Reconsume in the script data double escaped state.
            Reconsume(TokenizerState.ScriptDataDoubleEscaped, c);
        }
    }

    private void ScriptDataDoubleEscapeEnd(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escape-end-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c) || c == '/' || c == '>')
        {
            if (CompareTemporaryBuffer("script"))
            {
                // If the temporary buffer is the string "script", then switch
                //   to the script data escaped state.
                _state = TokenizerState.ScriptDataEscaped;
            }
            else
            {
                // Otherwise, switch to the script data double escaped state.
                _state = TokenizerState.ScriptDataDoubleEscaped;
                // Emit the current input character as a character token.
                EmitCharacterToken(c);
            }
        }
        else if (IsAsciiUpperAlpha(c))
        {
            // Append the lowercase version of the current input character (add
            //   0x0020 to the character's code point) to the temporary buffer.
            _tempBuffer.Add(ToAsciiLowercase(c));
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
        else if (IsAsciiLowerAlpha(c))
        {
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(new Rune(c));
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }
        else
        {
            // Reconsume in the script data double escaped state.
            Reconsume(TokenizerState.ScriptDataDoubleEscaped, c);
        }
    }

    private void BeforeAttributeName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-attribute-name-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c == '/' || c == '>' || c == EOF)
        {
            // Reconsume in the after attribute name state.
            Reconsume(TokenizerState.AfterAttributeName, c);
        }
        else if (c == '=')
        {
            // This is an unexpected-equals-sign-before-attribute-name parse
            //   error.
            AddParseError(ParseError.UnexpectedEqualsSignBeforeAttributeName);
            // Start a new attribute in the current tag token.
            _currentAttribute = _currentTag!.NewAttribute();
            // Set that attribute's name to the current input character, and its
            //   value to the empty string.
            _currentAttribute.AppendName('=');
            // Switch to the attribute name state.
            _state = TokenizerState.AttributeName;
        }
        else
        {
            // Start a new attribute in the current tag token.
            // Set that attribute name and value to the empty string.
            _currentAttribute = _currentTag!.NewAttribute();
            // Reconsume in the attribute name state.
            Reconsume(TokenizerState.AttributeName, c);
        }
    }

    private void AttributeName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#attribute-name-state>

        /* When the user agent leaves the attribute name state (and before
         *   emitting the tag token, if appropriate), the complete attribute's
         *   name must be compared to the other attributes on the same token;
         * If there is already an attribute on the token with the exact same
         *   name, then this is a duplicate-attribute parse error and the new
         *   attribute must be removed from the token.
         *
         * If an attribute is so removed from a token, it, and the value that
         *   gets associated with it, if any, are never subsequently used by the
         *   parser, and are therefore effectively discarded.
         * Removing the attribute in this way does not change its status as the
         *   "current attribute" for the purposes of the tokenizer, however.
         */

        // Consume the next input character:
        if (IsSpecialWhitespace(c) || c == '/' || c == '>' || c == EOF)
        {
            if (_currentTag!.CheckAndCorrectDuplicateAttributes())
                AddParseError(ParseError.DuplicateAttribute); // see block comment above
            // Reconsume in the after attribute name state.
            Reconsume(TokenizerState.AfterAttributeName, c);
        }
        else if (c == '=')
        {
            if (_currentTag!.CheckAndCorrectDuplicateAttributes())
                AddParseError(ParseError.DuplicateAttribute); // see block comment above
            // Switch to the before attribute value state.
            _state = TokenizerState.BeforeAttributeValue;
        }
        else if (IsAsciiUpperAlpha(c))
        {
            // Append the lowercase version of the current input character (add
            //   0x0020 to the character's code point) to the current
            //   attribute's name.
            _currentAttribute!.AppendName(ToAsciiLowercase(c));
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Append a U+FFFD REPLACEMENT CHARACTER character to the current
            //   attribute's name.
            _currentAttribute!.AppendName(REPLACEMENT_CHARACTER);
        }
        else
        {
            if (c == '"' || c == '\'' || c == '<')
            {
                // This is an unexpected-character-in-attribute-name parse
                //   error.
                AddParseError(ParseError.UnexpectedCharacterInAttributeName);
                // Treat it as per the "anything else" entry below.
            }
            // Append the current input character to the current attribute's
            //   name.
            _currentAttribute!.AppendName(c);
        }
    }

    private void AfterAttributeName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-attribute-name-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c == '/')
        {
            // Switch to the self-closing start tag state.
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (c == '=')
        {
            // Switch to the before attribute value state.
            _state = TokenizerState.BeforeAttributeValue;
        }
        else if (c == '>')
        {
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current tag token.
            EmitTagToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-tag parse error.
            AddParseError(ParseError.EofInTag);
            // Emit an end-of-file token
            EmitEndOfFileToken();
        }
        else
        {
            // Start a new attribute in the current tag token.
            // Set that attribute name and value to the empty string.
            _currentAttribute = _currentTag!.NewAttribute();
            // Reconsume in the attribute name state.
            Reconsume(TokenizerState.AttributeName, c);
        }
    }

    private void BeforeAttributeValue(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-attribute-value-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c == '"')
        {
            // Switch to the attribute value (double-quoted) state.
            _state = TokenizerState.AttributeValueDoubleQuoted;
        }
        else if (c == '\'')
        {
            // Switch to the attribute value (single-quoted) state.
            _state = TokenizerState.AttributeValueSingleQuoted;
        }
        else if (c == '>')
        {
            // This is a missing-attribute-value parse error.
            AddParseError(ParseError.MissingAttributeValue);
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current tag token.
            EmitTagToken();
        }
    }

    private void AttributeValueDoubleQuoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#attribute-value-(double-quoted)-state>

        // Consume the next input character:
        if (c == '"')
        {
            // Switch to the after attribute value (quoted) state.
            _state = TokenizerState.AfterAttributeValueQuoted;
        }
        else if (c == '&')
        {
            // Set the return state to the attribute value (double-quoted)
            //   state.
            _returnState = TokenizerState.AttributeValueDoubleQuoted;
            // Switch to the character reference state.
            _state = TokenizerState.CharacterReference;
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Append a U+FFFD REPLACEMENT CHARACTER character to the current
            //   attribute's value.
            _currentAttribute!.AppendValue(REPLACEMENT_CHARACTER);
        }
        else if (c == EOF)
        {
            // This is an eof-in-tag parse error.
            AddParseError(ParseError.EofInTag);
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Append the current input character to the current attribute's
            //   value.
            _currentAttribute!.AppendValue(c);
        }
    }

    private void AttributeValueSingleQuoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#attribute-value-(single-quoted)-state>

        // Consume the next input character:
        if (c == '\'')
        {
            // Switch to the after attribute value (quoted) state.
            _state = TokenizerState.AfterAttributeValueQuoted;
        }
        else if (c == '&')
        {
            // Set the return state to the attribute value (single-quoted)
            //   state.
            _returnState = TokenizerState.AttributeValueSingleQuoted;
            // Switch to the character reference state.
            _state = TokenizerState.CharacterReference;
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Append a U+FFFD REPLACEMENT CHARACTER character to the current
            //   attribute's value.
            _currentAttribute!.AppendValue(REPLACEMENT_CHARACTER);
        }
        else if (c == EOF)
        {
            // This is an eof-in-tag parse error.
            AddParseError(ParseError.EofInTag);
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Append the current input character to the current attribute's
            //   value.
            _currentAttribute!.AppendValue(c);
        }
    }

    private void AttributeValueUnquoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#attribute-value-(unquoted)-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Switch to the before attribute name state.
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (c == '&')
        {
            // Set the return state to the attribute value (unquoted) state.
            _returnState = TokenizerState.AttributeValueUnquoted;
            // Switch to the character reference state.
            _state = TokenizerState.CharacterReference;
        }
        else if (c == '>')
        {
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current tag token.
            EmitTagToken();
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Append a U+FFFD REPLACEMENT CHARACTER character to the current
            //   attribute's value.
            _currentAttribute!.AppendValue(REPLACEMENT_CHARACTER);
        }
        else if (c == EOF)
        {
            // This is an eof-in-tag parse error.
            AddParseError(ParseError.EofInTag);
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            if (c == '"' || c == '\'' || c == '<' || c == '=' || c == '`')
            {
                // This is an unexpected-character-in-unquoted-attribute-value
                //   parse error.
                AddParseError(ParseError.UnexpectedCharacterInUnquotedAttributeValue);
                // Treat it as per the "anything else" entry below.
            }
            // Append the current input character to the current attribute's
            //   value.
            _currentAttribute!.AppendValue(c);
        }
    }

    private void AfterAttributeValueQuoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-attribute-value-(quoted)-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Switch to the before attribute name state.
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (c == '/')
        {
            // Switch to the self-closing start tag state.
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (c == '>')
        {
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current tag token.
            EmitTagToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-tag parse error.
            AddParseError(ParseError.EofInTag);
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // This is a missing-whitespace-between-attributes parse error.
            AddParseError(ParseError.MissingWhitespaceBetweenAttributes);
            // Reconsume in the before attribute name state.
            Reconsume(TokenizerState.BeforeAttributeName, c);
        }
    }

    private void SelfClosingStartTag(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#self-closing-start-tag-state>

        // Consume the next input character:
        if (c == '>')
        {
            // Set the self-closing flag of the current tag token.
            _currentTag!.SetSelfClosingFlag();
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current tag token.
            EmitTagToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-tag parse error.
            AddParseError(ParseError.EofInTag);
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // This is an unexpected-solidus-in-tag parse error.
            AddParseError(ParseError.UnexpectedSolidusInTag);
            // Reconsume in the before attribute name state.
            Reconsume(TokenizerState.BeforeAttributeName, c);
        }
    }

    private void BogusComment(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#bogus-comment-state>

        // Consume the next input character:
        if (c == '>')
        {
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current comment token.
            EmitCommentToken();
        }
        else if (c == EOF)
        {
            // Emit the comment.
            EmitCommentToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Append a U+FFFD REPLACEMENT CHARACTER character to the comment
            //   token's data.
            _currentComment!.Append(REPLACEMENT_CHARACTER);
        }
        else
        {
            // Append the current input character to the comment token's data.
            _currentComment!.Append(c);
        }
    }

    private void MarkupDeclarationOpen(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#markup-declaration-open-state>

        // If the next few characters are:
        throw new NotImplementedException();
        // Two U+002D HYPHEN-MINUS characters (-)
        //     Consume those two characters, create a comment token whose data is the empty string, and switch to the comment start state.
        // ASCII case-insensitive match for the word "DOCTYPE"
        //     Consume those characters and switch to the DOCTYPE state.
        // The string "[CDATA[" (the five uppercase letters "CDATA" with a U+005B LEFT SQUARE BRACKET character before and after)
        //     Consume those characters. If there is an adjusted current node and it is not an element in the HTML namespace, then switch to the CDATA section state. Otherwise, this is a cdata-in-html-content parse error. Create a comment token whose data is the "[CDATA[" string. Switch to the bogus comment state.
        // Anything else
        //     This is an incorrectly-opened-comment parse error. Create a comment token whose data is the empty string. Switch to the bogus comment state (don't consume anything in the current state).
    }

    private void CommentStart(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-start-state>

        // Consume the next input character:
        if (c == '-')
        {
            // Switch to the comment start dash state.
            _state = TokenizerState.CommentStartDash;
        }
        else if (c == '>')
        {
            // This is an abrupt-closing-of-empty-comment parse error.
            AddParseError(ParseError.AbruptClosingOfEmptyComment);
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current comment token.
            EmitCommentToken();
        }
        else
        {
            // Reconsume in the comment state.
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void CommentStartDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-start-dash-state>

        // Consume the next input character:
        if (c == '-')
        {
            // Switch to the comment end state.
            _state = TokenizerState.CommentEnd;
        }
        else if (c == '>')
        {
            // This is an abrupt-closing-of-empty-comment parse error.
            AddParseError(ParseError.AbruptClosingOfEmptyComment);
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current comment token.
            EmitCommentToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-comment parse error.
            AddParseError(ParseError.EofInComment);
            // Emit the current comment token.
            EmitCommentToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Append a U+002D HYPHEN-MINUS character (-) to the comment token's
            //   data.
            _currentComment!.Append('-');
            _currentComment.Append('-');
            // Reconsume in the comment state.
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void Comment(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-state>

        // Consume the next input character:
        if (c == '<')
        {
            // Append the current input character to the comment token's data.
            _currentComment!.Append('<');
            // Switch to the comment less-than sign state.
            _state = TokenizerState.CommentLessThanSign;
        }
        else if (c == '-')
        {
            // Switch to the comment end dash state.
            _state = TokenizerState.CommentEndDash;
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Append a U+FFFD REPLACEMENT CHARACTER character to the comment
            //   token's data.
            _currentComment!.Append(REPLACEMENT_CHARACTER);
        }
        else if (c == EOF)
        {
            // This is an eof-in-comment parse error.
            AddParseError(ParseError.EofInComment);
            // Emit the current comment token.
            EmitCommentToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Append the current input character to the comment token's data.
            _currentComment!.Append(c);
        }
    }

    private void CommentLessThanSign(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-less-than-sign-state>

        // Consume the next input character:
        if (c == '!')
        {
            // Append the current input character to the comment token's data.
            _currentComment!.Append('!');
            // Switch to the comment less-than sign bang state.
            _state = TokenizerState.CommentLessThanSignBang;
        }
        else if (c == '<')
        {
            // Append the current input character to the comment token's data.
            _currentComment!.Append('<');
        }
        else
        {
            // Reconsume in the comment state.
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void CommentLessThanSignBang(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-less-than-sign-bang-state>

        // Consume the next input character:
        if (c == '-')
        {
            // Switch to the comment less-than sign bang dash state.
            _state = TokenizerState.CommentLessThanSignBangDash;
        }
        else
        {
            // Reconsume in the comment state.
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void CommentLessThanSignBangDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-less-than-sign-bang-dash-state>

        // Consume the next input character:
        if (c == '-')
        {
            // Switch to the comment less-than sign bang dash dash state.
            _state = TokenizerState.CommentLessThanSignBangDashDash;
        }
        else
        {
            // Reconsume in the comment state.
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void CommentLessThanSignBangDashDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-less-than-sign-bang-dash-dash-state>

        // Consume the next input character:
        if (c == '>' || c == EOF)
        {
            // Reconsume in the comment end state.
            Reconsume(TokenizerState.CommentEnd, c);
        }
        else
        {
            // This is a nested-comment parse error.
            AddParseError(ParseError.NestedComment);
            // Reconsume in the comment end state.
            Reconsume(TokenizerState.CommentEnd, c);
        }
    }

    private void CommentEndDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-end-dash-state>

        // Consume the next input character:
        if (c == '-')
        {
            // Switch to the comment end state.
            _state = TokenizerState.CommentEnd;
        }
        else if (c == EOF)
        {
            // This is an eof-in-comment parse error.
            AddParseError(ParseError.EofInComment);
            // Emit the current comment token.
            EmitCommentToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Append a U+002D HYPHEN-MINUS character (-) to the comment token's
            //   data.
            _currentComment!.Append('-');
            // Reconsume in the comment state.
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void CommentEnd(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-end-state>

        // Consume the next input character:
        if (c == '>')
        {
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current comment token.
            EmitCommentToken();
        }
        else if (c == '!')
        {
            // Switch to the comment end bang state.
            _state = TokenizerState.CommentEndBang;
        }
        else if (c == '-')
        {
            // Append a U+002D HYPHEN-MINUS character (-) to the comment token's
            //   data.
            _currentComment!.Append('-');
        }
        else if (c == EOF)
        {
            // This is an eof-in-comment parse error.
            AddParseError(ParseError.EofInComment);
            // Emit the current comment token.
            EmitCommentToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Append two U+002D HYPHEN-MINUS characters (-) to the comment
            //   token's data.
            _currentComment!.Append('-');
            _currentComment.Append('-');
            // Reconsume in the comment state.
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void CommentEndBang(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-end-bang-state>

        // Consume the next input character:
        if (c == '-')
        {
            // Append two U+002D HYPHEN-MINUS characters (-) and a U+0021
            //   EXCLAMATION MARK character (!) to the comment token's data.
            _currentComment!.Append('-');
            _currentComment.Append('-');
            _currentComment.Append('!');
            // Switch to the comment end dash state.
            _state = TokenizerState.CommentEndDash;
        }
        else if (c == '>')
        {
            // This is an incorrectly-closed-comment parse error.
            AddParseError(ParseError.IncorrectlyClosedComment);
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current comment token.
            EmitCommentToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-comment parse error.
            AddParseError(ParseError.EofInComment);
            // Emit the current comment token.
            EmitCommentToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Append two U+002D HYPHEN-MINUS characters (-) and a U+0021
            //   EXCLAMATION MARK character (!) to the comment token's data.
            _currentComment!.Append('-');
            _currentComment.Append('-');
            _currentComment.Append('!');
            // Reconsume in the comment state.
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void Doctype(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Switch to the before DOCTYPE name state.
            _state = TokenizerState.BeforeDoctypeName;
        }
        else if (c == '>')
        {
            // Reconsume in the before DOCTYPE name state.
            Reconsume(TokenizerState.BeforeDoctypeName, c);
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Create a new DOCTYPE token.
            _currentDoctype = new();
            // Set its force-quirks flag to on.
            _currentDoctype.SetQuirksFlag();
            // Emit the current token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // This is a missing-whitespace-before-doctype-name parse error.
            AddParseError(ParseError.MissingWhitespaceBeforeDoctypeName);
            // Reconsume in the before DOCTYPE name state.
            Reconsume(TokenizerState.BeforeDoctypeName, c);
        }
    }

    private void BeforeDoctypeName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-doctype-name-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (IsAsciiUpperAlpha(c))
        {
            // Create a new DOCTYPE token.
            _currentDoctype = new();
            // Set the token's name to the lowercase version of the current
            //   input character (add 0x0020 to the character's code point).
            _currentDoctype.AppendName(ToAsciiLowercase(c));
            // Switch to the DOCTYPE name state.
            _state = TokenizerState.DoctypeName;
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Create a new DOCTYPE token.
            _currentDoctype = new();
            // Set the token's name to a U+FFFD REPLACEMENT CHARACTER character.
            _currentDoctype.AppendName(REPLACEMENT_CHARACTER);
            // Switch to the DOCTYPE name state.
            _state = TokenizerState.DoctypeName;
        }
        else if (c == '>')
        {
            // This is a missing-doctype-name parse error.
            AddParseError(ParseError.MissingDoctypeName);
            // Create a new DOCTYPE token.
            _currentDoctype = new();
            // Set its force-quirks flag to on.
            _currentDoctype.SetQuirksFlag();
            // Emit the current token.
            EmitDoctypeToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Create a new DOCTYPE token.
            _currentDoctype = new();
            // Set its force-quirks flag to on.
            _currentDoctype.SetQuirksFlag();
            // Emit the current token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Create a new DOCTYPE token.
            _currentDoctype = new();
            // Set the token's name to the current input character.
            _currentDoctype.AppendName(c);
            // Switch to the DOCTYPE name state.
            _state = TokenizerState.DoctypeName;
        }
    }

    private void DoctypeName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-name-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Switch to the after DOCTYPE name state.
            _state = TokenizerState.AfterDoctypeName;
        }
        else if (c == '>')
        {
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
        }
        else if (IsAsciiUpperAlpha(c))
        {
            // Append the lowercase version of the current input character (add
            //   0x0020 to the character's code point) to the current DOCTYPE
            //   token's name.
            _currentDoctype!.AppendName(ToAsciiLowercase(c));
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Append a U+FFFD REPLACEMENT CHARACTER character to the current
            //   DOCTYPE token's name.
            _currentDoctype!.AppendName(REPLACEMENT_CHARACTER);
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Create a new DOCTYPE token.
            _currentDoctype = new();
            // Set its force-quirks flag to on.
            _currentDoctype.SetQuirksFlag();
            // Emit the current token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Append the current input character to the current DOCTYPE token's
            //   name.
            _currentDoctype!.AppendName(c);
        }
    }

    private void AfterDoctypeName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-name-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c == '>')
        {
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // If the six characters starting from the current input character are an ASCII case-insensitive match for the word "PUBLIC", then consume those characters and switch to the after DOCTYPE public keyword state.
            // Otherwise, if the six characters starting from the current input character are an ASCII case-insensitive match for the word "SYSTEM", then consume those characters and switch to the after DOCTYPE system keyword state.
            // Otherwise, this is an invalid-character-sequence-after-doctype-name parse error. Set the current DOCTYPE token's force-quirks flag to on. Reconsume in the bogus DOCTYPE state.
            throw new NotImplementedException();
        }
    }

    private void AfterDoctypePublicKeyword(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-public-keyword-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Switch to the before DOCTYPE public identifier state.
            _state = TokenizerState.BeforeDoctypePublicIdentifier;
        }
        else if (c == '"')
        {
            // This is a missing-whitespace-after-doctype-public-keyword parse
            //   error.
            AddParseError(ParseError.MissingWhitespaceAfterDoctypePublicKeyword);
            // Set the current DOCTYPE token's public identifier to the empty
            //   string (not missing), then switch to the DOCTYPE public
            //   identifier (double-quoted) state.
            _currentDoctype!.SetPublicIdentifierToEmptyString();
            _state = TokenizerState.DoctypePublicIdentifierDoubleQuoted;
        }
        else if (c == '\'')
        {
            // This is a missing-whitespace-after-doctype-public-keyword parse
            //   error.
            AddParseError(ParseError.MissingWhitespaceAfterDoctypePublicKeyword);
            // Set the current DOCTYPE token's public identifier to the empty
            //   string (not missing), then switch to the DOCTYPE public
            //   identifier (single-quoted) state.
            _currentDoctype!.SetPublicIdentifierToEmptyString();
            _state = TokenizerState.DoctypePublicIdentifierSingleQuoted;
        }
        else if (c == '>')
        {
            // This is a missing-doctype-public-identifier parse error.
            AddParseError(ParseError.MissingDoctypePublicIdentifier);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // This is a missing-quote-before-doctype-public-identifier parse
            //   error.
            AddParseError(ParseError.MissingQuoteBeforeDoctypePublicIdentifier);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Reconsume in the bogus DOCTYPE state.
            Reconsume(TokenizerState.BogusDoctype, c);
        }
    }

    private void BeforeDoctypePublicIdentifier(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-doctype-public-identifier-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c == '"')
        {
            // Set the current DOCTYPE token's public identifier to the empty
            // string (not missing), then switch to the DOCTYPE public
            // identifier (double-quoted) state.
            _currentDoctype!.SetPublicIdentifierToEmptyString();
            _state = TokenizerState.DoctypePublicIdentifierDoubleQuoted;
        }
        else if (c == '\'')
        {
            // Set the current DOCTYPE token's public identifier to the empty
            // string (not missing), then switch to the DOCTYPE public
            // identifier (single-quoted) state.
            _currentDoctype!.SetPublicIdentifierToEmptyString();
            _state = TokenizerState.DoctypePublicIdentifierSingleQuoted;
        }
        else if (c == '>')
        {
            // This is a missing-doctype-public-identifier parse error.
            AddParseError(ParseError.MissingDoctypePublicIdentifier);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // This is a missing-quote-before-doctype-public-identifier parse
            //   error.
            AddParseError(ParseError.MissingQuoteBeforeDoctypePublicIdentifier);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Reconsume in the bogus DOCTYPE state.
            Reconsume(TokenizerState.BogusDoctype, c);
        }
    }

    private void DoctypePublicIdentifierDoubleQuoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-public-identifier-(double-quoted)-state>

        // Consume the next input character:
        if (c == '"')
        {
            // Switch to the after DOCTYPE public identifier state.
            _state = TokenizerState.AfterDoctypePublicIdentifier;
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Append a U+FFFD REPLACEMENT CHARACTER character to the current
            //   DOCTYPE token's public identifier.
            _currentDoctype!.AppendPublicIdentifier(REPLACEMENT_CHARACTER);
        }
        else if (c == '>')
        {
            // This is an abrupt-doctype-public-identifier parse error.
            AddParseError(ParseError.AbruptDoctypePublicIdentifier);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Append the current input character to the current DOCTYPE token's
            //  public identifier.
            _currentDoctype!.AppendPublicIdentifier(c);
        }
    }

    private void DoctypePublicIdentifierSingleQuoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-public-identifier-(single-quoted)-state>

        // Consume the next input character:
        if (c == '\'')
        {
            // Switch to the after DOCTYPE public identifier state.
            _state = TokenizerState.AfterDoctypePublicIdentifier;
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Append a U+FFFD REPLACEMENT CHARACTER character to the current
            //   DOCTYPE token's public identifier.
            _currentDoctype!.AppendPublicIdentifier(REPLACEMENT_CHARACTER);
        }
        else if (c == '>')
        {
            // This is an abrupt-doctype-public-identifier parse error.
            AddParseError(ParseError.AbruptDoctypePublicIdentifier);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Append the current input character to the current DOCTYPE token's
            //  public identifier.
            _currentDoctype!.AppendPublicIdentifier(c);
        }
    }

    private void AfterDoctypePublicIdentifier(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-public-identifier-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Switch to the between DOCTYPE public and system identifiers
            //   state.
            _state = TokenizerState.BetweenDoctypePublicAndSystemIdentifiers;
        }
        else if (c == '>')
        {
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
        }
        else if (c == '"')
        {
            // This is a missing-whitespace-between-doctype-public-and-system-identifiers
            //   parse error.
            AddParseError(ParseError.MissingWhitespaceBetweenDoctypePublicAndSystemIdentifiers);
            // Set the current DOCTYPE token's system identifier to the empty
            //   string (not missing), then switch to the DOCTYPE system
            //   identifier (double-quoted) state.
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
        }
        else if (c == '\'')
        {
            // This is a missing-whitespace-between-doctype-public-and-system-identifiers
            //   parse error.
            AddParseError(ParseError.MissingWhitespaceBetweenDoctypePublicAndSystemIdentifiers);
            // Set the current DOCTYPE token's system identifier to the empty
            //   string (not missing), then switch to the DOCTYPE system
            //   identifier (single-quoted) state.
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // This is a missing-quote-before-doctype-system-identifier parse
            //   error.
            AddParseError(ParseError.MissingQuoteBeforeDoctypeSystemIdentifier);
            // Set the current DOCTYPE token's force-quirks flag to on.
            // Reconsume in the bogus DOCTYPE state.
            _currentDoctype!.SetQuirksFlag();
            Reconsume(TokenizerState.BogusDoctype, c);
        }
    }

    private void BetweenDoctypePublicAndSystemIdentifiers(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#between-doctype-public-and-system-identifiers-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c == '>')
        {
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
        }
        else if (c == '"')
        {
            // Set the current DOCTYPE token's system identifier to the empty
            //   string (not missing), then switch to the DOCTYPE system
            //   identifier (double-quoted) state.
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
        }
        else if (c == '\'')
        {
            // Set the current DOCTYPE token's system identifier to the empty
            //   string (not missing), then switch to the DOCTYPE system
            //   identifier (single-quoted) state.
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // This is a missing-quote-before-doctype-system-identifier parse
            //   error.
            AddParseError(ParseError.MissingQuoteBeforeDoctypeSystemIdentifier);
            // Set the current DOCTYPE token's force-quirks flag to on.
            // Reconsume in the bogus DOCTYPE state.
            _currentDoctype!.SetQuirksFlag();
            Reconsume(TokenizerState.BogusDoctype, c);
        }
    }

    private void AfterDoctypeSystemKeyword(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-system-keyword-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Switch to the before DOCTYPE system identifier state.
            _state = TokenizerState.BeforeDoctypeSystemIdentifier;
        }
        else if (c == '"')
        {
            // This is a missing-whitespace-after-doctype-system-keyword parse
            //   error.
            AddParseError(ParseError.MissingWhitespaceAfterDoctypeSystemKeyword);
            // Set the current DOCTYPE token's system identifier to the empty
            //   string (not missing), then switch to the DOCTYPE system
            //   identifier (double-quoted) state.
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
        }
        else if (c == '\'')
        {
            // This is a missing-whitespace-after-doctype-system-keyword parse
            //   error.
            AddParseError(ParseError.MissingWhitespaceAfterDoctypeSystemKeyword);
            // Set the current DOCTYPE token's system identifier to the empty
            //   string (not missing), then switch to the DOCTYPE system
            //   identifier (single-quoted) state.
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
        }
        else if (c == '>')
        {
            // This is a missing-doctype-system-identifier parse error.
            AddParseError(ParseError.MissingDoctypeSystemIdentifier);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // This is a missing-quote-before-doctype-system-identifier parse
            //   error.
            AddParseError(ParseError.MissingQuoteBeforeDoctypeSystemIdentifier);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Reconsume in the bogus DOCTYPE state.
            Reconsume(TokenizerState.BogusDoctype, c);
        }
    }

    private void BeforeDoctypeSystemIdentifier(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-doctype-system-identifier-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c == '"')
        {
            // Set the current DOCTYPE token's system identifier to the empty
            // string (not missing), then switch to the DOCTYPE system
            // identifier (double-quoted) state.
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
        }
        else if (c == '\'')
        {
            // Set the current DOCTYPE token's system identifier to the empty
            // string (not missing), then switch to the DOCTYPE system
            // identifier (single-quoted) state.
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
        }
        else if (c == '>')
        {
            // This is a missing-doctype-system-identifier parse error.
            AddParseError(ParseError.MissingDoctypeSystemIdentifier);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // This is a missing-quote-before-doctype-system-identifier parse
            //   error.
            AddParseError(ParseError.MissingQuoteBeforeDoctypeSystemIdentifier);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Reconsume in the bogus DOCTYPE state.
            Reconsume(TokenizerState.BogusDoctype, c);
        }
    }

    private void DoctypeSystemIdentifierDoubleQuoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-system-identifier-(double-quoted)-state>

        // Consume the next input character:
        if (c == '"')
        {
            // Switch to the after DOCTYPE system identifier state.
            _state = TokenizerState.AfterDoctypeSystemIdentifier;
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Append a U+FFFD REPLACEMENT CHARACTER character to the current
            //   DOCTYPE token's system identifier.
            _currentDoctype!.AppendSystemIdentifier(REPLACEMENT_CHARACTER);
        }
        else if (c == '>')
        {
            // This is an abrupt-doctype-system-identifier parse error.
            AddParseError(ParseError.AbruptDoctypeSystemIdentifier);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Append the current input character to the current DOCTYPE token's
            //  system identifier.
            _currentDoctype!.AppendSystemIdentifier(c);
        }
    }

    private void DoctypeSystemIdentifierSingleQuoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-system-identifier-(single-quoted)-state>

        // Consume the next input character:
        if (c == '\'')
        {
            // Switch to the after DOCTYPE system identifier state.
            _state = TokenizerState.AfterDoctypeSystemIdentifier;
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Append a U+FFFD REPLACEMENT CHARACTER character to the current
            //   DOCTYPE token's system identifier.
            _currentDoctype!.AppendSystemIdentifier(REPLACEMENT_CHARACTER);
        }
        else if (c == '>')
        {
            // This is an abrupt-doctype-system-identifier parse error.
            AddParseError(ParseError.AbruptDoctypeSystemIdentifier);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Append the current input character to the current DOCTYPE token's
            //  system identifier.
            _currentDoctype!.AppendSystemIdentifier(c);
        }
    }

    private void AfterDoctypeSystemIdentifier(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-system-identifier-state>

        // Consume the next input character:
        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c == '>')
        {
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
        }
        else if (c == EOF)
        {
            // This is an eof-in-doctype parse error.
            AddParseError(ParseError.EofInDoctype);
            // Set the current DOCTYPE token's force-quirks flag to on.
            _currentDoctype!.SetQuirksFlag();
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // This is an unexpected-character-after-doctype-system-identifier
            //   parse error.
            AddParseError(ParseError.UnexpectedCharacterAfterDoctypeSystemIdentifier);
            // Reconsume in the bogus DOCTYPE state. (This does not set the
            //   current DOCTYPE token's force-quirks flag to on.)
            Reconsume(TokenizerState.BogusDoctype, c);
        }
    }

    private void BogusDoctype(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#bogus-doctype-state>

        // Consume the next input character:
        if (c == '>')
        {
            // Switch to the data state.
            _state = TokenizerState.Data;
            // Emit the current DOCTYPE token.
            EmitDoctypeToken();
        }
        else if (c == '\0')
        {
            // This is an unexpected-null-character parse error.
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Ignore the character.
        }
        else if (c == EOF)
        {
            // Emit the DOCTYPE token.
            EmitDoctypeToken();
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Ignore the character.
        }
    }

    private void CDataSection(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#cdata-section-state>

        // Consume the next input character:
        if (c == ']')
        {
            // Switch to the CDATA section bracket state.
            _state = TokenizerState.CDataSectionBracket;
        }
        else if (c == EOF)
        {
            // This is an eof-in-cdata parse error.
            AddParseError(ParseError.EofInCData);
            // Emit an end-of-file token.
            EmitEndOfFileToken();
        }
        else
        {
            // Emit the current input character as a character token.
            EmitCharacterToken(c);
        }

        /* U+0000 NULL characters are handled in the tree construction stage,
         *   as part of the in foreign content insertion mode, which is the only
         *   place where CDATA sections can appear.
         */
    }

    private void CDataSectionBracket(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#cdata-section-bracket-state>

        // Consume the next input character:
        if (c == ']')
        {
            // Switch to the CDATA section end state.
            _state = TokenizerState.CDataSectionEnd;
        }
        else
        {
            // Emit a U+005D RIGHT SQUARE BRACKET character token.
            EmitCharacterToken(']');
            // Reconsume in the CDATA section state.
            Reconsume(TokenizerState.CDataSection, c);
        }
    }

    private void CDataSectionEnd(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#cdata-section-end-state>

        // Consume the next input character:
        if (c == ']')
        {
            // Emit a U+005D RIGHT SQUARE BRACKET character token.
            EmitCharacterToken(']');
        }
        else if (c == '>')
        {
            // Switch to the data state.
            _state = TokenizerState.Data;
        }
        else
        {
            // Emit two U+005D RIGHT SQUARE BRACKET character tokens.
            EmitCharacterToken(']');
            EmitCharacterToken(']');
            // Reconsume in the CDATA section state.
            Reconsume(TokenizerState.CDataSection, c);
        }
    }

    private void CharacterReference(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#character-reference-state>

        // Set the temporary buffer to the empty string.
        _tempBuffer.Clear();
        // Append a U+0026 AMPERSAND (&) character to the temporary buffer.
        _tempBuffer.Add(new('&'));
        // Consume the next input character:
        if (IsAsciiAlphanumeric(c))
        {
            // Reconsume in the named character reference state.
            Reconsume(TokenizerState.NamedCharacterReference, c);
        }
        else if (c == '#')
        {
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(new('#'));
            // Switch to the numeric character reference state.
            _state = TokenizerState.NumericCharacterReference;
        }
        else
        {
            // Flush code points consumed as a character reference.
            FlushCodePointsConsumedAsCharacterReference();
            // Reconsume in the return state.
            Reconsume(_returnState!.Value, c);
        }
    }

    private void NamedCharacterReference(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#named-character-reference-state>

        // Consume the maximum number of characters possible, where the consumed
        //   characters are one of the identifiers in the first column of the
        //   named character references table.
        // Append each character to the temporary buffer when it's consumed.
        throw new NotImplementedException();
    }

    private void AmbiguousAmpersand(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#ambiguous-ampersand-state>

        // Consume the next input character:
        if (IsAsciiAlphanumeric(c))
        {
            // If the character reference was consumed as part of an attribute,
            //   then append the current input character to the current
            //   attribute's value. Otherwise, emit the current input character
            //   as a character token.
            if (WasConsumedAsPartOfAnAttribute())
                _currentAttribute!.AppendValue(c);
            else
                EmitCharacterToken(c);
        }
        else if (c == ';')
        {
            // This is an unknown-named-character-reference parse error.
            AddParseError(ParseError.UnknownNamedCharacterReference);
            // Reconsume in the return state.
            Reconsume(_returnState!.Value, c);
        }
        else
        {
            // Reconsume in the return state.
            Reconsume(_returnState!.Value, c);
        }
    }

    private void NumericCharacterReference(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#numeric-character-reference-state>

        // Set the character reference code to zero (0).
        _charRefCode = 0;
        // Consume the next input character:
        if (c == 'x' || c == 'X')
        {
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(new(c));
            // Switch to the hexadecimal character reference start state.
            _state = TokenizerState.HexadecimalCharacterReferenceStart;
        }
        else
        {
            // Reconsume in the decimal character reference start state.
            Reconsume(TokenizerState.DecimalCharacterReferenceStart, c);
        }
    }

    private void HexadecimalCharacterReferenceStart(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#hexadecimal-character-reference-start-state>

        // Consume the next input character:
        if (IsAsciiHexDigit(c))
        {
            // Reconsume in the hexadecimal character reference state.
            Reconsume(TokenizerState.HexadecimalCharacterReference, c);
        }
        else
        {
            // This is an absence-of-digits-in-numeric-character-reference parse
            //   error.
            AddParseError(ParseError.AbsenseOfDigitsInNumericCharacterReference);
            // Flush code points consumed as a character reference.
            FlushCodePointsConsumedAsCharacterReference();
            // Reconsume in the return state.
            Reconsume(_returnState!.Value, c);
        }
    }

    private void DecimalCharacterReferenceStart(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#decimal-character-reference-start-state>

        // Consume the next input character:
        if (IsAsciiDigit(c))
        {
            // Reconsume in the decimal character reference state.
            Reconsume(TokenizerState.DecimalCharacterReference, c);
        }
        else
        {
            // This is an absence-of-digits-in-numeric-character-reference parse
            //   error.
            AddParseError(ParseError.AbsenseOfDigitsInNumericCharacterReference);
            // Flush code points consumed as a character reference.
            FlushCodePointsConsumedAsCharacterReference();
            // Reconsume in the return state.
            Reconsume(_returnState!.Value, c);
        }
    }

    private void HexadecimalCharacterReference(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#hexadecimal-character-reference-state>

        // Consume the next input character:
        if (IsAsciiDigit(c))
        {
            // Multiply the character reference code by 16.
            _charRefCode *= 16;
            // Add a numeric version of the current input character (subtract
            //   0x0030 from the character's code point) to the character
            //   reference code.
            _charRefCode += c - '0';
        }
        else if (IsAsciiUpperHexDigit(c))
        {
            // Multiply the character reference code by 16.
            _charRefCode *= 16;
            // Add a numeric version of the current input character as a
            //   hexadecimal digit (subtract 0x0037 from the character's code
            //   point) to the character reference code.
            _charRefCode += c - 'A' + 10;
        }
        else if (IsAsciiLowerHexDigit(c))
        {
            // Multiply the character reference code by 16.
            _charRefCode *= 16;
            // Add a numeric version of the current input character as a
            //   hexadecimal digit (subtract 0x0057 from the character's code
            //   point) to the character reference code.
            _charRefCode += c - 'a' + 10;
        }
        else if (c == ';')
        {
            // Switch to the numeric character reference end state.
            _state = TokenizerState.NumericCharacterReferenceEnd;
        }
        else
        {
            // This is a missing-semicolon-after-character-reference parse
            //   error.
            AddParseError(ParseError.MissingSemicolonAfterCharacterReference);
            // Reconsume in the numeric character reference end state.
            Reconsume(TokenizerState.NumericCharacterReferenceEnd, c);
        }
    }

    private void DecimalCharacterReference(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#decimal-character-reference-state>

        // Consume the next input character:
        if (IsAsciiDigit(c))
        {
            // Multiply the character reference code by 10.
            _charRefCode *= 10;
            // Add a numeric version of the current input character (subtract
            //   0x0030 from the character's code point) to the character
            //   reference code.
            _charRefCode += c - '0';
        }
        else if (c == ';')
        {
            // Switch to the numeric character reference end state.
            _state = TokenizerState.NumericCharacterReferenceEnd;
        }
        else
        {
            // This is a missing-semicolon-after-character-reference parse
            //   error.
            AddParseError(ParseError.MissingSemicolonAfterCharacterReference);
            // Reconsume in the numeric character reference end state.
            Reconsume(TokenizerState.NumericCharacterReferenceEnd, c);
        }
    }

    private void NumericCharacterReferenceEnd(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#numeric-character-reference-end-state>

        PutBack(c); // we don't need it

        // Check the character reference code:
        if (_charRefCode == 0)
        {
            // If the number is 0x00, then this is a null-character-reference
            //   parse error.
            AddParseError(ParseError.NullCharacterReference);
            // Set the character reference code to 0xFFFD.
            _charRefCode = REPLACEMENT_CHARACTER.Value;
        }
        else if (_charRefCode > 0x10FFFF)
        {
            // If the number is greater than 0x10FFFF, then this is a
            //   character-reference-outside-unicode-range parse error.
            AddParseError(ParseError.CharacterReferenceOutsideUnicodeRange);
            // Set the character reference code to 0xFFFD.
            _charRefCode = REPLACEMENT_CHARACTER.Value;
        }
        else if (IsSurrogate(_charRefCode))
        {
            // If the number is a surrogate, then this is a
            //   surrogate-character-reference parse error.
            AddParseError(ParseError.SurrogateCharacterReference);
            // Set the character reference code to 0xFFFD.
            _charRefCode = REPLACEMENT_CHARACTER.Value;
        }
        else if (IsNoncharacter(_charRefCode))
        {
            // If the number is a noncharacter, then this is a
            //   noncharacter-character-reference parse error.
            AddParseError(ParseError.NoncharacterCharacterReference);
        }
        else if (_charRefCode == 0xD || (IsControl(_charRefCode) && !IsAsciiWhitespace(_charRefCode)))
        {
            // If the number is 0x0D, or a control that's not ASCII whitespace,
            //   then this is a control-character-reference parse error.
            AddParseError(ParseError.ControlCharacterReference);
        }
        else if (NumericCharReference.List.TryGetValue(_charRefCode, out int converted))
        {
            // If the number is one of the numbers in the first column of the
            //   following table, then find the row with that number in the
            //   first column, and set the character reference code to the
            //   number in the second column of that row.
            _charRefCode = converted;
        }

        // Set the temporary buffer to the empty string.
        _tempBuffer.Clear();
        // Append a code point equal to the character reference code to the
        //   temporary buffer.
        _tempBuffer.Add(new Rune(_charRefCode));
        // Flush code points consumed as a character reference.
        FlushCodePointsConsumedAsCharacterReference();
        // Switch to the return state.
        _state = _returnState!.Value;
        _returnState = null;
    }
}
