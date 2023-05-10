// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.RAD.Common.Enums;
namespace Crestron.RAD.Common.Interfaces
{
    public interface IAvrZone : IPower, IInputs, IVolume, IAudio, ITuner, ISurround, IMediaTransport, IChannel, ICRPCMediaPlayer
    {
        #region MediaPlayer

        /// <summary>
        /// Property indicating that the Media Player Playback Status Feedback is supported
        /// </summary>
        bool SupportsMediaPlayerPlaybackStatus { get; }

        /// <summary>
        /// Property indicating the playback status
        /// </summary>
        MediaPlayerPlaypackStatus MediaPlayerPlaybackStatus { get; }

        #endregion
    }
}