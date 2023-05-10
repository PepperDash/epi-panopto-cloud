// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common
{
    public class AudioToneDetail
    {
        public uint CurrentLevel { get; set; }
        public uint MaxLevel { get; private set; }
        public uint MinLevel { get; private set; }
        public bool IsRamping { get; set; }
        public RampingVolumeState RampingToneState { get; set; }
        public uint UnscaledRampingToneLevel { get; set; }

        public AudioToneDetail(uint currentLevel, uint maxLevel, uint minLevel, bool isRamping,
            RampingVolumeState rampingToneState, uint unscaledRampingToneLevel)
        {
            CurrentLevel = currentLevel;
            MaxLevel = maxLevel;
            MinLevel = minLevel;
            IsRamping = isRamping;
            RampingToneState = rampingToneState;
            UnscaledRampingToneLevel = unscaledRampingToneLevel;
        }
    }
}