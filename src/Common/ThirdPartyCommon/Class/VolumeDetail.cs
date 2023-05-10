// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common
{
    public class VolumeDetail
    {
        public uint CurrentVolume { get; set; }
        public uint MaxVolume { get; private set; }
        public uint MinVolume { get; private set; }
        public bool IsRamping { get; set; }
        public RampingVolumeState RampingVolumeState { get; set; }
        public uint UnscaledRampingVolume { get; set; }

        public VolumeDetail(uint currentVolume, uint minVolume, uint maxVolume,
            bool isRamping, RampingVolumeState rampingVolumeState, uint unscaledRampingVolume)
        {
            CurrentVolume = currentVolume;
            MinVolume = minVolume;
            MaxVolume = maxVolume;
            IsRamping = isRamping;
            RampingVolumeState = rampingVolumeState;
            UnscaledRampingVolume = unscaledRampingVolume;
        }
    }
}