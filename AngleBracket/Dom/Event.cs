/* =============================================================================
 * File:   Event.cs
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

public class Event
{
    public Event(string type)
        : this(type, new())
    { }

    public Event(string type, EventInit eventInitDict)
    {

    }

    public string Type { get; }
    public EventTarget? Target { get; }
    public EventTarget? SrcElement { get; }
    public EventTarget? CurrentTarget { get; }
    public List<EventTarget?> ComposedPath() =>
        throw new NotImplementedException();

    public const short NONE = 0;
    public const short CAPTURING_PHASE = 1;
    public const short AT_TARGET = 2;
    public const short BUBBLING_PHASE = 3;
    public short EventPhase { get; }

    public void StopPropagation() =>
        throw new NotImplementedException();
    public bool CancelBubble { get; set; }
    public void StopImmediatePropogation() =>
        throw new NotImplementedException();

    public bool Bubbles { get; }
    public bool Cancelable { get; }
    public bool ReturnValue { get; set; }
    public bool DefaultPrevented { get; }
    public bool Composed { get; }

    public bool IsTrusted { get; }
    public TimeOnly TimeStamp { get; }

    public void InitEvent(string type, bool bubbles = false, bool cancelable = false) =>
        throw new NotImplementedException();
}

public record struct EventInit(
    bool Bubbles = false,
    bool Cancelable = false,
    bool Composed = false);
