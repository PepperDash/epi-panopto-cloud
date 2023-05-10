// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Collections.Generic;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Transports;

namespace Crestron.RAD.Common
{
    public class DeviceInformationRoot
    {
        public DeviceTypes SelectedDeviceType { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public bool IsSerialSupported { get; set; }
        public SerialInformation SerialData { get; set; }
        public bool IsIpSupported { get; set; }
        public int Port { get; set; }
        public int ResponseTimeout { get; set; }
        public string Ack { get; set; }
        public string Nak { get; set; }
        public int WarmUpTime { get; set; }
        public int CoolDownTime { get; set; }
        public int TimeBetweenCommands { get; set; }
        public bool IsWaitForResponse { get; set; }
        public bool IsEnableAutoPolling { get; set; }
        public bool IsEnableAutoReconnect { get; set; }
        public CommandInformation CommandData { get; set; }
        public bool HasFeedback { get; set; }
        public FeedbackInformation FeedbackData { get; set; }
    }

    public class SerialInformation
    {
        public eComProtocolType SelectedProtocolType { get; set; }
        public eComBaudRates SelectedBaudType { get; set; }
        public eComParityType SelectedParityType { get; set; }
        public eComHardwareHandshakeType SelectedHardwareHandshakeType { get; set; }
        public eComSoftwareHandshakeType SelectedSoftwareHandshakeType { get; set; }
        public eComDataBits SelectedDataBitsType { get; set; }
        public eComStopBits SelectedStopBitsType { get; set; }
    }

    public class FeedbackInformation
    {
        public string FeedbackHeader { get; set; }
        public bool HasPowerFeedback { get; set; }
        public PowerInformation PowerData { get; set; }
        public bool HasInputFeedback { get; set; }
        public InputInformation InputData { get; set; }
        public bool HasVolumeFeedback { get; set; }
        public VolumeInformation VolumeData { get; set; }
        public bool HasChannelFeedback { get; set; }
        public ChannelInformation ChannelData { get; set; }
        public bool HasMuteFeedback { get; set; }
        public MuteInformation MuteData { get; set; }
    }

    public class CommandInformation
    {
        public Dictionary<string, string> Commands { get; set; }
    }

    public class PowerInformation
    {
        public string PowerHeader { get; private set; }
        public Dictionary<StandardFeedback.PowerStatesFeedback, string> Feedback { get; set; }
        public PowerInformation(string header)
        {
            if (!string.IsNullOrEmpty(header))
            {
                PowerHeader = header;
            }
            else
            {
                PowerHeader = string.Empty;
            }
        }
    }

    public class InputInformation
    {
        public string InputHeader { get; set; }
        public Dictionary<StandardFeedback.InputTypesFeeback, string> Feedback { get; set; }
        public InputInformation(string header)
        {
            if (!string.IsNullOrEmpty(header))
            {
                InputHeader = header;
            }
            else
            {
                InputHeader = string.Empty;
            }
        }
    }

    public class ChannelInformation
    {
        public string ChannelHeader { get; set; }
        public Dictionary<StandardFeedback.InputTypesFeeback, string> Feedback { get; set; }
        public ChannelInformation(string header)
        {
            if (!string.IsNullOrEmpty(header))
            {
                ChannelHeader = header;
            }
            else
            {
                ChannelHeader = string.Empty;
            }
        }
    }

    public class MuteInformation
    {
        public string MuteHeader { get; set; }
        public Dictionary<StandardFeedback.MuteStatesFeedback, string> Feedback { get; set; }
        public MuteInformation(string header)
        {
            if (!string.IsNullOrEmpty(header))
            {
                MuteHeader = header;
            }
            else
            {
                MuteHeader = string.Empty;
            }
        }
    }

    public class VolumeInformation
    {
        public string VolumeHeader { get; set; }
        public Dictionary<StandardFeedback.MuteStatesFeedback, string> Feedback { get; set; }
        public VolumeInformation(string header)
        {
            if (!string.IsNullOrEmpty(header))
            {
                VolumeHeader = header;
            }
            else
            {
                VolumeHeader = string.Empty;
            }
        }
    }

    public class TunerBandInformation
    {
        public string TunerBandHeader { get; set; }
        public Dictionary<StandardFeedback.TunerFrequencyBandStatesFeedback, string> Feedback { get; set; }

        public TunerBandInformation(string header)
        {
            if (!string.IsNullOrEmpty(header))
            {
                TunerBandHeader = header;
            }
            else
            {
                TunerBandHeader = string.Empty;
            }
        }
    }
}
