// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IChannel
    {
        /// <summary>
        /// Property indicating that the Channel Feedback is supported.
        /// </summary>
        bool SupportsChannelFeedback { get; }

        /// <summary>
        /// Gets the Device's channel.
        /// </summary>
        string Channel { get; }

        /// <summary>
        /// Property indicating that the change channel command is supported.
        /// </summary>
        bool SupportsChangeChannel { get; }

        /// <summary>
        /// Method to send the Channel Down command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released</param>
        void ChannelDown(CommandAction action);

        /// <summary>
        /// Method to send the Channel Up command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released</param>
        void ChannelUp(CommandAction action);

        /// <summary>
        /// Property indicating that the set channel command is supported.
        /// </summary>
        bool SupportsSetChannel { get; }

        /// <summary>
        /// Method to turn the Device volume to a given channel number.
        /// </summary>
        /// <param name="value">The channel number to set.</param>
        void SetChannel(string value);

        /// <summary>
        /// Property indicating that the Guide command is supported.
        /// </summary>
        bool SupportsGuide { get; }

        /// <summary>
        /// Method to send the Guide command to the Device.
        /// </summary>
        void Guide();

        /// <summary>
        /// Property indicating that the Device supports page changes.
        /// </summary>
        bool SupportsPageChange { get; }

        /// <summary>
        /// Method to send the Page Down command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released</param>
        void PageDown(CommandAction action);

        /// <summary>
        /// Method to send the Page Up command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released</param>
        void PageUp(CommandAction action);
    }
}
