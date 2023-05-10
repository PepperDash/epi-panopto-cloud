// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Crestron.Panopto.Common;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Interfaces;
using Crestron.Panopto.Common.Enums;
using Crestron.Panopto.Common.ExtensionMethods;

namespace Crestron.Panopto.Common
{
    public class ConnectionsNode
    {
        public VideoAudioInputNode inputs;
        public VideoAudioOutputNode outputs;

        public ConnectionsNode()
        { }

        public ConnectionsNode(VideoInOut video, AudioInOut audio)
        {
            inputs = new VideoAudioInputNode
            {
                video = new List<VideoInputDetail>(),
                audio = new List<AudioInputDetail>()
            };
            outputs = new VideoAudioOutputNode
            {
                video = new List<VideoOutputDetail>(),
                audio = new List<AudioOutputDetail>()
            };
            if (video.DoesNotExist() || audio.DoesNotExist())
                return;

            AssignConnectionInputs(video, audio);
            AssignConnectionOutputs(video, audio);
        }
        
        /// <summary>
        /// Assign inputs node for both video and audio.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="video"></param>
        /// <param name="audio"></param>
        private void AssignConnectionInputs(VideoInOut video, AudioInOut audio)
        {
            AssignVideoInputs(video);
            AssignAudioInputs(audio);
        }

        /// <summary>
        /// Assign inputs for video.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="video"></param>
        private void AssignVideoInputs(VideoInOut video)
        {
            if (video.Inputs.DoesNotExist())
                return;

            for (int i = 0; i < video.Inputs.Count; i++)
            {
                inputs.video.Add(new VideoInputDetail
                {
                    type = video.Inputs[i].type,
                    connector = video.Inputs[i].connector,
                    description = video.Inputs[i].description,
                    friendlyName = video.Inputs[i].friendlyName
                });
            }
        }

        /// <summary>
        /// Assign inputs for audio.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="audio"></param>
        private void AssignAudioInputs(AudioInOut audio)
        {
            if (audio.Inputs.DoesNotExist())
                return;

            for (int i = 0; i < audio.Inputs.Count; i++)
            {
                inputs.audio.Add(new AudioInputDetail
                {
                    type = audio.Inputs[i].type,
                    connector = audio.Inputs[i].connector,
                    description = audio.Inputs[i].description,
                    friendlyName = audio.Inputs[i].friendlyName
                });
            }
        }

        /// <summary>
        /// Assign outputs node for both video and audio.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="video"></param>
        /// <param name="audio"></param>
        private void AssignConnectionOutputs(VideoInOut video, AudioInOut audio)
        {
            AssignVideoOutputs(video);
            AssignAudioOutputs(audio);
        }

        /// <summary>
        /// Assign outputs for video.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="video"></param>
        private void AssignVideoOutputs(VideoInOut video)
        {
            if (video.Outputs.DoesNotExist())
                return;

            for (int i = 0; i < video.Outputs.Count; i++)
            {
                outputs.video.Add(new VideoOutputDetail
                {
                    type = video.Outputs[i].type,
                    connector = video.Outputs[i].connector,
                    description = video.Outputs[i].description,
                    friendlyName = video.Outputs[i].friendlyName
                });
            }
        }

        /// <summary>
        /// Assign outputs for audio.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="audio"></param>
        private void AssignAudioOutputs(AudioInOut audio)
        {
            if (audio.Outputs.DoesNotExist())
                return;

            for (int i = 0; i < audio.Outputs.Count; i++)
            {
                outputs.audio.Add(new AudioOutputDetail
                {
                    type = audio.Outputs[i].type,
                    connector = audio.Outputs[i].connector,
                    description = audio.Outputs[i].description,
                    friendlyName = audio.Outputs[i].friendlyName
                });
            }
        }
    }
}