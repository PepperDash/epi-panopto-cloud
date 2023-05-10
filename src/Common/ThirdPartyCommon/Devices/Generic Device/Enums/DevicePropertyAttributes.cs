// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
namespace Crestron.Panopto.Common.Enums
{
    [Flags]
    public enum DevicePropertyAttributes
    {
        /// <summary>
        /// Indicates the property has no additional attributes;
        /// It is a read-write property with no limits other than those imposed by the <see cref="IDeviceProperty.Type"/>.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Indicates the property is read-only and its value cannot be set.
        /// Only the device itself can change the value and report changes
        /// via the <see cref="IDeviceWithProperties.PropertyChanged"/> event.
        /// </summary>
        ReadOnly = 0x01,

        /// <summary>
        /// Indicates the property has a minimum value that must be respected;
        /// It can be retrieved via the <see cref="IDeviceProperty.GetMinValue{TValue}"/> method
        /// or the <see cref="IDeviceProperty{TValue}.MinValue"/> property.
        /// </summary>
        MinValue = 0x02,

        /// <summary>
        /// Indicates the property has a maximum value that must be respected;
        /// It can be retrieved via the <see cref="IDeviceProperty.GetMaxValue{TValue}"/> method
        /// or the <see cref="IDeviceProperty{TValue}.MaxValue"/> property.
        /// </summary>
        MaxValue = 0x04,

        /// <summary>
        /// Indicates the property has a step size that must be respected;
        /// It can be retrieved via the <see cref="IDeviceProperty.GetStepSize{TValue}"/> method
        /// or the <see cref="IDeviceProperty{T}.StepSize"/> property.
        /// </summary>
        StepSize = 0x08
    }
}