// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Collections.Generic;

namespace Crestron.Panopto.Common
{
    public class SupportedCommand
    {
        public string Label;
        public Crestron.Panopto.Common.Enums.StandardCommandsEnum StandardCommandEnum;
        public int StandardCommandId { get { return (int)StandardCommandEnum; } }

        public SupportedCommand(string label, Crestron.Panopto.Common.Enums.StandardCommandsEnum standardCommandEnum)
        {
            Label = label;
            StandardCommandEnum = standardCommandEnum;
        }
    }
}