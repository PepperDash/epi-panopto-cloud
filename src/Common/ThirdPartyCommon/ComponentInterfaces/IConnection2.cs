// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IConnection2 : IAuthentication2
    {
        /// <summary>
        /// Used to indicate if connection to device is secure.
        /// 
        /// Note: This is set true when the connection to the device 
        /// uses a secure socket (I.E. HTTPS).
        /// </summary>
        bool IsSecure { get; }
    }
}