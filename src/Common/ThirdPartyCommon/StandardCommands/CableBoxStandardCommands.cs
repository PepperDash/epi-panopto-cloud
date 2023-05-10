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
    public static class CableBoxStandardCommands
    {
        public static List<StandardCommandsEnum> CommandCollection
        {
            get
            {
                return new List<StandardCommandsEnum>
                {
                    StandardCommandsEnum.PowerOn,
                    StandardCommandsEnum.PowerOff,
                    StandardCommandsEnum.Power,
                    StandardCommandsEnum.PowerPoll,
                    StandardCommandsEnum.Vol,
                    StandardCommandsEnum.VolMinus,
                    StandardCommandsEnum.VolPlus,
                    StandardCommandsEnum.VolumePoll,
                    StandardCommandsEnum.MuteOn,
                    StandardCommandsEnum.MuteOff,
                    StandardCommandsEnum.Mute,
                    StandardCommandsEnum.MutePoll,
                    StandardCommandsEnum.DownArrow,
                    StandardCommandsEnum.LeftArrow,
                    StandardCommandsEnum.RightArrow,
                    StandardCommandsEnum.UpArrow,
                    StandardCommandsEnum.Select,
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
                    StandardCommandsEnum.Octothorpe,
                    StandardCommandsEnum.Asterisk,
                    StandardCommandsEnum.KeypadBackSpace,
                    StandardCommandsEnum.Enter,
                    StandardCommandsEnum.Clear,
                    StandardCommandsEnum.Exit,
                    StandardCommandsEnum.Home,
                    StandardCommandsEnum.Menu,
                    StandardCommandsEnum.Dvr,
                    StandardCommandsEnum.Live,
                    StandardCommandsEnum.Record,
                    StandardCommandsEnum.SpeedSlow,
                    StandardCommandsEnum.ForwardScan,
                    StandardCommandsEnum.ReverseScan,
                    StandardCommandsEnum.Play,
                    StandardCommandsEnum.Pause,
                    StandardCommandsEnum.Stop,
                    StandardCommandsEnum.ForwardSkip,
                    StandardCommandsEnum.ReverseSkip,
                    StandardCommandsEnum.Repeat,
                    StandardCommandsEnum.Return,
                    StandardCommandsEnum.Back,
                    StandardCommandsEnum.ChannelUp,
                    StandardCommandsEnum.ChannelDown,
                    StandardCommandsEnum.Channel,
                    StandardCommandsEnum.ChannelPoll,
                    StandardCommandsEnum.Guide,
                    StandardCommandsEnum.PageDown,
                    StandardCommandsEnum.PageUp,
                    StandardCommandsEnum.A,
                    StandardCommandsEnum.B,
                    StandardCommandsEnum.C,
                    StandardCommandsEnum.D,
                    StandardCommandsEnum.Favorite,
                    StandardCommandsEnum.Info,
                    StandardCommandsEnum.Last,
                    StandardCommandsEnum.Replay,
                    StandardCommandsEnum.ThumbsUp,
                    StandardCommandsEnum.ThumbsDown,
                    StandardCommandsEnum.Dash,
                    StandardCommandsEnum.Period,
                    StandardCommandsEnum.Blue,
                    StandardCommandsEnum.Green,
                    StandardCommandsEnum.Red,
                    StandardCommandsEnum.Yellow,
                    StandardCommandsEnum.EnergyStarOn,
                    StandardCommandsEnum.EnergyStarOff,
                    StandardCommandsEnum.EnergyStar,
                    StandardCommandsEnum.EnergyStarPoll
                };
            }
        }
    }
}
