// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IMicrophoneControl
    {
        /// <summary>
        /// Method to mute the microphone
        /// </summary>
        void MicMuteOn();

        /// <summary>
        /// Method to unmute the microphone
        /// </summary>
        void MicMuteOff();

        /// <summary>
        /// Method to toggle mute state of the microphone
        /// </summary>
        void MicMute();

        /// <summary>
        /// Returns true if the microphone is muted
        /// </summary>
		bool MicMuted{ get; }
    }
}
