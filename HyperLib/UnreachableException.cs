/* =============================================================================
 * File:   UnreachableException.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Implements an exception that indicates a line of code should never be
 *   reached.
 * If a debugger is attached, `Debug.Fail` will be called.
 * =============================================================================
 * Copyright (c) 2022 Cole Tobin
 *
 * This file is part of CurlyBracket.
 *
 * CurlyBracket is free software: you can redistribute it and/or modify it under
 *   the terms of the GNU General Public License as published by the Free
 *   Software Foundation, either version 3 of the License, or (at your option)
 *   any later version.
 *
 * CurlyBracket is distributed in the hope that it will be useful, but WITHOUT
 *   ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 *   FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License
 *   for more details.
 *
 * You should have received a copy of the GNU General Public License along with
 *   CurlyBracket. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System.Diagnostics;

namespace HyperLib;

/// <summary>
/// Represents an <see cref="Exception" /> that should never be reached.
/// </summary>
public class UnreachableException : Exception
{
    /// <summary>
    /// Constructs a new <see cref="UnreachableException" />.
    /// </summary>
    public UnreachableException()
    {
        if (Debugger.IsAttached)
            Debug.Fail("Unreachable code was reached.");
    }
}
