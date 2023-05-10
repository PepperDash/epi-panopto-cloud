// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.Panopto.Common.Enums;
using System.Collections.Generic;
namespace Crestron.Panopto.Common.Interfaces
{
    public interface ISupportedCommandsHelper
    {
        /// <summary>
        /// Returns a list of SupportedCommand for all custom and non-custom/standard commands supported by the driver
        /// </summary>
        List<SupportedCommand> SupportedCommands { get; }
    }
}