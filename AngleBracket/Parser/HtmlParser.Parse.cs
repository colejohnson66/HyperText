/* =============================================================================
 * File:   HtmlParser.Parse.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Contains the actual HTML parser implementation.
 * =============================================================================
 * Copyright (c) 2022 Cole Tobin
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
using Attribute = AngleBracket.Tokenizer.Attribute;

namespace AngleBracket.Parser;

public partial class HtmlParser
{
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
            _stateMap![(int)_insertionMode](token);
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
}
