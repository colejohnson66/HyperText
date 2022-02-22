/* =============================================================================
 * File:   Result.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * <TODO>
 * =============================================================================
 * Copyright (c) 2022 Cole Tobin
 *
 * This file is part of HyperLib.
 *
 * HyperLib is free software: you can redistribute it and/or modify it under the
 *   terms of the GNU General Public License as published by the Free Software
 *   Foundation, either version 3 of the License, or (at your option) any later
 *   version.
 *
 * HyperLib is distributed in the hope that it will be useful, but WITHOUT ANY
 *   WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 *   FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
 *   details.
 *
 * You should have received a copy of the GNU General Public License along with
 *   HyperLib. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

namespace HyperLib;

/// <summary>
/// A class mimicing Rust's <c>std::result::Result</c> type.
/// </summary>
/// <typeparam name="TOK">The type of a successful <c>Result</c>.</typeparam>
/// <typeparam name="TError">The type of an errored <c>Result</c>.</typeparam>
public class Result<TOK, TError>
{
    private Result(TOK value)
    {
        IsOK = true;
        OKValue = value;
    }
    private Result(TError value)
    {
        IsOK = false;
        ErrorValue = value;
    }

    public bool IsOK { get; }
    public bool IsError => !IsOK;

    public TOK? OKValue { get; }
    public TError? ErrorValue { get; }

    public static Result<TOK, TError> OK(TOK value) => new(value);
    public static Result<TOK, TError> Error(TError value) => new(value);
}
