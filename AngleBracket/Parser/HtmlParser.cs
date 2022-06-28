/* =============================================================================
 * File:   HtmlParser.cs
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

using AngleBracket.Tokenizer;
using CodePoint.IO;
using Attribute = AngleBracket.Tokenizer.Attribute;

namespace AngleBracket.Parser;

public partial class HtmlParser : IDisposable
{
    private readonly HtmlTokenizer _tokenizer;
    private readonly bool _scripting;
    private readonly bool _fragment;
    private Action<Token>[] _stateMap = null!; // SAFETY: initialized in `InitStateMap()`

    private InsertionMode _insertionMode;
    private InsertionMode _originalInsertionMode; // similar to the tokenizer's return state
    private readonly Stack<InsertionMode> _templateInsertionModeStack = new();
    private readonly Stack<HtmlNode> _openElementsStack = new();
    private readonly List<HtmlNode> _activeFormattingElements = new();

    private HtmlNode? _htmlPointer;
    private HtmlNode? _formPointer;

    private bool _framesetOk = true;

    public HtmlParser(RuneReader input, bool scripting)
        : this(input, scripting, false)
    { }
    public HtmlParser(RuneReader input, bool scripting, bool fragment)
    {
        _tokenizer = new(input);
        _scripting = scripting;
        _fragment = fragment;

        InitStateMap();
    }

    private void InitStateMap()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_stateMap is not null)
            return;

        _stateMap = new Action<Token>[Enum.GetValues<InsertionMode>().Length];
        _stateMap[(int)InsertionMode.Initial] = ParseInitial;
        _stateMap[(int)InsertionMode.BeforeHtml] = ParseBeforeHtml;
        _stateMap[(int)InsertionMode.BeforeHead] = ParseBeforeHead;
        _stateMap[(int)InsertionMode.InHead] = ParseInHead;
        _stateMap[(int)InsertionMode.InHeadNoScript] = ParseInHeadNoScript;
        _stateMap[(int)InsertionMode.AfterHead] = ParseAfterHead;
        _stateMap[(int)InsertionMode.InBody] = ParseInBody;
        _stateMap[(int)InsertionMode.Text] = ParseText;
        _stateMap[(int)InsertionMode.InTable] = ParseInTable;
        _stateMap[(int)InsertionMode.InTableText] = ParseInTableText;
        _stateMap[(int)InsertionMode.InCaption] = ParseInCaption;
        _stateMap[(int)InsertionMode.InColumnGroup] = ParseInColumnGroup;
        _stateMap[(int)InsertionMode.InTableBody] = ParseInTableBody;
        _stateMap[(int)InsertionMode.InRow] = ParseInRow;
        _stateMap[(int)InsertionMode.InCell] = ParseInCell;
        _stateMap[(int)InsertionMode.InSelect] = ParseInSelect;
        _stateMap[(int)InsertionMode.InSelectInTable] = ParseInSelectInTable;
        _stateMap[(int)InsertionMode.InTemplate] = ParseInTemplate;
        _stateMap[(int)InsertionMode.AfterBody] = ParseAfterBody;
        _stateMap[(int)InsertionMode.InFrameset] = ParseInFrameset;
        _stateMap[(int)InsertionMode.AfterFrameset] = ParseAfterFrameset;
        _stateMap[(int)InsertionMode.AfterAfterBody] = ParseAfterAfterBody;
        _stateMap[(int)InsertionMode.AfterAfterFrameset] = ParseAfterAfterFrameset;
    }

    private InsertionMode CurrentTemplateInsertionMode => _templateInsertionModeStack.Peek();

    private void ResetInsertionModeAppropriately()
    {
        bool last = false;
        HtmlNode node = _openElementsStack.Peek();
        // TODO: see section 13.2.4.1 "The insertion mode"
    }

    private HtmlNode CurrentNode => _openElementsStack.Peek();
    private HtmlNode AdjustedCurrentNode
    {
        get
        {
            // The adjusted current node is the context element if the parser
            //   was created as part of the HTML fragment parsing algorithm and
            //   the stack of open elements has only one element in it (fragment
            //   case);
            if (_fragment && _openElementsStack.Count == 1)
                throw new NotImplementedException();

            // otherwise, the adjusted current node is the current node.
            return CurrentNode;
        }
    }

    private bool ElementInScopeInternal(params NamespaceAndElement[] list)
    {
        // see section 13.2.4.2
        throw new NotImplementedException();
    }
    private bool HasParticularElementInScope()
    {
        return ElementInScopeInternal(
            NamespaceAndElement.NewHtml("applet"),
            NamespaceAndElement.NewHtml("caption"),
            NamespaceAndElement.NewHtml("html"),
            NamespaceAndElement.NewHtml("table"),
            NamespaceAndElement.NewHtml("td"),
            NamespaceAndElement.NewHtml("th"),
            NamespaceAndElement.NewHtml("marquee"),
            NamespaceAndElement.NewHtml("object"),
            NamespaceAndElement.NewHtml("template"),
            NamespaceAndElement.NewMathML("mi"),
            NamespaceAndElement.NewMathML("mo"),
            NamespaceAndElement.NewMathML("mn"),
            NamespaceAndElement.NewMathML("ms"),
            NamespaceAndElement.NewMathML("mtext"),
            NamespaceAndElement.NewMathML("annotation-xml"),
            NamespaceAndElement.NewSvg("foreignObject"),
            NamespaceAndElement.NewSvg("desc"),
            NamespaceAndElement.NewSvg("title"));
    }
    private bool HasParticularElementInListScope()
    {
        return HasParticularElementInScope() ||
            ElementInScopeInternal(
                NamespaceAndElement.NewHtml("ol"),
                NamespaceAndElement.NewHtml("ul"));
    }
    private bool HasParticularElementInButtonScope()
    {
        return HasParticularElementInScope() ||
            ElementInScopeInternal(
                NamespaceAndElement.NewHtml("button"));
    }
    private bool HasParticularElementInTableScope()
    {
        return ElementInScopeInternal(
            NamespaceAndElement.NewHtml("html"),
            NamespaceAndElement.NewHtml("table"),
            NamespaceAndElement.NewHtml("template"));
    }
    private bool HasParticularElementInSelectScope() =>
        ElementInScopeInternal(NamespaceAndElement.NewHtml("optgroup"), NamespaceAndElement.NewHtml("option"));

    private void PushActiveFormattingElement(NamespaceAndElement element)
    {
        // see section 13.2.4.3
        throw new NotImplementedException();
    }
    private void ReconstructActiveFormattingElements()
    {
        // see section 13.2.4.3
        throw new NotImplementedException();
    }
    private void ClearActiveFormattingElementsUpToLastMarker(NamespaceAndElement element)
    {
        // see section 13.2.4.3
        throw new NotImplementedException();
    }

    private void TreeConstructionDispatcher(Token token)
    {
        HtmlNode currentNode = AdjustedCurrentNode;
        if (_openElementsStack.Count == 0 ||
            currentNode.NamespaceAndElement.IsHtml ||
            IsValidMathMLIntegrationPoint(currentNode, token) ||
            IsValidMathMLAnnotationXmlNode(currentNode, token) ||
            IsValidHtmlIntegrationPoint(currentNode, token) ||
            token.Type == TokenType.EndOfFile)
        {
            _stateMap[(int)_insertionMode](token);
        }
        else
        {
            ParseForeign(token);
        }
    }
    private static bool IsValidMathMLIntegrationPoint(HtmlNode currentNode, Token token)
    {
        // A node is a MathML text integration point if it is one of the following elements:
        if (!currentNode.NamespaceAndElement.IsMathML)
            return false;

        // A MathML mi element
        // A MathML mo element
        // A MathML mn element
        // A MathML ms element
        // A MathML mtext element
        if (currentNode.NamespaceAndElement.Element is not ("mi" or "mo" or "mn" or "ms" or "mtext"))
            return false;

        // If the adjusted current node is a MathML text integration point and the token is a start tag whose tag name
        //   is neither "mglyph" nor "malignmark"
        if (token.Type is TokenType.Tag)
        {
            Tag tag = token.TagValue;
            return !tag.IsEndTag && tag.Name is not ("mglyph" or "malignmark");
        }
        // If the adjusted current node is a MathML text integration point and the token is a character token
        return token.Type is TokenType.Character;
    }
    private static bool IsValidMathMLAnnotationXmlNode(HtmlNode currentNode, Token token)
    {
        if (!currentNode.NamespaceAndElement.IsMathML)
            return false;

        // If the adjusted current node is a MathML annotation-xml element...
        if (currentNode.NamespaceAndElement.Element is not "annotation-xml")
            return false;
        // ...and the token is a start tag whose tag name is "svg"
        if (token.Type is not TokenType.Tag)
            return false;
        Tag tag = token.TagValue;
        return !tag.IsEndTag && tag.Name == "svg";
    }
    private static bool IsValidHtmlIntegrationPoint(HtmlNode currentNode, Token token)
    {
        // A node is an HTML integration point if it is one of the following elements:

        if (currentNode.NamespaceAndElement.IsMathML)
        {
            // A MathML annotation-xml element whose start tag token...
            if (currentNode.NamespaceAndElement.Element is not "annotation-xml" || !currentNode.TokenizedTag.IsEndTag)
                return false;
            // ...had an attribute with the name "encoding" whose value...
            Attribute? encodingAttr = currentNode.TokenizedTag.FindAttribute("encoding");
            if (encodingAttr is null)
                return false;
            // ...was an ASCII case-insensitive match for the string "text/html"
            // ...was an ASCII case-insensitive match for the string "application/xhtml+xml"
            return encodingAttr.Value.ToLowerInvariant() is "text/html" or "application/xhtml+xml";
        }

        if (currentNode.NamespaceAndElement.IsSvg)
        {
            // An SVG foreignObject element
            // An SVG desc element
            // An SVG title element
            return currentNode.NamespaceAndElement.Element is "foreignObject" or "desc" or "title";
        }

        // If the adjusted current node is an HTML integration point and the token is a start tag
        if (token.Type is TokenType.Tag)
            return !token.TagValue.IsEndTag;
        // If the adjusted current node is an HTML integration point and the token is a character token
        return token.Type is TokenType.Character;
    }

    private void ParseInitial(Token token)
    {
    }

    private void ParseBeforeHtml(Token token)
    {
    }

    private void ParseBeforeHead(Token token)
    {
    }

    private void ParseInHead(Token token)
    {
    }

    private void ParseInHeadNoScript(Token token)
    {
    }

    private void ParseAfterHead(Token token)
    {
    }

    private void ParseInBody(Token token)
    {
    }

    private void ParseText(Token token)
    {
    }

    private void ParseInTable(Token token)
    {
    }

    private void ParseInTableText(Token token)
    {
    }

    private void ParseInCaption(Token token)
    {
    }

    private void ParseInColumnGroup(Token token)
    {
    }

    private void ParseInTableBody(Token token)
    {
    }

    private void ParseInRow(Token token)
    {
    }

    private void ParseInCell(Token token)
    {
    }

    private void ParseInSelect(Token token)
    {
    }

    private void ParseInSelectInTable(Token token)
    {
    }

    private void ParseInTemplate(Token token)
    {
    }

    private void ParseAfterBody(Token token)
    {
    }

    private void ParseInFrameset(Token token)
    {
    }

    private void ParseAfterFrameset(Token token)
    {
    }

    private void ParseAfterAfterBody(Token token)
    {
    }

    private void ParseAfterAfterFrameset(Token token)
    {
    }

    private void ParseForeign(Token token)
    {
    }

    #region IDisposable
    public void Dispose()
    {
        _tokenizer.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion
}
