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
    /// <summary>
    /// Defines a device preset (which sets one or more devices into a preconfigured state).
    /// A device preset may affect only the owning device or many devices in this device's ecosystem.
    /// </summary>
    public interface IDevicePreset
    {
        /// <summary>
        /// ID of the preset (assigned by the device or its ecosystem).
        /// <para>
        /// Uniqueness is only guaranteed within the owning device.
        /// If the owning device is the primary controller of an entire ecosystem,
        /// this ID would be unique across all the devices provided by that ecosystem.
        /// </para>
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Name of the preset (as configured using the device or its software).
        /// <para>
        /// Changes will be notified via the <see cref="IPresetController"/> capability's event
        /// <see cref="IPresetController.PresetsListChanged"/> with the action <see cref="ListChangedAction.Replaced"/>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// NOTE: The changed event appears on the owning device interface <see cref="IPresetController"/>.
        /// This makes device driver implementations easier and allows consumers of these interfaces to
        /// have context as to which device's preset changed.
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Indicates if the <see cref="IsActive"/> property is supported for this preset.
        /// <para>
        /// Depending on the device capabilities affected by a preset,
        /// some presets may not be capable of providing feedback.
        /// This value can change if the device's preset is modified.
        /// </para>
        /// <para>
        /// Changes will be notified via the <see cref="IPresetController"/> capability's event
        /// <see cref="IPresetController.PresetsListChanged"/> with the action <see cref="ListChangedAction.Replaced"/>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// NOTE: The changed event appears on the owning device interface <see cref="IPresetController"/>.
        /// This makes device driver implementations easier and allows consumers of these interfaces to
        /// have context as to which device's preset changed.
        /// </remarks>
        bool SupportsIsActive { get; }

        /// <summary>
        /// Indicates if this preset is currently active.
        /// <para>
        /// In other words, this is feedback from the device indicating that either
        /// all devices and properties affected by this preset are at their target values
        /// or that this was the last preset selected.
        /// </para>
        /// <para>
        /// Changes will be notified via the <see cref="IPresetController"/> capability's event
        /// <see cref="IPresetController.PresetStateChanged"/>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// NOTE: The changed event appears on the owning device interface <see cref="IPresetController"/>.
        /// This makes device driver implementations easier and allows consumers of these interfaces to
        /// have context as to which device's preset changed.
        /// </remarks>
        bool IsActive { get; }

        /// <summary>
        /// Indicates if calling <see cref="IPresetController.RecallPreset"/>
        /// while the preset is already active will act as a toggle.
        /// <para>
        /// Depending on the device capabilities/properties affected by a preset,
        /// it may not be capable of toggling or it may not make sense to support toggling.
        /// This value can change if the device's preset is modified.
        /// </para>
        /// <para>
        /// Changes will be notified via the <see cref="IPresetController"/> capability's event
        /// <see cref="IPresetController.PresetsListChanged"/> with the action <see cref="ListChangedAction.Replaced"/>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// NOTE: The changed event appears on the owning device interface <see cref="IPresetController"/>.
        /// This makes device driver implementations easier and allows consumers of these interfaces to
        /// have context as to which device's preset changed.
        /// </remarks>
        bool SupportsToggle { get; }
    }
}