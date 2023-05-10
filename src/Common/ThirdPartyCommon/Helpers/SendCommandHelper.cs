// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using Crestron.Panopto.Common.BasicDriver;
using Crestron.Panopto.Common.Enums;
using Crestron.Panopto.Common.ExtensionMethods;

namespace Crestron.Panopto.Common.Helpers
{
    public static class CommandHelper
    {
        #region Send

        private static bool IsPowerCommand(CommandSet commandSet)
        {
            return CommonCommandGroupType.Power == commandSet.CommandGroup;
        }

        private static bool IsInputCommand(CommandSet commandSet)
        {
            return CommonCommandGroupType.Input == commandSet.CommandGroup;
        }

        private static bool CanSendNonPollingInputCommand(CommandSet commandSet)
        {
            var handled = false;
            if (IsInputCommand(commandSet)
                && commandSet.IsPollingCommand == false)
            {
                handled = true;
                commandSet.CommandPriority = CommandPriority.Highest;
            }
            return handled;
        }

        private static bool CanSendPowerPollingCommand(CommandSet commandSet)
        {
            return IsPowerCommand(commandSet)
                && commandSet.IsPollingCommand;
        }

        public static SendCommandResult Send(SendCommandVariables variables, CommandSet commandSet)
        {
            if (variables.DoesNotExist() || !variables.DriverLoaded)
            {
                return new SendCommandResult
                {
                    SendToQueue = false,
                    SendToTransport = false
                };
            }

            var sendResult = new SendCommandResult {SendToQueue = false, SendToTransport = false};
            if (!variables.CanSendCommands && IsPowerCommand(commandSet))
            {
                sendResult.SendToTransport = true;
            }
            else if (variables.Queue)
            {
                if ((variables.WarmingUp || variables.CoolingDown)
                    && CanSendPowerPollingCommand(commandSet))
                {
                    sendResult.SendToTransport = true;
                    return sendResult;
                }
                if (variables.HasPower)
                {
                    if (variables.WarmingUp)
                    {
                        sendResult.SendToQueue = CanSendNonPollingInputCommand(commandSet);
                    }
                    else if (variables.CoolingDown)
                    {
                    }
                    else
                    {
                        if (variables.PendingRequest.DoesNotExist())
                        {
                            sendResult.SendToTransport = true;
                        }
                        else
                        {
                            sendResult.SendToQueue = true;
                        }
                    }
                }
                else
                {
                    if (variables.WarmingUp)
                    {
                        sendResult.SendToQueue = CanSendNonPollingInputCommand(commandSet);
                    }
                    else if (variables.CoolingDown)
                    {
                    }
                    else
                    {
                        if (IsPowerCommand(commandSet))
                        {
                            sendResult.SendToTransport = true;
                        }
                        else
                        {
                            // **Log command not being sent or queued
                        }
                    }
                }
            }
            else
            {
                if (variables.HasPower || IsPowerCommand(commandSet))
                {
                    sendResult.SendToTransport = true;
                }
            }
            return sendResult;
        }

        #endregion

        #region Warmup / Cooldown

        public static bool HandleWarmupCallback(WarmupCallbackVariables variables,
            ref CommandSet command)
        {
            command.CallBack = (variables.SupportsLocalTimer && variables.HasPower == false)
                ? variables.Callback
                : null;
            return command.CallBack.Exists();
        }

        public static bool HandleCooldownCallback(CoolingCallbackVariables variables,
            ref CommandSet command)
        {
            command.CallBack = (variables.SupportsLocalTimer && variables.HasPower)
                ? variables.Callback
                : null;
            return command.CallBack.Exists();
        }

        #endregion
    }

    public class SendCommandResult
    {
        public bool SendToTransport { get; set; }
        public bool SendToQueue { get; set; }
    }

    public class SendCommandVariables
    {
        public bool Queue { get; set; }
        public bool CanSendCommands { get; set; }
        public bool HasPower { get; set; }
        public bool WarmingUp { get; set; }
        public bool CoolingDown { get; set; }
        public bool DriverLoaded { get; set; }
        public CommandSet PendingRequest { get; set; }
    }

    public class WarmupCallbackVariables
    {
        public bool IsWarmingUp { get; set; }
        public bool HasPower { get; set; }
        public bool SupportsLocalTimer { get; set; }
        public Action Callback { get; set; }
    }

    public class CoolingCallbackVariables
    {
        public bool IsCoolingDown { get; set; }
        public bool HasPower { get; set; }
        public bool SupportsLocalTimer { get; set; }
        public Action Callback { get; set; }
    }
}