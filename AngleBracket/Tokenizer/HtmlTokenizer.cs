/* =============================================================================
 * File:   HtmlTokenizer.cs
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace AngleBracket.Tokenizer;

/// <summary>
/// An HTML tokenizer complying with the WHATWG HTML standard.
/// </summary>
public partial class HtmlTokenizer : IDisposable
{
    private const int REPLACEMENT_CHARACTER = 0xFFFD;
    private const int EOF = -1;

    private Action<int>[]? _stateMap = null;
    private readonly Queue<Token> _tokensToEmit = new();
    private readonly List<int> _peekBuffer = new();

    private TokenizerState _state = TokenizerState.Data;
    private TokenizerState? _returnState = null;
    private readonly List<int> _tempBuffer = new();
    private int _charRefCode = 0;

    private Attribute? _currentAttribute = null;
    private StringBuilder? _currentComment = null;
    private Doctype? _currentDoctype = null;
    private Tag? _currentTag = null;

    private readonly TextReader _input;
    private int _inputOffset = 0;
    private readonly Action<ParseErrorInfo>? _errorReporter;


    /// <summary>Construct a new HTML tokenizer with a specified <see cref="TextReader" />.</summary>
    /// <param name="input">The <see cref="TextReader" /> to read from.</param>
    public HtmlTokenizer(TextReader input)
        : this(input, null!)
    { }

    /// <summary>Construct a new HTML tokenizer with a specified <see cref="TextReader" />.</summary>
    /// <param name="input">The <see cref="TextReader" /> to read from.</param>
    /// <param name="errorReporter">
    /// The <see cref="Action{T}" /> to call when a <see cref="ParseError" /> is encountered.
    /// </param>
    public HtmlTokenizer(TextReader input, Action<ParseErrorInfo> errorReporter)
    {
        _input = input;
        _errorReporter = errorReporter;
        InitStateMap();
    }

    private int Peek()
    {
        if (_peekBuffer.Any())
            return _peekBuffer[0];

        // normalize out the carriage returns
        // <https://html.spec.whatwg.org/multipage/parsing.html#preprocessing-the-input-stream>
        int c;
        do
        {
            c = _input.Read();
        } while (c is '\r');

        if (c > 0)
            _peekBuffer.Add(c);
        _inputOffset--; // counteract the increment in `Read`
        return c;
    }

    private int Read()
    {
        int c;
        if (_peekBuffer.Any())
        {
            c = _peekBuffer[0];
            _peekBuffer.RemoveAt(0);
            _inputOffset++;
            return c;
        }

        // normalize out the carriage returns
        // <https://html.spec.whatwg.org/multipage/parsing.html#preprocessing-the-input-stream>
        do
        {
            c = _input.Read();
        } while (c is '\r');

        _inputOffset++;
        return c;
    }

    private string Read(int count)
    {
        Span<char> buf = stackalloc char[count];
        int index = 0;
        while (index < count)
        {
            int c = Read();
            if (c is -1)
                break;
            buf[index++] = (char)c;
        }

        return new(buf[..index]);
    }

    private void PutBack(int c)
    {
        Debug.Assert(c > 0); // no EOF
        _inputOffset--;
        _peekBuffer.Add(c);
    }

    private void PutBack(string s)
    {
        Debug.Assert(_inputOffset >= s.Length);
        _inputOffset -= s.Length;
        foreach (char c in s)
            _peekBuffer.Add(c);
    }

    private void ReportParseError(ParseError error) =>
        _errorReporter?.Invoke(new(error, _inputOffset));

    [MemberNotNull(nameof(_stateMap))]
    private void InitStateMap()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_stateMap is not null)
            return;

        _stateMap = new Action<int>[Enum.GetValues<TokenizerState>().Length];
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

    private static bool IsSpecialWhitespace(int c) =>
        c is '\t' or '\n' or '\f' or ' ';
    private static int ToAsciiLowercase(int c) => c + ('a' - 'A');

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private bool IsCurrentEndTagAnAppropriateOne()
    {
        // ReSharper disable once ArrangeMethodOrOperatorBody
        throw new NotImplementedException();
    }

    private bool WasConsumedAsPartOfAnAttribute() =>
        _returnState is TokenizerState.AttributeValueDoubleQuoted
            or TokenizerState.AttributeValueSingleQuoted
            or TokenizerState.AttributeValueUnquoted;

    private void FlushCodePointsConsumedAsCharacterReference()
    {
        if (WasConsumedAsPartOfAnAttribute())
        {
            foreach (int c in _tempBuffer)
                _currentAttribute!.AppendValue(c);
            return;
        }

        foreach (int c in _tempBuffer)
            EmitCharacterToken(c);
    }

    private void EmitCharacterToken(int c) => _tokensToEmit.Enqueue(Token.NewCharacterToken(c));
    private void EmitReplacementCharacterToken() => EmitCharacterToken(REPLACEMENT_CHARACTER);
    private void EmitCharacterTokensFromTemporaryBuffer()
    {
        foreach (int c in _tempBuffer)
            EmitCharacterToken(c);
        _tempBuffer.Clear();
    }
    private void EmitCommentToken()
    {
        Debug.Assert(_currentComment is not null);
        _tokensToEmit.Enqueue(Token.NewCommentToken(_currentComment!.ToString()));
        _currentComment = null;
    }
    private void EmitDoctypeToken()
    {
        Debug.Assert(_currentDoctype is not null);
        _tokensToEmit.Enqueue(Token.NewDoctypeToken(_currentDoctype!));
        _currentDoctype = null;
    }
    private void EmitEndOfFileToken() => _tokensToEmit.Enqueue(Token.NewEndOfFileToken());
    private void EmitTagToken()
    {
        Debug.Assert(_currentTag is not null);
        _tokensToEmit.Enqueue(Token.NewTagToken(_currentTag!));
        _currentAttribute = null;
        _currentTag = null;
    }

    private bool CompareTemporaryBuffer(string compareTo)
    {
        StringBuilder str = new(_tempBuffer.Count);
        foreach (int c in _tempBuffer)
            str.Append(new Rune(c).ToString());
        return str.ToString() == compareTo;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _input.Dispose();
        GC.SuppressFinalize(this);
    }
}
