/* =============================================================================
 * File:   HtmlTokenizer.Tokenizer.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Contains the actual HTML tokenizer implementation.
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
            // use any tokens we need to emit first
            while (_tokensToEmit.Any())
            {
                Token token = _tokensToEmit.Dequeue();
                yield return token;
                if (token.Type is TokenType.EndOfFile)
                    yield break;
            }

            // ... before getting new ones
            int c = Read();
            _stateMap[(int)_state](c);
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

        if (c is '&')
        {
            _returnState = TokenizerState.Data;
            _state = TokenizerState.CharacterReference;
        }
        else if (c is '<')
        {
            _state = TokenizerState.TagOpen;
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            EmitCharacterToken('\0');
        }
        else if (c is EOF)
        {
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(c);
        }
    }

    private void RCData(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rcdata-state>

        if (c is '&')
        {
            _returnState = TokenizerState.RCData;
            _state = TokenizerState.CharacterReference;
        }
        else if (c is '<')
        {
            _state = TokenizerState.RCDataLessThanSign;
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            EmitReplacementCharacterToken();
        }
        else if (c is EOF)
        {
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(c);
        }
    }

    private void RawText(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rawtext-state>

        if (c is '<')
        {
            _state = TokenizerState.RawTextLessThanSign;
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            EmitReplacementCharacterToken();
        }
        else if (c is EOF)
        {
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(c);
        }
    }

    private void ScriptData(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-state>

        if (c is '<')
        {
            _state = TokenizerState.ScriptDataLessThanSign;
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            EmitReplacementCharacterToken();
        }
        else if (c is EOF)
        {
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(c);
        }
    }

    private void Plaintext(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#plaintext-state>

        if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            EmitReplacementCharacterToken();
        }
        else if (c is EOF)
        {
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(c);
        }
    }

    private void TagOpen(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#tag-open-state>

        if (c is '!')
        {
            _state = TokenizerState.MarkupDeclarationOpen;
        }
        else if (c is '/')
        {
            _state = TokenizerState.EndTagOpen;
        }
        else if (IsAsciiAlpha(c))
        {
            _currentTag = Tag.NewStartTag();
            Reconsume(TokenizerState.TagName, c);
        }
        else if (c is '?')
        {
            ReportParseError(ParseError.UnexpectedQuestionMarkInsteadOfTagName);
            _currentComment = new();
            Reconsume(TokenizerState.BogusComment, c);
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofBeforeTagName);
            EmitCharacterToken('<');
            EmitEndOfFileToken();
        }
        else
        {
            ReportParseError(ParseError.InvalidFirstCharacterOfTagName);
            EmitCharacterToken('<');
            Reconsume(TokenizerState.Data, c);
        }
    }

    private void EndTagOpen(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#end-tag-open-state>

        if (IsAsciiAlpha(c))
        {
            _currentTag = Tag.NewEndTag();
            Reconsume(TokenizerState.TagName, c);
        }
        else if (c is '>')
        {
            ReportParseError(ParseError.MissingEndTagName);
            _state = TokenizerState.Data;
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofBeforeTagName);
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitEndOfFileToken();
        }
        else
        {
            ReportParseError(ParseError.InvalidFirstCharacterOfTagName);
            _currentComment = new();
            Reconsume(TokenizerState.BogusComment, c);
        }
    }

    private void TagName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#tag-name-state>

        if (IsSpecialWhitespace(c))
        {
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (c is '/')
        {
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (c is '>')
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(c))
        {
            _currentTag!.AppendName(ToAsciiLowercase(c));
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _currentTag!.AppendName(REPLACEMENT_CHARACTER);
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInTag);
            EmitEndOfFileToken();
        }
        else
        {
            _currentTag!.AppendName(c);
        }
    }

    private void RCDataLessThanSign(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rcdata-less-than-sign-state>

        if (c is '/')
        {
            _tempBuffer.Clear();
            _state = TokenizerState.RCDataEndTagOpen;
        }
        else
        {
            EmitCharacterToken('<');
            Reconsume(TokenizerState.RCData, c);
        }
    }

    private void RCDataEndTagOpen(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rcdata-end-tag-open-state>

        if (IsAsciiAlpha(c))
        {
            _currentTag = Tag.NewEndTag();
            Reconsume(TokenizerState.RCDataEndTagName, c);
        }
    }

    private void RCDataEndTagName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rcdata-end-tag-name-state>

        if (IsSpecialWhitespace(c) && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (c is '/' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (c is '>' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(c))
        {
            _currentTag!.AppendName(ToAsciiLowercase(c));
            _tempBuffer.Add(c);
        }
        else if (IsAsciiLowerAlpha(c))
        {
            _currentTag!.AppendName(c);
            _tempBuffer.Add(c);
        }
        else
        {
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitCharacterTokensFromTemporaryBuffer();
            Reconsume(TokenizerState.RCData, c);
        }
    }

    private void RawTextLessThanSign(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rawtext-less-than-sign-state>

        if (c is '/')
        {
            _tempBuffer.Clear();
            _state = TokenizerState.RawTextEndTagOpen;
        }
        else
        {
            EmitCharacterToken('<');
            Reconsume(TokenizerState.RawText, c);
        }
    }

    private void RawTextEndTagOpen(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rawtext-end-tag-open-state>

        if (IsAsciiAlpha(c))
        {
            _currentTag = Tag.NewEndTag();
            Reconsume(TokenizerState.RawTextEndTagName, c);
        }
        else
        {
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            Reconsume(TokenizerState.RawText, c);
        }
    }

    private void RawTextEndTagName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#rawtext-end-tag-name-state>

        if (IsSpecialWhitespace(c) && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (c is '/' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (c is '>' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(c))
        {
            _currentTag!.AppendName(ToAsciiLowercase(c));
            _tempBuffer.Add(c);
        }
        else if (IsAsciiLowerAlpha(c))
        {
            _currentTag!.AppendName(c);
            _tempBuffer.Add(c);
        }
        else
        {
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitCharacterTokensFromTemporaryBuffer();
            Reconsume(TokenizerState.RawText, c);
        }
    }

    private void ScriptDataLessThanSign(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-less-than-sign-state>

        if (c is '/')
        {
            _tempBuffer.Clear();
            _state = TokenizerState.ScriptDataEndTagOpen;
        }
        else if (c is '!')
        {
            _state = TokenizerState.ScriptDataEscapeStart;
            EmitCharacterToken('<');
            EmitCharacterToken('!');
        }
        else
        {
            EmitCharacterToken('<');
            Reconsume(TokenizerState.ScriptData, c);
        }
    }

    private void ScriptDataEndTagOpen(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-end-tag-open-state>

        if (IsAsciiAlpha(c))
        {
            _currentTag = Tag.NewEndTag();
            Reconsume(TokenizerState.ScriptDataEndTagName, c);
        }
        else
        {
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            Reconsume(TokenizerState.ScriptData, c);
        }
    }

    private void ScriptDataEndTagName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-end-tag-name-state>

        if (IsSpecialWhitespace(c) && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (c is '/' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (c is '>' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(c))
        {
            _currentTag!.AppendName(ToAsciiLowercase(c));
            _tempBuffer.Add(c);
        }
        else if (IsAsciiLowerAlpha(c))
        {
            _currentTag!.AppendName(c);
            _tempBuffer.Add(c);
        }
        else
        {
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitCharacterTokensFromTemporaryBuffer();
            Reconsume(TokenizerState.ScriptData, c);
        }
    }

    private void ScriptDataEscapeStart(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escape-start-state>

        if (c is '-')
        {
            _state = TokenizerState.ScriptDataEscapeStartDash;
            EmitCharacterToken('-');
        }
        else
        {
            Reconsume(TokenizerState.ScriptData, c);
        }
    }

    private void ScriptDataEscapeStartDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escape-start-dash-state>

        if (c is '-')
        {
            _state = TokenizerState.ScriptDataEscapedDashDash;
            EmitCharacterToken('-');
        }
        else
        {
            Reconsume(TokenizerState.ScriptData, c);
        }
    }

    private void ScriptDataEscaped(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-state>

        if (c is '-')
        {
            _state = TokenizerState.ScriptDataEscapedDash;
            EmitCharacterToken('-');
        }
        else if (c is '<')
        {
            _state = TokenizerState.ScriptDataEscapedLessThanSign;
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            EmitReplacementCharacterToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInScriptHtmlCommentLikeText);
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(c);
        }
    }

    private void ScriptDataEscapedDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-dash-state>

        if (c is '-')
        {
            _state = TokenizerState.ScriptDataEscapedDashDash;
            EmitCharacterToken('-');
        }
        else if (c is '<')
        {
            _state = TokenizerState.ScriptDataEscapedLessThanSign;
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _state = TokenizerState.ScriptDataEscaped;
            EmitReplacementCharacterToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInScriptHtmlCommentLikeText);
            EmitEndOfFileToken();
        }
        else
        {
            _state = TokenizerState.ScriptDataEscaped;
            EmitCharacterToken(c);
        }
    }

    private void ScriptDataEscapedDashDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-dash-dash-state>

        if (c is '-')
        {
            EmitCharacterToken('-');
        }
        else if (c is '<')
        {
            _state = TokenizerState.ScriptDataEscapedLessThanSign;
        }
        else if (c is '>')
        {
            _state = TokenizerState.ScriptData;
            EmitCharacterToken('>');
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _state = TokenizerState.ScriptDataEscaped;
            EmitReplacementCharacterToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInScriptHtmlCommentLikeText);
            EmitEndOfFileToken();
        }
        else
        {
            _state = TokenizerState.ScriptDataEscaped;
            EmitCharacterToken(c);
        }
    }

    private void ScriptDataEscapedLessThanSign(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-less-than-sign-state>

        if (c is '/')
        {
            _tempBuffer.Clear();
            _state = TokenizerState.ScriptDataEscapedEndTagOpen;
        }
        else if (IsAsciiAlpha(c))
        {
            _tempBuffer.Clear();
            EmitCharacterToken('<');
            Reconsume(TokenizerState.ScriptDataDoubleEscapeStart, c);
        }
        else
        {
            EmitCharacterToken('<');
            Reconsume(TokenizerState.ScriptDataEscaped, c);
        }
    }

    private void ScriptDataEscapedEndTagOpen(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-end-tag-open-state>

        if (IsAsciiAlpha(c))
        {
            _currentTag = Tag.NewEndTag();
            Reconsume(TokenizerState.ScriptDataEscapedEndTagName, c);
        }
        else
        {
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            Reconsume(TokenizerState.ScriptDataEscaped, c);
        }
    }

    private void ScriptDataEscapedEndTagName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-escaped-end-tag-name-state>

        if (IsSpecialWhitespace(c) && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (c is '/' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (c is '>' && IsCurrentEndTagAnAppropriateOne())
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (IsAsciiUpperAlpha(c))
        {
            _currentTag!.AppendName(ToAsciiLowercase(c));
            _tempBuffer.Add(c);
        }
        else if (IsAsciiLowerAlpha(c))
        {
            _currentTag!.AppendName(c);
            _tempBuffer.Add(c);
        }
        else
        {
            EmitCharacterToken('<');
            EmitCharacterToken('/');
            EmitCharacterTokensFromTemporaryBuffer();
            Reconsume(TokenizerState.ScriptDataEscaped, c);
        }
    }

    private void ScriptDataDoubleEscapeStart(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escape-start-state>

        if (IsSpecialWhitespace(c) || c is '/' or '>')
        {
            if (CompareTemporaryBuffer("script"))
            {
                _state = TokenizerState.ScriptDataDoubleEscaped;
            }
            else
            {
                _state = TokenizerState.ScriptDataEscaped;
                EmitCharacterToken(c);
            }
        }
        else if (IsAsciiUpperAlpha(c))
        {
            _tempBuffer.Add(ToAsciiLowercase(c));
            EmitCharacterToken(c);
        }
        else if (IsAsciiLowerAlpha(c))
        {
            _tempBuffer.Add(c);
            EmitCharacterToken(c);
        }
        else
        {
            Reconsume(TokenizerState.ScriptDataEscaped, c);
        }
    }

    private void ScriptDataDoubleEscaped(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escaped-state>

        if (c is '-')
        {
            _state = TokenizerState.ScriptDataDoubleEscapedDash;
            EmitCharacterToken('-');
        }
        else if (c is '<')
        {
            _state = TokenizerState.ScriptDataDoubleEscapedLessThanSign;
            EmitCharacterToken('<');
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            EmitReplacementCharacterToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInScriptHtmlCommentLikeText);
            EmitEndOfFileToken();
        }
        else
        {
            EmitCharacterToken(c);
        }
    }

    private void ScriptDataDoubleEscapedDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escaped-dash-state>

        if (c is '-')
        {
            _state = TokenizerState.ScriptDataDoubleEscapedDashDash;
            EmitCharacterToken('-');
        }
        else if (c is '<')
        {
            _state = TokenizerState.ScriptDataDoubleEscapedLessThanSign;
            EmitCharacterToken('<');
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _state = TokenizerState.ScriptDataDoubleEscaped;
            EmitReplacementCharacterToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInScriptHtmlCommentLikeText);
            EmitEndOfFileToken();
        }
        else
        {
            _state = TokenizerState.ScriptDataDoubleEscaped;
            EmitCharacterToken(c);
        }
    }

    private void ScriptDataDoubleEscapedDashDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escaped-dash-dash-state>

        if (c is '-')
        {
            EmitCharacterToken('-');
        }
        else if (c is '<')
        {
            _state = TokenizerState.ScriptDataDoubleEscapedLessThanSign;
            EmitCharacterToken('<');
        }
        else if (c is '>')
        {
            _state = TokenizerState.ScriptData;
            EmitCharacterToken('>');
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _state = TokenizerState.ScriptDataDoubleEscaped;
            EmitReplacementCharacterToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInScriptHtmlCommentLikeText);
            EmitEndOfFileToken();
        }
        else
        {
            _state = TokenizerState.ScriptDataDoubleEscaped;
            EmitCharacterToken(c);
        }
    }

    private void ScriptDataDoubleEscapedLessThanSign(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escaped-less-than-sign-state>

        if (c is '/')
        {
            _tempBuffer.Clear();
            _state = TokenizerState.ScriptDataDoubleEscapeEnd;
            EmitCharacterToken('/');
        }
        else
        {
            Reconsume(TokenizerState.ScriptDataDoubleEscaped, c);
        }
    }

    private void ScriptDataDoubleEscapeEnd(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#script-data-double-escape-end-state>

        if (IsSpecialWhitespace(c) || c is '/' or '>')
        {
            if (CompareTemporaryBuffer("script"))
            {
                _state = TokenizerState.ScriptDataEscaped;
            }
            else
            {
                _state = TokenizerState.ScriptDataDoubleEscaped;
                EmitCharacterToken(c);
            }
        }
        else if (IsAsciiUpperAlpha(c))
        {
            _tempBuffer.Add(ToAsciiLowercase(c));
            EmitCharacterToken(c);
        }
        else if (IsAsciiLowerAlpha(c))
        {
            _tempBuffer.Add(c);
            EmitCharacterToken(c);
        }
        else
        {
            Reconsume(TokenizerState.ScriptDataDoubleEscaped, c);
        }
    }

    private void BeforeAttributeName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-attribute-name-state>

        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c is '/' or '>' or -1)
        {
            Reconsume(TokenizerState.AfterAttributeName, c);
        }
        else if (c is '=')
        {
            ReportParseError(ParseError.UnexpectedEqualsSignBeforeAttributeName);
            _currentAttribute = _currentTag!.NewAttribute();
            _currentAttribute.AppendName('=');
            _state = TokenizerState.AttributeName;
        }
        else
        {
            _currentAttribute = _currentTag!.NewAttribute();
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

        if (IsSpecialWhitespace(c) || c is '/' or '>' or -1)
        {
            if (_currentTag!.CheckAndCorrectDuplicateAttributes())
                ReportParseError(ParseError.DuplicateAttribute); // see block comment above
            Reconsume(TokenizerState.AfterAttributeName, c);
        }
        else if (c is '=')
        {
            if (_currentTag!.CheckAndCorrectDuplicateAttributes())
                ReportParseError(ParseError.DuplicateAttribute); // see block comment above
            _state = TokenizerState.BeforeAttributeValue;
        }
        else if (IsAsciiUpperAlpha(c))
        {
            _currentAttribute!.AppendName(ToAsciiLowercase(c));
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _currentAttribute!.AppendName(REPLACEMENT_CHARACTER);
        }
        else
        {
            if (c is '"' or '\'' or '<')
            {
                ReportParseError(ParseError.UnexpectedCharacterInAttributeName);
            }
            _currentAttribute!.AppendName(c);
        }
    }

    private void AfterAttributeName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-attribute-name-state>

        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c is '/')
        {
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (c is '=')
        {
            _state = TokenizerState.BeforeAttributeValue;
        }
        else if (c is '>')
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInTag);
            EmitEndOfFileToken();
        }
        else
        {
            _currentAttribute = _currentTag!.NewAttribute();
            Reconsume(TokenizerState.AttributeName, c);
        }
    }

    private void BeforeAttributeValue(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-attribute-value-state>

        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c is '"')
        {
            _state = TokenizerState.AttributeValueDoubleQuoted;
        }
        else if (c is '\'')
        {
            _state = TokenizerState.AttributeValueSingleQuoted;
        }
        else if (c is '>')
        {
            ReportParseError(ParseError.MissingAttributeValue);
            _state = TokenizerState.Data;
            EmitTagToken();
        }
    }

    private void AttributeValueDoubleQuoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#attribute-value-(double-quoted)-state>

        if (c is '"')
        {
            _state = TokenizerState.AfterAttributeValueQuoted;
        }
        else if (c is '&')
        {
            _returnState = TokenizerState.AttributeValueDoubleQuoted;
            _state = TokenizerState.CharacterReference;
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _currentAttribute!.AppendValue(REPLACEMENT_CHARACTER);
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInTag);
            EmitEndOfFileToken();
        }
        else
        {
            _currentAttribute!.AppendValue(c);
        }
    }

    private void AttributeValueSingleQuoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#attribute-value-(single-quoted)-state>

        if (c is '\'')
        {
            _state = TokenizerState.AfterAttributeValueQuoted;
        }
        else if (c is '&')
        {
            _returnState = TokenizerState.AttributeValueSingleQuoted;
            _state = TokenizerState.CharacterReference;
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _currentAttribute!.AppendValue(REPLACEMENT_CHARACTER);
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInTag);
            EmitEndOfFileToken();
        }
        else
        {
            _currentAttribute!.AppendValue(c);
        }
    }

    private void AttributeValueUnquoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#attribute-value-(unquoted)-state>

        if (IsSpecialWhitespace(c))
        {
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (c is '&')
        {
            _returnState = TokenizerState.AttributeValueUnquoted;
            _state = TokenizerState.CharacterReference;
        }
        else if (c is '>')
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _currentAttribute!.AppendValue(REPLACEMENT_CHARACTER);
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInTag);
            EmitEndOfFileToken();
        }
        else
        {
            if (c is '"' or '\'' or '<' or '=' or '`')
            {
                ReportParseError(ParseError.UnexpectedCharacterInUnquotedAttributeValue);
            }
            _currentAttribute!.AppendValue(c);
        }
    }

    private void AfterAttributeValueQuoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-attribute-value-(quoted)-state>

        if (IsSpecialWhitespace(c))
        {
            _state = TokenizerState.BeforeAttributeName;
        }
        else if (c is '/')
        {
            _state = TokenizerState.SelfClosingStartTag;
        }
        else if (c is '>')
        {
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInTag);
            EmitEndOfFileToken();
        }
        else
        {
            ReportParseError(ParseError.MissingWhitespaceBetweenAttributes);
            Reconsume(TokenizerState.BeforeAttributeName, c);
        }
    }

    private void SelfClosingStartTag(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#self-closing-start-tag-state>

        if (c is '>')
        {
            _currentTag!.SetSelfClosingFlag();
            _state = TokenizerState.Data;
            EmitTagToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInTag);
            EmitEndOfFileToken();
        }
        else
        {
            ReportParseError(ParseError.UnexpectedSolidusInTag);
            Reconsume(TokenizerState.BeforeAttributeName, c);
        }
    }

    private void BogusComment(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#bogus-comment-state>

        if (c is '>')
        {
            _state = TokenizerState.Data;
            EmitCommentToken();
        }
        else if (c is EOF)
        {
            EmitCommentToken();
            EmitEndOfFileToken();
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _currentComment!.Append(REPLACEMENT_CHARACTER);
        }
        else
        {
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

        if (c is '-')
        {
            _state = TokenizerState.CommentStartDash;
        }
        else if (c is '>')
        {
            ReportParseError(ParseError.AbruptClosingOfEmptyComment);
            _state = TokenizerState.Data;
            EmitCommentToken();
        }
        else
        {
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void CommentStartDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-start-dash-state>

        if (c is '-')
        {
            _state = TokenizerState.CommentEnd;
        }
        else if (c is '>')
        {
            ReportParseError(ParseError.AbruptClosingOfEmptyComment);
            _state = TokenizerState.Data;
            EmitCommentToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInComment);
            EmitCommentToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentComment!.Append('-');
            _currentComment.Append('-');
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void Comment(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-state>

        if (c is '<')
        {
            _currentComment!.Append('<');
            _state = TokenizerState.CommentLessThanSign;
        }
        else if (c is '-')
        {
            _state = TokenizerState.CommentEndDash;
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _currentComment!.Append(REPLACEMENT_CHARACTER);
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInComment);
            EmitCommentToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentComment!.Append(c);
        }
    }

    private void CommentLessThanSign(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-less-than-sign-state>

        if (c is '!')
        {
            _currentComment!.Append('!');
            _state = TokenizerState.CommentLessThanSignBang;
        }
        else if (c is '<')
        {
            _currentComment!.Append('<');
        }
        else
        {
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void CommentLessThanSignBang(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-less-than-sign-bang-state>

        if (c is '-')
        {
            _state = TokenizerState.CommentLessThanSignBangDash;
        }
        else
        {
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void CommentLessThanSignBangDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-less-than-sign-bang-dash-state>

        if (c is '-')
        {
            _state = TokenizerState.CommentLessThanSignBangDashDash;
        }
        else
        {
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void CommentLessThanSignBangDashDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-less-than-sign-bang-dash-dash-state>

        if (c is '>' or -1)
        {
            Reconsume(TokenizerState.CommentEnd, c);
        }
        else
        {
            ReportParseError(ParseError.NestedComment);
            Reconsume(TokenizerState.CommentEnd, c);
        }
    }

    private void CommentEndDash(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-end-dash-state>

        if (c is '-')
        {
            _state = TokenizerState.CommentEnd;
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInComment);
            EmitCommentToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentComment!.Append('-');
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void CommentEnd(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-end-state>

        if (c is '>')
        {
            _state = TokenizerState.Data;
            EmitCommentToken();
        }
        else if (c is '!')
        {
            _state = TokenizerState.CommentEndBang;
        }
        else if (c is '-')
        {
            _currentComment!.Append('-');
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInComment);
            EmitCommentToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentComment!.Append('-');
            _currentComment.Append('-');
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void CommentEndBang(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#comment-end-bang-state>

        if (c is '-')
        {
            _currentComment!.Append('-');
            _currentComment.Append('-');
            _currentComment.Append('!');
            _state = TokenizerState.CommentEndDash;
        }
        else if (c is '>')
        {
            ReportParseError(ParseError.IncorrectlyClosedComment);
            _state = TokenizerState.Data;
            EmitCommentToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInComment);
            EmitCommentToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentComment!.Append('-');
            _currentComment.Append('-');
            _currentComment.Append('!');
            Reconsume(TokenizerState.Comment, c);
        }
    }

    private void Doctype(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-state>

        if (IsSpecialWhitespace(c))
        {
            _state = TokenizerState.BeforeDoctypeName;
        }
        else if (c is '>')
        {
            Reconsume(TokenizerState.BeforeDoctypeName, c);
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
            _currentDoctype = new();
            _currentDoctype.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            ReportParseError(ParseError.MissingWhitespaceBeforeDoctypeName);
            Reconsume(TokenizerState.BeforeDoctypeName, c);
        }
    }

    private void BeforeDoctypeName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-doctype-name-state>

        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (IsAsciiUpperAlpha(c))
        {
            _currentDoctype = new();
            _currentDoctype.AppendName(ToAsciiLowercase(c));
            _state = TokenizerState.DoctypeName;
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _currentDoctype = new();
            _currentDoctype.AppendName(REPLACEMENT_CHARACTER);
            _state = TokenizerState.DoctypeName;
        }
        else if (c is '>')
        {
            ReportParseError(ParseError.MissingDoctypeName);
            _currentDoctype = new();
            _currentDoctype.SetQuirksFlag();
            EmitDoctypeToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
            _currentDoctype = new();
            _currentDoctype.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentDoctype = new();
            _currentDoctype.AppendName(c);
            _state = TokenizerState.DoctypeName;
        }
    }

    private void DoctypeName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-name-state>

        if (IsSpecialWhitespace(c))
        {
            _state = TokenizerState.AfterDoctypeName;
        }
        else if (c is '>')
        {
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (IsAsciiUpperAlpha(c))
        {
            _currentDoctype!.AppendName(ToAsciiLowercase(c));
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _currentDoctype!.AppendName(REPLACEMENT_CHARACTER);
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
            _currentDoctype = new();
            _currentDoctype.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentDoctype!.AppendName(c);
        }
    }

    private void AfterDoctypeName(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-name-state>

        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c is '>')
        {
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
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

    private void AfterDoctypePublicKeyword(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-public-keyword-state>

        if (IsSpecialWhitespace(c))
        {
            _state = TokenizerState.BeforeDoctypePublicIdentifier;
        }
        else if (c is '"')
        {
            ReportParseError(ParseError.MissingWhitespaceAfterDoctypePublicKeyword);
            _currentDoctype!.SetPublicIdentifierToEmptyString();
            _state = TokenizerState.DoctypePublicIdentifierDoubleQuoted;
        }
        else if (c is '\'')
        {
            ReportParseError(ParseError.MissingWhitespaceAfterDoctypePublicKeyword);
            _currentDoctype!.SetPublicIdentifierToEmptyString();
            _state = TokenizerState.DoctypePublicIdentifierSingleQuoted;
        }
        else if (c is '>')
        {
            ReportParseError(ParseError.MissingDoctypePublicIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            ReportParseError(ParseError.MissingQuoteBeforeDoctypePublicIdentifier);
            _currentDoctype!.SetQuirksFlag();
            Reconsume(TokenizerState.BogusDoctype, c);
        }
    }

    private void BeforeDoctypePublicIdentifier(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-doctype-public-identifier-state>

        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c is '"')
        {
            _currentDoctype!.SetPublicIdentifierToEmptyString();
            _state = TokenizerState.DoctypePublicIdentifierDoubleQuoted;
        }
        else if (c is '\'')
        {
            _currentDoctype!.SetPublicIdentifierToEmptyString();
            _state = TokenizerState.DoctypePublicIdentifierSingleQuoted;
        }
        else if (c is '>')
        {
            ReportParseError(ParseError.MissingDoctypePublicIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            ReportParseError(ParseError.MissingQuoteBeforeDoctypePublicIdentifier);
            _currentDoctype!.SetQuirksFlag();
            Reconsume(TokenizerState.BogusDoctype, c);
        }
    }

    private void DoctypePublicIdentifierDoubleQuoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-public-identifier-(double-quoted)-state>

        if (c is '"')
        {
            _state = TokenizerState.AfterDoctypePublicIdentifier;
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _currentDoctype!.AppendPublicIdentifier(REPLACEMENT_CHARACTER);
        }
        else if (c is '>')
        {
            ReportParseError(ParseError.AbruptDoctypePublicIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentDoctype!.AppendPublicIdentifier(c);
        }
    }

    private void DoctypePublicIdentifierSingleQuoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-public-identifier-(single-quoted)-state>

        if (c is '\'')
        {
            _state = TokenizerState.AfterDoctypePublicIdentifier;
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _currentDoctype!.AppendPublicIdentifier(REPLACEMENT_CHARACTER);
        }
        else if (c is '>')
        {
            ReportParseError(ParseError.AbruptDoctypePublicIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentDoctype!.AppendPublicIdentifier(c);
        }
    }

    private void AfterDoctypePublicIdentifier(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-public-identifier-state>

        if (IsSpecialWhitespace(c))
        {
            _state = TokenizerState.BetweenDoctypePublicAndSystemIdentifiers;
        }
        else if (c is '>')
        {
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (c is '"')
        {
            ReportParseError(ParseError.MissingWhitespaceBetweenDoctypePublicAndSystemIdentifiers);
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
        }
        else if (c is '\'')
        {
            ReportParseError(ParseError.MissingWhitespaceBetweenDoctypePublicAndSystemIdentifiers);
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            ReportParseError(ParseError.MissingQuoteBeforeDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            Reconsume(TokenizerState.BogusDoctype, c);
        }
    }

    private void BetweenDoctypePublicAndSystemIdentifiers(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#between-doctype-public-and-system-identifiers-state>

        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c is '>')
        {
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (c is '"')
        {
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
        }
        else if (c is '\'')
        {
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            ReportParseError(ParseError.MissingQuoteBeforeDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            Reconsume(TokenizerState.BogusDoctype, c);
        }
    }

    private void AfterDoctypeSystemKeyword(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-system-keyword-state>

        if (IsSpecialWhitespace(c))
        {
            _state = TokenizerState.BeforeDoctypeSystemIdentifier;
        }
        else if (c is '"')
        {
            ReportParseError(ParseError.MissingWhitespaceAfterDoctypeSystemKeyword);
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
        }
        else if (c is '\'')
        {
            ReportParseError(ParseError.MissingWhitespaceAfterDoctypeSystemKeyword);
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
        }
        else if (c is '>')
        {
            ReportParseError(ParseError.MissingDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            ReportParseError(ParseError.MissingQuoteBeforeDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            Reconsume(TokenizerState.BogusDoctype, c);
        }
    }

    private void BeforeDoctypeSystemIdentifier(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#before-doctype-system-identifier-state>

        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c is '"')
        {
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
        }
        else if (c is '\'')
        {
            _currentDoctype!.SetSystemIdentifierToEmptyString();
            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
        }
        else if (c is '>')
        {
            ReportParseError(ParseError.MissingDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            ReportParseError(ParseError.MissingQuoteBeforeDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            Reconsume(TokenizerState.BogusDoctype, c);
        }
    }

    private void DoctypeSystemIdentifierDoubleQuoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-system-identifier-(double-quoted)-state>

        if (c is '"')
        {
            _state = TokenizerState.AfterDoctypeSystemIdentifier;
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _currentDoctype!.AppendSystemIdentifier(REPLACEMENT_CHARACTER);
        }
        else if (c is '>')
        {
            ReportParseError(ParseError.AbruptDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentDoctype!.AppendSystemIdentifier(c);
        }
    }

    private void DoctypeSystemIdentifierSingleQuoted(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#doctype-system-identifier-(single-quoted)-state>

        if (c is '\'')
        {
            _state = TokenizerState.AfterDoctypeSystemIdentifier;
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            _currentDoctype!.AppendSystemIdentifier(REPLACEMENT_CHARACTER);
        }
        else if (c is '>')
        {
            ReportParseError(ParseError.AbruptDoctypeSystemIdentifier);
            _currentDoctype!.SetQuirksFlag();
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            _currentDoctype!.AppendSystemIdentifier(c);
        }
    }

    private void AfterDoctypeSystemIdentifier(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#after-doctype-system-identifier-state>

        if (IsSpecialWhitespace(c))
        {
            // Ignore the character.
        }
        else if (c is '>')
        {
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInDoctype);
            _currentDoctype!.SetQuirksFlag();
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
        else
        {
            ReportParseError(ParseError.UnexpectedCharacterAfterDoctypeSystemIdentifier);
            Reconsume(TokenizerState.BogusDoctype, c);
        }
    }

    private void BogusDoctype(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#bogus-doctype-state>

        if (c is '>')
        {
            _state = TokenizerState.Data;
            EmitDoctypeToken();
        }
        else if (c is '\0')
        {
            ReportParseError(ParseError.UnexpectedNullCharacter);
            // Ignore the character.
        }
        else if (c is EOF)
        {
            EmitDoctypeToken();
            EmitEndOfFileToken();
        }
    }

    private void CDataSection(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#cdata-section-state>

        if (c is ']')
        {
            _state = TokenizerState.CDataSectionBracket;
        }
        else if (c is EOF)
        {
            ReportParseError(ParseError.EofInCData);
            EmitEndOfFileToken();
        }
        else
        {
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

        if (c is ']')
        {
            _state = TokenizerState.CDataSectionEnd;
        }
        else
        {
            EmitCharacterToken(']');
            Reconsume(TokenizerState.CDataSection, c);
        }
    }

    private void CDataSectionEnd(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#cdata-section-end-state>

        if (c is ']')
        {
            EmitCharacterToken(']');
        }
        else if (c is '>')
        {
            _state = TokenizerState.Data;
        }
        else
        {
            EmitCharacterToken(']');
            EmitCharacterToken(']');
            Reconsume(TokenizerState.CDataSection, c);
        }
    }

    private void CharacterReference(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#character-reference-state>

        _tempBuffer.Clear();
        _tempBuffer.Add('&');
        if (IsAsciiAlphanumeric(c))
        {
            Reconsume(TokenizerState.NamedCharacterReference, c);
        }
        else if (c is '#')
        {
            _tempBuffer.Add('#');
            _state = TokenizerState.NumericCharacterReference;
        }
        else
        {
            FlushCodePointsConsumedAsCharacterReference();
            Reconsume(_returnState!.Value, c);
        }
    }

    // ReSharper disable once MemberCanBeMadeStatic.Local
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

        if (IsAsciiAlphanumeric(c))
        {
            if (WasConsumedAsPartOfAnAttribute())
                _currentAttribute!.AppendValue(c);
            else
                EmitCharacterToken(c);
        }
        else if (c is ';')
        {
            ReportParseError(ParseError.UnknownNamedCharacterReference);
            Reconsume(_returnState!.Value, c);
        }
        else
        {
            Reconsume(_returnState!.Value, c);
        }
    }

    private void NumericCharacterReference(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#numeric-character-reference-state>

        _charRefCode = 0;
        if (c is 'x' or 'X')
        {
            _tempBuffer.Add(c);
            _state = TokenizerState.HexadecimalCharacterReferenceStart;
        }
        else
        {
            Reconsume(TokenizerState.DecimalCharacterReferenceStart, c);
        }
    }

    private void HexadecimalCharacterReferenceStart(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#hexadecimal-character-reference-start-state>

        if (IsAsciiHexDigit(c))
        {
            Reconsume(TokenizerState.HexadecimalCharacterReference, c);
        }
        else
        {
            ReportParseError(ParseError.AbsenseOfDigitsInNumericCharacterReference);
            FlushCodePointsConsumedAsCharacterReference();
            Reconsume(_returnState!.Value, c);
        }
    }

    private void DecimalCharacterReferenceStart(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#decimal-character-reference-start-state>

        if (IsAsciiDigit(c))
        {
            Reconsume(TokenizerState.DecimalCharacterReference, c);
        }
        else
        {
            ReportParseError(ParseError.AbsenseOfDigitsInNumericCharacterReference);
            FlushCodePointsConsumedAsCharacterReference();
            Reconsume(_returnState!.Value, c);
        }
    }

    private void HexadecimalCharacterReference(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#hexadecimal-character-reference-state>

        if (IsAsciiDigit(c))
        {
            _charRefCode *= 16;
            _charRefCode += c - '0';
        }
        else if (IsAsciiUpperHexDigit(c))
        {
            _charRefCode *= 16;
            _charRefCode += c - 'A' + 10;
        }
        else if (IsAsciiLowerHexDigit(c))
        {
            _charRefCode *= 16;
            _charRefCode += c - 'a' + 10;
        }
        else if (c is ';')
        {
            _state = TokenizerState.NumericCharacterReferenceEnd;
        }
        else
        {
            ReportParseError(ParseError.MissingSemicolonAfterCharacterReference);
            Reconsume(TokenizerState.NumericCharacterReferenceEnd, c);
        }
    }

    private void DecimalCharacterReference(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#decimal-character-reference-state>

        if (IsAsciiDigit(c))
        {
            _charRefCode *= 10;
            _charRefCode += c - '0';
        }
        else if (c is ';')
        {
            _state = TokenizerState.NumericCharacterReferenceEnd;
        }
        else
        {
            ReportParseError(ParseError.MissingSemicolonAfterCharacterReference);
            Reconsume(TokenizerState.NumericCharacterReferenceEnd, c);
        }
    }

    private void NumericCharacterReferenceEnd(int c)
    {
        // <https://html.spec.whatwg.org/multipage/parsing.html#numeric-character-reference-end-state>

        PutBack(c); // we don't need it

        if (_charRefCode is 0)
        {
            ReportParseError(ParseError.NullCharacterReference);
            _charRefCode = REPLACEMENT_CHARACTER;
        }
        else if (_charRefCode > 0x10FFFF)
        {
            ReportParseError(ParseError.CharacterReferenceOutsideUnicodeRange);
            _charRefCode = REPLACEMENT_CHARACTER;
        }
        else if (IsSurrogate(_charRefCode))
        {
            ReportParseError(ParseError.SurrogateCharacterReference);
            _charRefCode = REPLACEMENT_CHARACTER;
        }
        else if (IsNoncharacter(_charRefCode))
        {
            ReportParseError(ParseError.NoncharacterCharacterReference);
        }
        else if (_charRefCode is '\r' || (IsControl(_charRefCode) && !IsAsciiWhitespace(_charRefCode)))
        {
            ReportParseError(ParseError.ControlCharacterReference);
        }
        else if (CharReference.NumericList.TryGetValue(_charRefCode, out char converted))
        {
            _charRefCode = converted;
        }

        _tempBuffer.Clear();
        _tempBuffer.Add(_charRefCode);
        FlushCodePointsConsumedAsCharacterReference();
        _state = _returnState!.Value;
        _returnState = null;
    }
}
