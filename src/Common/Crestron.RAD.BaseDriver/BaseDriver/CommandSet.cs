// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.ExtensionMethods;

namespace Crestron.RAD.BaseDriver
{
    public class CommandSet
    {
        private string _command;
        public CommandSet(string name, string commmand, CommonCommandGroupType groups,
            Action callback, bool prepared, CommandPriority priority, StandardCommandsEnum commandEnum)
        {
            CommandName = name;
            Command = commmand;
            CommandGroup = groups;
            CallBack = callback;
            CommandPrepared = prepared;
            CommandPriority = priority;
            StandardCommand = commandEnum;
        }

        public bool CommandPrepared { get; set; }
        public string Command
        {
            get { return _command; }
            set { _command = value.GetSafeCommandString(); }
        }
        public Action CallBack { get; set; }
        public CommonCommandGroupType CommandGroup { get; private set; }
        public CommandPriority CommandPriority { get; set; }
        public string CommandName { get; private set; }
        public StandardCommandsEnum StandardCommand { get; private set; }
        public object[] Parameters { get; private set; }

        public bool IsPollingCommand
        {
            get
            {
                return new List<StandardCommandsEnum>
                {
                    StandardCommandsEnum.MutePoll,
                    StandardCommandsEnum.AspectRatioPoll,
                    StandardCommandsEnum.AvPoll,
                    StandardCommandsEnum.ChannelPoll,
                    StandardCommandsEnum.ChapterElapsedTimePoll,
                    StandardCommandsEnum.ChapterPoll,
                    StandardCommandsEnum.ChapterRemainingTimePoll,
                    StandardCommandsEnum.EnergyStarPoll, 
                    StandardCommandsEnum.InputPoll,
                    StandardCommandsEnum.LampHoursPoll,
                    StandardCommandsEnum.MainVideoSourcePoll,
                    StandardCommandsEnum.MicMutePoll,
                    StandardCommandsEnum.MutePoll,
                    StandardCommandsEnum.OnScreenDisplayPoll,
                    StandardCommandsEnum.PipLocationPoll,
                    StandardCommandsEnum.PlayBackStatusPoll,
                    StandardCommandsEnum.PowerPoll,
                    StandardCommandsEnum.SelfViewPoll,
                    StandardCommandsEnum.TotalElapsedTimePoll,
                    StandardCommandsEnum.TotalRemainingTimePoll,
                    StandardCommandsEnum.TrackElapsedTimePoll,
                    StandardCommandsEnum.TrackPoll,
                    StandardCommandsEnum.TrackRemainingTimePoll,
                    StandardCommandsEnum.VideoMutePoll,
                    StandardCommandsEnum.VolumePoll
                }.Contains(StandardCommand);
            }
        }
    }

    public enum CommandPriority
    {
        Special = 0,        
        Highest = 1,        
        High = 2,           
        Normal = 3,         
        Low = 4,            
        Lowest = 5          
    }
}
