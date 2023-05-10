// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Crestron.Panopto.Common
{
    public class EmptyCommunication : ACommunication
    {
        public EmptyCommunication()
        {
            this.Type = "None";
        }

        public EmptyCommunication(Communication coms)
            : base(coms)
        {
            this.Type = "None";
        }
    }
}