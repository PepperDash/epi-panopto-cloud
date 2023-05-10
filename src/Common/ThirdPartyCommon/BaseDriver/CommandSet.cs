// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using Crestron.Panopto.Common.Enums;
using Crestron.Panopto.Common.ExtensionMethods;

namespace Crestron.Panopto.Common.BasicDriver
{
    /// <summary>
    /// Represent a command that can be sent to the device. 
    /// These can be instantiated by the driver and they can call SendCommand 
    /// to send them to the device.
    /// </summary>
    public class CommandSet
    {
        private string _command;
        public CommandSet(string name, string commmand, CommonCommandGroupType groups,
            Action callback, bool prepared, CommandPriority priority, StandardCommandsEnum commandEnum)
        {
            CommandName = name;
            Command = commmand;
            CommandGroup = groups;
            SubCommandGroup = CommonCommandGroupType.Unknown;
            CallBack = callback;
            CommandPrepared = prepared;
            CommandPriority = priority;
            StandardCommand = commandEnum;
        }

        /// <summary>
        /// Specifies if the command is already prepared. 
        /// If overriding <see cref="ABaseDriverProtocol.PrepareStringThenSend"/> then this must be checked
        /// prior to preparing the command. This overriden method should also set this to true before calling 
        /// the base method.
        /// </summary>
        public bool CommandPrepared { get; set; }

        /// <summary>
        /// The command string that should be sent to the device.
        /// </summary>
        public string Command
        {
            get { return _command; }
            set
            {
                _command = value.GetSafeCommandString(out CommandSetterError);
            }
        }

        /// <summary>
        /// An error message that is set when the setter for <see cref="Command"/> had an exception.
        /// This will be logged by the command sending logic if it is not null.
        /// </summary>
        internal Exception CommandSetterError;

        public Action CallBack { get; set; }
        public CommonCommandGroupType CommandGroup { get; private set; }

        /// <summary>
        /// Primarly used for AVR Zones, but can be used for cases where the <see cref="CommandGroup"/> doesn't
        /// provide enough details about the group of commands this command belongs to.
        /// </summary>
        public CommonCommandGroupType SubCommandGroup { get; set; }

        public CommandPriority CommandPriority { get; set; }
        public string CommandName { get; private set; }
        public StandardCommandsEnum StandardCommand { get; private set; }
        public object[] Parameters { get; private set; }
        internal bool PrepareOnly { get; set; }
        internal Action<StandardCommandsEnum, CommonCommandGroupType> FakeFeedbackCallback { get; set; }

        internal bool IsNonStandardPollingCommand { get; set; }

        /// <summary>
        /// Avr Zone Field to allow for Sending of zone related commands
        /// </summary>
        public bool AllowIsSendableOverride { get; set; }

        /// <summary>
        /// Avr Zone Field to allow for queuing of zone related commands
        /// </summary>
        public bool AllowIsQueueableOverride { get; set; }

        /// <summary>
        /// Avr Zone Field to allow for removing zone-related commands from the queue
        /// </summary>
        public bool AllowRemoveCommandOverride { get; set; }

        private List<StandardCommandsEnum> _validPollingCommands;

        public bool IsPollingCommand
        {
            get
            {
                if (_validPollingCommands == null)
                {
                    _validPollingCommands = new List<StandardCommandsEnum>() {

                    StandardCommandsEnum.MutePoll,
                    StandardCommandsEnum.AspectRatioPoll,
                    StandardCommandsEnum.AvPoll,
                    StandardCommandsEnum.ChannelPoll,
                    StandardCommandsEnum.ChapterElapsedTimePoll,
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
                    StandardCommandsEnum.VolumePoll,
                    StandardCommandsEnum.TunerFrequencyPoll,
                    StandardCommandsEnum.SurroundModePoll,
                    StandardCommandsEnum.LoudnessPoll,
                    StandardCommandsEnum.ToneStatePoll,
                    StandardCommandsEnum.ToneBassPoll,
                    StandardCommandsEnum.ToneTreblePoll,
                    StandardCommandsEnum.AudioInputPoll };
                }
                return _validPollingCommands.Contains(StandardCommand);
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
