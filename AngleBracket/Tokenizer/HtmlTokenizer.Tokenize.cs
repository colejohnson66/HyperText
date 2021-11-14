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

namespace AngleBracket.Tokenizer;

public partial class HtmlTokenizer
{
    private const int EOF = -1;
    private const char REPLACEMENT_CHARACTER = '\uFFFD';

    private static List<Action<int>> _stateMap = new();
    private readonly Queue<Token> _tokensToEmit = new();
    private readonly Stack<int> _peekBuffer;

    private TokenizerState _state = TokenizerState.Data;
    private TokenizerState? _returnState = null;
    private readonly List<int> _tempBuffer = new();

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
        _stateMap = new(128); // 80 states ATM; round up
        _stateMap.Insert((int)TokenizerState.Data, Data);
        _stateMap.Insert((int)TokenizerState.RCData, RCData);
        _stateMap.Insert((int)TokenizerState.RawText, RawText);
        _stateMap.Insert((int)TokenizerState.ScriptData, ScriptData);
        _stateMap.Insert((int)TokenizerState.Plaintext, Plaintext);
        _stateMap.Insert((int)TokenizerState.TagOpen, TagOpen);
        _stateMap.Insert((int)TokenizerState.EndTagOpen, EndTagOpen);
        _stateMap.Insert((int)TokenizerState.TagName, TagName);
        _stateMap.Insert((int)TokenizerState.RCDataLessThanSign, RCDataLessThanSign);
        _stateMap.Insert((int)TokenizerState.RCDataEndTagOpen, RCDataEndTagOpen);
        _stateMap.Insert((int)TokenizerState.RCDataEndTagName, RCDataEndTagName);
        _stateMap.Insert((int)TokenizerState.RawTextLessThanSign, RawTextLessThanSign);
        _stateMap.Insert((int)TokenizerState.RawTextEndTagOpen, RawTextEndTagOpen);
        _stateMap.Insert((int)TokenizerState.RawTextEndTagName, RawTextEndTagName);
        _stateMap.Insert((int)TokenizerState.ScriptDataLessThanSign, ScriptDataLessThanSign);
        _stateMap.Insert((int)TokenizerState.ScriptDataEndTagOpen, ScriptDataEndTagOpen);
        _stateMap.Insert((int)TokenizerState.ScriptDataEndTagName, ScriptDataEndTagName);
        _stateMap.Insert((int)TokenizerState.ScriptDataEscapeStart, ScriptDataEscapeStart);
        _stateMap.Insert((int)TokenizerState.ScriptDataEscapeStartDash, ScriptDataEscapeStartDash);
        _stateMap.Insert((int)TokenizerState.ScriptDataEscaped, ScriptDataEscaped);
        _stateMap.Insert((int)TokenizerState.ScriptDataEscapedDash, ScriptDataEscapedDash);
        _stateMap.Insert((int)TokenizerState.ScriptDataEscapedDashDash, ScriptDataEscapedDashDash);
        _stateMap.Insert((int)TokenizerState.ScriptDataEscapedLessThanSign, ScriptDataEscapedLessThanSign);
        _stateMap.Insert((int)TokenizerState.ScriptDataEscapedEndTagOpen, ScriptDataEscapedEndTagOpen);
        _stateMap.Insert((int)TokenizerState.ScriptDataEscapedEndTagName, ScriptDataEscapedEndTagName);
        _stateMap.Insert((int)TokenizerState.ScriptDataDoubleEscapeStart, ScriptDataDoubleEscapeStart);
        _stateMap.Insert((int)TokenizerState.ScriptDataDoubleEscaped, ScriptDataDoubleEscaped);
        _stateMap.Insert((int)TokenizerState.ScriptDataDoubleEscapedDash, ScriptDataDoubleEscapedDash);
        _stateMap.Insert((int)TokenizerState.ScriptDataDoubleEscapedDashDash, ScriptDataDoubleEscapedDashDash);
        _stateMap.Insert((int)TokenizerState.ScriptDataDoubleEscapedLessThanSign, ScriptDataDoubleEscapedLessThanSign);
        _stateMap.Insert((int)TokenizerState.ScriptDataDoubleEscapeEnd, ScriptDataDoubleEscapeEnd);
        _stateMap.Insert((int)TokenizerState.BeforeAttributeName, BeforeAttributeName);
        _stateMap.Insert((int)TokenizerState.AttributeName, AttributeName);
        _stateMap.Insert((int)TokenizerState.AfterAttributeName, AfterAttributeName);
        _stateMap.Insert((int)TokenizerState.BeforeAttributeValue, BeforeAttributeValue);
        _stateMap.Insert((int)TokenizerState.AttributeValueDoubleQuoted, AttributeValueDoubleQuoted);
        _stateMap.Insert((int)TokenizerState.AttributeValueSingleQuoted, AttributeValueSingleQuoted);
        _stateMap.Insert((int)TokenizerState.AttributeValueUnquoted, AttributeValueUnquoted);
        _stateMap.Insert((int)TokenizerState.AfterAttributeValueQuoted, AfterAttributeValueQuoted);
        _stateMap.Insert((int)TokenizerState.SelfClosingStartTag, SelfClosingStartTag);
        _stateMap.Insert((int)TokenizerState.BogusComment, BogusComment);
        _stateMap.Insert((int)TokenizerState.MarkupDeclarationOpen, MarkupDeclarationOpen);
        _stateMap.Insert((int)TokenizerState.CommentStart, CommentStart);
        _stateMap.Insert((int)TokenizerState.CommentStartDash, CommentStartDash);
        _stateMap.Insert((int)TokenizerState.Comment, Comment);
        _stateMap.Insert((int)TokenizerState.CommentLessThanSign, CommentLessThanSign);
        _stateMap.Insert((int)TokenizerState.CommentLessThanSignBang, CommentLessThanSignBang);
        _stateMap.Insert((int)TokenizerState.CommentLessThanSignBangDash, CommentLessThanSignBangDash);
        _stateMap.Insert((int)TokenizerState.CommentLessThanSignBangDashDash, CommentLessThanSignBangDashDash);
        _stateMap.Insert((int)TokenizerState.CommentEndDash, CommentEndDash);
        _stateMap.Insert((int)TokenizerState.CommentEnd, CommentEnd);
        _stateMap.Insert((int)TokenizerState.CommentEndBang, CommentEndBang);
        _stateMap.Insert((int)TokenizerState.Doctype, Doctype);
        _stateMap.Insert((int)TokenizerState.BeforeDoctypeName, BeforeDoctypeName);
        _stateMap.Insert((int)TokenizerState.DoctypeName, DoctypeName);
        _stateMap.Insert((int)TokenizerState.AfterDoctypeName, AfterDoctypeName);
        _stateMap.Insert((int)TokenizerState.AfterDoctypePublicKeyword, AfterDoctypePublicKeyword);
        _stateMap.Insert((int)TokenizerState.BeforeDoctypePublicIdentifier, BeforeDoctypePublicIdentifier);
        _stateMap.Insert((int)TokenizerState.DoctypePublicIdentifierDoubleQuoted, DoctypePublicIdentifierDoubleQuoted);
        _stateMap.Insert((int)TokenizerState.DoctypePublicIdentifierSingleQuoted, DoctypePublicIdentifierSingleQuoted);
        _stateMap.Insert((int)TokenizerState.AfterDoctypePublicIdentifier, AfterDoctypePublicIdentifier);
        _stateMap.Insert((int)TokenizerState.BetweenDoctypePublicAndSystemIdentifiers, BetweenDoctypePublicAndSystemIdentifiers);
        _stateMap.Insert((int)TokenizerState.AfterDoctypeSystemKeyword, AfterDoctypeSystemKeyword);
        _stateMap.Insert((int)TokenizerState.BeforeDoctypeSystemIdentifier, BeforeDoctypeSystemIdentifier);
        _stateMap.Insert((int)TokenizerState.DoctypeSystemIdentifierDoubleQuoted, DoctypeSystemIdentifierDoubleQuoted);
        _stateMap.Insert((int)TokenizerState.DoctypeSystemIdentifierSingleQuoted, DoctypeSystemIdentifierSingleQuoted);
        _stateMap.Insert((int)TokenizerState.AfterDoctypeSystemIdentifier, AfterDoctypeSystemIdentifier);
        _stateMap.Insert((int)TokenizerState.BogusDoctype, BogusDoctype);
        _stateMap.Insert((int)TokenizerState.CDataSection, CDataSection);
        _stateMap.Insert((int)TokenizerState.CDataSectionBracket, CDataSectionBracket);
        _stateMap.Insert((int)TokenizerState.CDataSectionEnd, CDataSectionEnd);
        _stateMap.Insert((int)TokenizerState.CharacterReference, CharacterReference);
        _stateMap.Insert((int)TokenizerState.NamedCharacterReference, NamedCharacterReference);
        _stateMap.Insert((int)TokenizerState.AmbiguousAmpersand, AmbiguousAmpersand);
        _stateMap.Insert((int)TokenizerState.NumericCharacterReference, NumericCharacterReference);
        _stateMap.Insert((int)TokenizerState.HexadecimalCharacterReferenceStart, HexadecimalCharacterReferenceStart);
        _stateMap.Insert((int)TokenizerState.DecimalCharacterReferenceStart, DecimalCharacterReferenceStart);
        _stateMap.Insert((int)TokenizerState.HexadecimalCharacterReference, HexadecimalCharacterReference);
        _stateMap.Insert((int)TokenizerState.DecimalCharacterReference, DecimalCharacterReference);
        _stateMap.Insert((int)TokenizerState.NumericCharacterReferenceEnd, NumericCharacterReferenceEnd);
    }

#pragma warning disable CA1822
    // could be static, but to avoid having to prefix all calls with
    //   `HtmlTokenizer`, disable that warning
    private bool IsSpecialWhitespace(int c) => c == '\t' || c == '\n' || c == '\f' || c == ' ';
    private int ToAsciiLowercase(int c) => c + ('a' - 'A');
#pragma warning restore CA1822

    private bool IsCurrentEndTagAnAppropriateOne()
    {
        throw new NotImplementedException();
    }

    private void EmitCharacterToken(int c)
    {
        Contract.Requires(c >= 0 && c <= 0x10FFFF);
        _tokensToEmit.Enqueue(Token.NewCharacterToken(c));
    }
    private void EmitReplacementCharacterToken() => EmitCharacterToken(REPLACEMENT_CHARACTER);
    private void EmitCharacterTokensFromTemporaryBuffer()
    {
        foreach (int c in _tempBuffer)
            EmitCharacterToken(c);
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

        for (int i = 0; i < compareTo.Length; i++)
        {
            if (_tempBuffer[i] != compareTo[i])
                return false;
        }

        return true;
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

                _stateMap[(int)_state](Read());
            }
        }
    }

    private void Reconsume(TokenizerState newState, int c)
    {
        _state = newState;
        _stateMap[(int)newState](c);
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
            _tempBuffer.Add(c);
        }
        else if (IsAsciiLowerAlpha(c))
        {
            // Append the current input character to the current tag token's tag
            //   name.
            _currentTag!.AppendName(c);
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(c);
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
            _tempBuffer.Add(c);
        }
        else if (IsAsciiLowerAlpha(c))
        {
            // Append the current input character to the current tag token's tag
            //   name.
            _currentTag!.AppendName(c);
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(c);
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
            _tempBuffer.Add(c);
        }
        else if (IsAsciiLowerAlpha(c))
        {
            // Append the current input character to the current tag token's tag
            //   name.
            _currentTag!.AppendName(c);
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(c);
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
            _tempBuffer.Add(c);
        }
        else if (IsAsciiLowerAlpha(c))
        {
            // Append the current input character to the current tag token's tag
            //   name.
            _currentTag!.AppendName(c);
            // Append the current input character to the temporary buffer.
            _tempBuffer.Add(c);
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
            _tempBuffer.Add(c);
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
            _tempBuffer.Add(c);
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
        throw new NotImplementedException();
    }

    private void BeforeDoctypeName(int c)
    {
        throw new NotImplementedException();
    }

    private void DoctypeName(int c)
    {
        throw new NotImplementedException();
    }

    private void AfterDoctypeName(int c)
    {
        throw new NotImplementedException();
    }

    private void AfterDoctypePublicKeyword(int c)
    {
        throw new NotImplementedException();
    }

    private void BeforeDoctypePublicIdentifier(int c)
    {
        throw new NotImplementedException();
    }

    private void DoctypePublicIdentifierDoubleQuoted(int c)
    {
        throw new NotImplementedException();
    }

    private void DoctypePublicIdentifierSingleQuoted(int c)
    {
        throw new NotImplementedException();
    }

    private void AfterDoctypePublicIdentifier(int c)
    {
        throw new NotImplementedException();
    }

    private void BetweenDoctypePublicAndSystemIdentifiers(int c)
    {
        throw new NotImplementedException();
    }

    private void AfterDoctypeSystemKeyword(int c)
    {
        throw new NotImplementedException();
    }

    private void BeforeDoctypeSystemIdentifier(int c)
    {
        throw new NotImplementedException();
    }

    private void DoctypeSystemIdentifierDoubleQuoted(int c)
    {
        throw new NotImplementedException();
    }

    private void DoctypeSystemIdentifierSingleQuoted(int c)
    {
        throw new NotImplementedException();
    }

    private void AfterDoctypeSystemIdentifier(int c)
    {
        throw new NotImplementedException();
    }

    private void BogusDoctype(int c)
    {
        throw new NotImplementedException();
    }

    private void CDataSection(int c)
    {
        throw new NotImplementedException();
    }

    private void CDataSectionBracket(int c)
    {
        throw new NotImplementedException();
    }

    private void CDataSectionEnd(int c)
    {
        throw new NotImplementedException();
    }

    private void CharacterReference(int c)
    {
        throw new NotImplementedException();
    }

    private void NamedCharacterReference(int c)
    {
        throw new NotImplementedException();
    }

    private void AmbiguousAmpersand(int c)
    {
        throw new NotImplementedException();
    }

    private void NumericCharacterReference(int c)
    {
        throw new NotImplementedException();
    }

    private void HexadecimalCharacterReferenceStart(int c)
    {
        throw new NotImplementedException();
    }

    private void DecimalCharacterReferenceStart(int c)
    {
        throw new NotImplementedException();
    }

    private void HexadecimalCharacterReference(int c)
    {
        throw new NotImplementedException();
    }

    private void DecimalCharacterReference(int c)
    {
        throw new NotImplementedException();
    }

    private void NumericCharacterReferenceEnd(int c)
    {
        throw new NotImplementedException();
    }
}
