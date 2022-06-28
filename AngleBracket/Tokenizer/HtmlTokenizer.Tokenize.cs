/* =============================================================================
 * File:   HtmlTokenizer.Tokenizer.cs
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

using AngleBracket.Parser;
using System.Linq;
using System.Text;
using static AngleBracket.Infra.CodePoint;

// ReSharper disable ConvertIfStatementToSwitchStatement

namespace AngleBracket.Tokenizer;

public partial class HtmlTokenizer
{
    /// <summary>Tokenize the input stream.</summary>
    /// <returns>An enumeration of <see cref="Token" />s from the input stream.</returns>
    public IEnumerable<Token> Tokenize()
    {
        InitStateMap();
        while (true)
        {
            while (_tokensToEmit.Any())
            {
                Token token = _tokensToEmit.Dequeue();
                yield return _tokensToEmit.Dequeue();
                if (token.Type is TokenType.EndOfFile)
                    yield break;
            }

            Rune? r = Read();
            _stateMap[(int)_state](r);
        }
    }

    private void Reconsume(TokenizerState newState, Rune? r)
    {
        _state = newState;
        _stateMap[(int)newState](r);
    }

    private void Data(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#data-state>

        if (r?.Value is '&')
        {
            _returnState = TokenizerState.Data;
            _state = TokenizerState.CharacterReference;
        }
        else if (r?.Value is '<')
        {
            _state = TokenizerState.TagOpen;
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            EmitCharacterToken('\0');
        }
        else if (r is null)
        {
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(r.Value);
        }
    }

    private void RCData(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rcdata-state>

        if (r?.Value is '&')
        {
            _returnState = TokenizerState.RCData;
            _state = TokenizerState.CharacterReference;
        }
        else if (r?.Value is '<')
        {
            _state = TokenizerState.RCDataLessThanSign;
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            EmitReplacementCharacterToken();
        }
        else if (r is null)
        {
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(r.Value);
        }
    }

    private void RawText(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rawtext-state>

        if (r?.Value is '<')
        {
            _state = TokenizerState.RawTextLessThanSign;
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            EmitReplacementCharacterToken();
        }
        else if (r is null)
        {
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(r.Value);
        }
    }

    private void ScriptData(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-state>

        if (r?.Value is '<')
        {
            _state = TokenizerState.ScriptDataLessThanSign;
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            EmitReplacementCharacterToken();
        }
        else if (r is null)
        {
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(r.Value);
        }
    }

    private void Plaintext(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#plaintext-state>

        if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            EmitReplacementCharacterToken();
        }
        else if (r is null)
        {
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(r.Value);
        }
    }

    private void TagOpen(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#tag-open-state>

        if (r?.Value is '!')
        {
            _state = TokenizerState.MarkupDeclarationOpen;
        }
        else if (r?.Value is '/')
        {
            _state = TokenizerState.EndTagOpen;
        }
        else if (IsAsciiAlpha(r))
        {
            _currentTag = Tag.NewStartTag();
            Reconsume(TokenizerState.TagName, r);
        }
        else if (r?.Value is '?')
        {
            AddParseError(ParseError.UnexpectedQuestionMarkInsteadOfTagName);
            _currentComment = new();
            Reconsume(TokenizerState.BogusComment, r);
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofBeforeTagName);
            EmitCharacterToken('<');
            EmitEndOfFileToken();
        }
        else
        {
            AddParseError(ParseError.InvalidFirstCharacterOfTagName);
            EmitCharacterToken('<');
            Reconsume(TokenizerState.Data, r);
        }
    }

    private void EndTagOpen(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#end-tag-open-state>

        if (IsAsciiAlpha(r))
        {
            _currentTag = Tag.NewEndTag();
            Reconsume(TokenizerState.TagName, r);
        }
        else if (r?.Value is '>')
        {
            AddParseError(ParseError.MissingEndTagName);
            _state = TokenizerState.Data;
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofBeforeTagName);
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitEndOfFileToken();
        }
        else
        {
            AddParseError(ParseError.InvalidFirstCharacterOfTagName);
            _currentComment = new();
            Reconsume(TokenizerState.BogusComment, r);
        }
    }

    private void TagName(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#tag-name-state>

        if (IsSpecialWhitespace(r))
        {
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (r?.Value is '/')
        {
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (r?.Value is '>')
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(r))
        {
            _currentTag!.AppendName(ToAsciiLowercase(r!.Value));
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _currentTag!.AppendName(REPLACEMENT_CHARACTER);
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInTag);
            EmitEndOfFileToken();
        }
        else
        {
            _currentTag!.AppendName(r.Value);
        }
    }

    private void RCDataLessThanSign(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rcdata-less-than-sign-state>

        if (r?.Value is '/')
        {
            _tempBuffer.Clear();
            _state = TokenizerState.RCDataEndTagOpen;
        }
        else
        {
            EmitCharacterToken('<');
            Reconsume(TokenizerState.RCData, r);
        }
    }

    private void RCDataEndTagOpen(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rcdata-end-tag-open-state>

        if (IsAsciiAlpha(r))
        {
            _currentTag = Tag.NewEndTag();
            Reconsume(TokenizerState.RCDataEndTagName, r);
        }
    }

    private void RCDataEndTagName(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rcdata-end-tag-name-state>

        if (IsSpecialWhitespace(r) && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (r?.Value is '/' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (r?.Value is '>' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(r))
        {
            _currentTag!.AppendName(ToAsciiLowercase(r!.Value));
            _tempBuffer.Add(r.Value);
        }
        else if (IsAsciiLowerAlpha(r))
        {
            _currentTag!.AppendName(r!.Value);
            _tempBuffer.Add(r.Value);
        }
        else
        {
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitCharacterTokensFromTemporaryBuffer();
            Reconsume(TokenizerState.RCData, r);
        }
    }

    private void RawTextLessThanSign(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rawtext-less-than-sign-state>

        if (r?.Value is '/')
        {
            _tempBuffer.Clear();
            _state = TokenizerState.RawTextEndTagOpen;
        }
        else
        {
            EmitCharacterToken('<');
            Reconsume(TokenizerState.RawText, r);
        }
    }

    private void RawTextEndTagOpen(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rawtext-end-tag-open-state>

        if (IsAsciiAlpha(r))
        {
            _currentTag = Tag.NewEndTag();
            Reconsume(TokenizerState.RawTextEndTagName, r);
        }
        else
        {
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            Reconsume(TokenizerState.RawText, r);
        }
    }

    private void RawTextEndTagName(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rawtext-end-tag-name-state>

        if (IsSpecialWhitespace(r) && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (r?.Value is '/' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (r?.Value is '>' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(r))
        {
            _currentTag!.AppendName(ToAsciiLowercase(r!.Value));
            _tempBuffer.Add(r.Value);
        }
        else if (IsAsciiLowerAlpha(r))
        {
            _currentTag!.AppendName(r!.Value);
            _tempBuffer.Add(r.Value);
        }
        else
        {
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitCharacterTokensFromTemporaryBuffer();
            Reconsume(TokenizerState.RawText, r);
        }
    }

    private void ScriptDataLessThanSign(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-less-than-sign-state>

        if (r?.Value is '/')
        {
            _tempBuffer.Clear();
            _state = TokenizerState.ScriptDataEndTagOpen;
        }
        else if (r?.Value is '!')
        {
            _state = TokenizerState.ScriptDataEscapeStart;
            EmitCharacterToken('<');
            EmitCharacterToken('!');
        }
        else
        {
            EmitCharacterToken('<');
            Reconsume(TokenizerState.ScriptData, r);
        }
    }

    private void ScriptDataEndTagOpen(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-end-tag-open-state>

        if (IsAsciiAlpha(r))
        {
            _currentTag = Tag.NewEndTag();
            Reconsume(TokenizerState.ScriptDataEndTagName, r);
        }
        else
        {
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            Reconsume(TokenizerState.ScriptData, r);
        }
    }

    private void ScriptDataEndTagName(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-end-tag-name-state>

        if (IsSpecialWhitespace(r) && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (r?.Value is '/' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (r?.Value is '>' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(r))
        {
            _currentTag!.AppendName(ToAsciiLowercase(r!.Value));
            _tempBuffer.Add(r.Value);
        }
        else if (IsAsciiLowerAlpha(r))
        {
            _currentTag!.AppendName(r!.Value);
            _tempBuffer.Add(r.Value);
        }
        else
        {
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitCharacterTokensFromTemporaryBuffer();
            Reconsume(TokenizerState.ScriptData, r);
        }
    }

    private void ScriptDataEscapeStart(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escape-start-state>

        if (r?.Value is '-')
        {
            _state = TokenizerState.ScriptDataEscapeStartDash;
            EmitCharacterToken('-');
        }
        else
        {
            Reconsume(TokenizerState.ScriptData, r);
        }
    }

    private void ScriptDataEscapeStartDash(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escape-start-dash-state>

        if (r?.Value is '-')
        {
            _state = TokenizerState.ScriptDataEscapedDashDash;
            EmitCharacterToken('-');
        }
        else
        {
            Reconsume(TokenizerState.ScriptData, r);
        }
    }

    private void ScriptDataEscaped(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-state>

        if (r?.Value is '-')
        {
            _state = TokenizerState.ScriptDataEscapedDash;
            EmitCharacterToken('-');
        }
        else if (r?.Value is '<')
        {
            _state = TokenizerState.ScriptDataEscapedLessThanSign;
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            EmitReplacementCharacterToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInScriptHtmlCommentLikeText);
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(r.Value);
        }
    }

    private void ScriptDataEscapedDash(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-dash-state>

        if (r?.Value is '-')
        {
            _state = TokenizerState.ScriptDataEscapedDashDash;
            EmitCharacterToken('-');
        }
        else if (r?.Value is '<')
        {
            _state = TokenizerState.ScriptDataEscapedLessThanSign;
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _state = TokenizerState.ScriptDataEscaped;
            EmitReplacementCharacterToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInScriptHtmlCommentLikeText);
            EmitEndOfFileToken();
        }
        else
        {
            _state = TokenizerState.ScriptDataEscaped;
            EmitCharacterToken(r.Value);
        }
    }

    private void ScriptDataEscapedDashDash(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-dash-dash-state>

        if (r?.Value is '-')
        {
            EmitCharacterToken('-');
        }
        else if (r?.Value is '<')
        {
            _state = TokenizerState.ScriptDataEscapedLessThanSign;
        }
        else if (r?.Value is '>')
        {
            _state = TokenizerState.ScriptData;
            EmitCharacterToken('>');
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _state = TokenizerState.ScriptDataEscaped;
            EmitReplacementCharacterToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInScriptHtmlCommentLikeText);
            EmitEndOfFileToken();
        }
        else
        {
            _state = TokenizerState.ScriptDataEscaped;
            EmitCharacterToken(r.Value);
        }
    }

    private void ScriptDataEscapedLessThanSign(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-less-than-sign-state>

        if (r?.Value is '/')
        {
            _tempBuffer.Clear();
            _state = TokenizerState.ScriptDataEscapedEndTagOpen;
        }
        else if (IsAsciiAlpha(r))
        {
            _tempBuffer.Clear();
            EmitCharacterToken('<');
            Reconsume(TokenizerState.ScriptDataDoubleEscapeStart, r);
        }
        else
        {
            EmitCharacterToken('<');
            Reconsume(TokenizerState.ScriptDataEscaped, r);
        }
    }

    private void ScriptDataEscapedEndTagOpen(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-end-tag-open-state>

        if (IsAsciiAlpha(r))
        {
            _currentTag = Tag.NewEndTag();
            Reconsume(TokenizerState.ScriptDataEscapedEndTagName, r);
        }
        else
        {
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            Reconsume(TokenizerState.ScriptDataEscaped, r);
        }
    }

    private void ScriptDataEscapedEndTagName(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-end-tag-name-state>

        if (IsSpecialWhitespace(r) && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (r?.Value is '/' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (r?.Value is '>' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(r))
        {
            _currentTag!.AppendName(ToAsciiLowercase(r!.Value));
            _tempBuffer.Add(r.Value);
        }
        else if (IsAsciiLowerAlpha(r))
        {
            _currentTag!.AppendName(r!.Value);
            _tempBuffer.Add(r.Value);
        }
        else
        {
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitCharacterTokensFromTemporaryBuffer();
            Reconsume(TokenizerState.ScriptDataEscaped, r);
        }
    }

    private void ScriptDataDoubleEscapeStart(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escape-start-state>

        if (IsSpecialWhitespace(r) || r?.Value is '/' or '>')
        {
            if (CompareTemporaryBuffer("script"))
            {
                _state = TokenizerState.ScriptDataDoubleEscaped;
            }
            else
            {
                _state = TokenizerState.ScriptDataEscaped;
                EmitCharacterToken(r!.Value);
            }
        }
        else if (IsAsciiUpperAlpha(r))
        {
            _tempBuffer.Add(ToAsciiLowercase(r!.Value));
            EmitCharacterToken(r.Value);
        }
        else if (IsAsciiLowerAlpha(r))
        {
            _tempBuffer.Add(r!.Value);
            EmitCharacterToken(r.Value);
        }
        else
        {
            Reconsume(TokenizerState.ScriptDataEscaped, r);
        }
    }

    private void ScriptDataDoubleEscaped(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escaped-state>

        if (r?.Value is '-')
        {
            _state = TokenizerState.ScriptDataDoubleEscapedDash;
            EmitCharacterToken('-');
        }
        else if (r?.Value is '<')
        {
            _state = TokenizerState.ScriptDataDoubleEscapedLessThanSign;
            EmitCharacterToken('<');
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            EmitReplacementCharacterToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInScriptHtmlCommentLikeText);
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(r.Value);
        }
    }

    private void ScriptDataDoubleEscapedDash(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escaped-dash-state>

        if (r?.Value is '-')
        {
            _state = TokenizerState.ScriptDataDoubleEscapedDashDash;
            EmitCharacterToken('-');
        }
        else if (r?.Value is '<')
        {
            _state = TokenizerState.ScriptDataDoubleEscapedLessThanSign;
            EmitCharacterToken('<');
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _state = TokenizerState.ScriptDataDoubleEscaped;
            EmitReplacementCharacterToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInScriptHtmlCommentLikeText);
            EmitEndOfFileToken();
        }
        else
        {
            _state = TokenizerState.ScriptDataDoubleEscaped;
            EmitCharacterToken(r.Value);
        }
    }

    private void ScriptDataDoubleEscapedDashDash(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escaped-dash-dash-state>

        if (r?.Value is '-')
        {
            EmitCharacterToken('-');
        }
        else if (r?.Value is '<')
        {
            _state = TokenizerState.ScriptDataDoubleEscapedLessThanSign;
            EmitCharacterToken('<');
        }
        else if (r?.Value is '>')
        {
            _state = TokenizerState.ScriptData;
            EmitCharacterToken('>');
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _state = TokenizerState.ScriptDataDoubleEscaped;
            EmitReplacementCharacterToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInScriptHtmlCommentLikeText);
            EmitEndOfFileToken();
        }
        else
        {
            _state = TokenizerState.ScriptDataDoubleEscaped;
            EmitCharacterToken(r.Value);
        }
    }

    private void ScriptDataDoubleEscapedLessThanSign(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escaped-less-than-sign-state>

        if (r?.Value is '/')
        {
            _tempBuffer.Clear();
            _state = TokenizerState.ScriptDataDoubleEscapeEnd;
            EmitCharacterToken('/');
        }
        else
        {
            Reconsume(TokenizerState.ScriptDataDoubleEscaped, r);
        }
    }

    private void ScriptDataDoubleEscapeEnd(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escape-end-state>

        if (IsSpecialWhitespace(r) || r!.Value.Value is '/' or '>')
        {
            if (CompareTemporaryBuffer("script"))
            {
                _state = TokenizerState.ScriptDataEscaped;
            }
            else
            {
                _state = TokenizerState.ScriptDataDoubleEscaped;
                EmitCharacterToken(r!.Value);
            }
        }
        else if (IsAsciiUpperAlpha(r))
        {
            _tempBuffer.Add(ToAsciiLowercase(r.Value));
            EmitCharacterToken(r.Value);
        }
        else if (IsAsciiLowerAlpha(r))
        {
            _tempBuffer.Add(r.Value);
            EmitCharacterToken(r.Value);
        }
        else
        {
            Reconsume(TokenizerState.ScriptDataDoubleEscaped, r);
        }
    }

    private void BeforeAttributeName(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-attribute-name-state>

        if (IsSpecialWhitespace(r))
        {
            // Ignore the character.
        }
        else if (r?.Value is '/' or '>' || r is null)
        {
            Reconsume(TokenizerState.AfterAttributeName, r);
        }
        else if (r.Value.Value is '=')
        {
            AddParseError(ParseError.UnexpectedEqualsSignBeforeAttributeName);
            _currentAttribute = _currentTag!.NewAttribute();
            _currentAttribute.AppendName(new('='));
            _state = TokenizerState.AttributeName;
        }
        else
        {
            _currentAttribute = _currentTag!.NewAttribute();
            Reconsume(TokenizerState.AttributeName, r);
        }
    }

    private void AttributeName(Rune? r)
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

        if (IsSpecialWhitespace(r) || r?.Value is '/' or '>' || r is null)
        {
            if (_currentTag!.CheckAndCorrectDuplicateAttributes())
                AddParseError(ParseError.DuplicateAttribute); // see block comment above
            Reconsume(TokenizerState.AfterAttributeName, r);
        }
        else if (r.Value.Value is '=')
        {
            if (_currentTag!.CheckAndCorrectDuplicateAttributes())
                AddParseError(ParseError.DuplicateAttribute); // see block comment above
            _state = TokenizerState.BeforeAttributeValue;
        }
        else if (IsAsciiUpperAlpha(r))
        {
            _currentAttribute!.AppendName(ToAsciiLowercase(r.Value));
        }
        else if (r.Value.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _currentAttribute!.AppendName(REPLACEMENT_CHARACTER);
        }
        else
        {
            if (r.Value.Value is '"' or '\'' or '<')
            {
                AddParseError(ParseError.UnexpectedCharacterInAttributeName);
            }
            _currentAttribute!.AppendName(r.Value);
        }
    }

    private void AfterAttributeName(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-attribute-name-state>

        if (IsSpecialWhitespace(r))
        {
            // Ignore the character.
        }
        else if (r?.Value is '/')
        {
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (r?.Value is '=')
        {
            _state = TokenizerState.BeforeAttributeValue;
        }
        else if (r?.Value is '>')
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInTag);
            EmitEndOfFileToken();
        }
        else
        {
            _currentAttribute = _currentTag!.NewAttribute();
            Reconsume(TokenizerState.AttributeName, r);
        }
    }

    private void BeforeAttributeValue(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-attribute-value-state>

        if (IsSpecialWhitespace(r))
        {
            // Ignore the character.
        }
        else if (r?.Value is '"')
        {
            _state = TokenizerState.AttributeValueDoubleQuoted;
        }
        else if (r?.Value is '\'')
        {
            _state = TokenizerState.AttributeValueSingleQuoted;
        }
        else if (r?.Value is '>')
        {
            AddParseError(ParseError.MissingAttributeValue);
            _state = TokenizerState.Data;
            EmitTagToken();
        }
    }

    private void AttributeValueDoubleQuoted(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#attribute-value-(double-quoted)-state>

        if (r?.Value is '"')
        {
            _state = TokenizerState.AfterAttributeValueQuoted;
        }
        else if (r?.Value is '&')
        {
            _returnState = TokenizerState.AttributeValueDoubleQuoted;
            _state = TokenizerState.CharacterReference;
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _currentAttribute!.AppendValue(REPLACEMENT_CHARACTER);
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInTag);
            EmitEndOfFileToken();
        }
        else
        {
            _currentAttribute!.AppendValue(r.Value);
        }
    }

    private void AttributeValueSingleQuoted(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#attribute-value-(single-quoted)-state>

        if (r?.Value is '\'')
        {
            _state = TokenizerState.AfterAttributeValueQuoted;
        }
        else if (r?.Value is '&')
        {
            _returnState = TokenizerState.AttributeValueSingleQuoted;
            _state = TokenizerState.CharacterReference;
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _currentAttribute!.AppendValue(REPLACEMENT_CHARACTER);
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInTag);
            EmitEndOfFileToken();
        }
        else
        {
            _currentAttribute!.AppendValue(r.Value);
        }
    }

    private void AttributeValueUnquoted(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#attribute-value-(unquoted)-state>

        if (IsSpecialWhitespace(r))
        {
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (r?.Value is '&')
        {
            _returnState = TokenizerState.AttributeValueUnquoted;
            _state = TokenizerState.CharacterReference;
        }
        else if (r?.Value is '>')
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _currentAttribute!.AppendValue(REPLACEMENT_CHARACTER);
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInTag);
            EmitEndOfFileToken();
        }
        else
        {
            if (r.Value.Value is '"' or '\'' or '<' or '=' or '`')
            {
                AddParseError(ParseError.UnexpectedCharacterInUnquotedAttributeValue);
            }
            _currentAttribute!.AppendValue(r.Value);
        }
    }

    private void AfterAttributeValueQuoted(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-attribute-value-(quoted)-state>

        if (IsSpecialWhitespace(r))
        {
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (r?.Value is '/')
        {
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (r?.Value is '>')
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInTag);
            EmitEndOfFileToken();
        }
        else
        {
            AddParseError(ParseError.MissingWhitespaceBetweenAttributes);
            Reconsume(TokenizerState.BeforeAttributeName, r);
        }
    }

    private void SelfClosingStartTag(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#self-closing-start-tag-state>

        if (r?.Value is '>')
        {
            _currentTag!.SetSelfClosingFlag();
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInTag);
            EmitEndOfFileToken();
        }
        else
        {
            AddParseError(ParseError.UnexpectedSolidusInTag);
            Reconsume(TokenizerState.BeforeAttributeName, r);
        }
    }

    private void BogusComment(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#bogus-comment-state>

        if (r?.Value is '>')
        {
            _state = TokenizerState.Data;
            EmitCommentToken();
        }
        else if (r is null)
        {
            EmitCommentToken();
            EmitEndOfFileToken();
        }
        else if (r.Value.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _currentComment!.Append(REPLACEMENT_CHARACTER);
        }
        else
        {
            _currentComment!.Append(r.Value);
        }
    }

    private void MarkupDeclarationOpen(Rune? r)
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

    private void CommentStart(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-start-state>

        if (r?.Value is '-')
        {
            _state = TokenizerState.CommentStartDash;
        }
        else if (r?.Value is '>')
        {
            AddParseError(ParseError.AbruptClosingOfEmptyComment);
            _state = TokenizerState.Data;
            EmitCommentToken();
        }
        else
        {
            Reconsume(TokenizerState.Comment, r);
        }
    }

    private void CommentStartDash(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-start-dash-state>

        if (r?.Value is '-')
        {
            _state = TokenizerState.CommentEnd;
        }
        else if (r?.Value is '>')
        {
            AddParseError(ParseError.AbruptClosingOfEmptyComment);
            _state = TokenizerState.Data;
            EmitCommentToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInComment);
            EmitCommentToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentComment!.Append('-');
            _currentComment.Append('-');
            Reconsume(TokenizerState.Comment, r);
        }
    }

    private void Comment(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-state>

        if (r?.Value is '<')
        {
            _currentComment!.Append('<');
            _state = TokenizerState.CommentLessThanSign;
        }
        else if (r?.Value is '-')
        {
            _state = TokenizerState.CommentEndDash;
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _currentComment!.Append(REPLACEMENT_CHARACTER);
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInComment);
            EmitCommentToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentComment!.Append(r.Value);
        }
    }

    private void CommentLessThanSign(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-less-than-sign-state>

        if (r?.Value is '!')
        {
            _currentComment!.Append('!');
            _state = TokenizerState.CommentLessThanSignBang;
        }
        else if (r?.Value is '<')
        {
            _currentComment!.Append('<');
        }
        else
        {
            Reconsume(TokenizerState.Comment, r);
        }
    }

    private void CommentLessThanSignBang(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-less-than-sign-bang-state>

        if (r?.Value is '-')
        {
            _state = TokenizerState.CommentLessThanSignBangDash;
        }
        else
        {
            Reconsume(TokenizerState.Comment, r);
        }
    }

    private void CommentLessThanSignBangDash(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-less-than-sign-bang-dash-state>

        if (r?.Value is '-')
        {
            _state = TokenizerState.CommentLessThanSignBangDashDash;
        }
        else
        {
            Reconsume(TokenizerState.Comment, r);
        }
    }

    private void CommentLessThanSignBangDashDash(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-less-than-sign-bang-dash-dash-state>

        if (r?.Value is '>' || r is null)
        {
            Reconsume(TokenizerState.CommentEnd, r);
        }
        else
        {
            AddParseError(ParseError.NestedComment);
            Reconsume(TokenizerState.CommentEnd, r);
        }
    }

    private void CommentEndDash(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-end-dash-state>

        if (r?.Value is '-')
        {
            _state = TokenizerState.CommentEnd;
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInComment);
            EmitCommentToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentComment!.Append('-');
            Reconsume(TokenizerState.Comment, r);
        }
    }

    private void CommentEnd(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-end-state>

        if (r?.Value is '>')
        {
            _state = TokenizerState.Data;
            EmitCommentToken();
        }
        else if (r?.Value is '!')
        {
            _state = TokenizerState.CommentEndBang;
        }
        else if (r?.Value is '-')
        {
            _currentComment!.Append('-');
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInComment);
            EmitCommentToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentComment!.Append('-');
            _currentComment.Append('-');
            Reconsume(TokenizerState.Comment, r);
        }
    }

    private void CommentEndBang(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-end-bang-state>

        if (r?.Value is '-')
        {
            _currentComment!.Append('-');
            _currentComment.Append('-');
            _currentComment.Append('!');
            _state = TokenizerState.CommentEndDash;
        }
        else if (r?.Value is '>')
        {
            AddParseError(ParseError.IncorrectlyClosedComment);
            _state = TokenizerState.Data;
            EmitCommentToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInComment);
            EmitCommentToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentComment!.Append('-');
            _currentComment.Append('-');
            _currentComment.Append('!');
            Reconsume(TokenizerState.Comment, r);
        }
    }

    private void Doctype(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-state>

        if (IsSpecialWhitespace(r))
        {
            _state = TokenizerState.BeforeDoctypeName;
        }
        else if (r?.Value is '>')
        {
            Reconsume(TokenizerState.BeforeDoctypeName, r);
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype = new();
            _currentDoctype.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            AddParseError(ParseError.MissingWhitespaceBeforeDoctypeName);
            Reconsume(TokenizerState.BeforeDoctypeName, r);
        }
    }

    private void BeforeDoctypeName(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-doctype-name-state>

        if (IsSpecialWhitespace(r))
        {
            // Ignore the character.
        }
        else if (IsAsciiUpperAlpha(r))
        {
            _currentDoctype = new();
            _currentDoctype.AppendName(ToAsciiLowercase(r!.Value));
            _state = TokenizerState.DoctypeName;
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _currentDoctype = new();
            _currentDoctype.AppendName(REPLACEMENT_CHARACTER);
            _state = TokenizerState.DoctypeName;
        }
        else if (r?.Value is '>')
        {
            AddParseError(ParseError.MissingDoctypeName);
            _currentDoctype = new();
            _currentDoctype.SetQuirksFlag();
            EmitDoctypeToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype = new();
            _currentDoctype.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentDoctype = new();
            _currentDoctype.AppendName(r.Value);
            _state = TokenizerState.DoctypeName;
        }
    }

    private void DoctypeName(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-name-state>

        if (IsSpecialWhitespace(r))
        {
            _state = TokenizerState.AfterDoctypeName;
        }
        else if (r?.Value is '>')
        {
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (IsAsciiUpperAlpha(r))
        {
            _currentDoctype!.AppendName(ToAsciiLowercase(r!.Value));
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _currentDoctype!.AppendName(REPLACEMENT_CHARACTER);
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype = new();
            _currentDoctype.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentDoctype!.AppendName(r.Value);
        }
    }

    private void AfterDoctypeName(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-name-state>

        if (IsSpecialWhitespace(r))
        {
            // Ignore the character.
        }
        else if (r?.Value is '>')
        {
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
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

    private void AfterDoctypePublicKeyword(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-public-keyword-state>

        if (IsSpecialWhitespace(r))
        {
            _state = TokenizerState.BeforeDoctypePublicIdentifier;
        }
        else if (r?.Value is '"')
        {
            AddParseError(ParseError.MissingWhitespaceAfterDoctypePublicKeyword);
            _currentDoctype!.SetPublicIdentifierToEmptyString();
            _state = TokenizerState.DoctypePublicIdentifierDoubleQuoted;
        }
        else if (r?.Value is '\'')
        {
            AddParseError(ParseError.MissingWhitespaceAfterDoctypePublicKeyword);
            _currentDoctype!.SetPublicIdentifierToEmptyString();
            _state = TokenizerState.DoctypePublicIdentifierSingleQuoted;
        }
        else if (r?.Value is '>')
        {
            AddParseError(ParseError.MissingDoctypePublicIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            AddParseError(ParseError.MissingQuoteBeforeDoctypePublicIdentifier);
            _currentDoctype!.SetQuirksFlag();
            Reconsume(TokenizerState.BogusDoctype, r);
        }
    }

    private void BeforeDoctypePublicIdentifier(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-doctype-public-identifier-state>

        if (IsSpecialWhitespace(r))
        {
            // Ignore the character.
        }
        else if (r?.Value is '"')
        {
            _currentDoctype!.SetPublicIdentifierToEmptyString();
            _state = TokenizerState.DoctypePublicIdentifierDoubleQuoted;
        }
        else if (r?.Value is '\'')
        {
            _currentDoctype!.SetPublicIdentifierToEmptyString();
            _state = TokenizerState.DoctypePublicIdentifierSingleQuoted;
        }
        else if (r?.Value is '>')
        {
            AddParseError(ParseError.MissingDoctypePublicIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            AddParseError(ParseError.MissingQuoteBeforeDoctypePublicIdentifier);
            _currentDoctype!.SetQuirksFlag();
            Reconsume(TokenizerState.BogusDoctype, r);
        }
    }

    private void DoctypePublicIdentifierDoubleQuoted(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-public-identifier-(double-quoted)-state>

        if (r?.Value is '"')
        {
            _state = TokenizerState.AfterDoctypePublicIdentifier;
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _currentDoctype!.AppendPublicIdentifier(REPLACEMENT_CHARACTER);
        }
        else if (r?.Value is '>')
        {
            AddParseError(ParseError.AbruptDoctypePublicIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentDoctype!.AppendPublicIdentifier(r.Value);
        }
    }

    private void DoctypePublicIdentifierSingleQuoted(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-public-identifier-(single-quoted)-state>

        if (r?.Value is '\'')
        {
            _state = TokenizerState.AfterDoctypePublicIdentifier;
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _currentDoctype!.AppendPublicIdentifier(REPLACEMENT_CHARACTER);
        }
        else if (r?.Value is '>')
        {
            AddParseError(ParseError.AbruptDoctypePublicIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentDoctype!.AppendPublicIdentifier(r.Value);
        }
    }

    private void AfterDoctypePublicIdentifier(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-public-identifier-state>

        if (IsSpecialWhitespace(r))
        {
            _state = TokenizerState.BetweenDoctypePublicAndSystemIdentifiers;
        }
        else if (r?.Value is '>')
        {
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (r?.Value is '"')
        {
            AddParseError(ParseError.MissingWhitespaceBetweenDoctypePublicAndSystemIdentifiers);
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
        }
        else if (r?.Value is '\'')
        {
            AddParseError(ParseError.MissingWhitespaceBetweenDoctypePublicAndSystemIdentifiers);
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            AddParseError(ParseError.MissingQuoteBeforeDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            Reconsume(TokenizerState.BogusDoctype, r);
        }
    }

    private void BetweenDoctypePublicAndSystemIdentifiers(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#between-doctype-public-and-system-identifiers-state>

        if (IsSpecialWhitespace(r))
        {
            // Ignore the character.
        }
        else if (r?.Value is '>')
        {
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (r?.Value is '"')
        {
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
        }
        else if (r?.Value is '\'')
        {
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            AddParseError(ParseError.MissingQuoteBeforeDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            Reconsume(TokenizerState.BogusDoctype, r);
        }
    }

    private void AfterDoctypeSystemKeyword(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-system-keyword-state>

        if (IsSpecialWhitespace(r))
        {
            _state = TokenizerState.BeforeDoctypeSystemIdentifier;
        }
        else if (r?.Value is '"')
        {
            AddParseError(ParseError.MissingWhitespaceAfterDoctypeSystemKeyword);
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
        }
        else if (r?.Value is '\'')
        {
            AddParseError(ParseError.MissingWhitespaceAfterDoctypeSystemKeyword);
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
        }
        else if (r?.Value is '>')
        {
            AddParseError(ParseError.MissingDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            AddParseError(ParseError.MissingQuoteBeforeDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            Reconsume(TokenizerState.BogusDoctype, r);
        }
    }

    private void BeforeDoctypeSystemIdentifier(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-doctype-system-identifier-state>

        if (IsSpecialWhitespace(r))
        {
            // Ignore the character.
        }
        else if (r?.Value is '"')
        {
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
        }
        else if (r?.Value is '\'')
        {
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
        }
        else if (r?.Value is '>')
        {
            AddParseError(ParseError.MissingDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            AddParseError(ParseError.MissingQuoteBeforeDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            Reconsume(TokenizerState.BogusDoctype, r);
        }
    }

    private void DoctypeSystemIdentifierDoubleQuoted(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-system-identifier-(double-quoted)-state>

        if (r?.Value is '"')
        {
            _state = TokenizerState.AfterDoctypeSystemIdentifier;
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _currentDoctype!.AppendSystemIdentifier(REPLACEMENT_CHARACTER);
        }
        else if (r?.Value is '>')
        {
            AddParseError(ParseError.AbruptDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentDoctype!.AppendSystemIdentifier(r.Value);
        }
    }

    private void DoctypeSystemIdentifierSingleQuoted(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-system-identifier-(single-quoted)-state>

        if (r?.Value is '\'')
        {
            _state = TokenizerState.AfterDoctypeSystemIdentifier;
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            _currentDoctype!.AppendSystemIdentifier(REPLACEMENT_CHARACTER);
        }
        else if (r?.Value is '>')
        {
            AddParseError(ParseError.AbruptDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentDoctype!.AppendSystemIdentifier(r.Value);
        }
    }

    private void AfterDoctypeSystemIdentifier(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-system-identifier-state>

        if (IsSpecialWhitespace(r))
        {
            // Ignore the character.
        }
        else if (r?.Value is '>')
        {
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            AddParseError(ParseError.UnexpectedCharacterAfterDoctypeSystemIdentifier);
            Reconsume(TokenizerState.BogusDoctype, r);
        }
    }

    private void BogusDoctype(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#bogus-doctype-state>

        if (r?.Value is '>')
        {
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (r?.Value is '\0')
        {
            AddParseError(ParseError.UnexpectedNullCharacter);
            // Ignore the character.
        }
        else if (r is null)
        {
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
    }

    private void CDataSection(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#cdata-section-state>

        if (r?.Value is ']')
        {
            _state = TokenizerState.CDataSectionBracket;
        }
        else if (r is null)
        {
            AddParseError(ParseError.EofInCData);
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(r.Value);
        }

        /* U+0000 NULL characters are handled in the tree construction stage,
         *   as part of the in foreign content insertion mode, which is the only
         *   place where CDATA sections can appear.
         */
    }

    private void CDataSectionBracket(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#cdata-section-bracket-state>

        if (r?.Value is ']')
        {
            _state = TokenizerState.CDataSectionEnd;
        }
        else
        {
            EmitCharacterToken(']');
            Reconsume(TokenizerState.CDataSection, r);
        }
    }

    private void CDataSectionEnd(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#cdata-section-end-state>

        if (r?.Value is ']')
        {
            EmitCharacterToken(']');
        }
        else if (r?.Value is '>')
        {
            _state = TokenizerState.Data;
        }
        else
        {
            EmitCharacterToken(']');
            EmitCharacterToken(']');
            Reconsume(TokenizerState.CDataSection, r);
        }
    }

    private void CharacterReference(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#character-reference-state>

        _tempBuffer.Clear();
        _tempBuffer.Add(new('&'));
        if (IsAsciiAlphanumeric(r))
        {
            Reconsume(TokenizerState.NamedCharacterReference, r);
        }
        else if (r?.Value is '#')
        {
            _tempBuffer.Add(new('#'));
            _state = TokenizerState.NumericCharacterReference;
        }
        else
        {
            FlushCodePointsConsumedAsCharacterReference();
            Reconsume(_returnState!.Value, r);
        }
    }

    private void NamedCharacterReference(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#named-character-reference-state>

        // Consume the maximum number of characters possible, where the consumed
        //   characters are one of the identifiers in the first column of the
        //   named character references table.
        // Append each character to the temporary buffer when it's consumed.
        throw new NotImplementedException();
    }

    private void AmbiguousAmpersand(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#ambiguous-ampersand-state>

        if (IsAsciiAlphanumeric(r))
        {
            if (WasConsumedAsPartOfAnAttribute())
                _currentAttribute!.AppendValue(r!.Value);
            else
                EmitCharacterToken(r!.Value);
        }
        else if (r?.Value is ';')
        {
            AddParseError(ParseError.UnknownNamedCharacterReference);
            Reconsume(_returnState!.Value, r);
        }
        else
        {
            Reconsume(_returnState!.Value, r);
        }
    }

    private void NumericCharacterReference(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#numeric-character-reference-state>

        _charRefCode = 0;
        if (r?.Value is 'x' or 'X')
        {
            _tempBuffer.Add(r.Value);
            _state = TokenizerState.HexadecimalCharacterReferenceStart;
        }
        else
        {
            Reconsume(TokenizerState.DecimalCharacterReferenceStart, r);
        }
    }

    private void HexadecimalCharacterReferenceStart(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#hexadecimal-character-reference-start-state>

        if (IsAsciiHexDigit(r))
        {
            Reconsume(TokenizerState.HexadecimalCharacterReference, r);
        }
        else
        {
            AddParseError(ParseError.AbsenseOfDigitsInNumericCharacterReference);
            FlushCodePointsConsumedAsCharacterReference();
            Reconsume(_returnState!.Value, r);
        }
    }

    private void DecimalCharacterReferenceStart(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#decimal-character-reference-start-state>

        if (IsAsciiDigit(r))
        {
            Reconsume(TokenizerState.DecimalCharacterReference, r);
        }
        else
        {
            AddParseError(ParseError.AbsenseOfDigitsInNumericCharacterReference);
            FlushCodePointsConsumedAsCharacterReference();
            Reconsume(_returnState!.Value, r);
        }
    }

    private void HexadecimalCharacterReference(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#hexadecimal-character-reference-state>

        if (IsAsciiDigit(r))
        {
            _charRefCode *= 16;
            _charRefCode += r!.Value.Value - '0';
        }
        else if (IsAsciiUpperHexDigit(r))
        {
            _charRefCode *= 16;
            _charRefCode += r!.Value.Value - 'A' + 10;
        }
        else if (IsAsciiLowerHexDigit(r))
        {
            _charRefCode *= 16;
            _charRefCode += r!.Value.Value - 'a' + 10;
        }
        else if (r?.Value is ';')
        {
            _state = TokenizerState.NumericCharacterReferenceEnd;
        }
        else
        {
            AddParseError(ParseError.MissingSemicolonAfterCharacterReference);
            Reconsume(TokenizerState.NumericCharacterReferenceEnd, r);
        }
    }

    private void DecimalCharacterReference(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#decimal-character-reference-state>

        if (IsAsciiDigit(r))
        {
            _charRefCode *= 10;
            _charRefCode += r!.Value.Value - '0';
        }
        else if (r?.Value is ';')
        {
            _state = TokenizerState.NumericCharacterReferenceEnd;
        }
        else
        {
            AddParseError(ParseError.MissingSemicolonAfterCharacterReference);
            Reconsume(TokenizerState.NumericCharacterReferenceEnd, r);
        }
    }

    private void NumericCharacterReferenceEnd(Rune? r)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#numeric-character-reference-end-state>

        PutBack(r); // we don't need it

        if (_charRefCode is 0)
        {
            AddParseError(ParseError.NullCharacterReference);
            _charRefCode = REPLACEMENT_CHARACTER.Value;
        }
        else if (_charRefCode > 0x10FFFF)
        {
            AddParseError(ParseError.CharacterReferenceOutsideUnicodeRange);
            _charRefCode = REPLACEMENT_CHARACTER.Value;
        }
        else if (IsSurrogate(_charRefCode))
        {
            AddParseError(ParseError.SurrogateCharacterReference);
            _charRefCode = REPLACEMENT_CHARACTER.Value;
        }
        else if (IsNoncharacter(_charRefCode))
        {
            AddParseError(ParseError.NoncharacterCharacterReference);
        }
        else if (_charRefCode is 0xD || (IsControl(_charRefCode) && !IsAsciiWhitespace(_charRefCode)))
        {
            AddParseError(ParseError.ControlCharacterReference);
        }
        else if (CharReference.NumericList.TryGetValue(_charRefCode, out int converted))
        {
            _charRefCode = converted;
        }

        _tempBuffer.Clear();
        _tempBuffer.Add(new(_charRefCode));
        FlushCodePointsConsumedAsCharacterReference();
        _state = _returnState!.Value;
        _returnState = null;
    }
}
