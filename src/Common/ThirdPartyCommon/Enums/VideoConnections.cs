﻿// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.Panopto.Common.Enums
{
    public enum VideoConnections
    {
        None = -1,
        Unknown = short.MaxValue,
        Uncontrolled = short.MaxValue - 1,
        Antenna1 = StandardCommandsEnum.Antenna1,
        Antenna2 = StandardCommandsEnum.Antenna2,
        Component1 = StandardCommandsEnum.Component1,
        Component10 = StandardCommandsEnum.Component10,
        Component2 = StandardCommandsEnum.Component2,
        Component3 = StandardCommandsEnum.Component3,
        Component4 = StandardCommandsEnum.Component4,
        Component5 = StandardCommandsEnum.Component5,
        Component6 = StandardCommandsEnum.Component6,
        Component7 = StandardCommandsEnum.Component7,
        Component8 = StandardCommandsEnum.Component8,
        Component9 = StandardCommandsEnum.Component9,
        Composite1 = StandardCommandsEnum.Composite1,
        Composite10 = StandardCommandsEnum.Composite10,
        Composite2 = StandardCommandsEnum.Composite2,
        Composite3 = StandardCommandsEnum.Composite3,
        Composite4 = StandardCommandsEnum.Composite4,
        Composite5 = StandardCommandsEnum.Composite5,
        Composite6 = StandardCommandsEnum.Composite6,
        Composite7 = StandardCommandsEnum.Composite7,
        Composite8 = StandardCommandsEnum.Composite8,
        Composite9 = StandardCommandsEnum.Composite9,
        DisplayPort1 = StandardCommandsEnum.DisplayPort1,
        DisplayPort10 = StandardCommandsEnum.DisplayPort10,
        DisplayPort2 = StandardCommandsEnum.DisplayPort2,
        DisplayPort3 = StandardCommandsEnum.DisplayPort3,
        DisplayPort4 = StandardCommandsEnum.DisplayPort4,
        DisplayPort5 = StandardCommandsEnum.DisplayPort5,
        DisplayPort6 = StandardCommandsEnum.DisplayPort6,
        DisplayPort7 = StandardCommandsEnum.DisplayPort7,
        DisplayPort8 = StandardCommandsEnum.DisplayPort8,
        DisplayPort9 = StandardCommandsEnum.DisplayPort9,
        Dvi1 = StandardCommandsEnum.Dvi1,
        Dvi10 = StandardCommandsEnum.Dvi10,
        Dvi2 = StandardCommandsEnum.Dvi2,
        Dvi3 = StandardCommandsEnum.Dvi3,
        Dvi4 = StandardCommandsEnum.Dvi4,
        Dvi5 = StandardCommandsEnum.Dvi5,
        Dvi6 = StandardCommandsEnum.Dvi6,
        Dvi7 = StandardCommandsEnum.Dvi7,
        Dvi8 = StandardCommandsEnum.Dvi8,
        Dvi9 = StandardCommandsEnum.Dvi9,
        Hdmi1 = StandardCommandsEnum.Hdmi1,
        Hdmi10 = StandardCommandsEnum.Hdmi10,
        Hdmi2 = StandardCommandsEnum.Hdmi2,
        Hdmi3 = StandardCommandsEnum.Hdmi3,
        Hdmi4 = StandardCommandsEnum.Hdmi4,
        Hdmi5 = StandardCommandsEnum.Hdmi5,
        Hdmi6 = StandardCommandsEnum.Hdmi6,
        Hdmi7 = StandardCommandsEnum.Hdmi7,
        Hdmi8 = StandardCommandsEnum.Hdmi8,
        Hdmi9 = StandardCommandsEnum.Hdmi9,
        Input1 = StandardCommandsEnum.Input1,
        Input10 = StandardCommandsEnum.Input10,
        Input2 = StandardCommandsEnum.Input2,
        Input3 = StandardCommandsEnum.Input3,
        Input4 = StandardCommandsEnum.Input4,
        Input5 = StandardCommandsEnum.Input5,
        Input6 = StandardCommandsEnum.Input6,
        Input7 = StandardCommandsEnum.Input7,
        Input8 = StandardCommandsEnum.Input8,
        Input9 = StandardCommandsEnum.Input9,
        Input11 = StandardCommandsEnum.Input11,
        Input12 = StandardCommandsEnum.Input12,
        Input13 = StandardCommandsEnum.Input13,
        Input14 = StandardCommandsEnum.Input14,
        Input15 = StandardCommandsEnum.Input15,
        Network1 = StandardCommandsEnum.Network1,
        Network10 = StandardCommandsEnum.Network10,
        Network2 = StandardCommandsEnum.Network2,
        Network3 = StandardCommandsEnum.Network3,
        Network4 = StandardCommandsEnum.Network4,
        Network5 = StandardCommandsEnum.Network5,
        Network6 = StandardCommandsEnum.Network6,
        Network7 = StandardCommandsEnum.Network7,
        Network8 = StandardCommandsEnum.Network8,
        Network9 = StandardCommandsEnum.Network9,
        Usb1 = StandardCommandsEnum.Usb1,
        Usb2 = StandardCommandsEnum.Usb2,
        Usb3 = StandardCommandsEnum.Usb3,
        Usb4 = StandardCommandsEnum.Usb4,
        Usb5 = StandardCommandsEnum.Usb5,
        Vga1 = StandardCommandsEnum.Vga1,
        Vga10 = StandardCommandsEnum.Vga10,
        Vga2 = StandardCommandsEnum.Vga2,
        Vga3 = StandardCommandsEnum.Vga3,
        Vga4 = StandardCommandsEnum.Vga4,
        Vga5 = StandardCommandsEnum.Vga5,
        Vga6 = StandardCommandsEnum.Vga6,
        Vga7 = StandardCommandsEnum.Vga7,
        Vga8 = StandardCommandsEnum.Vga8,
        Vga9 = StandardCommandsEnum.Vga9,
        Dvd1 = StandardCommandsEnum.DVD,
        Sat1 = StandardCommandsEnum.SAT,
        Aux1 = StandardCommandsEnum.Aux1,
        Aux2 = StandardCommandsEnum.Aux2,
        Tv1 = StandardCommandsEnum.TV,
        Dss1 = StandardCommandsEnum.DSS,
        /* AVR Inputs */
        MediaInternetRadio = StandardCommandsEnum.InternetRadio,
        MediaSiriusRadio = StandardCommandsEnum.Sirius,
        MediaXmRadio = StandardCommandsEnum.Xm,
        MediaSiriusXmRadio = StandardCommandsEnum.SiriusXm,
        MediaPandoraRadio = StandardCommandsEnum.Pandora,
        MediaLastFmRadio = StandardCommandsEnum.LastFm,
        MediaRhapsodyRadio = StandardCommandsEnum.Rhapsody,
        MediaHdRadio = StandardCommandsEnum.HdRadio,
        Spotify = StandardCommandsEnum.Spotify,
        YouTube = StandardCommandsEnum.YouTube,
        YouTubeTv = StandardCommandsEnum.YouTubeTv,
        Netflix = StandardCommandsEnum.Netflix,
        Hulu = StandardCommandsEnum.Hulu,
        DirectvNow = StandardCommandsEnum.DirecTvNow,
        AmazonVideo = StandardCommandsEnum.AmazonVideo,
        PlaystationVue = StandardCommandsEnum.PlayStationVue,
        SlingTv = StandardCommandsEnum.SlingTv,
        Airplay = StandardCommandsEnum.AirPlay,
        GoogleCast = StandardCommandsEnum.GoogleCast,
        DLNA = StandardCommandsEnum.Dlna,
        Tidal = StandardCommandsEnum.Tidal,
        Deezer = StandardCommandsEnum.Deezer,
        Crackle = StandardCommandsEnum.Crackle,
        OnDemand = StandardCommandsEnum.OnDemand,
        Bd1 = StandardCommandsEnum.Bd1,
        Catv1 = StandardCommandsEnum.Catv1,
        Game1 = StandardCommandsEnum.Game1,
        Pc1 = StandardCommandsEnum.Pc1,
        Bluetooth1 = StandardCommandsEnum.Bluetooth1,
        MediaPlayer1 = StandardCommandsEnum.MediaPlayer1,
        Ipod1 = StandardCommandsEnum.Ipod1,
        Cd1 = StandardCommandsEnum.CD,
        Tuner1 = StandardCommandsEnum.Tuner,
        Phono1 = StandardCommandsEnum.Phono,
    }
}