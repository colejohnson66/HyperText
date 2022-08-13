/* =============================================================================
 * File:   EventTarget.cs
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

using System.Diagnostics.Tracing;

namespace AngleBracket.Dom;

public class EventTarget
{
    public EventTarget()
    {
        throw new NotImplementedException();
    }

    public void AddEventListener(string type, EventListener? callback, AddEventListenerOptions options = new()) =>
        throw new NotImplementedException();
    public void AddEventListener(string type, EventListener? callback, bool options) =>
        throw new NotImplementedException();

    public void RemoveEventListener(string type, EventListener? callback, EventListenerOptions options = new()) =>
        throw new NotImplementedException();
    public void RemoveEventListener(string type, EventListener? callback, bool options) =>
        throw new NotImplementedException();
}

public interface IEventListener
{
    public void HandleEvent(Event @event);
}

public record struct EventListenerOptions(bool Capture = false);

public record struct AddEventListenerOptions(
    bool? Passive = null,
    bool Capture = false,
    bool Once = false,
    AbortSignal? signal = null);
