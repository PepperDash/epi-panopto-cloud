// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Collections.Generic;
using Crestron.RAD.Common.Enums;

namespace Crestron.RAD.Common.StandardCommands
{
    public static class VideoServerStandardCommands
    {
        public static List<StandardCommandsEnum> CommandCollection
        {
            get
            {
                return new List<StandardCommandsEnum>
                {
                    StandardCommandsEnum.DownArrow,
                    StandardCommandsEnum.LeftArrow,
                    StandardCommandsEnum.RightArrow,
                    StandardCommandsEnum.UpArrow,
                    StandardCommandsEnum.Enter,
                    StandardCommandsEnum.Home,
                    StandardCommandsEnum.Clear,
                    StandardCommandsEnum.Exit,
                    StandardCommandsEnum._0,
                    StandardCommandsEnum._1,
                    StandardCommandsEnum._2,
                    StandardCommandsEnum._3,
                    StandardCommandsEnum._4,
                    StandardCommandsEnum._5,
                    StandardCommandsEnum._6,
                    StandardCommandsEnum._7,
                    StandardCommandsEnum._8,
                    StandardCommandsEnum._9,
                    StandardCommandsEnum.ForwardScan,
                    StandardCommandsEnum.ReverseScan,
                    StandardCommandsEnum.Pause,
                    StandardCommandsEnum.Play,
                    StandardCommandsEnum.Repeat,
                    StandardCommandsEnum.Return,
                    StandardCommandsEnum.Select,
                    StandardCommandsEnum.Stop,
                    StandardCommandsEnum.ForwardSkip,
                    StandardCommandsEnum.ReverseSkip,
                    StandardCommandsEnum.Menu,
                    StandardCommandsEnum.Back,
                    StandardCommandsEnum.Octothorpe,
                    StandardCommandsEnum.Asterisk,
                    StandardCommandsEnum.KeypadBackSpace,
                    StandardCommandsEnum.Dash,
                    StandardCommandsEnum.Period,
                    StandardCommandsEnum.Search
                };
            }
        }
    }
}
