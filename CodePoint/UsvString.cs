/* =============================================================================
 * File:   UsvString.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * <TODO>
 * =============================================================================
 * Copyright (c) 2021-2022 Cole Tobin
 *
 * This file is part of CodePoint.
 *
 * CodePoint is free software: you can redistribute it and/or modify it under
 *   the terms of the GNU General Public License as published by the Free
 *   Software Foundation, either version 3 of the License, or (at your option)
 *   any later version.
 *
 * CodePoint is distributed in the hope that it will be useful, but WITHOUT ANY
 *   WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 *   FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
 *   details.
 *
 * You should have received a copy of the GNU General Public License along with
 *   CodePoint. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CodePoint;

/// <summary>
/// Represents text as a series of UTF-32 code points with no surrogates.
/// This is in contrast to <see cref="string" /> which uses UTF-16 code points.
/// </summary>
/// <remarks>
/// Note that this class does <i>not</i> enforce a Unicode normalization method.
/// Whatever code points are used to create a <see cref="UsvString" /> will be copied (almost) verbatim into the backing
///   store.
/// </remarks>
#pragma warning disable CS1591
public sealed class UsvString :
    ICloneable, IComparable, IComparable<UsvString>, IComparable<string>, IEnumerable<Rune>, IEnumerable<int>
{
    private readonly List<int> _codePoints;

    public UsvString()
    {
        _codePoints = new();
    }

    public UsvString(char[] chars)
        : this(chars.AsSpan())
    { }
    public UsvString(string str)
        : this(str.AsSpan())
    { }
    public UsvString(ReadOnlySpan<char> chars)
    {
        _codePoints = new(chars.Length * 2);

        for (int i = 0; i < chars.Length; i++)
        {
            // TODO: replace Rune.TryCreate with a custom UTF-16 -> UTF-32 converter
            char c = chars[i];
            if (Rune.IsValid(c))
            {
                _codePoints.Add(c);
                continue;
            }

            char c2 = chars[i++];
            if (Rune.TryCreate(c, c2, out Rune r))
            {
                _codePoints.Add(r.Value);
                continue;
            }

            // TODO: Use HyperLib.EM
            throw new ArgumentException($"Invalid Unicode codepoint in input at index {i}.");
        }

        _codePoints.TrimExcess();
    }
    public UsvString(ReadOnlySpan<int> utf32)
    {
        _codePoints = new(utf32.Length);
        foreach (int c in utf32)
        {
            if (!Rune.IsValid(c))
                throw new ArgumentException("", nameof(utf32));
            _codePoints.Add(c);
        }
    }

    public int this[int index] => _codePoints[index];

    public int Length => _codePoints.Count;

    public static bool operator ==(UsvString? lhs, UsvString? rhs)
    {
        if (ReferenceEquals(lhs, rhs))
            return true;
        if (lhs is null || rhs is null)
            return false;
        if (lhs.Length != rhs.Length)
            return false;

        for (int i = 0; i < lhs.Length; i++)
        {
            if (lhs[i] != rhs[i])
                return false;
        }

        return true;
    }
    public static bool operator !=(UsvString? lhs, UsvString? rhs) =>
        !(lhs == rhs);

    public override bool Equals(object? obj) =>
        obj is UsvString other && this == other;

    public override int GetHashCode() => _codePoints.GetHashCode();

    public override string ToString()
    {
        StringBuilder builder = new(Length * 2);
        foreach (int c in this)
            builder.Append(new Rune(c).ToString()); // TODO: inefficient
        return builder.ToString();
    }

    public object Clone() => this; // UsvString is immutable

    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1; // nulls come first

        Debug.Assert(obj is UsvString);

        return CompareTo((UsvString)obj);
    }

    public int CompareTo(UsvString? other)
    {
        if (other is null)
            return 1; // nulls come first

        int length = Math.Min(Length, other.Length);
        for (int i = 0; i < length; i++)
        {
            int val = this[i].CompareTo(other[i]);
            if (val == 0)
                continue;
            return val;
        }

        // equal up to this point; compare lengths now
        return Length.CompareTo(other.Length);
    }

    public int CompareTo(string? other)
    {
        throw new NotImplementedException();
    }


    public IEnumerator GetEnumerator() => throw new NotImplementedException();

    IEnumerator<Rune> IEnumerable<Rune>.GetEnumerator() => throw new NotImplementedException();
    IEnumerator<int> IEnumerable<int>.GetEnumerator() => _codePoints.GetEnumerator();
}
