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

using Crestron.Panopto.Common.Interfaces;
using Crestron.SimplSharp;
using Newtonsoft.Json;

namespace Crestron.Panopto.Common
{
    public class MediaServiceProviderData
    {
        public bool SupportsMediaServices { get; set; }
        public bool HasDirectAccessToApps { get; set; }
        public bool SupportsActiveMediaServiceFeedback { get; set; }
        public bool SupportsActiveMediaServiceStateFeedback { get; set; }
        public bool SupportsMediaServiceSubscriptionStateFeedback { get; set; }
        public List<MediaServiceData> AvailableServices { get; set; }
        public Feedback FeedbackData { get; set; }

        public string MediaServicePollCommand { get; set; }
        public bool MediaServicePollingEnabled { get; set; }
        public string MediaServiceSubscriptionStatePollCommand { get; set; }
        public bool MediaServiceSubscriptionStatePollingEnabled { get; set; }
        public string MediaServicePlaybackStatePollCommand { get; set; }
        public bool MediaServicePlaybackStatePollingEnabled { get; set; }

        public bool ShouldSerializeMediaServicePollCommand() { return false; }
        public bool ShouldSerializeMediaServicePollingEnabled() { return false; }
        public bool ShouldSerializeMediaServiceSubscriptionStatePollCommand() { return false; }
        public bool ShouldSerializeMediaServiceSubscriptionStatePollingEnabled() { return false; }
        public bool ShouldSerializeMediaServicePlaybackStatePollCommand() { return false; }
        public bool ShouldSerializeMediaServicePlaybackStatePollingEnabled() { return false; }
        public bool ShouldSerializeFeedbackData() { return false; }

        public class MediaServiceData
        {
            public string Id { get; set; }
            public string FriendlyName { get; set; }
            public string Command { get; set; }
            public List<SupportedFeature> SupportedFeatureData { get; set; }
            public bool IsSelectable { get; set; }
            public bool IsBranded { get; set; }

            public class SupportedFeature
            {
                public string ComponentInterface { get; set; }
                public List<string> SupportStatements { get; set; }
            }

            public bool ShouldSerializeCommand() { return false; }
        }

        public class Feedback
        {
            public string GroupHeader { get; set; }
            public ActiveFeedback ActiveServiceFeedbackData { get; set; }
            public PlaybackStateFeedback PlaybackStateFeedbackData { get; set; }

            public class ActiveFeedback
            {
                public string GroupHeader { get; set; }

                /// <summary>
                /// Key is the name of the service such as "Netflix"
                /// Value is the response from the device for the service
                /// </summary>
                public Dictionary<string, string> Feedback { get; set; }
            }

            public class PlaybackStateFeedback
            {
                public string GroupHeader { get; set; }

                /// <summary>
                /// Key is the status such as "Playing", "Paused", or "Stopped"
                /// Value is the response from the device for the service
                /// It will be assumed that the status will be for the active service
                /// since more than one service cannot be active at the same time
                /// </summary>
                public Dictionary<string, string> Feedback { get; set; }
            }
        }
    }
}