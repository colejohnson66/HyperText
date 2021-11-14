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
        throw new NotImplementedException();
    }

    private void AttributeName(int c)
    {
        throw new NotImplementedException();
    }

    private void AfterAttributeName(int c)
    {
        throw new NotImplementedException();
    }

    private void BeforeAttributeValue(int c)
    {
        throw new NotImplementedException();
    }

    private void AttributeValueDoubleQuoted(int c)
    {
        throw new NotImplementedException();
    }

    private void AttributeValueSingleQuoted(int c)
    {
        throw new NotImplementedException();
    }

    private void AttributeValueUnquoted(int c)
    {
        throw new NotImplementedException();
    }

    private void AfterAttributeValueQuoted(int c)
    {
        throw new NotImplementedException();
    }

    private void SelfClosingStartTag(int c)
    {
        throw new NotImplementedException();
    }

    private void BogusComment(int c)
    {
        throw new NotImplementedException();
    }

    private void MarkupDeclarationOpen(int c)
    {
        throw new NotImplementedException();
    }

    private void CommentStart(int c)
    {
        throw new NotImplementedException();
    }

    private void CommentStartDash(int c)
    {
        throw new NotImplementedException();
    }

    private void Comment(int c)
    {
        throw new NotImplementedException();
    }

    private void CommentLessThanSign(int c)
    {
        throw new NotImplementedException();
    }

    private void CommentLessThanSignBang(int c)
    {
        throw new NotImplementedException();
    }

    private void CommentLessThanSignBangDash(int c)
    {
        throw new NotImplementedException();
    }

    private void CommentLessThanSignBangDashDash(int c)
    {
        throw new NotImplementedException();
    }

    private void CommentEndDash(int c)
    {
        throw new NotImplementedException();
    }

    private void CommentEnd(int c)
    {
        throw new NotImplementedException();
    }

    private void CommentEndBang(int c)
    {
        throw new NotImplementedException();
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
