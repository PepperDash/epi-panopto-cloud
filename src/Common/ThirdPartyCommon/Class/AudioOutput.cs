// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common
{
    /// <summary>
    /// Used for Audio Outputs
    /// </summary>
    public class AudioOutput
    {
        #region Properties

        public AudioConnections OutputType { get; set; }
        public AudioConnectionTypes OutputConnector { get; set; }
        public string Description { get; set; }

        #endregion

        /// <summary>
        /// Create Audio Output with audio connection
        /// </summary>
        /// <param name="outputType">AudioConections</param>
        /// <param name="outputConnector">AudioConnectionsType</param>
        /// <param name="description">Description</param>
        public AudioOutput(AudioConnections outputType, AudioConnectionTypes outputConnector, string description)
        {
            OutputType = outputType;
            OutputConnector = outputConnector;
            Description = description;
        }

        public AudioOutput() { }
    }
}