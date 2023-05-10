// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using Crestron.RAD.Common.Enums;

namespace Crestron.RAD.Common.Helpers
{
    public class HandleIrInputVariables
    {
        public bool FeatureSupport { get; set; }
        public string FeatureName { get; set; }
        public string CommandName { get; set; }
        public IrActions ActionType { get; set; }
        public Action<IrActions> ActionMethod { get; set; }
        public bool IsPowerOn { get; set; }
        public bool IsPowerOff { get; set; }
        public bool IsInput { get; set; }
        public bool SendCommands { get; set; }
        public uint WarmupTime { get; set; }
        public uint CooldownTime { get; set; }
    }

    public class IrCommandResult
    {
        public bool NeedToQueue { get; set; }
        public bool NeedToTap { get; set; }
        public bool NotSupported { get; set; }
        public uint Delay { get; set; }
        public bool WarmingUp { get; set; }
        public bool CoolingDown { get; set; }
        public bool ResetDelay { get; set; }
    }

    public class IrCommand
    {
        public string Command { get; set; }
        public uint Delay { get; set; }
        public bool WarmingUp { get; set; }
        public bool CoolingDown { get; set; }
        public IrActions ActionType { get; set; }
        public Action<IrActions> ActionMethod { get; set; }
    }

    public static class HandleIrCommandHelper
    {
        /// <summary>
        /// Handle the logic of issuing IR command, sending to queue, and logging for no support.
        /// </summary>
        /// <param name="variables"></param>
        /// <returns></returns>
        public static IrCommandResult HandleIrCommand(HandleIrInputVariables variables)
        {
            var result = new IrCommandResult();
            if (variables.FeatureSupport == false)
            {
                result.NotSupported = true;
            }
            else
            {
                result.Delay = GetDelayValue(variables);
                if (variables.SendCommands)
                {
                    result.WarmingUp = variables.IsPowerOn;
                    result.CoolingDown = variables.IsPowerOff;
                    result.ResetDelay = (variables.IsPowerOff || variables.IsPowerOn);
                    result.NeedToTap = true;
                }
                else if (variables.IsInput && variables.ActionType == IrActions.Pulse)
                {
                    result.NeedToQueue = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Get Appropriate value of delay depending on power on/off values.
        /// If both values are true, it is assumed toggle, and get larger value.
        /// </summary>
        /// <param name="variables"></param>
        /// <returns></returns>
        private static uint GetDelayValue(HandleIrInputVariables variables)
        {
            uint result = 0;

            if (variables.IsPowerOn && variables.IsPowerOff)
                result = Math.Max(variables.WarmupTime, variables.CooldownTime);
            else if (variables.IsPowerOn)
                result = variables.WarmupTime;
            else if (variables.IsPowerOff)
                result = variables.CooldownTime;
            return result;
        }
    }
}