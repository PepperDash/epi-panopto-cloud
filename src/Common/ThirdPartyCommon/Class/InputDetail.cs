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
    /// Used for Video Inputs
    /// </summary>
    public class InputDetail
    {
        #region Properties

        public VideoConnections InputType { get; set; }
        public VideoConnectionTypes InputConnector { get; set; }
        public string Description { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create input detail for video connection
        /// </summary>
        /// <param name="inputType">VideoConnections</param>
        /// <param name="inputConnector">VideoConnectionsType</param>
        /// <param name="description">string description</param>
        public InputDetail(VideoConnections inputType, VideoConnectionTypes inputConnector, string description) 
        {
            InputType = inputType;
            InputConnector = inputConnector;
            Description = description;
        }

        public InputDetail() { }

        #endregion
    }
}
