// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IConnection : IAuthentication
    {
        /// <summary>
        /// Indicates if the Driver is Connected to Device.
        /// </summary>
        /// <returns>True if connected.</returns>
        bool Connected { get; }

        /// <summary>
        /// Property indicating that Driver supports Disconnecting.
        /// </summary>
        bool SupportsDisconnect { get; }

        /// <summary>
        /// Disconnects from the device.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Property indicating that Driver supports Reconnecting.
        /// </summary>
        bool SupportsReconnect { get; }

        /// <summary>
        /// Reconnects to the device.  
        /// </summary>
        void Reconnect();

        /// <summary>
        /// Connects to the device.
        /// </summary>
        void Connect();


    }
}
