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
using System.Text;

namespace CodePoint;

#pragma warning disable CA1710, CA1036
// CA1710 "Identifiers should have correct suffix" (should be "UsvStringCollection")
// CA1036 "Override methods on comparable types" (should implement >, >=, <, and <=)

/// <summary>
/// Represents text as a series of UTF-32 code points with no surrogates.
/// This is in contrast to <see cref="string" /> which uses UTF-16 code points.
/// </summary>
/// <remarks>
/// Note that this class does <i>not</i> enforce a Unicode normalization method.
/// Whatever characters or Runes are used to create a <see cref="UsvString" />
///   will be copied (almost) verbatim into the backing store.
/// To create a normalized <see cref="UsvString" />, use <c>TODO</c>.
/// </remarks>
public sealed class UsvString :
    ICloneable, IComparable, IComparable<UsvString>, IComparable<string>, IEnumerable<Rune>
#pragma warning restore CA1710, CA1036
{
    private readonly List<Rune> _runes;

    public UsvString(char[] chars)
        : this(chars.AsSpan())
    { }
    public UsvString(string str)
        : this(str.AsSpan())
    { }
    public UsvString(ReadOnlySpan<char> chars)
    {
        _runes = new(chars.Length * 2);

        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (Rune.TryCreate(c, out Rune r))
            {
                _runes.Add(r);
                continue;
            }

            char c2 = chars[i + 1];
            if (Rune.TryCreate(c, c2, out r))
            {
                _runes.Add(r);
                continue;
            }

            throw new ArgumentException($"Invalid Unicode codepoint in input at index {i}.");
        }

        _runes.TrimExcess();
    }
    public UsvString(ReadOnlySpan<Rune> runes)
    {
        _runes = new(runes.Length);
        foreach (Rune r in runes)
            _runes.Add(r);
    }

    public Rune this[int index] => _runes[index];

    public int Length => _runes.Count;

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

    public override bool Equals(object? obj)
    {
        if (obj is UsvString other)
            return this == other;
        return false;
    }

    public override int GetHashCode() => _runes.GetHashCode();

    public override string ToString()
    {
        StringBuilder builder = new(Length * 2);
        foreach (Rune r in this)
            builder.Append(r.ToString());
        return builder.ToString();
    }

    #region IClonable
    public object Clone() => this; // UsvString is immutable
    #endregion

    #region IComparable
    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1; // nulls come first

        Contract.Assert(obj is UsvString);

        return CompareTo((UsvString)obj);
    }
    #endregion

    #region IComparable<UsvString>
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
    #endregion

    #region IComparable<string>
    public int CompareTo(string? other)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region IEnumerator<Rune>
    public IEnumerator GetEnumerator() => _runes.GetEnumerator();

    IEnumerator<Rune> IEnumerable<Rune>.GetEnumerator() => _runes.GetEnumerator();
    #endregion
}
