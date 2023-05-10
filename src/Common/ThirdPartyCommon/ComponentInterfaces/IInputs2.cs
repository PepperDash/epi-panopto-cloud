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
    public interface IInputs2
    {
        /// <summary>
        /// Property indicating that the Video input Source feedback is supported.
        /// </summary>
        bool SupportsVideoInputFeedback { get; }

        /// <summary>
        /// Returns the current video input source
        /// </summary>
        VideoInputDetail VideoInputSource { get; }

        /// <summary>
        /// Property indicating that the video input source command is supported.
        /// </summary>
        bool SupportsSetVideoInputSource { get; }

        /// <summary>
        /// Method to select an video input source. VideoConnections is a superset of all
        /// possible inputs. For finding available inputs per device use
        /// GetUsableInput.
        /// </summary>
        /// <param name="input"></param>
        void SetVideoInputSource(VideoConnections input);

        /// <summary>
        /// Gets array of the usable video inputs on this Device.
        /// </summary>
        /// <returns></returns>
        List<VideoInputDetail> GetUsableVideoInputs();

        /// <summary>
        /// Property indicating that the audio input Source feedback is supported.
        /// </summary>
        bool SupportsAudioInputFeedback { get; }

        /// <summary>
        /// Returns the current audio input source
        /// </summary>
        AudioInputDetail AudioInputSource { get; }

        /// <summary>
        /// Property indicating that the audio input source command is supported.
        /// </summary>
        bool SupportsSetAudioInputSource { get; }

        /// <summary>
        /// Method to select an audio input source. AudioConnections is a superset of all
        /// possible inputs. For finding available inputs per device use
        /// GetUsableInput.
        /// </summary>
        /// <param name="input"></param>
        void SetAudioInputSource(AudioConnections input);

        /// <summary>
        /// Gets array of the usable audio inputs on this Device.
        /// </summary>
        /// <returns></returns>
        List<AudioInputDetail> GetUsableAudioInputs();
    }
}