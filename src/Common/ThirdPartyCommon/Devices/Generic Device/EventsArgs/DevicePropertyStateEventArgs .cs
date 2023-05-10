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
    /// Argument for events that refer to device property and its (non-value) states,
    /// such as <see cref="IDeviceProperty.IsEnabled"/>.
    /// </summary>
    public sealed class DevicePropertyStateEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs these arguments with all the required values.
        /// </summary>
        /// <param name="key">Key of the <see cref="IDeviceProperty"/> whose state changed.</param>
        /// <param name="isEnabled">New value of the <see cref="IDeviceProperty.State"/> flag.</param>
        public DevicePropertyStateEventArgs(string key, DevicePropertyStates state)
        {
            Key = key;
            State = state;
        }

        /// <summary>
        /// Key of property; See <see cref="IDeviceProperty.Key"/>.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// New value of <see cref="IDeviceProperty.IsEnabled"/> state;
        /// </summary>
        public DevicePropertyStates State { get; private set; }
    }
}