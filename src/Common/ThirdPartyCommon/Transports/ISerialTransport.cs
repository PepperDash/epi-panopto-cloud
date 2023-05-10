// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using Crestron.Panopto.Common.Interfaces;

namespace Crestron.Panopto.Common.Transports
{
    public interface ISerialTransport : ITransport, ITransportLogger
    {
        bool IsConnected { get;}
        int DriverID { get; set; }

        Action<string> DataHandler { set; }
        Action<string, object[]> Send { get; set; }

        void Start();
        void Stop();

        Action<string> MessageTimedOut { set; }
        uint TimeOut { get; set; }
        Action<bool> ConnectionChanged { set; }
    }
}
