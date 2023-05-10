// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Events
{
    /// <summary>
    /// Argument for events that refer to device preset's state,
    /// such as <see cref="IDevicePreset.IsActive"/>.
    /// </summary>
    public sealed class DevicePresetStateEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs the event arguments with all the required parameters.
        /// Pass null for each each state that did not change.
        /// </summary>
        /// <param name="id">See <see cref="Id"/></param>
        /// <param name="supportsIsActive">See <see cref="SupportsIsActive"/> </param>
        /// <param name="isActive">See <see cref="IsActive"/></param>
        public DevicePresetStateEventArgs(string id, bool isActive)
        {
            Id = id;
            IsActive = isActive;
        }

        /// <summary>
        /// ID of the preset whose state changed; See <see cref="IDevicePreset.Id"/>.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// New value of <see cref="IDevicePreset.IsActive"/>;
        /// Null if it did not change.
        /// </summary>
        public bool IsActive { get; private set; }
    }
}