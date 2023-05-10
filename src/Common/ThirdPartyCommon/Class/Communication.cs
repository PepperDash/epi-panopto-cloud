// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.


using System.Collections.Generic;

using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common
{
    /// <summary>
    /// Specifies any communication settings on a driver such as the COM port specifications,
    /// polling settings, and command intervals.
    /// </summary>
    public class Communication
    {
        /// <summary>
        /// The type of communication used to communicate with the third-party device.
        /// </summary>
        public CommunicationType CommunicationType { get; set; }

        /// <summary>
        /// The ethernet port used for communication
        /// <para>If this driver is not using ethernet for communication, this should be set to 0.</para>
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Specifies if the driver should poll automatically.
        /// <para>If this is set to false, then the driver will not poll unless 
        /// the driver sets <see cref="Crestron.Panopto.Common.BasicDriver.ABaseDriverProtocol.PollingEnabled"/>
        /// to true.</para>
        /// <para>This should be set to true even if <see cref="Crestron.Panopto.Common.BasicDriver.Feedback.SupportsUnsolicitedFeedback"/> is set to true. 
        /// This allows the driver to poll at least once to get initial states.</para>
        /// </summary>
        public bool EnableAutoPolling { get; set; }

        /// <summary>
        /// Specifies if the driver should reconnect to a device automatically if there was a disconnect.
        /// <para>This is not used if the driver uses the COM port.</para>
        /// </summary>
        public bool EnableAutoReconnect { get; set; }

        /// <summary>
        /// Specifies the amount of time in ms to wait in between sending commands.
        /// <para>This value must be greater than 0 and a multiple of 250. 
        /// If it is not, it will be automatically quantized to be a multiple of 250.
        /// Example: A value of 200 will be treated as 250 internally and a a value of 251 will be treated 
        /// as 500 internally.</para>
        /// <para>If the driver wishes to specify a value less than 250, then it should set <see cref="TimeBetweenCommandsOverride"/>.</para>
        /// </summary>
        public uint TimeBetweenCommands { get; set; }

        /// <summary>
        /// Specifies the amount of time in ms to wait in between sending commands.
        /// <para>If this is not null, this will be used instead of <see cref="TimeBetweenCommands"/>.</para>
        /// <para>This value may be 0 and must be a multiple of 25.</para>
        /// <para>This value should only be set in the driver's embedded JSON file and should not be referenced by the driver.</para>
        /// </summary>
        public uint? TimeBetweenCommandsOverride { get; set; }

        /// <summary>
        /// Specifies the amount of time in ms to wait before considering a polling command to be timed out.
        /// </summary>
        public uint ResponseTimeout { get; set; }

        /// <summary>
        /// Specifies if the driver should wait for a response to a previous command before sending another command.
        /// This is tracked using <see cref="Crestron.Panopto.Common.BasicDriver.ABaseDriverProtocol.ValidateResponse"/> 
        /// when it returns <see cref="Crestron.Panopto.Common.BasicDriver.ValidatedRxData"/> 
        /// with <see cref="Crestron.Panopto.Common.BasicDriver.ValidatedRxData.Ready"/> set to true.
        /// </summary>
        public bool WaitForResponse { get; set; }

        /// <summary>
        /// Specifies the protocol used for ethernet communication.
        /// <para>This should be set to NotSpecified if the driver doesn't use ethernet for communication.</para>
        /// </summary>
        public ethernetProtocol IpProtocol { get; set; }

        /// <summary>
        /// Specifies if there are any values in the Communciation node that can be modified.
        /// <para>This is used by applications to potentially allow users to change the baud rate and other settings.
        /// Applications will look at <see cref="UserAdjustableProperties"/> to find which properties can be changed.</para>
        /// </summary>
        public bool IsUserAdjustable { get; set; }

        /// <summary>
        /// Specifies any authentication settings for communciation with the device.
        /// <para>This is referring to APIs that require the driver to send a username and/or password.</para>
        /// </summary>
        public AuthenticationNode Authentication { get; set; }

        /// <summary>
        /// Not used by the framework.
        /// </summary>
        public bool IsSecure { get; set; }

        /// <summary>
        /// Specifies which properties are adjustable. This is used by applications.
        /// <para>Valid values are defined in <see cref="Crestron.Panopto.Common.Enums.eTransportAdjustableProperties"/>.</para> 
        /// </summary>
        public List<string> UserAdjustableProperties { get; set; }

        /// <summary>
        /// Specifies the default device ID that should be used for communication.
        /// <para>This is used when the API requires an ID to be included in commands sent to the device.</para>
        /// </summary>
        public int DeviceId { get; set; }
    }
}
