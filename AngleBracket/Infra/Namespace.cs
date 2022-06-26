/* =============================================================================
 * File:   Namespace.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Contains the list of namespaces as defined in the Infra standard from WHATWG.
 *
 * As of the 6 May 2022 version, this is located in section 8 "Namespaces":
 *   <https://infra.spec.whatwg.org/#namespaces>
 * =============================================================================
 * Copyright (c) 2021-2022 Cole Tobin
 *
 * This file is part of AngleBracket.
 *
 * AngleBracket is free software: you can redistribute it and/or modify it under
 *   the terms of the GNU Lesser General Public License as published by the Free
 *   Software Foundation, either version 3 of the License, or (at your option)
 *   any later version.
 *
 * AngleBracket is distributed in the hope that it will be useful, but WITHOUT
 *   ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 *   FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License
 *   for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 *   along with AngleBracket. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

namespace AngleBracket.Infra;

[PublicAPI]
public static class Namespace
{
    public const string HTML = "http://www.w3.org/1999/xhtml";
    public const string MATHML = "http://www.w3.org/1998/Math/MathML";
    public const string SVG = "http://www.w3.org/2000/svg";
    // ReSharper disable once IdentifierTypo
    public const string XLINK = "http://www.w3.org/1999/xlink";
    public const string XML = "http://www.w3.org/XML/1998/namespace";
    public const string XMLNS = "http://www.w3.org/2000/xmlns/";
}
