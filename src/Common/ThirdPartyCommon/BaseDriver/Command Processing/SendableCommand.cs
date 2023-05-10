using Crestron.Panopto.Common.Enums;
using Crestron.SimplSharp;
namespace Crestron.Panopto.Common.BasicDriver
{
    /// <summary>
    /// Represents a command that can be sent to the device or to the queue. 
    /// </summary>
    internal sealed class Command
    {
        /// <summary>
        /// Unique identifier of the object. 
        /// This Id could be used when a command response is received 
        /// and the command object needs to be removed from the queue.
        /// </summary>
        internal ulong Id { get; private set; }

        /// <summary>
        /// CommandSet object to use when sending to the device
        /// </summary>
        internal CommandSet CommandSet { get; set; }

        /// <summary>
        /// Command supports feedback from the device.
        /// </summary>
        internal bool SupportsFeedback { get; private set; }

        /// <summary>
        /// Command is persistent and should not be removed from the queue. 
        /// This is commonly used for "keep-alive" commands.
        /// </summary>
        internal bool Persistent { get; private set; }

        /// <summary>
        /// Timestamp when the command was sent to the device. 
        /// This will be used to determine if the command timeout value has been reached.
        /// </summary>
        internal int TransmissionTickCount { get; set; }

        /// <summary>
        /// Command is waiting for feedback response. 
        /// This will be set by the Process Command Queue logic after the command is sent and "SupportsFeedback" is True.
        /// </summary>
        internal bool WaitingForFeedback { get; set; }

        /// <summary>
        /// Indicates this command needs to be removed from the queue.
        /// </summary>
        internal bool MarkedForRemoval { get; set; }

        internal bool IsSendable(bool warmingUp, bool coolingDown, bool powerIsOn, bool supportsPowerFeedback)
        {
            string notUsed = null;
            return IsSendable(warmingUp, coolingDown, powerIsOn, supportsPowerFeedback, false, out notUsed);
        }

        internal bool IsSendable(bool warmingUp, bool coolingDown, bool powerIsOn, bool supportsPowerFeedback, bool log, out string logMessage)
        {
            var isSendable = false;
            logMessage = null;

            if (warmingUp)
            {
                isSendable = _isSendableDuringWarmup;

                if (!isSendable && log)
                    logMessage = "Only the PowerPoll and PowerOn commands with no callback are allowed during WarmUp";
            }
            else if (coolingDown)
            {
                isSendable = _isSendableDuringCooldown;

                if (!isSendable && log)
                    logMessage = "Only PowerPoll is allowed during CoolDown";
            }
            else if (!powerIsOn)
            {
                isSendable = supportsPowerFeedback ? _isSendableWhenPowerIsOff : true;

                if (!isSendable && log)
                    logMessage = "Only the Power command group is allowed while power is off";
            }
            else
                isSendable = true;

            return isSendable;
        }

        internal bool IsQueueable(bool warmingUp, bool coolingDown, bool powerIsOn, bool supportsPowerFeedback)
        {
            string notUsed = null;
            return IsQueueable(warmingUp, coolingDown, powerIsOn, supportsPowerFeedback, false, out notUsed);
        }
        internal bool IsQueueable(bool warmingUp, bool coolingDown, bool powerIsOn, bool supportsPowerFeedback, bool log, out string logMessage)
        {
            var isQueueable = false;
            logMessage = null;

            if (warmingUp)
            {
                isQueueable = _isQueueableDuringWarmup;
                if (!isQueueable && log)
                    logMessage = "Only Input/AudioInput/MediaService/Power command groups are allowed during WarmUp";
            }
            else if (coolingDown)
            {
                isQueueable = _isQueueableDuringCooldown;
                if (!isQueueable && log)
                    logMessage = "Only the Power command group is allowed during CoolDown";
            }
            else if (!powerIsOn)
            {
                isQueueable = supportsPowerFeedback ? _isQueueableWhenPowerIsOff : true;
                if (!isQueueable && log)
                    logMessage = "Only the Power command group is allowed while power is off";
            }
            else
                isQueueable = true;

            return isQueueable;
        }

        /// <summary>
        /// Finds the first occourence of the the command's StandardCommandEnum value and CommandGroup in the
        /// non-polling queue and replaces it with the given instance.
        /// </summary>
        /// <returns>True if something was modified</returns>
        internal bool IsReplaceableByCommandGroup
        {
            get
            {
                if (CommandSet == null) { return false; }

                var isReplaceable = false;

                switch (CommandSet.CommandGroup)
                {
                    case CommonCommandGroupType.AvrZone1:
                    case CommonCommandGroupType.AvrZone2:
                    case CommonCommandGroupType.AvrZone3:
                    case CommonCommandGroupType.AvrZone4:
                    case CommonCommandGroupType.AvrZone5:
                        isReplaceable =
                            CommandSet.SubCommandGroup.Equals(CommonCommandGroupType.Power) ||
                            CommandSet.SubCommandGroup.Equals(CommonCommandGroupType.Input) || CommandSet.SubCommandGroup.Equals(CommonCommandGroupType.AudioInput);
                        break;
                    default:
                        isReplaceable =
                            CommandSet.CommandGroup.Equals(CommonCommandGroupType.Power) ||
                            CommandSet.CommandGroup.Equals(CommonCommandGroupType.Input) ||
                            CommandSet.CommandGroup.Equals(CommonCommandGroupType.AudioInput) ||
                            CommandSet.CommandGroup.Equals(CommonCommandGroupType.MediaService);
                        break;
                }
                return isReplaceable;
            }
        }

        internal CommonCommandGroupType AlternateCommandGroup
        {
            get
            {
                if (CommandSet == null) { return CommonCommandGroupType.Unknown; }

                switch (CommandSet.CommandGroup)
                {
                    case CommonCommandGroupType.MediaService:
                        return CommonCommandGroupType.Input;

                    case CommonCommandGroupType.Input:
                        return CommonCommandGroupType.MediaService;

                    default:
                        return CommonCommandGroupType.Unknown;
                }
            }
        }

        internal bool IsReplaceableByStandardCommand
        {
            get
            {
                if (CommandSet == null) { return false; }

                return CommandSet.StandardCommand.Equals(StandardCommandsEnum.Vol) ||
                    CommandSet.StandardCommand.Equals(StandardCommandsEnum.Channel) ||
                    CommandSet.StandardCommand.Equals(StandardCommandsEnum.ToneSetBass) ||
                    CommandSet.StandardCommand.Equals(StandardCommandsEnum.ToneSetTreble);
            }
        }

        /// <summary>
        /// Specifies if this command is something that can have associated feedback faked 
        /// using <see cref="Crestron.Panopto.Common.BasicDriver.FakeFeedback.FeedbackController"/>
        /// </summary>
        internal bool IsFakeFeedbackCommand
        {
            get
            {
                if (CommandSet == null) { return false; }

                return
                    (!CommandSet.CommandGroup.Equals(CommonCommandGroupType.AvrZone1) &&
                    !CommandSet.CommandGroup.Equals(CommonCommandGroupType.AvrZone2) &&
                    !CommandSet.CommandGroup.Equals(CommonCommandGroupType.AvrZone3) &&
                    !CommandSet.CommandGroup.Equals(CommonCommandGroupType.AvrZone4) &&
                    !CommandSet.CommandGroup.Equals(CommonCommandGroupType.AvrZone5) &&

                    // Volume commands result in the mute state being faked. 
                    // This will not fake volume level feedback.
                    CommandSet.CommandGroup.Equals(CommonCommandGroupType.Volume) &&
                     !CommandSet.StandardCommand.Equals(StandardCommandsEnum.VolumePoll)) ||


                    // This set of command groups and standard commands fake the state 
                    // tied to their command/group.
                    CommandSet.CommandGroup.Equals(CommonCommandGroupType.Input) ||
                    CommandSet.CommandGroup.Equals(CommonCommandGroupType.AudioInput) ||


                    CommandSet.StandardCommand.Equals(StandardCommandsEnum.PowerOn) ||
                    CommandSet.StandardCommand.Equals(StandardCommandsEnum.PowerOff) ||
                    CommandSet.StandardCommand.Equals(StandardCommandsEnum.Power) ||


                    CommandSet.StandardCommand.Equals(StandardCommandsEnum.MuteOn) ||
                    CommandSet.StandardCommand.Equals(StandardCommandsEnum.MuteOff) ||
                    CommandSet.StandardCommand.Equals(StandardCommandsEnum.Mute) ||


                    CommandSet.StandardCommand.Equals(StandardCommandsEnum.VideoMuteOn) ||
                    CommandSet.StandardCommand.Equals(StandardCommandsEnum.VideoMuteOff) ||
                    CommandSet.StandardCommand.Equals(StandardCommandsEnum.VideoMute);
            }
        }


        private bool _isSendableDuringWarmup
        {
            get
            {
                if (CommandSet == null) { return false; }

                return CommandSet.StandardCommand.Equals(StandardCommandsEnum.PowerPoll) ||
                    (CommandSet.StandardCommand.Equals(StandardCommandsEnum.PowerOn) &&
                    CommandSet.CallBack == null);
            }
        }

        private bool _isQueueableDuringWarmup
        {
            get
            {
                if (CommandSet == null) { return false; }

                return CommandSet.CommandGroup.Equals(CommonCommandGroupType.Input) ||
                    CommandSet.CommandGroup.Equals(CommonCommandGroupType.AudioInput) ||
                    CommandSet.CommandGroup.Equals(CommonCommandGroupType.MediaService) ||
                    CommandSet.CommandGroup.Equals(CommonCommandGroupType.Power);
            }
        }

        private bool _isSendableDuringCooldown
        {
            get
            {
                if (CommandSet == null) { return false; }

                return CommandSet.StandardCommand.Equals(StandardCommandsEnum.PowerPoll);
            }
        }

        private bool _isQueueableDuringCooldown
        {
            get
            {
                if (CommandSet == null) { return false; }

                return CommandSet.CommandGroup.Equals(CommonCommandGroupType.Power);
            }
        }

        private bool _isSendableWhenPowerIsOff
        {
            get
            {
                if (CommandSet == null) { return false; }

                return CommandSet.CommandGroup.Equals(CommonCommandGroupType.Power) ||
                    CommandSet.CommandGroup.Equals(CommonCommandGroupType.Connection);
            }
        }

        private bool _isQueueableWhenPowerIsOff
        {
            get
            {
                if (CommandSet == null) { return false; }

                return CommandSet.CommandGroup.Equals(CommonCommandGroupType.Power) ||
                    CommandSet.CommandGroup.Equals(CommonCommandGroupType.Connection);
            }
        }

        internal Command(ulong id, CommandSet commandSet, bool supportsFeedback, bool persistent)
        {
            Id = id;
            CommandSet = commandSet;
            SupportsFeedback = supportsFeedback;
            Persistent = persistent;

            MarkedForRemoval = false;
            TransmissionTickCount = 0;
            WaitingForFeedback = false;
        }

        public override bool Equals(object obj)
        {
            var command = obj as Command;

            return command == null ? false : command.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            // Equals is overridden to only look at the immutable property Id,
            // so this method will do the same
            return Id.GetHashCode();
        }
    }
}