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
    /// Interface to be implemented by a device that implements one or more component interfaces
    /// that return strings that are intended to be displayed to the user and need to be localized.
    /// <para>
    /// Some component interfaces will return a string identifier that should then be
    /// resolved using this interface.
    /// </para>
    /// </summary>      
    public interface ILocalizedDevice
    {
        /// <summary>
        /// Retrieves all cultures supported by this device.
        /// See the <see cref="GetLocalizedStrings"/> method for more information
        /// about the format of each culture string.
        /// </summary>
        IEnumerable<string> GetSupportedCultures();

        /// <summary>
        /// Retrieves all the localized strings (and their IDs) used by this device.
        /// </summary>
        /// <param name="culture">
        /// Specifies the language code and country region code to be retrieved.
        /// The string must formatted as the "languagecode2" as defined by the ISO 639-1 standard,
        /// followed by a hyphen and then the "country/regioncode2" as defined by the ISO 3166.
        /// <para>
        /// For example, "en-US" for English in the United States.
        /// </para>
        /// <para>
        /// If the specified culture is not supported, this will return null.
        /// </para>
        /// </param>
        /// <para>
        /// Keys of 0 are not supported.
        /// Any property that returns 0 as a localized string ID indicates an empty string.
        /// </para>
        IEnumerable<KeyValuePair<int, string>> GetLocalizedStrings(string culture);
    }
}