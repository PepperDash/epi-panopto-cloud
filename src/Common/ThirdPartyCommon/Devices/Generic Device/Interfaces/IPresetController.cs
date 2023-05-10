// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using Crestron.Panopto.Common.Events;

namespace Crestron.Panopto.Common.Interfaces
{
    /// <summary>
    /// Interface to be implemented by a device
    /// which supports a list of presets (a.k.a. macros or scenes)
    /// that can be executed to change several aspects of one or more devices.
    /// <para>
    /// Each preset may affect only this device or many devices in this device's ecosystem.
    /// </para>
    /// </summary>
    /// <remarks>See <see cref="IDeviceCapability"/> for more information about device capabilities.</remarks>
    public interface IPresetController
    {
        /// <summary>
        /// Gets the list of presets available on this device.
        /// </summary>
        IEnumerable<IDevicePreset> GetPresets();

        /// <summary>
        /// Recalls/executes the preset with the specified ID
        /// (previously retrieved from the <see cref="GetPresets"/>).
        /// </summary>
        void RecallPreset(string devicePresetId);

        /// <summary>
        /// Raised when the list of presets has changed.
        /// <para>
        /// Presumably, subscribers of this event will have called <see cref="GetPresets"/> method once to
        /// retrieve a copy of the list. Based on the arguments of this event, the subscribers
        /// can then update their copy of the list without asking this device for the full list again
        /// (unless the event was a <see cref="ListChangedAction.Reset"/>; In that case, the full list would
        /// need to be retrieved again anyway).
        /// </para>
        /// </summary>
        event EventHandler<ListChangedEventArgs<IDevicePreset>> PresetsListChanged;

        /// <summary>
        /// Raised when the state of a preset has changed.
        /// </summary>
        event EventHandler<DevicePresetStateEventArgs> PresetStateChanged;
    }
}