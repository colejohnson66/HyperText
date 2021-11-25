/* =============================================================================
 * File:   NamespaceAndElement.cs
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

namespace AngleBracket.Parser;

public record NamespaceAndElement(string Namespace, string Element)
{
    public const string HTML_NAMESPACE = Infra.Namespace.HTML;
    public const string MATHML_NAMESPACE = Infra.Namespace.MATHML;
    public const string SVG_NAMESPACE = Infra.Namespace.SVG;

    public bool IsHtml => Namespace == HTML_NAMESPACE;
    public bool IsMathML => Namespace == MATHML_NAMESPACE;
    public bool IsSvg => Namespace == SVG_NAMESPACE;

    public static NamespaceAndElement NewHtml(string Element) => new(HTML_NAMESPACE, Element);
    public static NamespaceAndElement NewMathML(string Element) => new(MATHML_NAMESPACE, Element);
    public static NamespaceAndElement NewSvg(string Element) => new(SVG_NAMESPACE, Element);
}
