// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IArtwork
    {
        /// <summary>
        /// Turns artwork mode on
        /// </summary>
        void ArtworkOn();

        /// <summary>
        /// Turns artwork mode off
        /// </summary>
        void ArtworkOff();

        /// <summary>
        /// Specifies if artwork on/off is supported by the driver
        /// </summary>
        bool SupportsArtworkMode { get; }

        /// <summary>
        /// Enables putting the device into artwork mode when powering it off
        /// </summary>
        void EnableArtworkOnPowerOff();

        /// <summary>
        /// Disable putting the device into artwork mode when powering it off
        /// </summary>
        void DisableArtworkOnPowerOff();

        /// <summary>
        /// Specifies if the device supports putting the device into artwork mode when powering it off
        /// </summary>
        bool SupportsArtworkModeOnPowerOff { get; }
    }
}