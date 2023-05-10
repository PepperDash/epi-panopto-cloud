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
    /// Defines the possible units of measurement of a device property;
    /// Specified by the interface property <see cref="IDeviceProperty.Units"/>.
    /// </summary>
    public enum DevicePropertyUnit
    {
        /// <summary>
        /// Indicates that units are not applicable or not known.
        /// </summary>
        None,

        /// <summary>
        /// Measures whole degrees in Celsius.
        /// </summary>
        Celsius,

        /// <summary>
        /// Measures whole degrees in Fahrenheit.
        /// </summary>
        Fahrenheit,

        /// <summary>
        /// Measures a percentage, typically between 0 and 100,
        /// but the minimum and maximum may vary depending on the <see cref="IDeviceProperty{T}"/>.
        /// </summary>
        Percentage,

        /// <summary>
        /// Measures 1/1,000th of a volt; Abbreviated as mV.
        /// </summary>
        Millivolts,

        /// <summary>
        /// Imperial system unit of measure for the concentration of a substance; Abbreviated as ppm.
        /// </summary>
        PartsPerMillion,

        /// <summary>
        /// Metric system unit of measure for the concentration of a substance; Abbreviated as g/L.
        /// </summary>
        GramsPerLiter,

        Hours,

        Minutes,

        Seconds,

        Milliseconds
    }

}