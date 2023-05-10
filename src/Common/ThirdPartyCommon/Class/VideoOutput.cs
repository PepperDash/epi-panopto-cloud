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
    /// Used for Video Outputs
    /// </summary>
    public class VideoOutput
    {
        #region Properties

        public VideoConnections OutputType { get; set; }
        public VideoConnectionTypes OutputConnector { get; set; }
        public string Description { get; set; }

        #endregion

        /// <summary>
        /// Create Video Output with video connection
        /// </summary>
        /// <param name="outputType">VideoConections</param>
        /// <param name="outputConnector">VideoConnectionsType</param>
        /// <param name="description">Description</param>
        public VideoOutput(VideoConnections outputType, VideoConnectionTypes outputConnector, string description)
        {
            OutputType = outputType;
            OutputConnector = outputConnector;
            Description = description;
        }

        public VideoOutput() { }
    }
}