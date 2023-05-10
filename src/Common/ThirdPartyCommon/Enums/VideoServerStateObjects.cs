// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.Panopto.Common.Enums
{
    public enum VideoServerStateObjects
    {
        Connection = 0,
        Power = 1,
        Authentication = 2,
        WarmingUp = 3,
        WarmedUp = 4,
        CoolingDown = 5,
        CooledDown = 6,
        PoweredOn = 7,
        PoweredOff = 8,
        ActiveMediaService = 19,
        ActiveMediaServicePlaybackState = 20,
        Channel = 21
    }
}