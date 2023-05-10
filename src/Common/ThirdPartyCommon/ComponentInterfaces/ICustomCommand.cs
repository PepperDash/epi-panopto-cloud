// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface ICustomCommand
    {
        /// <summary>
        /// Method to send a custom command.
        /// </summary>
        /// <param name="command">Custom command.</param>
        void SendCustomCommand(string command);

        /// <summary>
        /// Enables RxOut event.
        /// </summary>
        bool EnableRxOut { get; set; }

        /// <summary>
        /// Sends strings sent from the device.
        /// </summary>
        event Action<string> RxOut;
    }

    public interface ICustomCommandCollection
    {
        /// <summary>
        /// Gets list of custom command names.
        /// </summary>
        List<string> CustomCommandNames { get; }

        /// <summary>
        /// Check if custom command exists by name of command.
        /// </summary>
        /// <param name="commandName">Command name to search by</param>
        /// <returns></returns>
        bool CheckIfCustomCommandExists(string commandName);

        /// <summary>
        /// Add custom command by providing name, value, and list of parameters.
        /// </summary>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandValue">Value of command</param>
        /// <param name="parameters">Collection of parameters</param>
        void AddCustomCommand(string commandName, string commandValue, List<Parameters> parameters);

        /// <summary>
        /// Remove custom command by command name if it exists.
        /// </summary>
        /// <param name="commandName">Name of command</param>
        /// <returns></returns>
        bool RemoveCustomCommandByName(string commandName);

        /// <summary>
        /// Send custom command by command name if it exists.
        /// </summary>
        /// <param name="commandName">Name of command</param>
        void SendCustomCommandByName(string commandName);

        /// <summary>
        /// Send custom command value.
        /// </summary>
        /// <param name="commandValue">Value of command to send</param>
        void SendCustomCommandValue(string commandValue);
    }

    public interface ICustomCommandCollection2
    {
        /// <summary>
        /// Send custom command by command name if it exists. 
        /// Allows for specifying if this is a Tap/Hold/Release.
        /// </summary>
        void SendCustomCommandByName(string commandName, Crestron.Panopto.Common.Enums.CommandAction action);
    }
}
