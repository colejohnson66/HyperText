/* =============================================================================
 * File:   AbortSignal.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * TODO
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

namespace AngleBracket.Dom;

public class AbortSignal : EventTarget
{
    public static AbortSignal Abort(object? reason = null) =>
        throw new NotImplementedException();

    public static AbortSignal Timeout(ulong Milliseconds) =>
        throw new NotImplementedException();

    public bool Aborted { get; }
    public object? Reason { get; }
    public void ThrowIfAborted() =>
        throw new NotImplementedException();

    // public EventHandler OnAbort { get; set; }
}
