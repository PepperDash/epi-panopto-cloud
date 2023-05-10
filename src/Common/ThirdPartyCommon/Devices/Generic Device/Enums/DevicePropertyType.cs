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
    /// Defines the possible types of a device property;
    /// Specified by the interface property <see cref="IDeviceProperty.Type"/>.
    /// </summary>
    public enum DevicePropertyType
    {
        Uninitialized,
        Boolean,
        String,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Float,
        Double,

        /// <summary>
        /// Indicates the type is a localized string that must be resolved using the <see cref="ILocalizedDevice"/> interface.
        /// <para>
        /// The actual value of this property will be equivalent to <see cref="Int32"/>.
        /// That integer then needs to be looked up in the key-value pairs returned
        /// by <see cref="ILocalizedDevice.GetLocalizedStrings"/>.
        /// </para>
        /// </summary>
        LocalizedString
    }
}