// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Crestron.Panopto.Common;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Interfaces;
using Crestron.Panopto.Common.Enums;
using Crestron.Panopto.Common.ExtensionMethods;

namespace Crestron.Panopto.Common
{
    public class DatFileMultiPowerOff
    {
        public List<string> commands;
        public float timeBetweenInSeconds;

        public DatFileMultiPowerOff()
        {
            commands = new List<string>();
            timeBetweenInSeconds = 0.0f;
        }
    }
}