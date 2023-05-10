// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.Panopto.Common.Enums
{
    /// <summary>
    /// Fields in .json that if specified will generate fields that are adjustable in resulting com .dat file
    /// 
    /// </summary>
    public enum eTransportAdjustableProperties
    {
        ComspecAdjustableBaud = 0,
        ComspecAdjustableParity = 1,
        ComspecAdjustableDataBits = 2,
        ComspecAdjustableStopBits = 3,
        ComspecAdjustableHardwareHandshaking = 4,
        ComspecAdjustableSoftwareHandshaking = 5,
        ComspecAdjustableDeviceId = 6,
        EthernetAdjustablePort = 7,
        EthernetAdjustableDeviceId = 8
    }
}
