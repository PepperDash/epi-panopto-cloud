// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Collections.Generic;
using Crestron.RAD.Common.Enums;

namespace Crestron.RAD.Common.Helpers
{
    public static class DeviceTypeDefaultHelper
    {
        public static List<DeviceTypeDefaults> Defaults
        {
            get
            {
                var results = new List<DeviceTypeDefaults>
                {
                    new DeviceTypeDefaults(DeviceType.FlatPanelDisplay),
                    new DeviceTypeDefaults(DeviceType.Projector),
                    new DeviceTypeDefaults(DeviceType.CableBox),
                    new DeviceTypeDefaults(DeviceType.VideoServer),
                    new DeviceTypeDefaults(DeviceType.BlurayPlayer),
                    new DeviceTypeDefaults(DeviceType.VideoConferenceCodec),
                    new DeviceTypeDefaults(DeviceType.AvReciever)
                };
                return results;
            }
        }
    }

    public class DeviceTypeDefaults
    {
        #region Properties

        public DeviceType DeviceType { get; internal set; }

        public FeedbackSupportEnum PowerFeedback { get; internal set; }
        public FeedbackSupportEnum InputFeedback { get; internal set; }
        public FeedbackSupportEnum VolumeFeedback { get; internal set; }
        public FeedbackSupportEnum ChannelFeedback { get; internal set; }
        public FeedbackSupportEnum MuteFeedback { get; internal set; }
        public FeedbackSupportEnum EnergyStarFeedback { get; internal set; }
        public FeedbackSupportEnum VideoMuteFeedback { get; internal set; }
        public FeedbackSupportEnum LampHourFeedback { get; internal set; }
        public FeedbackSupportEnum OnScreenDisplayFeedback { get; internal set; }

        public FeedbackSupportEnum CodecFeedback { get; internal set; }

        public FeedbackSupportEnum EncryptionFeedback { get; internal set; }
        public FeedbackSupportEnum StandbyFeedback { get; internal set; }
        public FeedbackSupportEnum SelfViewFeedback { get; internal set; }
        public FeedbackSupportEnum DoNotDisturbFeedback { get; internal set; }
        public FeedbackSupportEnum MicMuteFeedback { get; internal set; }
        public FeedbackSupportEnum MicMuteOnAutoAnswerFeedback { get; internal set; }
        public FeedbackSupportEnum AutoAnswerFeedback { get; internal set; }
        public FeedbackSupportEnum FarEndControlFeedback { get; internal set; }

        public FeedbackSupportEnum CallFeedback { get; internal set; }
        public FeedbackSupportEnum NotificationFeedback { get; internal set; }
        public FeedbackSupportEnum CodecSystemFeedback { get; internal set; }
        public FeedbackSupportEnum MultipointFeedback { get; internal set; }
        public FeedbackSupportEnum MonitorPresentationSetting { get; internal set; }

        public SerialDefaults SerialDefaults { get; private set; }
        public EthernetDefaults EthernetDefaults { get; private set; }

        #endregion

        #region Constructor

        public DeviceTypeDefaults(DeviceType deviceType)
            : this()
        {
            DeviceType = deviceType;
            switch (DeviceType)
            {
                case DeviceType.VideoConferenceCodec:
                    SetCodecFeedbackSupport();
                    break;
                case DeviceType.CableBox:
                    SetCableBoxFeedbackSupport();
                    break;
                case DeviceType.BlurayPlayer:
                    SetBlurayFeedbackSupport();
                    break;
                case DeviceType.FlatPanelDisplay:
                    SetFlatPanelDisplayFeedbackSupport();
                    break;
                case DeviceType.Projector:
                    SetProjectorFeedbackSupport();
                    break;
                case DeviceType.VideoServer:
                    SetVideoServerFeedbackSupport();
                    break;
                case DeviceType.AvReciever:
                    SetAvrFeedbackSupport();
                    break;
            }
        }

        private DeviceTypeDefaults()
        {
            SerialDefaults = new SerialDefaults();
            EthernetDefaults = new EthernetDefaults();

            PowerFeedback = FeedbackSupportEnum.NoSupport;
            InputFeedback = FeedbackSupportEnum.NoSupport;
            VolumeFeedback = FeedbackSupportEnum.NoSupport;
            ChannelFeedback = FeedbackSupportEnum.NoSupport;
            MuteFeedback = FeedbackSupportEnum.NoSupport;
            CodecFeedback = FeedbackSupportEnum.NoSupport;
            EncryptionFeedback = FeedbackSupportEnum.NoSupport;
            StandbyFeedback = FeedbackSupportEnum.NoSupport;
            SelfViewFeedback = FeedbackSupportEnum.NoSupport;
            DoNotDisturbFeedback = FeedbackSupportEnum.NoSupport;
            MicMuteFeedback = FeedbackSupportEnum.NoSupport;
            MicMuteOnAutoAnswerFeedback = FeedbackSupportEnum.NoSupport;
            AutoAnswerFeedback = FeedbackSupportEnum.NoSupport;
            FarEndControlFeedback = FeedbackSupportEnum.NoSupport;
            CallFeedback = FeedbackSupportEnum.NoSupport;
            NotificationFeedback = FeedbackSupportEnum.NoSupport;
            LampHourFeedback = FeedbackSupportEnum.NoSupport;
            VideoMuteFeedback = FeedbackSupportEnum.NoSupport;
            EnergyStarFeedback = FeedbackSupportEnum.NoSupport;
            OnScreenDisplayFeedback = FeedbackSupportEnum.NoSupport;
            MultipointFeedback = FeedbackSupportEnum.NoSupport;
            MonitorPresentationSetting = FeedbackSupportEnum.NoSupport;
        }

        #endregion

        #region Private Methods

        private void SetAvrFeedbackSupport()
        {
            PowerFeedback = FeedbackSupportEnum.FullSupport;
            MuteFeedback = FeedbackSupportEnum.FullSupport;
            VolumeFeedback = FeedbackSupportEnum.HeaderOnly;
            InputFeedback = FeedbackSupportEnum.FullSupport;

            ChannelFeedback = FeedbackSupportEnum.NoSupport;
            CodecFeedback = FeedbackSupportEnum.NoSupport;
            EnergyStarFeedback = FeedbackSupportEnum.NoSupport;
            LampHourFeedback = FeedbackSupportEnum.NoSupport;
            VideoMuteFeedback = FeedbackSupportEnum.NoSupport;
            OnScreenDisplayFeedback = FeedbackSupportEnum.NoSupport;
        }

        private void SetCodecFeedbackSupport()
        {
            ChannelFeedback = FeedbackSupportEnum.NoSupport;
            InputFeedback = FeedbackSupportEnum.FullSupport;
            MuteFeedback = FeedbackSupportEnum.FullSupport;
            PowerFeedback = FeedbackSupportEnum.NoSupport;
            VolumeFeedback = FeedbackSupportEnum.HeaderOnly;
            
            EnergyStarFeedback = FeedbackSupportEnum.NoSupport;
            LampHourFeedback = FeedbackSupportEnum.NoSupport;
            VideoMuteFeedback = FeedbackSupportEnum.FullSupport;
            OnScreenDisplayFeedback = FeedbackSupportEnum.NoSupport;

            CodecFeedback = FeedbackSupportEnum.FullSupport;
            FarEndControlFeedback = FeedbackSupportEnum.FullSupport;
            MicMuteFeedback = FeedbackSupportEnum.FullSupport;
            MicMuteOnAutoAnswerFeedback = FeedbackSupportEnum.FullSupport;
            DoNotDisturbFeedback = FeedbackSupportEnum.FullSupport;
            AutoAnswerFeedback = FeedbackSupportEnum.FullSupport;
            StandbyFeedback = FeedbackSupportEnum.FullSupport;
            EncryptionFeedback = FeedbackSupportEnum.FullSupport;
            SelfViewFeedback = FeedbackSupportEnum.FullSupport;
            CallFeedback = FeedbackSupportEnum.HeaderOnly;
            NotificationFeedback = FeedbackSupportEnum.HeaderOnly;
            CodecSystemFeedback = FeedbackSupportEnum.HeaderOnly;
            MultipointFeedback = FeedbackSupportEnum.HeaderOnly;
            MonitorPresentationSetting = FeedbackSupportEnum.HeaderOnly;
        }

        private void SetVideoServerFeedbackSupport()
        {
            ChannelFeedback = FeedbackSupportEnum.NoSupport;
            InputFeedback = FeedbackSupportEnum.NoSupport;
            MuteFeedback = FeedbackSupportEnum.NoSupport;
            PowerFeedback = FeedbackSupportEnum.NoSupport;
            VolumeFeedback = FeedbackSupportEnum.NoSupport;
            CodecFeedback = FeedbackSupportEnum.NoSupport;

            EnergyStarFeedback = FeedbackSupportEnum.NoSupport;
            LampHourFeedback = FeedbackSupportEnum.NoSupport;
            VideoMuteFeedback = FeedbackSupportEnum.NoSupport;
            OnScreenDisplayFeedback = FeedbackSupportEnum.NoSupport;
        }

        private void SetFlatPanelDisplayFeedbackSupport()
        {
            InputFeedback = FeedbackSupportEnum.FullSupport;
            MuteFeedback = FeedbackSupportEnum.FullSupport;
            PowerFeedback = FeedbackSupportEnum.FullSupport;
            VolumeFeedback = FeedbackSupportEnum.HeaderOnly;
            EnergyStarFeedback = FeedbackSupportEnum.FullSupport;
            OnScreenDisplayFeedback = FeedbackSupportEnum.FullSupport;
        }

        private void SetProjectorFeedbackSupport()
        {
            InputFeedback = FeedbackSupportEnum.FullSupport;
            MuteFeedback = FeedbackSupportEnum.FullSupport;
            PowerFeedback = FeedbackSupportEnum.FullSupport;
            VolumeFeedback = FeedbackSupportEnum.HeaderOnly;
            EnergyStarFeedback = FeedbackSupportEnum.FullSupport;
            OnScreenDisplayFeedback = FeedbackSupportEnum.FullSupport;
            LampHourFeedback = FeedbackSupportEnum.HeaderOnly;
            VideoMuteFeedback = FeedbackSupportEnum.FullSupport;
        }

        private void SetBlurayFeedbackSupport()
        {
            ChannelFeedback = FeedbackSupportEnum.NoSupport;
            InputFeedback = FeedbackSupportEnum.NoSupport;
            MuteFeedback = FeedbackSupportEnum.NoSupport;
            PowerFeedback = FeedbackSupportEnum.FullSupport;
            VolumeFeedback = FeedbackSupportEnum.NoSupport;
            CodecFeedback = FeedbackSupportEnum.NoSupport;

            EnergyStarFeedback = FeedbackSupportEnum.FullSupport;
            LampHourFeedback = FeedbackSupportEnum.NoSupport;
            VideoMuteFeedback = FeedbackSupportEnum.NoSupport;
            OnScreenDisplayFeedback = FeedbackSupportEnum.NoSupport;
        }

        private void SetCableBoxFeedbackSupport()
        {
            ChannelFeedback = FeedbackSupportEnum.HeaderOnly;
            InputFeedback = FeedbackSupportEnum.NoSupport;
            MuteFeedback = FeedbackSupportEnum.FullSupport;
            PowerFeedback = FeedbackSupportEnum.FullSupport;
            VolumeFeedback = FeedbackSupportEnum.HeaderOnly;
            CodecFeedback = FeedbackSupportEnum.NoSupport;

            EnergyStarFeedback = FeedbackSupportEnum.FullSupport;
            LampHourFeedback = FeedbackSupportEnum.NoSupport;
            VideoMuteFeedback = FeedbackSupportEnum.NoSupport;
            OnScreenDisplayFeedback = FeedbackSupportEnum.NoSupport;
        }

        #endregion
    }
}
