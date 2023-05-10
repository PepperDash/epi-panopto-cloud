// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using Crestron.Panopto.Common.Enums;
using Crestron.Panopto.Common.Interfaces;
using Crestron.SimplSharp.CrestronDataStore;

namespace Crestron.Panopto.Common.Logging
{
    /// <summary>
    ///  Error - Exceptions
    ///  Warning - Exceptions and warnings
    ///  Debug - Exceptions, warnings, and everything else
    ///  
    /// Logs will only print if EnableLogging is enabled.
    /// Drivers should not reference this enumeration.
    /// Drivers should only check the property EnableLogging before making a call to the method Log.
    /// </summary>
    public enum LoggingLevel
    {
        Error = 1,
        Warning = 2,
        Debug = 3
    }
}