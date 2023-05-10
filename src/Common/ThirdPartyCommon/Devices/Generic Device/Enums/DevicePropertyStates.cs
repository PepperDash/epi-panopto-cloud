// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
namespace Crestron.Panopto.Common.Enums
{
    /// <summary>
    /// Defines state information about a device property;
    /// Specified by the interface property <see cref="IDeviceProperty.State"/>.
    /// </summary>
    [Flags]
    public enum DevicePropertyStates
    {
        /// <summary>
        /// Indicates a default, valid state.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Indicates if the property is currently disabled.
        /// <para>
        /// This may change based on the values of other properties.
        /// For example, changing the value of one property may disable another property.
        /// </para>
        /// </summary>
        Disabled = 0x01,

        /// <summary>
        /// Indicates if the value of the property is not currently available.
        /// <para>
        /// In other words, the device may not have the value yet or
        /// the device is not capable of retrieving the value
        /// (for example, if a temperature sensor is not connected).
        /// </para>
        /// <para>
        /// When this flag is present, the value returned by <see cref="IDeviceProperty.GetValue{TValue}"/>
        /// or <see cref="IDeviceProperty{T}.Value"/> must be ignored.
        /// </para>
        /// </summary>
        ValueNotAvailable = 0x02
    }
}