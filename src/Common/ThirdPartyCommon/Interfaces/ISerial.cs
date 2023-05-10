// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface ISerial :  IDisposable
    {
        /// <summary>
        /// Disconnects the connection
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Reestablishes the connection
        /// </summary>
        void Reconnect();

        /// <summary>
        /// Establishes the connection
        /// </summary>
        void Connect();
    }
}
