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
    /// Represents a device that has a configuration
    /// <para>
    /// Pool controllers would be devices that have a configuration, because they can have multiple parts and pools that can be changed.
    /// Security system would also have a configuration because the sensors, zones, and alarms can be changed.
    /// </para>
    /// <para>
    /// If a driver has a concept of a configuration, but there are reasons why it can't poll for this data either because 
    /// there is too much polling, or there is too much parsing to do at the regular polling interval, then it must
    /// state that <see cref="SupportsConfigurationUpdate"/> is false. Applications are then expected to call <see cref="Reconnect"/> 
    /// to get the latest configuration.
    /// </para>
    /// </summary>
    public interface IConfigurableDevice
    {
        /// <summary>
        /// Indicates if the driver is able to detect configuration changes on the device.
        /// <para>
        /// This will be null if the feature is not applicable to a device.
        /// This will be true if the driver is able to detect configuration changes and will report the changes
        /// This will be false if the driver not able to detect configuration changes, and <see cref="Reconnect"/>
        /// must be called to get the new configuration.
        /// </para>
        /// </summary>
        bool? SupportsConfigurationUpdate { get; }
    }
}