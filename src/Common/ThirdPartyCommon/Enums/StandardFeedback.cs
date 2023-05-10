// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.Panopto.Common.Enums
{
    public class StandardFeedback
    {
		public enum PowerStatesFeedback { On, Off };
		public enum MuteStatesFeedback { On, Off };
		public enum MicMuteStatesFeedback { On, Off }
		public enum StandbyStatesFeedback { On, Off }
		public enum SelfviewStatesFeedback { On, Off, Auto }
        public enum TunerFrequencyBandStatesFeedback { Am, Fm }

        public enum ResourceStatusFeedback
        {
            Unknown = 0,
            Unassigned = 1,
            ArmedStay = 2,
            ArmedAway = 3,
            Disarmed = 4,
            Bypassed = 5,
            Unbypassed = 6,
            Short = 7,
            Missing = 8,
            Error = 9,
            Name = 0
        }

        public enum CompatibleFeedback
        {
            Unknown,
            Yes,
            No
        }

        public enum AuthorizationFeedback
        {
            Unknown,
            Authorized,
            Unauthorized
        }

        public enum InputTypesFeeback
        {
            Vga1 = VideoConnections.Vga1,
            Vga10 = VideoConnections.Vga10,
            Vga2 = VideoConnections.Vga2,
            Vga3 = VideoConnections.Vga3,
            Vga4 = VideoConnections.Vga4,
            Vga5 = VideoConnections.Vga5,
            Vga6 = VideoConnections.Vga6,
            Vga7 = VideoConnections.Vga7,
            Vga8 = VideoConnections.Vga8,
            Vga9 = VideoConnections.Vga9,
            Hdmi1 = VideoConnections.Hdmi1,
            Hdmi10 = VideoConnections.Hdmi10,
            Hdmi2 = VideoConnections.Hdmi2,
            Hdmi3 = VideoConnections.Hdmi3,
            Hdmi4 = VideoConnections.Hdmi4,
            Hdmi5 = VideoConnections.Hdmi5,
            Hdmi6 = VideoConnections.Hdmi6,
            Hdmi7 = VideoConnections.Hdmi7,
            Hdmi8 = VideoConnections.Hdmi8,
            Hdmi9 = VideoConnections.Hdmi9,
            Dvi1 = VideoConnections.Dvi1,
            Dvi10 = VideoConnections.Dvi10,
            Dvi2 = VideoConnections.Dvi2,
            Dvi3 = VideoConnections.Dvi3,
            Dvi4 = VideoConnections.Dvi4,
            Dvi5 = VideoConnections.Dvi5,
            Dvi6 = VideoConnections.Dvi6,
            Dvi7 = VideoConnections.Dvi7,
            Dvi8 = VideoConnections.Dvi8,
            Dvi9 = VideoConnections.Dvi9,
            Component1 = VideoConnections.Component1,
            Component10 = VideoConnections.Component10,
            Component2 = VideoConnections.Component2,
            Component3 = VideoConnections.Component3,
            Component4 = VideoConnections.Component4,
            Component5 = VideoConnections.Component5,
            Component6 = VideoConnections.Component6,
            Component7 = VideoConnections.Component7,
            Component8 = VideoConnections.Component8,
            Component9 = VideoConnections.Component9,
            Composite1 = VideoConnections.Composite1,
            Composite10 = VideoConnections.Composite10,
            Composite2 = VideoConnections.Composite2,
            Composite3 = VideoConnections.Composite3,
            Composite4 = VideoConnections.Composite4,
            Composite5 = VideoConnections.Composite5,
            Composite6 = VideoConnections.Composite6,
            Composite7 = VideoConnections.Composite7,
            Composite8 = VideoConnections.Composite8,
            Composite9 = VideoConnections.Composite9,
            DisplayPort1 = VideoConnections.DisplayPort1,
            DisplayPort10 = VideoConnections.DisplayPort10,
            DisplayPort2 = VideoConnections.DisplayPort2,
            DisplayPort3 = VideoConnections.DisplayPort3,
            DisplayPort4 = VideoConnections.DisplayPort4,
            DisplayPort5 = VideoConnections.DisplayPort5,
            DisplayPort6 = VideoConnections.DisplayPort6,
            DisplayPort7 = VideoConnections.DisplayPort7,
            DisplayPort8 = VideoConnections.DisplayPort8,
            DisplayPort9 = VideoConnections.DisplayPort9,
            Usb1 = VideoConnections.Usb1,
            Usb2 = VideoConnections.Usb2,
            Usb3 = VideoConnections.Usb3,
            Usb4 = VideoConnections.Usb4,
            Usb5 = VideoConnections.Usb5,
            Antenna1 = VideoConnections.Antenna1,
            Antenna2 = VideoConnections.Antenna2,
            Network1 = VideoConnections.Network1,
            Network10 = VideoConnections.Network10,
            Network2 = VideoConnections.Network2,
            Network3 = VideoConnections.Network3,
            Network4 = VideoConnections.Network4,
            Network5 = VideoConnections.Network5,
            Network6 = VideoConnections.Network6,
            Network7 = VideoConnections.Network7,
            Network8 = VideoConnections.Network8,
            Network9 = VideoConnections.Network9,
            Input1 = VideoConnections.Input1,
            Input10 = VideoConnections.Input10,
            Input2 = VideoConnections.Input2,
            Input3 = VideoConnections.Input3,
            Input4 = VideoConnections.Input4,
            Input5 = VideoConnections.Input5,
            Input6 = VideoConnections.Input6,
            Input7 = VideoConnections.Input7,
            Input8 = VideoConnections.Input8,
            Input9 = VideoConnections.Input9,
            /* AVR Additions */
            Dvd1 = VideoConnections.Dvd1,
            Dss1 = VideoConnections.Dss1,
            Sat1 = VideoConnections.Sat1,
            Aux1 = VideoConnections.Aux1,
            Aux2 = VideoConnections.Aux2,
            Tv = VideoConnections.Tv1
        }

        public enum CallDirectionFeedback { Incoming, Outgoing, Unknown }
		public enum CallStateFeedback { Dialing, Ringing, Connecting, Connected, Disconnecting, OnHold, Unknown }
		public enum CallProtocolFeedback { H320, H323, SIP, Spark, Unknown }
		public enum CallTypeFeedback { Video, Audio, Unknown }
		public enum AllowFarEndControlOfNearEndCameraFeedback { On, Off }
		public enum MuteMicsOnAutoAnswerFeedback { On, Off }
		public enum AutoAnswerFeedback { On, Off }
		public enum DoNotDisturbFeedback { On, Off }
		public enum EncryptionFeedback { On, Off }
        public enum EnergyStarFeedback { On, Off }
        public enum VideoMuteStatesFeedback { On, Off }
        public enum OnScreenDisplayFeedback { On, Off }
    }
 }
