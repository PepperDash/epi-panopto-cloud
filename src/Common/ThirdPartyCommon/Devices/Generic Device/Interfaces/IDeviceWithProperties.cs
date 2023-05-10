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
    public interface IDeviceWithProperties
    {
        IEnumerable<IDeviceProperty> GetProperties();

        void SetValue<T>(string propertyKey, T value);

        event EventHandler<DevicePropertyValueEventArgs> PropertyValueChanged;

        /// <summary>
        /// Raised when any non-value portion of a property has changed.
        /// </summary>
        event EventHandler<DevicePropertyStateEventArgs> PropertyStateChanged;
    }
}