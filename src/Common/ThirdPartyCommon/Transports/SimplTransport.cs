// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;

namespace Crestron.Panopto.Common.Transports
{
    public class SimplTransport : ATransportDriver
    {
        public override void SendMethod(string message, object[] paramaters)
        {
            throw new NotSupportedException();
        }

        public override void Start()
        {
        }

        public override void Stop()
        {
        }

        public void ReceiveData(string data)
        {
            if (EnableRxDebug)
            {
                var rxBytes = Encoding.GetBytes(data);
                var debugStringBuilder = new System.Text.StringBuilder("RX: ");
                debugStringBuilder.Append(LogTxAndRxAsBytes ? BitConverter.ToString(rxBytes).Replace("-", " ") : data);
                debugStringBuilder.Append('\n');
                Log(debugStringBuilder.ToString());
            }

            if (DataHandler != null)
            {
                DataHandler(data);
            }
        }
    }
}
