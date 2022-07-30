/* =============================================================================
 * File:   Property.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * Represents the property descriptor type.
 * <https://tc39.es/ecma262/#sec-property-descriptor-specification-type>
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

using CurlyBracket.Types;
using DotNext;

namespace CurlyBracket.Runtime.Properties;

/// <summary>
/// Represents an ECMAScript property descriptor.
/// The various kinds of properties, such as CLR ones, inherit from this.
/// </summary>
/// <remarks>https://tc39.es/ecma262/#sec-property-attributes</remarks>
public class Property
{
    private Optional<bool> _writable = Optional<bool>.None;
    private Optional<bool> _enumerable = Optional<bool>.None;
    private Optional<bool> _configurable = Optional<bool>.None;

    /// <summary>
    /// Construct a new <see cref="Property" /> with no value, getter, or setter.
    /// The new <see cref="Property" /> will not be writable, enumerable, or configurable.
    /// </summary>
    public Property()
    { }

    /// <summary>
    /// Construct a new <see cref="Property" /> object with no defined getter or setter.
    /// </summary>
    /// <param name="value">The initial value of this property.</param>
    /// <param name="writable">
    /// Indicates if this property is "writable".
    /// If <c>false</c>, calls to <see cref="Set" /> will fail.
    /// </param>
    /// <param name="enumerable">
    /// Indicates if this property is "enumerable".
    /// If <c>true</c>, this property can be enumerated by a <c>for-in</c> loop.
    /// </param>
    /// <param name="configurable">
    /// Indicates if this property is "configurable".
    /// If <c>false</c>, this property cannot be deleted, changed between a data and accessor property, or have the
    ///   other properties changed (excluding <see cref="Value" /> and <see cref="Writable" />).
    /// </param>
    public Property(Optional<JSValue> value, Optional<bool> writable, Optional<bool> enumerable, Optional<bool> configurable)
    {
        Value = value;
        _writable = writable;
        _enumerable = enumerable;
        _configurable = configurable;
    }

    /// <summary>
    /// The actual value this property contains.
    /// </summary>
    public Optional<JSValue> Value { get; set; } = Optional<JSValue>.None;

    /// <summary>
    /// The "getter" for this property.
    /// </summary>
    public virtual Optional<JSValue> Get => Optional<JSValue>.None;

    /// <summary>
    /// The "setter" for this property.
    /// If <see cref="Writable" /> is <c>false</c>, this call will fail.
    /// </summary>
    public virtual Optional<JSValue> Set => Optional<JSValue>.None;

    /// <summary>
    /// Get or set a boolean indicating if this property is "writable".
    /// If this value has not been set yet, it will be <c>false</c>.
    /// </summary>
    public bool Writable
    {
        get => _writable.Or(false);
        set => _writable = value;
    }

    /// <summary>
    /// Get or set a boolean indicating if this property is "enumerable".
    /// If this value has not been set yet, it will be <c>false</c>.
    /// </summary>
    public bool Enumerable
    {
        get => _enumerable.Or(false);
        set => _enumerable = value;
    }

    /// <summary>
    /// Get or set a boolean indicating if this property is "configurable".
    /// If this value has not been set yet, it will be <c>false</c>.
    /// </summary>
    public bool Configurable
    {
        get => _configurable.Or(false);
        set => _configurable = value;
    }

    /// <summary>
    /// Indicates if this property is an accessor descriptor.
    /// An accessor descriptor is defined as having at least a <see cref="Get" /> (getter) or <see cref="Set" />
    ///   (setter).
    /// </summary>
    /// <returns>A boolean indicating if this is an accessor descriptor.</returns>
    /// <remarks>
    /// This implements the <c>IsAccessorDescriptor</c> abstract operation:
    /// https://tc39.es/ecma262/#sec-isaccessordescriptor
    /// </remarks>
    public bool IsAccessorDescriptor() =>
        Get.HasValue || Set.HasValue;

    /// <summary>
    /// Determine if this property is a data descriptor.
    /// A data descriptor is defined as having either a value or being writable.
    /// </summary>
    /// <returns>A boolean indicating if this is a data descriptor.</returns>
    /// <remarks>
    /// This implements the <c>IsDataDescriptor</c> abstract operation:
    /// https://tc39.es/ecma262/#sec-isdatadescriptor
    /// </remarks>
    public bool IsDataDescriptor() =>
        Value.HasValue || Writable;

    /// <summary>
    /// Determine if this property is neither an accessor or data descriptor (i.e. a generic one).
    /// </summary>
    /// <returns>A boolean indicating if this is generic descriptor.</returns>
    /// <remarks>
    /// This implements the <c>IsGenericDescriptor</c> abstract operation:
    /// https://tc39.es/ecma262/#sec-isgenericdescriptor
    /// </remarks>
    public bool IsGenericDescriptor() =>
        !IsAccessorDescriptor() && !IsDataDescriptor();

    /// <summary>
    /// Convert a property into a new <see cref="JSObject" />.
    /// </summary>
    /// <param name="desc">The property to convert to an object.</param>
    /// <returns>
    /// A <see cref="JSObject" /> created from the specified property.
    /// If the specified property is undefined, <see cref="JSUndefined">undefined</see> will be returned instead.
    /// </returns>
    /// <remarks>
    /// This implements the <c>FromPropertyDescriptor</c> abstract operation:
    /// https://tc39.es/ecma262/#sec-frompropertydescriptor
    /// </remarks>
    public static JSValue FromPropertyDescriptor(Property desc) =>
        throw new NotImplementedException();

    /// <summary>
    /// Convert an object into a new <see cref="Property" />.
    /// </summary>
    /// <param name="obj">The object to convert to a property.</param>
    /// <returns>The new <see cref="Property" /> object.</returns>
    /// <remarks>
    /// This implements the <c>ToPropertyDescriptor</c> abstract operation:
    /// https://tc39.es/ecma262/#sec-topropertydescriptor
    /// </remarks>
    public static Result<Property> ToPropertyDescriptor(JSValue obj)
    {
        if (obj.Type is not JSType.Object)
            return new(new TypeErrorException());

        throw new NotImplementedException();
    }

    /// <summary>
    /// "Complete" this property descriptor.
    /// This will, if unset, set <see cref="Enumerable" /> and <see cref="Configurable" /> to <c>false</c>.
    /// In addition, if this is a generic or data descriptor, this will also set <see cref="Writable" /> to
    ///   <c>false</c>.
    /// </summary>
    /// <remarks>
    /// This implements the <c>CompletePropertyDescriptor</c> abstract operation:
    /// https://tc39.es/ecma262/#sec-completepropertydescriptor
    /// </remarks>
    public void CompletePropertyDescriptor()
    {
        if (IsGenericDescriptor() || IsDataDescriptor())
        {
            if (!_writable.HasValue)
                _writable = false;
        }

        if (_enumerable.IsUndefined)
            _enumerable = false;
        if (_configurable.IsUndefined)
            _configurable = false;
    }
}
