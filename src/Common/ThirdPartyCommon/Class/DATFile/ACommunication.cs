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

using Newtonsoft.Json;

namespace Crestron.Panopto.Common
{
    public abstract class ACommunication
    {
        public bool adjustable;
        [JsonProperty("type")]
        public string Type;
        public bool secure;
        public AuthenticationNode authentication;

        public ACommunication() { }

        public ACommunication(Communication coms)
        {
            this.adjustable = coms.IsUserAdjustable;
            this.authentication = coms.Authentication;
            this.secure = coms.IsSecure;
        }
    }
}