// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be creproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IInputs
    {
        bool SupportsInputFeedback { get; }

        /// <summary>
        /// Returns the current input source
        /// </summary>
        InputDetail InputSource { get; }

        bool SupportsSetInputSource { get; }

        /// <summary>
        /// Method to select an input source.  VideoConnections is a superset of all possible inputs.  For finding available inputs per device use GetUsableInput.
        /// </summary>
        /// <param name="input"></param>
        void SetInputSource(VideoConnections input);

        /// <summary>
        /// Gets array of the usable inputs on this Device.
        /// </summary>
        InputDetail[] GetUsableInputs();
    }
}
