// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using Crestron.Panopto.Common.Enums;
using Crestron.Panopto.Common.Interfaces;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Crestron.Panopto.Common
{
    public class AudioOutputDetail : AConnectionDetail<AudioConnections,AudioConnectionTypes>
    {
        public AudioOutputDetail()
        {
            type = AudioConnections.Unknown;
            connector = AudioConnectionTypes.Unknown;
            description = string.Empty;
            friendlyName = string.Empty;
        }
    }
}