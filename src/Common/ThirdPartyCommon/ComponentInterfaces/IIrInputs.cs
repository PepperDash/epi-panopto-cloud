// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be creproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IIrInputs
    {
        /// <summary>
        /// Method to select an input source.  VideoConnections is a superset of all possible inputs.  For finding available inputs per device use GetUsableInput.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void SetInputSource(VideoConnections input, IrActions action);
    }
}
