// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using Crestron.ThirdPartyCommon.Interfaces;
using Crestron.ThirdPartyCommon.ComponentInterfaces;

namespace Crestron.ThirdPartyCommon.FactoryInterfaces
{
    public struct DeviceConstructorParams
    {
        public DeviceConstructorParams(Action<string, object[]> callBackMethod,
        Dictionary<string, string> commands)
        {
            this.callBackMethod = callBackMethod;
            this.commands = commands;
        }
        public Action<string, object[]> callBackMethod;
        public Dictionary<string, string> commands;
    }

    public interface IBasicCodecFactory
    {
        IBasicCodec make(string deviceType,
            Action<string, object[]> paramSendMethod,
            Dictionary<string, string> paramCommands);
        IBasicCodec make(string deviceType);
        List<String> deviceTypes { get; }
    }
}
