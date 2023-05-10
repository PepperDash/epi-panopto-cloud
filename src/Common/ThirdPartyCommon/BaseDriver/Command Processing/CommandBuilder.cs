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
using Crestron.Panopto.Common.Helpers;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.BasicDriver
{
    public abstract partial class ABaseDriverProtocol
    {
        protected bool SendCommand(CommandSet command)
        {
            if (command != null)
            {
                return PrepareStringThenSend(command);
            }
            return false;
        }

        protected CommandSet BuildCommand(StandardCommandsEnum Type, CommonCommandGroupType Group, CommandPriority Priority, string Name)
        {
            return BuildCommand(Type, Group, Priority, Name, String.Empty);
        }

        protected CommandSet BuildCommand(StandardCommandsEnum Type, CommonCommandGroupType Group, CommandPriority Priority)
        {
            return BuildCommand(Type, Group, Priority, Type.ToString(), String.Empty);
        }

        protected CommandSet BuildCommand(StandardCommandsEnum Type, CommonCommandGroupType Group, CommandPriority Priority, string Name, string ModifiedCommand)
        {
            Commands command = null;
            CommandSet builtCommand = null;

            if (DriverData.CrestronSerialDeviceApi.Api.StandardCommands.TryGetValue(Type, out command))
            {
                var commandString = String.IsNullOrEmpty(ModifiedCommand) ? command.Command : ModifiedCommand;
                builtCommand = new CommandSet(Name, commandString, Group, null, false, Priority, Type)
                {
                    AllowIsQueueableOverride = command.AllowIsQueueableOverride,
                    AllowIsSendableOverride = command.AllowIsSendableOverride
                };
            }

            return builtCommand;
        }

        protected bool CheckIfCommandExists(StandardCommandsEnum Type)
        {
            return DriverData.CrestronSerialDeviceApi.Api.StandardCommands.ContainsKey(Type);
        }
    }
}
