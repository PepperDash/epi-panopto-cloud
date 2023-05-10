// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be creproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Collections.Generic;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IOutputs
    {
        /// <summary>
        /// Property indicating that the Video output Source feedback is supported.
        /// </summary>
        bool SupportsVideoOutputFeedback { get; }

        /// <summary>
        /// Returns the current video output source
        /// </summary>
        VideoOutputDetail VideoOutputSource { get; }

        /// <summary>
        /// Property indicating that the video output source command is supported.
        /// </summary>
        bool SupportsSetVideoOutputSource { get; }

        /// <summary>
        /// Method to select an video output source. VideoConnections is a superset of all
        /// possible outputs. For finding available inputs per device use
        /// GetUsableVideoOutputs.
        /// </summary>
        /// <param name="input"></param>
        void SetVideoOutputSource(VideoConnections input);

        /// <summary>
        /// Gets array of the usable video outputs on this Device.
        /// </summary>
        /// <returns></returns>
        List<VideoOutputDetail> GetUsableVideoOutputs();

        /// <summary>
        /// Property indicating that the audio output Source feedback is supported.
        /// </summary>
        bool SupportsAudioOutputFeedback { get; }

        /// <summary>
        /// Returns the current audio output source
        /// </summary>
        AudioOutputDetail AudioOutputSource { get; }

        /// <summary>
        /// Property indicating that the audio output source command is supported.
        /// </summary>
        bool SupportsSetAudioOutputSource { get; }

        /// <summary>
        /// Method to select an audio output source. AudioConnections is a superset of all
        /// possible outputs. For finding available outputs per device use
        /// GetUsableAudioInputs.
        /// </summary>
        /// <param name="input"></param>
        void SetAudioOutputSource(AudioConnections input);

        /// <summary>
        /// Gets array of the usable audio outputs on this Device.
        /// </summary>
        /// <returns></returns>
        List<AudioOutputDetail> GetUsableAudioOutputs();
    }
}