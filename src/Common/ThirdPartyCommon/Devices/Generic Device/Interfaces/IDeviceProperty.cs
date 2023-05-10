// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IDeviceProperty
    {
        string Key { get; }
        int LocalizedNameId { get; }
        DevicePropertyAttributes Attributes { get; }
        DevicePropertyType Type { get; }
        DevicePropertyUnit Units { get; }
        DevicePropertyRenderHint RenderHint { get; }

        /// <summary>
        /// Used to associate this property with another property
        /// (in addition to any <see cref="RenderHint"/> values that imply relationships).
        /// <para>
        /// For example, a device may define 2 properties, where the 1st property is a Boolean on-off switch,
        /// and the 2nd property has a render hint of <see cref="DevicePropertyRenderHint.Status"/> which
        /// indicates the status of the device turning on.
        /// The 2nd property will refer the 1st property key here to indicate that
        /// it is associated with the 1st property.
        /// </para>
        /// <para>
        /// Another example would be if a device had 2 sets of setpoints
        /// (i.e. 2 properties with <see cref="DevicePropertyRenderHint.SetpointTarget"/> and
        /// another 2 properties <see cref="DevicePropertyRenderHint.SetpointActual"/>).
        /// This key would indicate which target setpoint belongs with which actual setpoint (or vice-versa).
        /// </para>
        /// <para>
        /// This may be null if the device only contains one set of associated properties.
        /// Using the above example, if a device only has one target and one actual setpoint property,
        /// the relationship is already implied by the render hint.
        /// </para>
        /// </summary>
        string ParentPropertyKey { get; }

        /// <summary>
        /// Indicates the state of this property.
        /// This may change based on the values of other properties.
        /// <para>
        /// Changes will be notified via the <see cref="IDeviceWithProperties"/> capability's event
        /// <see cref="IDeviceWithProperties.PropertyStateChanged"/>.
        /// </para>
        /// </summary>
        DevicePropertyStates State { get; }

        /// <summary>
        /// Retrieves the value of this property.
        /// <para>
        /// If the requested type does not match the type referred to by <see cref="Type"/>,
        /// an <see cref="InvalidCastException"/> will be thrown.
        /// </para>
        /// </summary>
        TValue GetValue<TValue>();

        TValue GetMinValue<TValue>();
        TValue GetMaxValue<TValue>();
        TValue GetStepSize<TValue>();
    }

    public interface IDeviceProperty<T> : IDeviceProperty
    {
        /// <summary>
        /// Available values for the property;
        /// The property’s value can only be set to one of these.
        /// <para>
        /// In addition to being the property’s value, each ID also refers to a
        /// localized string that is display name of this value.
        /// The localized string can be retrieved via the <see cref="ILocalizedDevice"/> capability.
        /// </para>
        /// <para>
        /// Also, based on the <see cref="IDeviceProperty.RenderHint"/>, this may also contain labels
        /// for each value defined by <see cref="MinValue"/> through <see cref="MaxValue"/>.
        /// For example, if the value range is 0 to 3, this may define the labels for those
        /// values as "Off", "Slow", "Medium" and "Fast", respectively.
        /// </para>
        /// </summary>
        IEnumerable<IDevicePropertyAvailableValue<T>> AvailableValues { get; }

        /// <summary>
        /// Current value of this property.
        /// <para>
        /// Changes will be notified via the <see cref="IDeviceWithProperties"/> capability's event
        /// <see cref="IDeviceWithProperties.PropertyChanged"/>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The changed event appears on the owning device interface <see cref="IDeviceWithProperties"/>.
        /// This makes device driver implementations easier and allows consumers of these interfaces to
        /// have context as to which device's property changed.
        /// </remarks>
        T Value { get; }

        /// <summary>
        /// Minimum value allowed for numeric values; For strings, minimum length.
        /// Not applicable for all other non-numeric values.
        /// </summary>
        /// <seealso cref="Type"/>
        T MinValue { get; }

        /// <summary>
        /// Maximum value allowed for numeric values; For strings, maximum length.
        /// Not applicable for all other non-numeric values.
        /// </summary>
        /// <seealso cref="Type"/>
        T MaxValue { get; }

        /// <summary>
        /// Amount to increment or decrement the value at a time;
        /// Applicable only when the <see cref="IDeviceProperty.Type"/> refers to a numeric value.
        /// <para>
        /// If the type is <see cref="DevicePropertyType.Float"/> or <see cref="DevicePropertyType.Double"/>,
        /// this also defines the precision of the floating point value.
        /// </para>
        /// <para>
        /// For example, if this value is 0.2, it would mean valid values could be 0, 0.2, 0.4, etc...
        /// Any invalid values passed to the <see cref="IDeviceWithProperties.SetValue{T}"/> method
        /// will be adjusted to the nearest valid value in the direction the value is being adjusted.
        /// </para>
        /// </summary>
        T StepSize { get; }
    }
}