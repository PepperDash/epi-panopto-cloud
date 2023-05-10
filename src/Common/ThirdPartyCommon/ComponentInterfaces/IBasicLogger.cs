// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IBasicLogger
    {
        /// <summary>
        /// Property to enable TX Debug statements.
        /// </summary>
        bool EnableTxDebug { get;  set; }

        /// <summary>
        /// Property to enable RX Debug statements.
        /// </summary>
        bool EnableRxDebug { get;  set; }

        /// <summary>
        /// Property to enable Logging statements.
        /// </summary>
        bool EnableLogging { get; set; }

        /// <summary>
        /// Property set a Custom Logger.
        /// </summary>
        Action<string> CustomLogger { get; set; }

    }
}
