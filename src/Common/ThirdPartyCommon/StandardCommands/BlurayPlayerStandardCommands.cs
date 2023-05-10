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
    public static class BlurayPlayerStandardCommands
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
                    StandardCommandsEnum.Select,
                    StandardCommandsEnum.Eject,
                    StandardCommandsEnum.Enter,
                    StandardCommandsEnum.Home,
                    StandardCommandsEnum.Info,
                    StandardCommandsEnum.Power,
                    StandardCommandsEnum.PowerOff,
                    StandardCommandsEnum.PowerOn,
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
                    StandardCommandsEnum.Audio,
                    StandardCommandsEnum.Blue,
                    StandardCommandsEnum.Clear,
                    StandardCommandsEnum.Display,
                    StandardCommandsEnum.Exit,
                    StandardCommandsEnum.ForwardScan,
                    StandardCommandsEnum.ReverseScan,
                    StandardCommandsEnum.Green,
                    StandardCommandsEnum.Options,
                    StandardCommandsEnum.Pause,
                    StandardCommandsEnum.Play,
                    StandardCommandsEnum.Red,
                    StandardCommandsEnum.Repeat,
                    StandardCommandsEnum.Return,
                    StandardCommandsEnum.Stop,
                    StandardCommandsEnum.Subtitle,
                    StandardCommandsEnum.TopMenu,
                    StandardCommandsEnum.ForwardSkip,
                    StandardCommandsEnum.ReverseSkip,
                    StandardCommandsEnum.Yellow,
                    StandardCommandsEnum.PopUpMenu,
                    StandardCommandsEnum.Menu,
                    StandardCommandsEnum.Back,
                    StandardCommandsEnum.Octothorpe,
                    StandardCommandsEnum.Asterisk,
                    StandardCommandsEnum.KeypadBackSpace,
                    StandardCommandsEnum.Dash,
                    StandardCommandsEnum.Period,
                    StandardCommandsEnum.EnergyStarOn,
                    StandardCommandsEnum.EnergyStarOff,
                    StandardCommandsEnum.EnergyStar,
                    StandardCommandsEnum.EnergyStarPoll,
                };
            }
        }
    }
}
