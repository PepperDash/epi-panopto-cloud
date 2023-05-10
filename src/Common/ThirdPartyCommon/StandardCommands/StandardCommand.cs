// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Collections.Generic;
using System;
using System.Linq;
using Crestron.Panopto.Common.Enums;
using Crestron.SimplSharp.Reflection;

namespace Crestron.Panopto.Common.StandardCommands
{
    public class IrCommandConstants
    {
        public const string AllLampsOff = "ALL_LAMPS_OFF";
        public const string AllLampsOn = "ALL_LAMPS_ON";
        public const string Antenna = "ANTENNA";
        public const string ArrowDown = "DN_ARROW";
        public const string ArrowLeft = "LEFT_ARROW";
        public const string ArrowRight = "RIGHT_ARROW";
        public const string ArrowUp = "UP_ARROW";
        public const string Aspect1 = "ASPECTRATIO1";
        public const string Aspect2 = "ASPECTRATIO2";
        public const string Aspect3 = "ASPECTRATIO3";
        public const string Aspect4 = "ASPECTRATIO4";
        public const string Aspect5 = "ASPECTRATIO5";
        public const string Aspect6 = "ASPECTRATIO6";
        public const string Aspect7 = "ASPECTRATIO7";
        public const string Aspect8 = "ASPECTRATIO8";
        public const string Asterisk = "*";
        public const string Auto = "AUTO";
        public const string Aux1 = "AUX1";
        public const string Aux2 = "AUX2";
        public const string ChannelUp = "CH+";
        public const string ChannelDown = "CH-";
        public const string ChannelPoll = "ChannelPoll";
        public const string ChannelTune = "TUNE";
        public const string Eject = "EJECT";
        public const string Enter = "ENTER";
        public const string Home = "HOME";
        public const string Info = "INFO";
        public const string Input1 = "IN_1";
        public const string Input10 = "IN_10";
        public const string Input2 = "IN_2";
        public const string Input3 = "IN_3";
        public const string Input4 = "IN_4";
        public const string Input5 = "IN_5";
        public const string Input6 = "IN_6";
        public const string Input7 = "IN_7";
        public const string Input8 = "IN_8";
        public const string Input9 = "IN_9";
        public const string Input11 = "IN_11";
        public const string Input12 = "IN_12";
        public const string Input13 = "IN_13";
        public const string Input14 = "IN_14";
        public const string Input15 = "IN_15";
        public const string Mute = "MUTE";
        public const string MuteOff = "MUTE_OFF";
        public const string MuteOn = "MUTE_ON";
        public const string Osd = "OSD";
        public const string OsdOff = "OSD_OFF";
        public const string OsdOn = "OSD_ON";
        public const string OsdPoll = "OsdPoll";
        public const string Power = "POWER";
        public const string PowerOff = "POWER_OFF";
        public const string PowerOn = "POWER_ON";
        public const string Select = "SELECT";
        public const string VolMinus = "VOL-";
        public const string VolPlus = "VOL+";
        public const string Vol = "VOL";
        public const string _0 = "0";
        public const string _1 = "1";
        public const string _2 = "2";
        public const string _3 = "3";
        public const string _4 = "4";
        public const string _5 = "5";
        public const string _6 = "6";
        public const string _7 = "7";
        public const string _8 = "8";
        public const string _9 = "9";
        public const string Octothorpe = "#";
        public const string KeypadBackSpace = "BACKSPACE";
        public const string Vga1 = "VGA_1";
        public const string Vga10 = "VGA_10";
        public const string Vga2 = "VGA_2";
        public const string Vga3 = "VGA_3";
        public const string Vga4 = "VGA_4";
        public const string Vga5 = "VGA_5";
        public const string Vga6 = "VGA_6";
        public const string Vga7 = "VGA_7";
        public const string Vga8 = "VGA_8";
        public const string Vga9 = "VGA_9";
        public const string Hdmi1 = "HDMI_1";
        public const string Hdmi10 = "HDMI_10";
        public const string Hdmi2 = "HDMI_2";
        public const string Hdmi3 = "HDMI_3";
        public const string Hdmi4 = "HDMI_4";
        public const string Hdmi5 = "HDMI_5";
        public const string Hdmi6 = "HDMI_6";
        public const string Hdmi7 = "HDMI_7";
        public const string Hdmi8 = "HDMI_8";
        public const string Hdmi9 = "HDMI_9";
        public const string Dvi1 = "DVI_1";
        public const string Dvi10 = "DVI_10";
        public const string Dvi2 = "DVI_2";
        public const string Dvi3 = "DVI_3";
        public const string Dvi4 = "DVI_4";
        public const string Dvi5 = "DVI_5";
        public const string Dvi6 = "DVI_6";
        public const string Dvi7 = "DVI_7";
        public const string Dvi8 = "DVI_8";
        public const string Dvi9 = "DVI_9";
        public const string Component1 = "COMPONENT_1";
        public const string Component10 = "COMPONENT_10";
        public const string Component2 = "COMPONENT_2";
        public const string Component3 = "COMPONENT_3";
        public const string Component4 = "COMPONENT_4";
        public const string Component5 = "COMPONENT_5";
        public const string Component6 = "COMPONENT_6";
        public const string Component7 = "COMPONENT_7";
        public const string Component8 = "COMPONENT_8";
        public const string Component9 = "COMPONENT_9";
        public const string Composite1 = "COMPOSITE_1";
        public const string Composite10 = "COMPOSITE_10";
        public const string Composite2 = "COMPOSITE_2";
        public const string Composite3 = "COMPOSITE_3";
        public const string Composite4 = "COMPOSITE_4";
        public const string Composite5 = "COMPOSITE_5";
        public const string Composite6 = "COMPOSITE_6";
        public const string Composite7 = "COMPOSITE_7";
        public const string Composite8 = "COMPOSITE_8";
        public const string Composite9 = "COMPOSITE_9";
        public const string DisplayPort1 = "DISPLAY_PORT_1";
        public const string DisplayPort10 = "DISPLAY_PORT_10";
        public const string DisplayPort2 = "DISPLAY_PORT_2";
        public const string DisplayPort3 = "DISPLAY_PORT_3";
        public const string DisplayPort4 = "DISPLAY_PORT_4";
        public const string DisplayPort5 = "DISPLAY_PORT_5";
        public const string DisplayPort6 = "DISPLAY_PORT_6";
        public const string DisplayPort7 = "DISPLAY_PORT_7";
        public const string DisplayPort8 = "DISPLAY_PORT_8";
        public const string DisplayPort9 = "DISPLAY_PORT_9";
        public const string Usb1 = "USB_1";
        public const string Usb2 = "USB_2";
        public const string Usb3 = "USB_3";
        public const string Usb4 = "USB_4";
        public const string Usb5 = "USB_5";
        public const string Antenna1 = "ANTENNA_1";
        public const string Antenna2 = "ANTENNA_2";
        public const string Network1 = "NETWORK_1";
        public const string Network10 = "NETWORK_10";
        public const string Network2 = "NETWORK_2";
        public const string Network3 = "NETWORK_3";
        public const string Network4 = "NETWORK_4";
        public const string Network5 = "NETWORK_5";
        public const string Network6 = "NETWORK_6";
        public const string Network7 = "NETWORK_7";
        public const string Network8 = "NETWORK_8";
        public const string Network9 = "NETWORK_9";

        //Added for AVR/Digital Video Server
        public const string Dvd1 = "DVD";
        public const string Sat1 = "SAT";
        public const string Tv = "TV";
        public const string Cd = "CD";
        public const string Tuner = "TUNER";
        public const string Phono = "PHONO";
        public const string Dss = "DSS";
        public const string EnergyStarOn = "ENERGYSTAR_ON";
        public const string EnergyStarOff = "ENERGYSTAR_OFF";
        public const string EnergyStar = "ENERGYSTAR_TOGGLE";
        public const string VideoMuteOn = "VIDEOMUTE_ON";
        public const string VideoMuteOff = "VIDEOMUTE_OFF";
        public const string VideoMute = "VIDEOMUTE_TOGGLE";

        //added for Bluray Player
        public const string Audio = "AUDIO";
        public const string Blue = "BLUE";
        public const string Clear = "CLEAR";
        public const string Display = "DISPLAY";
        public const string DownArrow = "DN_ARROW";
        public const string UpArrow = "UP_ARROW";
        public const string LeftArrow = "LEFT_ARROW";
        public const string RightArrow = "RIGHT_ARROW";
        public const string Exit = "EXIT";
        public const string ForwardScan = "FSCAN";
        public const string ReverseScan = "RSCAN";
        public const string Green = "GREEN";
        public const string Options = "OPTIONS";
        public const string Pause = "PAUSE";
        public const string Play = "PLAY";
        public const string PlayPause = "PLAY_PAUSE";
        public const string Red = "RED";
        public const string Repeat = "REPEAT";
        public const string Return = "RETURN";
        public const string Stop = "STOP";
        public const string Subtitle = "SUBTITLE";
        public const string TopMenu = "TOPMENU";
        public const string ForwardSkip = "TRACK+";
        public const string ReverseSkip = "TRACK-";
        public const string Yellow = "YELLOW";
        public const string PopUpMenu = "POPUPMENU";
        public const string Menu = "MENU";

        //added for Cable Boxes
        public const string A = "A";
        public const string B = "B";
        public const string C = "C";
        public const string D = "D";
        public const string Back = "BACK";
        public const string Dvr = "DVR";
        public const string Favorite = "FAVORITE";
        public const string Guide = "GUIDE";
        public const string Last = "LAST";
        public const string Live = "LIVE";
        public const string PageDown = "PAGE_DOWN";
        public const string PageUp = "PAGE_UP";
        public const string Record = "RECORD";
        public const string Replay = "REPLAY";
        public const string SpeedSlow = "SPEED_SLOW";
        public const string ThumbsUp = "THUMBS_UP";
        public const string ThumbsDown = "THUMBS_DOWN";
        public const string Channel = "CHANNEL";
        public const string Dash = "DIGIT_-";
        public const string Period = ".";

        //Additional Commands to Support IR Functionality
        public const string XmRadio = "XM";
        public const string InternetRadio = "INTERNET_RADIO";
        public const string Sirius = "SIRIUS";
        public const string SiriusXm = "SIRIUS_XM";
        public const string Pandora = "PANDORA";
        public const string LastFm = "LAST_FM";
        public const string Rhapsody = "RHAPSODY";
        public const string HdRadio = "HD_RADIO";
        public const string Spotify = "SPOTIFY";
        public const string YouTube = "YOUTUBE";
        public const string YouTubeTv = "YOUTUBE_TV";
        public const string Netflix = "NETFLIX";
        public const string Hulu = "HULU";
        public const string DirecTvNow = "DIRECTV_NOW";
        public const string AmazonVideo = "AMAZON_VIDEO";
        public const string PlaystationVue = "PLAYSTATION_VUE";
        public const string SlingTv = "SLING_TV";
        public const string Airplay = "AIRPLAY";
        public const string GoogleCast = "GOOGLECAST";
        public const string DLNA = "DLNA";
        public const string Tidal = "TIDAL";
        public const string Deezer = "DEEZER";
        public const string Crackle = "CRACKLE";
        public const string OnDemand = "ON_DEMAND";
        public const string GooglePlay = "GOOGLEPLAY";
        public const string Bluetooth = "BLUETOOTH";
        public const string TuneUp = "TUNE+";
        public const string TuneDown = "TUNE-";
        public const string Tivo = "TIVO";
        public const string RSkip = "R_SKIP";
        public const string FSkip = "F_SKIP";
        public const string Settings = "SETTINGS";
        public const string Optical1 = "OPTICAL_1";
        public const string Optical2 = "OPTICAL_2";
        public const string Optical3 = "OPTICAL_3";
        public const string Optical4 = "OPTICAL_4";
        public const string Optical5 = "OPTICAL_5";
        public const string Optical6 = "OPTICAL_6";
        public const string Optical7 = "OPTICAL_7";
        public const string Optical8 = "OPTICAL_8";
        public const string Optical9 = "OPTICAL_9";
        public const string Optical10 = "OPTICAL_10";
        public const string Coax1 = "COAX_1";
        public const string Coax2 = "COAX_2";
        public const string Coax3 = "COAX_3";
        public const string Coax4 = "COAX_4";
        public const string Coax5 = "COAX_5";
        public const string Coax6 = "COAX_6";
        public const string Coax7 = "COAX_7";
        public const string Coax8 = "COAX_8";
        public const string Coax9 = "COAX_9";
        public const string Coax10 = "COAX_10";
        public const string AnalogAudio1 = "ANALOGAUDIO_1";
        public const string AnalogAudio2 = "ANALOGAUDIO_2";
        public const string AnalogAudio3 = "ANALOGAUDIO_3";
        public const string AnalogAudio4 = "ANALOGAUDIO_4";
        public const string AnalogAudio5 = "ANALOGAUDIO_5";
        public const string AnalogAudio6 = "ANALOGAUDIO_6";
        public const string AnalogAudio7 = "ANALOGAUDIO_7";
        public const string AnalogAudio8 = "ANALOGAUDIO_8";
        public const string AnalogAudio9 = "ANALOGAUDIO_9";
        public const string AnalogAudio10 = "ANALOGAUDIO_10";

        public const string Search = "SEARCH";
        public const string Bd1 = "BD";
        public const string Catv1 = "CATV";
        public const string Game1 = "GAME";
        public const string Pc1 = "PC";
        public const string Bluetooth1 = "BLUETOOTH";
        public const string MediaPlayer1 = "MEDIA_PLAYER";
        public const string Ipod1 = "IPOD";

        public const string TunerPresetUp = "PRESET+";
        public const string TunerPresetDown = "PRESET-";
        public const string AutoFrequencyUp = "TunerAutoFrequencyUp";
        public const string AutoFrequencyDown = "TunerAutoFrequencyDown";
        public const string BandAm = "BandAm";
        public const string BandFm = "BandFm";


        public const string BassLevelUp = "ToneBassLevelUp";
        public const string BassLevelDown = "ToneBassLevelDown";
        public const string TrebleLevelUp = "ToneTrebleLevelUp";
        public const string TrebleLevelDown = "ToneTrebleLevelDown";
        public const string LoudnessOn = "LoudnessOn";
        public const string LoudnessOff = "LoudnessOff";
        public const string LoudnessToggle = "LoudnessToggle";
        public const string ToneControlOn = "ToneControlOn";
        public const string ToneControlOff = "ToneControlOff";
        public const string ToneControlToggle = "ToneControlToggle";
        public const string SurroundModeCycle = "SurroundModeCycle";

    }

    public enum IrStandardCommandIds
    {
        UpArrow = 236,
        DownArrow = 96,
        LeftArrow = 138,
        RightArrow = 191,
        Select = 611,
        A = 411,
        B = 412,
        C = 413,
        Red = 491,
        Green = 492,
        Blue = 493,
        Yellow = 494,
        Enter = 99,
        Exit = 100,
        Guide = 113,
        Home = 114,
        Menu = 142,
        Return = 186,
        Info = 410,
        TopMenu = 489,
        PopUpMenu = 490,
        Back = 495,
        ForwardScan = 112,
        ReverseScan = 194,
        Pause = 161,
        Play = 163,
        Record = 182,
        Replay = 414,
        Live = 415,
        Dvr = 416,
        Power = 164,
        PowerOff = 165,
        PowerOn = 166,
        VolMinus = 243,
        VolPlus = 244,
        Mute = 157,
        MuteOff = 158,
        MuteOn = 159,
        PageUp = 252,
        PageDown = 253,
        OnScreenDisplay = 364,
        OnScreenDisplayOn = 365,
        OnScreenDisplayOff = 366,
        Component1 = 363,
        Component2 = 374,
        Dvi1 = 375,
        Hdmi1 = 406,
        Hdmi2 = 407,
        Hdmi3 = 408,
        Hdmi4 = 409,
        Hdmi5 = 530,
        Hdmi6 = 531,
        Input1 = 115,
        Input2 = 117,
        Input3 = 118,
        Input4 = 119,
        Input5 = 120,
        Input6 = 121,
        Input7 = 122,
        Input8 = 123,
        Input9 = 124,
        Input10 = 116,
        Last = 136,
        Octothorpe = 1,
        Asterisk = 2,
        _0 = 3,
        _1 = 4,
        _2 = 5,
        _3 = 6,
        _4 = 7,
        _5 = 8,
        _6 = 9,
        _7 = 10,
        _8 = 11,
        _9 = 12,
        Eject = 98,
        HangUp = 321,
        PictureMute = 379,
        PictureMuteOn = 380,
        PictureMuteOff = 381,
        NearEndZoomPlus = 329,
        NearEndZoomMinus = 330,
        NearEndFocusPlus = 331,
        NearEndFocusMinus = 332,
        NearEndPanLeft = 333,
        NearEndPanRight = 334,
        NearEndTiltUp = 335,
        NearEndTiltDown = 336,
        FarEndZoomPlus = 342,
        FarEndZoomMinus = 343,
        FarEndFocusPlus = 344,
        FarEndFocusMinus = 345,
        FarEndPanLeft = 346,
        FarEndPanRight = 347,
        FarEndTiltUp = 348,
        FarEndTiltDown = 349,
        Clear = 34,
        ChannelUp = 33,
        ChannelDown = 32,
        Repeat = 185,
        ForwardSkip = 218,
        ReverseSkip = 217,
        DVD = 389,
        SAT = 391,
        AUX1 = 18,
        AUX2 = 19,
        TV = 390,
        CD = 31,
        TUNER = 235,
        PHONO = 294,
        DSS = 97,
        //Additional Command Ids added
        PLAY_PAUSE = 617,
        VGA_1 = 618,
        VGA_2 = 619,
        VGA_3 = 620,
        VGA_4 = 621,
        VGA_5 = 622,
        DVI_1 = 623,
        DVI_2 = 624,
        DVI_3 = 625,
        DVI_4 = 626,
        DVI_5 = 627,
        DISPLAY_PORT_1 = 628,
        DISPLAY_PORT_2 = 629,
        DISPLAY_PORT_3 = 630,
        DISPLAY_PORT_4 = 631,
        DISPLAY_PORT_5 = 632,
        NETWORK_1 = 633,
        NETWORK_2 = 634,
        NETWORK_3 = 635,
        NETWORK_4 = 636,
        NETWORK_5 = 637,
        NETWORK_6 = 638,
        NETWORK_7 = 639,
        NETWORK_8 = 640,
        NETWORK_9 = 641,
        NETWORK_10 = 642,
        HDMI_7 = 643,
        HDMI_8 = 644,
        HDMI_9 = 645,
        HDMI_10 = 646,
        D = 647,
        AUDIO = 648,
        INTERNET_RADIO = 649,
        SIRIUS = 650,
        SIRIUS_XM = 651,
        PANDORA = 652,
        LAST_FM = 653,
        RHAPSODY = 654,
        HD_RADIO = 655,
        SPOTIFY = 656,
        YOUTUBE = 657,
        YOUTUBE_TV = 658,
        NETFLIX = 659,
        HULU = 660,
        DIRECTV_NOW = 661,
        AMAZON_VIDEO = 662,
        PLAYSTATION_VUE = 663,
        SLING_TV = 664,
        THUMBS_UP = 665,
        THUMBS_DOWN = 666,
        PERIOD = 667,
        OPTIONS = 668,
        SUBTITLE = 669,
        TUNE_PLUS = 670,
        TUNE_MINUS = 671,
        AIRPLAY = 672,
        GOOGLECAST = 673,
        DLNA = 674,
        TIDAL = 675,
        DEEZER = 676,
        CRACKLE = 677,
        TIVO = 678,
        ON_DEMAND = 679,
        R_SKIP = 680,
        F_SKIP = 681,
        //SETTINGS = 682,
        OPTICAL_1 = 683,
        OPTICAL_2 = 684,
        OPTICAL_3 = 685,
        OPTICAL_4 = 686,
        OPTICAL_5 = 687,
        OPTICAL_6 = 688,
        OPTICAL_7 = 689,
        OPTICAL_8 = 690,
        OPTICAL_9 = 691,
        OPTICAL_10 = 692,
        COAX_1 = 693,
        COAX_2 = 694,
        COAX_3 = 695,
        COAX_4 = 696,
        COAX_5 = 697,
        COAX_6 = 698,
        COAX_7 = 699,
        COAX_8 = 700,
        COAX_9 = 701,
        COAX_10 = 702,
        ANALOGAUDIO_1 = 703,
        ANALOGAUDIO_2 = 704,
        ANALOGAUDIO_3 = 705,
        ANALOGAUDIO_4 = 706,
        ANALOGAUDIO_5 = 707,
        ANALOGAUDIO_6 = 708,
        ANALOGAUDIO_7 = 709,
        ANALOGAUDIO_8 = 710,
        ANALOGAUDIO_9 = 711,
        ANALOGAUDIO_10 = 712,
        USB_1 = 713,
        USB_2 = 714,
        USB_3 = 715,
        USB_4 = 716,
        USB_5 = 717,
        Dash = 418,
        KeypadBackSpace = 565,
        XmRadio = 487,
        Search = 496,
        Bd1 = 724,
        Catv1 = 725,
        Game1 = 726,
        Pc1 = 727,
        Bluetooth1 = 728,
        MediaPlayer1 = 729,
        Ipod1 = 730,
        TUNE_TOGGLE = 424,
        Favorite = 103,

        //Tuner commands added
        PresetPlus = 178,
        PresetMinus = 167
    }

    public class StandardCommand
    {
        public string StandardCommandString { get; set; }
        public string Description { get; set; }
    }

    public static class StandardCommands
    {
        private static Dictionary<StandardCommandsEnum, int> _irStandardCommandIdToStandardEnumMapping;

        public static Dictionary<StandardCommandsEnum, int> IrStandardCommandIdToStandardEnumMapping
        {
            get
            {
                if (_irStandardCommandIdToStandardEnumMapping != null)
                {
                    return _irStandardCommandIdToStandardEnumMapping;
                }

                var result = new Dictionary<StandardCommandsEnum, int>();
                // The values are from a CSV file mapping Standard Command IDs to the standard command (Dropdown in Toolbox-DeviceLearner)
                result.Add(StandardCommandsEnum.UpArrow, (int)IrStandardCommandIds.UpArrow);
                result.Add(StandardCommandsEnum.DownArrow, (int)IrStandardCommandIds.DownArrow);
                result.Add(StandardCommandsEnum.LeftArrow, (int)IrStandardCommandIds.LeftArrow);
                result.Add(StandardCommandsEnum.RightArrow, (int)IrStandardCommandIds.RightArrow);
                result.Add(StandardCommandsEnum.Select, (int)IrStandardCommandIds.Select);
                result.Add(StandardCommandsEnum.A, (int)IrStandardCommandIds.A);
                result.Add(StandardCommandsEnum.B, (int)IrStandardCommandIds.B);
                result.Add(StandardCommandsEnum.C, (int)IrStandardCommandIds.C);
                result.Add(StandardCommandsEnum.Red, (int)IrStandardCommandIds.Red);
                result.Add(StandardCommandsEnum.Green, (int)IrStandardCommandIds.Green);
                result.Add(StandardCommandsEnum.Blue, (int)IrStandardCommandIds.Blue);
                result.Add(StandardCommandsEnum.Yellow, (int)IrStandardCommandIds.Yellow);
                result.Add(StandardCommandsEnum.Enter, (int)IrStandardCommandIds.Enter);
                result.Add(StandardCommandsEnum.Exit, (int)IrStandardCommandIds.Exit);
                result.Add(StandardCommandsEnum.Guide, (int)IrStandardCommandIds.Guide);
                result.Add(StandardCommandsEnum.Home, (int)IrStandardCommandIds.Home);
                result.Add(StandardCommandsEnum.Menu, (int)IrStandardCommandIds.Menu);
                result.Add(StandardCommandsEnum.Return, (int)IrStandardCommandIds.Return);
                result.Add(StandardCommandsEnum.Info, (int)IrStandardCommandIds.Info);
                result.Add(StandardCommandsEnum.TopMenu, (int)IrStandardCommandIds.TopMenu);
                result.Add(StandardCommandsEnum.PopUpMenu, (int)IrStandardCommandIds.PopUpMenu);
                result.Add(StandardCommandsEnum.Back, (int)IrStandardCommandIds.Back);
                result.Add(StandardCommandsEnum.ForwardScan, (int)IrStandardCommandIds.ForwardScan);
                result.Add(StandardCommandsEnum.ReverseScan, (int)IrStandardCommandIds.ReverseScan);
                result.Add(StandardCommandsEnum.Pause, (int)IrStandardCommandIds.Pause);
                result.Add(StandardCommandsEnum.Play, (int)IrStandardCommandIds.Play);
                result.Add(StandardCommandsEnum.Record, (int)IrStandardCommandIds.Record);
                result.Add(StandardCommandsEnum.Replay, (int)IrStandardCommandIds.Replay);
                result.Add(StandardCommandsEnum.Live, (int)IrStandardCommandIds.Live);
                result.Add(StandardCommandsEnum.Dvr, (int)IrStandardCommandIds.Dvr);
                result.Add(StandardCommandsEnum.Power, (int)IrStandardCommandIds.Power);
                result.Add(StandardCommandsEnum.PowerOff, (int)IrStandardCommandIds.PowerOff);
                result.Add(StandardCommandsEnum.PowerOn, (int)IrStandardCommandIds.PowerOn);
                result.Add(StandardCommandsEnum.VolMinus, (int)IrStandardCommandIds.VolMinus);
                result.Add(StandardCommandsEnum.VolPlus, (int)IrStandardCommandIds.VolPlus);
                result.Add(StandardCommandsEnum.Mute, (int)IrStandardCommandIds.Mute);
                result.Add(StandardCommandsEnum.MuteOff, (int)IrStandardCommandIds.MuteOff);
                result.Add(StandardCommandsEnum.MuteOn, (int)IrStandardCommandIds.MuteOn);
                result.Add(StandardCommandsEnum.PageUp, (int)IrStandardCommandIds.PageUp);
                result.Add(StandardCommandsEnum.PageDown, (int)IrStandardCommandIds.PageDown);
                result.Add(StandardCommandsEnum.OnScreenDisplay, (int)IrStandardCommandIds.OnScreenDisplay);
                result.Add(StandardCommandsEnum.OnScreenDisplayOn, (int)IrStandardCommandIds.OnScreenDisplayOn);
                result.Add(StandardCommandsEnum.OnScreenDisplayOff, (int)IrStandardCommandIds.OnScreenDisplayOff);
                result.Add(StandardCommandsEnum.Component1, (int)IrStandardCommandIds.Component1);
                result.Add(StandardCommandsEnum.Component2, (int)IrStandardCommandIds.Component2);
                result.Add(StandardCommandsEnum.Hdmi1, (int)IrStandardCommandIds.Hdmi1);
                result.Add(StandardCommandsEnum.Hdmi2, (int)IrStandardCommandIds.Hdmi2);
                result.Add(StandardCommandsEnum.Hdmi3, (int)IrStandardCommandIds.Hdmi3);
                result.Add(StandardCommandsEnum.Hdmi4, (int)IrStandardCommandIds.Hdmi4);
                result.Add(StandardCommandsEnum.Hdmi5, (int)IrStandardCommandIds.Hdmi5);
                result.Add(StandardCommandsEnum.Hdmi6, (int)IrStandardCommandIds.Hdmi6);
                result.Add(StandardCommandsEnum.Input1, (int)IrStandardCommandIds.Input1);
                result.Add(StandardCommandsEnum.Input2, (int)IrStandardCommandIds.Input2);
                result.Add(StandardCommandsEnum.Input3, (int)IrStandardCommandIds.Input3);
                result.Add(StandardCommandsEnum.Input4, (int)IrStandardCommandIds.Input4);
                result.Add(StandardCommandsEnum.Input5, (int)IrStandardCommandIds.Input5);
                result.Add(StandardCommandsEnum.Input6, (int)IrStandardCommandIds.Input6);
                result.Add(StandardCommandsEnum.Input7, (int)IrStandardCommandIds.Input7);
                result.Add(StandardCommandsEnum.Input8, (int)IrStandardCommandIds.Input8);
                result.Add(StandardCommandsEnum.Input9, (int)IrStandardCommandIds.Input9);
                result.Add(StandardCommandsEnum.Input10, (int)IrStandardCommandIds.Input10);
                result.Add(StandardCommandsEnum.Last, (int)IrStandardCommandIds.Last);
                result.Add(StandardCommandsEnum.Octothorpe, (int)IrStandardCommandIds.Octothorpe);
                result.Add(StandardCommandsEnum.Asterisk, (int)IrStandardCommandIds.Asterisk);
                result.Add(StandardCommandsEnum._0, (int)IrStandardCommandIds._0);
                result.Add(StandardCommandsEnum._1, (int)IrStandardCommandIds._1);
                result.Add(StandardCommandsEnum._2, (int)IrStandardCommandIds._2);
                result.Add(StandardCommandsEnum._3, (int)IrStandardCommandIds._3);
                result.Add(StandardCommandsEnum._4, (int)IrStandardCommandIds._4);
                result.Add(StandardCommandsEnum._5, (int)IrStandardCommandIds._5);
                result.Add(StandardCommandsEnum._6, (int)IrStandardCommandIds._6);
                result.Add(StandardCommandsEnum._7, (int)IrStandardCommandIds._7);
                result.Add(StandardCommandsEnum._8, (int)IrStandardCommandIds._8);
                result.Add(StandardCommandsEnum._9, (int)IrStandardCommandIds._9);
                result.Add(StandardCommandsEnum.Eject, (int)IrStandardCommandIds.Eject);
                result.Add(StandardCommandsEnum.HangUp, (int)IrStandardCommandIds.HangUp);
                result.Add(StandardCommandsEnum.Clear, (int)IrStandardCommandIds.Clear);
                result.Add(StandardCommandsEnum.ChannelUp, (int)IrStandardCommandIds.ChannelUp);
                result.Add(StandardCommandsEnum.ChannelDown, (int)IrStandardCommandIds.ChannelDown);
                result.Add(StandardCommandsEnum.VideoMute, (int)IrStandardCommandIds.PictureMute);
                result.Add(StandardCommandsEnum.VideoMuteOn, (int)IrStandardCommandIds.PictureMuteOn);
                result.Add(StandardCommandsEnum.VideoMuteOff, (int)IrStandardCommandIds.PictureMuteOff);
                result.Add(StandardCommandsEnum.CameraFarEndFocusFar, (int)IrStandardCommandIds.FarEndFocusPlus);
                result.Add(StandardCommandsEnum.CameraFarEndFocusNear, (int)IrStandardCommandIds.FarEndFocusMinus);
                result.Add(StandardCommandsEnum.CameraFarEndPanLeft, (int)IrStandardCommandIds.FarEndPanLeft);
                result.Add(StandardCommandsEnum.CameraFarEndPanRight, (int)IrStandardCommandIds.FarEndPanRight);
                result.Add(StandardCommandsEnum.CameraFarEndZoomIn, (int)IrStandardCommandIds.FarEndZoomPlus);
                result.Add(StandardCommandsEnum.CameraFarEndZoomOut, (int)IrStandardCommandIds.FarEndZoomMinus);
                result.Add(StandardCommandsEnum.CameraFarEndTiltDown, (int)IrStandardCommandIds.FarEndTiltDown);
                result.Add(StandardCommandsEnum.CameraFarEndTiltUp, (int)IrStandardCommandIds.FarEndTiltUp);
                result.Add(StandardCommandsEnum.CameraNearEndFocusFar, (int)IrStandardCommandIds.NearEndFocusPlus);
                result.Add(StandardCommandsEnum.CameraNearEndFocusNear, (int)IrStandardCommandIds.NearEndFocusMinus);
                result.Add(StandardCommandsEnum.CameraNearEndPanLeft, (int)IrStandardCommandIds.NearEndPanLeft);
                result.Add(StandardCommandsEnum.CameraNearEndPanRight, (int)IrStandardCommandIds.NearEndPanRight);
                result.Add(StandardCommandsEnum.CameraNearEndZoomIn, (int)IrStandardCommandIds.NearEndZoomPlus);
                result.Add(StandardCommandsEnum.CameraNearEndZoomOut, (int)IrStandardCommandIds.NearEndZoomMinus);
                result.Add(StandardCommandsEnum.CameraNearEndTiltDown, (int)IrStandardCommandIds.NearEndTiltDown);
                result.Add(StandardCommandsEnum.CameraNearEndTiltUp, (int)IrStandardCommandIds.NearEndTiltUp);
                result.Add(StandardCommandsEnum.Repeat, (int)IrStandardCommandIds.Repeat);
                result.Add(StandardCommandsEnum.DVD, (int)IrStandardCommandIds.DVD);
                result.Add(StandardCommandsEnum.SAT, (int)IrStandardCommandIds.SAT);
                result.Add(StandardCommandsEnum.Aux1, (int)IrStandardCommandIds.AUX1);
                result.Add(StandardCommandsEnum.Aux2, (int)IrStandardCommandIds.AUX2);
                result.Add(StandardCommandsEnum.TV, (int)IrStandardCommandIds.TV);
                result.Add(StandardCommandsEnum.CD, (int)IrStandardCommandIds.CD);
                result.Add(StandardCommandsEnum.Tuner, (int)IrStandardCommandIds.TUNER);
                result.Add(StandardCommandsEnum.Phono, (int)IrStandardCommandIds.PHONO);
                result.Add(StandardCommandsEnum.DSS, (int)IrStandardCommandIds.DSS);
                result.Add(StandardCommandsEnum.PlayPause, (int)IrStandardCommandIds.PLAY_PAUSE);
                result.Add(StandardCommandsEnum.Vga1, (int)IrStandardCommandIds.VGA_1);
                result.Add(StandardCommandsEnum.Vga2, (int)IrStandardCommandIds.VGA_2);
                result.Add(StandardCommandsEnum.Vga3, (int)IrStandardCommandIds.VGA_3);
                result.Add(StandardCommandsEnum.Vga4, (int)IrStandardCommandIds.VGA_4);
                result.Add(StandardCommandsEnum.Vga5, (int)IrStandardCommandIds.VGA_5);
                result.Add(StandardCommandsEnum.Dvi1, (int)IrStandardCommandIds.DVI_1);
                result.Add(StandardCommandsEnum.Dvi2, (int)IrStandardCommandIds.DVI_2);
                result.Add(StandardCommandsEnum.Dvi3, (int)IrStandardCommandIds.DVI_3);
                result.Add(StandardCommandsEnum.Dvi4, (int)IrStandardCommandIds.DVI_4);
                result.Add(StandardCommandsEnum.Dvi5, (int)IrStandardCommandIds.DVI_5);
                result.Add(StandardCommandsEnum.DisplayPort1, (int)IrStandardCommandIds.DISPLAY_PORT_1);
                result.Add(StandardCommandsEnum.DisplayPort2, (int)IrStandardCommandIds.DISPLAY_PORT_2);
                result.Add(StandardCommandsEnum.DisplayPort3, (int)IrStandardCommandIds.DISPLAY_PORT_3);
                result.Add(StandardCommandsEnum.DisplayPort4, (int)IrStandardCommandIds.DISPLAY_PORT_4);
                result.Add(StandardCommandsEnum.DisplayPort5, (int)IrStandardCommandIds.DISPLAY_PORT_5);
                result.Add(StandardCommandsEnum.Network1, (int)IrStandardCommandIds.NETWORK_1);
                result.Add(StandardCommandsEnum.Network2, (int)IrStandardCommandIds.NETWORK_2);
                result.Add(StandardCommandsEnum.Network3, (int)IrStandardCommandIds.NETWORK_3);
                result.Add(StandardCommandsEnum.Network4, (int)IrStandardCommandIds.NETWORK_4);
                result.Add(StandardCommandsEnum.Network5, (int)IrStandardCommandIds.NETWORK_5);
                result.Add(StandardCommandsEnum.Network6, (int)IrStandardCommandIds.NETWORK_6);
                result.Add(StandardCommandsEnum.Network7, (int)IrStandardCommandIds.NETWORK_7);
                result.Add(StandardCommandsEnum.Network8, (int)IrStandardCommandIds.NETWORK_8);
                result.Add(StandardCommandsEnum.Network9, (int)IrStandardCommandIds.NETWORK_9);
                result.Add(StandardCommandsEnum.Network10, (int)IrStandardCommandIds.NETWORK_10);
                result.Add(StandardCommandsEnum.Hdmi7, (int)IrStandardCommandIds.HDMI_7);
                result.Add(StandardCommandsEnum.Hdmi8, (int)IrStandardCommandIds.HDMI_8);
                result.Add(StandardCommandsEnum.Hdmi9, (int)IrStandardCommandIds.HDMI_9);
                result.Add(StandardCommandsEnum.Hdmi10, (int)IrStandardCommandIds.HDMI_10);
                result.Add(StandardCommandsEnum.D, (int)IrStandardCommandIds.D);
                result.Add(StandardCommandsEnum.Audio, (int)IrStandardCommandIds.AUDIO);
                result.Add(StandardCommandsEnum.InternetRadio, (int)IrStandardCommandIds.INTERNET_RADIO);
                result.Add(StandardCommandsEnum.Sirius, (int)IrStandardCommandIds.SIRIUS);
                result.Add(StandardCommandsEnum.SiriusXm, (int)IrStandardCommandIds.SIRIUS_XM);
                result.Add(StandardCommandsEnum.Pandora, (int)IrStandardCommandIds.PANDORA);
                result.Add(StandardCommandsEnum.LastFm, (int)IrStandardCommandIds.LAST_FM);
                result.Add(StandardCommandsEnum.Rhapsody, (int)IrStandardCommandIds.RHAPSODY);
                result.Add(StandardCommandsEnum.HdRadio, (int)IrStandardCommandIds.HD_RADIO);
                result.Add(StandardCommandsEnum.Spotify, (int)IrStandardCommandIds.SPOTIFY);
                result.Add(StandardCommandsEnum.YouTube, (int)IrStandardCommandIds.YOUTUBE);
                result.Add(StandardCommandsEnum.YouTubeTv, (int)IrStandardCommandIds.YOUTUBE_TV);
                result.Add(StandardCommandsEnum.Netflix, (int)IrStandardCommandIds.NETFLIX);
                result.Add(StandardCommandsEnum.Hulu, (int)IrStandardCommandIds.HULU);
                result.Add(StandardCommandsEnum.DirecTvNow, (int)IrStandardCommandIds.DIRECTV_NOW);
                result.Add(StandardCommandsEnum.AmazonVideo, (int)IrStandardCommandIds.AMAZON_VIDEO);
                result.Add(StandardCommandsEnum.PlayStationVue, (int)IrStandardCommandIds.PLAYSTATION_VUE);
                result.Add(StandardCommandsEnum.SlingTv, (int)IrStandardCommandIds.SLING_TV);
                result.Add(StandardCommandsEnum.ThumbsUp, (int)IrStandardCommandIds.THUMBS_UP);
                result.Add(StandardCommandsEnum.ThumbsDown, (int)IrStandardCommandIds.THUMBS_DOWN);
                result.Add(StandardCommandsEnum.Period, (int)IrStandardCommandIds.PERIOD);
                result.Add(StandardCommandsEnum.Options, (int)IrStandardCommandIds.OPTIONS);
                result.Add(StandardCommandsEnum.Subtitle, (int)IrStandardCommandIds.SUBTITLE);
                result.Add(StandardCommandsEnum.TunerFrequencyUp, (int)IrStandardCommandIds.TUNE_PLUS);
                result.Add(StandardCommandsEnum.TunerFrequencyDown, (int)IrStandardCommandIds.TUNE_MINUS);
                result.Add(StandardCommandsEnum.TunerFrequencyBand, (int)IrStandardCommandIds.TUNE_TOGGLE);
                result.Add(StandardCommandsEnum.AirPlay, (int)IrStandardCommandIds.AIRPLAY);
                result.Add(StandardCommandsEnum.GoogleCast, (int)IrStandardCommandIds.GOOGLECAST);
                result.Add(StandardCommandsEnum.Dlna, (int)IrStandardCommandIds.DLNA);
                result.Add(StandardCommandsEnum.Tidal, (int)IrStandardCommandIds.TIDAL);
                result.Add(StandardCommandsEnum.Deezer, (int)IrStandardCommandIds.DEEZER);
                result.Add(StandardCommandsEnum.Crackle, (int)IrStandardCommandIds.CRACKLE);
                result.Add(StandardCommandsEnum.Tivo, (int)IrStandardCommandIds.TIVO);
                result.Add(StandardCommandsEnum.OnDemand, (int)IrStandardCommandIds.ON_DEMAND);
                result.Add(StandardCommandsEnum.RSkip, (int)IrStandardCommandIds.R_SKIP);
                result.Add(StandardCommandsEnum.FSkip, (int)IrStandardCommandIds.F_SKIP);

                //TODO JASON L
                //result.Add(StandardCommandsEnum.Settings, (int)IrStandardCommandIds.SETTINGS);
                result.Add(StandardCommandsEnum.Optical1, (int)IrStandardCommandIds.OPTICAL_1);
                result.Add(StandardCommandsEnum.Optical2, (int)IrStandardCommandIds.OPTICAL_2);
                result.Add(StandardCommandsEnum.Optical3, (int)IrStandardCommandIds.OPTICAL_3);
                result.Add(StandardCommandsEnum.Optical4, (int)IrStandardCommandIds.OPTICAL_4);
                result.Add(StandardCommandsEnum.Optical5, (int)IrStandardCommandIds.OPTICAL_5);
                result.Add(StandardCommandsEnum.Optical6, (int)IrStandardCommandIds.OPTICAL_6);
                result.Add(StandardCommandsEnum.Optical7, (int)IrStandardCommandIds.OPTICAL_7);
                result.Add(StandardCommandsEnum.Optical8, (int)IrStandardCommandIds.OPTICAL_8);
                result.Add(StandardCommandsEnum.Optical9, (int)IrStandardCommandIds.OPTICAL_9);
                result.Add(StandardCommandsEnum.Optical10, (int)IrStandardCommandIds.OPTICAL_10);

                result.Add(StandardCommandsEnum.Coax1, (int)IrStandardCommandIds.COAX_1);
                result.Add(StandardCommandsEnum.Coax2, (int)IrStandardCommandIds.COAX_2);
                result.Add(StandardCommandsEnum.Coax3, (int)IrStandardCommandIds.COAX_3);
                result.Add(StandardCommandsEnum.Coax4, (int)IrStandardCommandIds.COAX_4);
                result.Add(StandardCommandsEnum.Coax5, (int)IrStandardCommandIds.COAX_5);
                result.Add(StandardCommandsEnum.Coax6, (int)IrStandardCommandIds.COAX_6);
                result.Add(StandardCommandsEnum.Coax7, (int)IrStandardCommandIds.COAX_7);
                result.Add(StandardCommandsEnum.Coax8, (int)IrStandardCommandIds.COAX_8);
                result.Add(StandardCommandsEnum.Coax9, (int)IrStandardCommandIds.COAX_9);
                result.Add(StandardCommandsEnum.Coax10, (int)IrStandardCommandIds.COAX_10);

                result.Add(StandardCommandsEnum.AnalogAudio1, (int)IrStandardCommandIds.ANALOGAUDIO_1);
                result.Add(StandardCommandsEnum.AnalogAudio2, (int)IrStandardCommandIds.ANALOGAUDIO_2);
                result.Add(StandardCommandsEnum.AnalogAudio3, (int)IrStandardCommandIds.ANALOGAUDIO_3);
                result.Add(StandardCommandsEnum.AnalogAudio4, (int)IrStandardCommandIds.ANALOGAUDIO_4);
                result.Add(StandardCommandsEnum.AnalogAudio5, (int)IrStandardCommandIds.ANALOGAUDIO_5);
                result.Add(StandardCommandsEnum.AnalogAudio6, (int)IrStandardCommandIds.ANALOGAUDIO_6);
                result.Add(StandardCommandsEnum.AnalogAudio7, (int)IrStandardCommandIds.ANALOGAUDIO_7);
                result.Add(StandardCommandsEnum.AnalogAudio8, (int)IrStandardCommandIds.ANALOGAUDIO_8);
                result.Add(StandardCommandsEnum.AnalogAudio9, (int)IrStandardCommandIds.ANALOGAUDIO_9);
                result.Add(StandardCommandsEnum.AnalogAudio10, (int)IrStandardCommandIds.ANALOGAUDIO_10);

                result.Add(StandardCommandsEnum.Usb1, (int)IrStandardCommandIds.USB_1);
                result.Add(StandardCommandsEnum.Usb2, (int)IrStandardCommandIds.USB_2);
                result.Add(StandardCommandsEnum.Usb3, (int)IrStandardCommandIds.USB_3);
                result.Add(StandardCommandsEnum.Usb4, (int)IrStandardCommandIds.USB_4);
                result.Add(StandardCommandsEnum.Usb5, (int)IrStandardCommandIds.USB_5);

                result.Add(StandardCommandsEnum.Dash, (int)IrStandardCommandIds.Dash);
                result.Add(StandardCommandsEnum.KeypadBackSpace, (int)IrStandardCommandIds.KeypadBackSpace);
                result.Add(StandardCommandsEnum.Xm, (int)IrStandardCommandIds.XmRadio);
                result.Add(StandardCommandsEnum.Search, (int)IrStandardCommandIds.Search);

                result.Add(StandardCommandsEnum.Bd1, (int)IrStandardCommandIds.Bd1);
                result.Add(StandardCommandsEnum.Catv1, (int)IrStandardCommandIds.Catv1);
                result.Add(StandardCommandsEnum.Game1, (int)IrStandardCommandIds.Game1);
                result.Add(StandardCommandsEnum.Pc1, (int)IrStandardCommandIds.Pc1);
                result.Add(StandardCommandsEnum.Bluetooth1, (int)IrStandardCommandIds.Bluetooth1);
                result.Add(StandardCommandsEnum.MediaPlayer1, (int)IrStandardCommandIds.MediaPlayer1);
                result.Add(StandardCommandsEnum.Ipod1, (int)IrStandardCommandIds.Ipod1);

                //tuner commands
                result.Add(StandardCommandsEnum.TunerPresetUp, (int)IrStandardCommandIds.PresetPlus);
                result.Add(StandardCommandsEnum.TunerPresetDown, (int)IrStandardCommandIds.PresetMinus);

                _irStandardCommandIdToStandardEnumMapping = result;

                return _irStandardCommandIdToStandardEnumMapping;
            }
        }

        private static Dictionary<StandardCommandsEnum, StandardCommand> _standardEnumToCommandMapping;
        public static Dictionary<StandardCommandsEnum, StandardCommand> StandardEnumToCommandMapping
        {
            get
            {
                if (_standardEnumToCommandMapping != null)
                    return _standardEnumToCommandMapping;
                var result = new Dictionary<StandardCommandsEnum, StandardCommand>();
                result.Add(StandardCommandsEnum.Vga1, new StandardCommand { StandardCommandString = IrCommandConstants.Vga1 });
                result.Add(StandardCommandsEnum.Vga2, new StandardCommand { StandardCommandString = IrCommandConstants.Vga2 });
                result.Add(StandardCommandsEnum.Vga3, new StandardCommand { StandardCommandString = IrCommandConstants.Vga3 });
                result.Add(StandardCommandsEnum.Vga4, new StandardCommand { StandardCommandString = IrCommandConstants.Vga4 });
                result.Add(StandardCommandsEnum.Vga5, new StandardCommand { StandardCommandString = IrCommandConstants.Vga5 });
                result.Add(StandardCommandsEnum.Vga6, new StandardCommand { StandardCommandString = IrCommandConstants.Vga6 });
                result.Add(StandardCommandsEnum.Vga7, new StandardCommand { StandardCommandString = IrCommandConstants.Vga7 });
                result.Add(StandardCommandsEnum.Vga8, new StandardCommand { StandardCommandString = IrCommandConstants.Vga8 });
                result.Add(StandardCommandsEnum.Vga9, new StandardCommand { StandardCommandString = IrCommandConstants.Vga9 });
                result.Add(StandardCommandsEnum.Vga10, new StandardCommand { StandardCommandString = IrCommandConstants.Vga10 });
                result.Add(StandardCommandsEnum.Hdmi1, new StandardCommand { StandardCommandString = IrCommandConstants.Hdmi1 });
                result.Add(StandardCommandsEnum.Hdmi2, new StandardCommand { StandardCommandString = IrCommandConstants.Hdmi2 });
                result.Add(StandardCommandsEnum.Hdmi3, new StandardCommand { StandardCommandString = IrCommandConstants.Hdmi3 });
                result.Add(StandardCommandsEnum.Hdmi4, new StandardCommand { StandardCommandString = IrCommandConstants.Hdmi4 });
                result.Add(StandardCommandsEnum.Hdmi5, new StandardCommand { StandardCommandString = IrCommandConstants.Hdmi5 });
                result.Add(StandardCommandsEnum.Hdmi6, new StandardCommand { StandardCommandString = IrCommandConstants.Hdmi6 });
                result.Add(StandardCommandsEnum.Hdmi7, new StandardCommand { StandardCommandString = IrCommandConstants.Hdmi7 });
                result.Add(StandardCommandsEnum.Hdmi8, new StandardCommand { StandardCommandString = IrCommandConstants.Hdmi8 });
                result.Add(StandardCommandsEnum.Hdmi9, new StandardCommand { StandardCommandString = IrCommandConstants.Hdmi9 });
                result.Add(StandardCommandsEnum.Hdmi10, new StandardCommand { StandardCommandString = IrCommandConstants.Hdmi10 });
                result.Add(StandardCommandsEnum.Dvi1, new StandardCommand { StandardCommandString = IrCommandConstants.Dvi1 });
                result.Add(StandardCommandsEnum.Dvi2, new StandardCommand { StandardCommandString = IrCommandConstants.Dvi2 });
                result.Add(StandardCommandsEnum.Dvi3, new StandardCommand { StandardCommandString = IrCommandConstants.Dvi3 });
                result.Add(StandardCommandsEnum.Dvi4, new StandardCommand { StandardCommandString = IrCommandConstants.Dvi4 });
                result.Add(StandardCommandsEnum.Dvi5, new StandardCommand { StandardCommandString = IrCommandConstants.Dvi5 });
                result.Add(StandardCommandsEnum.Dvi6, new StandardCommand { StandardCommandString = IrCommandConstants.Dvi6 });
                result.Add(StandardCommandsEnum.Dvi7, new StandardCommand { StandardCommandString = IrCommandConstants.Dvi7 });
                result.Add(StandardCommandsEnum.Dvi8, new StandardCommand { StandardCommandString = IrCommandConstants.Dvi8 });
                result.Add(StandardCommandsEnum.Dvi9, new StandardCommand { StandardCommandString = IrCommandConstants.Dvi9 });
                result.Add(StandardCommandsEnum.Dvi10, new StandardCommand { StandardCommandString = IrCommandConstants.Dvi10 });
                result.Add(StandardCommandsEnum.Component1, new StandardCommand { StandardCommandString = IrCommandConstants.Component1 });
                result.Add(StandardCommandsEnum.Component2, new StandardCommand { StandardCommandString = IrCommandConstants.Component2 });
                result.Add(StandardCommandsEnum.Component3, new StandardCommand { StandardCommandString = IrCommandConstants.Component3 });
                result.Add(StandardCommandsEnum.Component4, new StandardCommand { StandardCommandString = IrCommandConstants.Component4 });
                result.Add(StandardCommandsEnum.Component5, new StandardCommand { StandardCommandString = IrCommandConstants.Component5 });
                result.Add(StandardCommandsEnum.Component6, new StandardCommand { StandardCommandString = IrCommandConstants.Component6 });
                result.Add(StandardCommandsEnum.Component7, new StandardCommand { StandardCommandString = IrCommandConstants.Component7 });
                result.Add(StandardCommandsEnum.Component8, new StandardCommand { StandardCommandString = IrCommandConstants.Component8 });
                result.Add(StandardCommandsEnum.Component9, new StandardCommand { StandardCommandString = IrCommandConstants.Component9 });
                result.Add(StandardCommandsEnum.Component10, new StandardCommand { StandardCommandString = IrCommandConstants.Component10 });
                result.Add(StandardCommandsEnum.Composite1, new StandardCommand { StandardCommandString = IrCommandConstants.Composite1 });
                result.Add(StandardCommandsEnum.Composite2, new StandardCommand { StandardCommandString = IrCommandConstants.Composite2 });
                result.Add(StandardCommandsEnum.Composite3, new StandardCommand { StandardCommandString = IrCommandConstants.Composite3 });
                result.Add(StandardCommandsEnum.Composite4, new StandardCommand { StandardCommandString = IrCommandConstants.Composite4 });
                result.Add(StandardCommandsEnum.Composite5, new StandardCommand { StandardCommandString = IrCommandConstants.Composite5 });
                result.Add(StandardCommandsEnum.Composite6, new StandardCommand { StandardCommandString = IrCommandConstants.Composite6 });
                result.Add(StandardCommandsEnum.Composite7, new StandardCommand { StandardCommandString = IrCommandConstants.Composite7 });
                result.Add(StandardCommandsEnum.Composite8, new StandardCommand { StandardCommandString = IrCommandConstants.Composite8 });
                result.Add(StandardCommandsEnum.Composite9, new StandardCommand { StandardCommandString = IrCommandConstants.Composite9 });
                result.Add(StandardCommandsEnum.Composite10, new StandardCommand { StandardCommandString = IrCommandConstants.Composite10 });
                result.Add(StandardCommandsEnum.DisplayPort1, new StandardCommand { StandardCommandString = IrCommandConstants.DisplayPort1 });
                result.Add(StandardCommandsEnum.DisplayPort2, new StandardCommand { StandardCommandString = IrCommandConstants.DisplayPort2 });
                result.Add(StandardCommandsEnum.DisplayPort3, new StandardCommand { StandardCommandString = IrCommandConstants.DisplayPort3 });
                result.Add(StandardCommandsEnum.DisplayPort4, new StandardCommand { StandardCommandString = IrCommandConstants.DisplayPort4 });
                result.Add(StandardCommandsEnum.DisplayPort5, new StandardCommand { StandardCommandString = IrCommandConstants.DisplayPort5 });
                result.Add(StandardCommandsEnum.DisplayPort6, new StandardCommand { StandardCommandString = IrCommandConstants.DisplayPort6 });
                result.Add(StandardCommandsEnum.DisplayPort7, new StandardCommand { StandardCommandString = IrCommandConstants.DisplayPort7 });
                result.Add(StandardCommandsEnum.DisplayPort8, new StandardCommand { StandardCommandString = IrCommandConstants.DisplayPort8 });
                result.Add(StandardCommandsEnum.DisplayPort9, new StandardCommand { StandardCommandString = IrCommandConstants.DisplayPort9 });
                result.Add(StandardCommandsEnum.DisplayPort10, new StandardCommand { StandardCommandString = IrCommandConstants.DisplayPort10 });
                result.Add(StandardCommandsEnum.Usb1, new StandardCommand { StandardCommandString = IrCommandConstants.Usb1 });
                result.Add(StandardCommandsEnum.Usb2, new StandardCommand { StandardCommandString = IrCommandConstants.Usb2 });
                result.Add(StandardCommandsEnum.Usb3, new StandardCommand { StandardCommandString = IrCommandConstants.Usb3 });
                result.Add(StandardCommandsEnum.Usb4, new StandardCommand { StandardCommandString = IrCommandConstants.Usb4 });
                result.Add(StandardCommandsEnum.Usb5, new StandardCommand { StandardCommandString = IrCommandConstants.Usb5 });
                result.Add(StandardCommandsEnum.Antenna1, new StandardCommand { StandardCommandString = IrCommandConstants.Antenna1 });
                result.Add(StandardCommandsEnum.Antenna2, new StandardCommand { StandardCommandString = IrCommandConstants.Antenna2 });
                result.Add(StandardCommandsEnum.Network1, new StandardCommand { StandardCommandString = IrCommandConstants.Network1 });
                result.Add(StandardCommandsEnum.Network2, new StandardCommand { StandardCommandString = IrCommandConstants.Network2 });
                result.Add(StandardCommandsEnum.Network3, new StandardCommand { StandardCommandString = IrCommandConstants.Network3 });
                result.Add(StandardCommandsEnum.Network4, new StandardCommand { StandardCommandString = IrCommandConstants.Network4 });
                result.Add(StandardCommandsEnum.Network5, new StandardCommand { StandardCommandString = IrCommandConstants.Network5 });
                result.Add(StandardCommandsEnum.Network6, new StandardCommand { StandardCommandString = IrCommandConstants.Network6 });
                result.Add(StandardCommandsEnum.Network7, new StandardCommand { StandardCommandString = IrCommandConstants.Network7 });
                result.Add(StandardCommandsEnum.Network8, new StandardCommand { StandardCommandString = IrCommandConstants.Network8 });
                result.Add(StandardCommandsEnum.Network9, new StandardCommand { StandardCommandString = IrCommandConstants.Network9 });
                result.Add(StandardCommandsEnum.Network10, new StandardCommand { StandardCommandString = IrCommandConstants.Network10 });
                result.Add(StandardCommandsEnum.Input1, new StandardCommand { StandardCommandString = IrCommandConstants.Input1 });
                result.Add(StandardCommandsEnum.Input2, new StandardCommand { StandardCommandString = IrCommandConstants.Input2 });
                result.Add(StandardCommandsEnum.Input3, new StandardCommand { StandardCommandString = IrCommandConstants.Input3 });
                result.Add(StandardCommandsEnum.Input4, new StandardCommand { StandardCommandString = IrCommandConstants.Input4 });
                result.Add(StandardCommandsEnum.Input5, new StandardCommand { StandardCommandString = IrCommandConstants.Input5 });
                result.Add(StandardCommandsEnum.Input6, new StandardCommand { StandardCommandString = IrCommandConstants.Input6 });
                result.Add(StandardCommandsEnum.Input7, new StandardCommand { StandardCommandString = IrCommandConstants.Input7 });
                result.Add(StandardCommandsEnum.Input8, new StandardCommand { StandardCommandString = IrCommandConstants.Input8 });
                result.Add(StandardCommandsEnum.Input9, new StandardCommand { StandardCommandString = IrCommandConstants.Input9 });
                result.Add(StandardCommandsEnum.Input10, new StandardCommand { StandardCommandString = IrCommandConstants.Input10 });
                result.Add(StandardCommandsEnum.AspectSideBar, null);
                result.Add(StandardCommandsEnum.AspectStrech, null);
                result.Add(StandardCommandsEnum.AspectNormal, null);
                result.Add(StandardCommandsEnum.AspectDotByDot, null);
                result.Add(StandardCommandsEnum.AspectFullScreen, null);
                result.Add(StandardCommandsEnum.AspectAuto, null);
                result.Add(StandardCommandsEnum.AspectOriginal, null);
                result.Add(StandardCommandsEnum.Aspect16By9, null);
                result.Add(StandardCommandsEnum.AspectWideZoom, null);
                result.Add(StandardCommandsEnum.Aspect4By3, null);
                result.Add(StandardCommandsEnum.AspectSubTitle, null);
                result.Add(StandardCommandsEnum.AspectJust, null);
                result.Add(StandardCommandsEnum.AspectZoom, null);
                result.Add(StandardCommandsEnum.AspectZoom2, null);
                result.Add(StandardCommandsEnum.AspectZoom3, null);
                result.Add(StandardCommandsEnum.AspectRatio1, new StandardCommand { StandardCommandString = IrCommandConstants.Aspect1 });
                result.Add(StandardCommandsEnum.AspectRatio2, new StandardCommand { StandardCommandString = IrCommandConstants.Aspect2 });
                result.Add(StandardCommandsEnum.AspectRatio3, new StandardCommand { StandardCommandString = IrCommandConstants.Aspect3 });
                result.Add(StandardCommandsEnum.AspectRatio4, new StandardCommand { StandardCommandString = IrCommandConstants.Aspect4 });
                result.Add(StandardCommandsEnum.AspectRatio5, new StandardCommand { StandardCommandString = IrCommandConstants.Aspect5 });
                result.Add(StandardCommandsEnum.AspectRatio6, new StandardCommand { StandardCommandString = IrCommandConstants.Aspect6 });
                result.Add(StandardCommandsEnum.AspectRatio7, new StandardCommand { StandardCommandString = IrCommandConstants.Aspect7 });
                result.Add(StandardCommandsEnum.AspectRatio8, new StandardCommand { StandardCommandString = IrCommandConstants.Aspect8 });
                result.Add(StandardCommandsEnum.AspectRatio9, null);
                result.Add(StandardCommandsEnum.AspectRatio10, null);
                result.Add(StandardCommandsEnum.AspectRatio11, null);
                result.Add(StandardCommandsEnum.AvAuto, null);
                result.Add(StandardCommandsEnum.AvGame, null);
                result.Add(StandardCommandsEnum.AvGame3D, null);
                result.Add(StandardCommandsEnum.AvDynamic, null);
                result.Add(StandardCommandsEnum.AvDynamicFixed, null);
                result.Add(StandardCommandsEnum.AvMovie, null);
                result.Add(StandardCommandsEnum.AvMovie3D, null);
                result.Add(StandardCommandsEnum.AvPc, null);
                result.Add(StandardCommandsEnum.AvPoll, null);
                result.Add(StandardCommandsEnum.AvUser, null);
                result.Add(StandardCommandsEnum.AvVintageMovie, null);
                result.Add(StandardCommandsEnum.AvStandard, null);
                result.Add(StandardCommandsEnum.AvStandard3D, null);
                result.Add(StandardCommandsEnum.AvXvColor, null);
                result.Add(StandardCommandsEnum.AllLampsOff, new StandardCommand { StandardCommandString = IrCommandConstants.AllLampsOff });
                result.Add(StandardCommandsEnum.AllLampsOn, new StandardCommand { StandardCommandString = IrCommandConstants.AllLampsOn });
                result.Add(StandardCommandsEnum.Antenna, new StandardCommand { StandardCommandString = IrCommandConstants.Antenna });
                result.Add(StandardCommandsEnum.Asterisk, new StandardCommand { StandardCommandString = IrCommandConstants.Asterisk });
                result.Add(StandardCommandsEnum.Mute, new StandardCommand { StandardCommandString = IrCommandConstants.Mute });
                result.Add(StandardCommandsEnum.MuteOff, new StandardCommand { StandardCommandString = IrCommandConstants.MuteOff });
                result.Add(StandardCommandsEnum.MuteOn, new StandardCommand { StandardCommandString = IrCommandConstants.MuteOn });
                result.Add(StandardCommandsEnum.Auto, new StandardCommand { StandardCommandString = IrCommandConstants.Auto });
                result.Add(StandardCommandsEnum.Aux1, new StandardCommand { StandardCommandString = IrCommandConstants.Aux1 });
                result.Add(StandardCommandsEnum.Aux2, new StandardCommand { StandardCommandString = IrCommandConstants.Aux2 });
                result.Add(StandardCommandsEnum.DigitalChannel, null);
                result.Add(StandardCommandsEnum.AnalogChannel, null);
                result.Add(StandardCommandsEnum.Channel, new StandardCommand { StandardCommandString = IrCommandConstants.Channel });
                result.Add(StandardCommandsEnum.ChannelUp, new StandardCommand { StandardCommandString = IrCommandConstants.ChannelUp });
                result.Add(StandardCommandsEnum.ChannelDown, new StandardCommand { StandardCommandString = IrCommandConstants.ChannelDown });
                result.Add(StandardCommandsEnum.ChannelPoll, new StandardCommand { StandardCommandString = IrCommandConstants.ChannelPoll });
                result.Add(StandardCommandsEnum.Tune, new StandardCommand { StandardCommandString = IrCommandConstants.ChannelTune });
                result.Add(StandardCommandsEnum.Eject, new StandardCommand { StandardCommandString = IrCommandConstants.Eject });
                result.Add(StandardCommandsEnum.OnScreenDisplay, new StandardCommand { StandardCommandString = IrCommandConstants.Osd });
                result.Add(StandardCommandsEnum.OnScreenDisplayOff, new StandardCommand { StandardCommandString = IrCommandConstants.OsdOff });
                result.Add(StandardCommandsEnum.OnScreenDisplayOn, new StandardCommand { StandardCommandString = IrCommandConstants.OsdOn });
                result.Add(StandardCommandsEnum.OnScreenDisplayPoll, new StandardCommand { StandardCommandString = IrCommandConstants.OsdPoll });
                result.Add(StandardCommandsEnum.Power, new StandardCommand { StandardCommandString = IrCommandConstants.Power });
                result.Add(StandardCommandsEnum.PowerOff, new StandardCommand { StandardCommandString = IrCommandConstants.PowerOff });
                result.Add(StandardCommandsEnum.PowerOn, new StandardCommand { StandardCommandString = IrCommandConstants.PowerOn });
                result.Add(StandardCommandsEnum.Vol, new StandardCommand { StandardCommandString = IrCommandConstants.Vol });
                result.Add(StandardCommandsEnum.VolMinus, new StandardCommand { StandardCommandString = IrCommandConstants.VolMinus });
                result.Add(StandardCommandsEnum.VolPlus, new StandardCommand { StandardCommandString = IrCommandConstants.VolPlus });
                result.Add(StandardCommandsEnum._0, new StandardCommand { StandardCommandString = IrCommandConstants._0 });
                result.Add(StandardCommandsEnum._1, new StandardCommand { StandardCommandString = IrCommandConstants._1 });
                result.Add(StandardCommandsEnum._2, new StandardCommand { StandardCommandString = IrCommandConstants._2 });
                result.Add(StandardCommandsEnum._3, new StandardCommand { StandardCommandString = IrCommandConstants._3 });
                result.Add(StandardCommandsEnum._4, new StandardCommand { StandardCommandString = IrCommandConstants._4 });
                result.Add(StandardCommandsEnum._5, new StandardCommand { StandardCommandString = IrCommandConstants._5 });
                result.Add(StandardCommandsEnum._6, new StandardCommand { StandardCommandString = IrCommandConstants._6 });
                result.Add(StandardCommandsEnum._7, new StandardCommand { StandardCommandString = IrCommandConstants._7 });
                result.Add(StandardCommandsEnum._8, new StandardCommand { StandardCommandString = IrCommandConstants._8 });
                result.Add(StandardCommandsEnum._9, new StandardCommand { StandardCommandString = IrCommandConstants._9 });
                result.Add(StandardCommandsEnum.Octothorpe, new StandardCommand { StandardCommandString = IrCommandConstants.Octothorpe });
                result.Add(StandardCommandsEnum.Nop, null);
                result.Add(StandardCommandsEnum.Audio, new StandardCommand { StandardCommandString = IrCommandConstants.Audio });
                result.Add(StandardCommandsEnum.DownArrow, new StandardCommand { StandardCommandString = IrCommandConstants.DownArrow });
                result.Add(StandardCommandsEnum.LeftArrow, new StandardCommand { StandardCommandString = IrCommandConstants.LeftArrow });
                result.Add(StandardCommandsEnum.RightArrow, new StandardCommand { StandardCommandString = IrCommandConstants.RightArrow });
                result.Add(StandardCommandsEnum.UpArrow, new StandardCommand { StandardCommandString = IrCommandConstants.UpArrow });
                result.Add(StandardCommandsEnum.Enter, new StandardCommand { StandardCommandString = IrCommandConstants.Enter });
                result.Add(StandardCommandsEnum.Home, new StandardCommand { StandardCommandString = IrCommandConstants.Home });
                result.Add(StandardCommandsEnum.Clear, new StandardCommand { StandardCommandString = IrCommandConstants.Clear });
                result.Add(StandardCommandsEnum.Display, new StandardCommand { StandardCommandString = IrCommandConstants.Display });
                result.Add(StandardCommandsEnum.Exit, new StandardCommand { StandardCommandString = IrCommandConstants.Exit });
                result.Add(StandardCommandsEnum.Blue, new StandardCommand { StandardCommandString = IrCommandConstants.Blue });
                result.Add(StandardCommandsEnum.Green, new StandardCommand { StandardCommandString = IrCommandConstants.Green });
                result.Add(StandardCommandsEnum.Red, new StandardCommand { StandardCommandString = IrCommandConstants.Red });
                result.Add(StandardCommandsEnum.Yellow, new StandardCommand { StandardCommandString = IrCommandConstants.Yellow });
                result.Add(StandardCommandsEnum.Options, new StandardCommand { StandardCommandString = IrCommandConstants.Options });
                result.Add(StandardCommandsEnum.ForwardScan, new StandardCommand { StandardCommandString = IrCommandConstants.ForwardScan });
                result.Add(StandardCommandsEnum.ReverseScan, new StandardCommand { StandardCommandString = IrCommandConstants.ReverseScan });
                result.Add(StandardCommandsEnum.Pause, new StandardCommand { StandardCommandString = IrCommandConstants.Pause });
                result.Add(StandardCommandsEnum.Play, new StandardCommand { StandardCommandString = IrCommandConstants.Play });
                result.Add(StandardCommandsEnum.PlayPause, new StandardCommand { StandardCommandString = IrCommandConstants.PlayPause });
                result.Add(StandardCommandsEnum.Repeat, new StandardCommand { StandardCommandString = IrCommandConstants.Repeat });
                result.Add(StandardCommandsEnum.Return, new StandardCommand { StandardCommandString = IrCommandConstants.Return });
                result.Add(StandardCommandsEnum.Select, new StandardCommand { StandardCommandString = IrCommandConstants.Select });
                result.Add(StandardCommandsEnum.Stop, new StandardCommand { StandardCommandString = IrCommandConstants.Stop });
                result.Add(StandardCommandsEnum.Subtitle, new StandardCommand { StandardCommandString = IrCommandConstants.Subtitle });
                result.Add(StandardCommandsEnum.TopMenu, new StandardCommand { StandardCommandString = IrCommandConstants.TopMenu });
                result.Add(StandardCommandsEnum.ForwardSkip, new StandardCommand { StandardCommandString = IrCommandConstants.ForwardSkip });
                result.Add(StandardCommandsEnum.ReverseSkip, new StandardCommand { StandardCommandString = IrCommandConstants.ReverseSkip });
                result.Add(StandardCommandsEnum.PopUpMenu, new StandardCommand { StandardCommandString = IrCommandConstants.PopUpMenu });
                result.Add(StandardCommandsEnum.Menu, new StandardCommand { StandardCommandString = IrCommandConstants.Menu });
                result.Add(StandardCommandsEnum.Info, new StandardCommand { StandardCommandString = IrCommandConstants.Info });
                result.Add(StandardCommandsEnum.A, new StandardCommand { StandardCommandString = IrCommandConstants.A });
                result.Add(StandardCommandsEnum.B, new StandardCommand { StandardCommandString = IrCommandConstants.B });
                result.Add(StandardCommandsEnum.C, new StandardCommand { StandardCommandString = IrCommandConstants.C });
                result.Add(StandardCommandsEnum.D, new StandardCommand { StandardCommandString = IrCommandConstants.D });
                result.Add(StandardCommandsEnum.Back, new StandardCommand { StandardCommandString = IrCommandConstants.Back });
                result.Add(StandardCommandsEnum.Dvr, new StandardCommand { StandardCommandString = IrCommandConstants.Dvr });
                result.Add(StandardCommandsEnum.Favorite, new StandardCommand { StandardCommandString = IrCommandConstants.Favorite });
                result.Add(StandardCommandsEnum.Guide, new StandardCommand { StandardCommandString = IrCommandConstants.Guide });
                result.Add(StandardCommandsEnum.Last, new StandardCommand { StandardCommandString = IrCommandConstants.Last });
                result.Add(StandardCommandsEnum.Live, new StandardCommand { StandardCommandString = IrCommandConstants.Live });
                result.Add(StandardCommandsEnum.PageDown, new StandardCommand { StandardCommandString = IrCommandConstants.PageDown });
                result.Add(StandardCommandsEnum.PageUp, new StandardCommand { StandardCommandString = IrCommandConstants.PageUp });

                result.Add(StandardCommandsEnum.Record, new StandardCommand { StandardCommandString = IrCommandConstants.Record });
                result.Add(StandardCommandsEnum.Replay, new StandardCommand { StandardCommandString = IrCommandConstants.Replay });
                result.Add(StandardCommandsEnum.SpeedSlow, new StandardCommand { StandardCommandString = IrCommandConstants.SpeedSlow });
                result.Add(StandardCommandsEnum.KeypadBackSpace, new StandardCommand { StandardCommandString = IrCommandConstants.KeypadBackSpace });
                result.Add(StandardCommandsEnum.ThumbsUp,
                    new StandardCommand { StandardCommandString = IrCommandConstants.ThumbsUp });
                result.Add(StandardCommandsEnum.ThumbsDown,
                    new StandardCommand { StandardCommandString = IrCommandConstants.ThumbsDown });
                result.Add(StandardCommandsEnum.Dash,
                    new StandardCommand { StandardCommandString = IrCommandConstants.Dash });
                result.Add(StandardCommandsEnum.Period,
                    new StandardCommand { StandardCommandString = IrCommandConstants.Period });
                result.Add(StandardCommandsEnum.EnergyStar,
                    new StandardCommand { StandardCommandString = IrCommandConstants.EnergyStar });
                result.Add(StandardCommandsEnum.EnergyStarOn,
                    new StandardCommand { StandardCommandString = IrCommandConstants.EnergyStarOn });
                result.Add(StandardCommandsEnum.EnergyStarOff,
                    new StandardCommand { StandardCommandString = IrCommandConstants.EnergyStarOff });
                result.Add(StandardCommandsEnum.VideoMute,
                    new StandardCommand { StandardCommandString = IrCommandConstants.VideoMute });
                result.Add(StandardCommandsEnum.VideoMuteOn,
                    new StandardCommand { StandardCommandString = IrCommandConstants.VideoMuteOn });
                result.Add(StandardCommandsEnum.VideoMuteOff,
                    new StandardCommand { StandardCommandString = IrCommandConstants.VideoMuteOff });
                result.Add(StandardCommandsEnum.DVD, new StandardCommand { StandardCommandString = IrCommandConstants.Dvd1 });
                result.Add(StandardCommandsEnum.SAT, new StandardCommand { StandardCommandString = IrCommandConstants.Sat1 });
                result.Add(StandardCommandsEnum.TV, new StandardCommand { StandardCommandString = IrCommandConstants.Tv });
                result.Add(StandardCommandsEnum.CD, new StandardCommand { StandardCommandString = IrCommandConstants.Cd });
                result.Add(StandardCommandsEnum.Tuner, new StandardCommand { StandardCommandString = IrCommandConstants.Tuner });
                result.Add(StandardCommandsEnum.Phono, new StandardCommand { StandardCommandString = IrCommandConstants.Phono });
                result.Add(StandardCommandsEnum.DSS, new StandardCommand { StandardCommandString = IrCommandConstants.Dss });
                result.Add(StandardCommandsEnum.InternetRadio, new StandardCommand { StandardCommandString = IrCommandConstants.InternetRadio });
                result.Add(StandardCommandsEnum.Sirius, new StandardCommand { StandardCommandString = IrCommandConstants.Sirius });
                result.Add(StandardCommandsEnum.SiriusXm, new StandardCommand { StandardCommandString = IrCommandConstants.SiriusXm });
                result.Add(StandardCommandsEnum.Pandora, new StandardCommand { StandardCommandString = IrCommandConstants.Pandora });
                result.Add(StandardCommandsEnum.LastFm, new StandardCommand { StandardCommandString = IrCommandConstants.LastFm });
                result.Add(StandardCommandsEnum.Rhapsody, new StandardCommand { StandardCommandString = IrCommandConstants.Rhapsody });
                result.Add(StandardCommandsEnum.HdRadio, new StandardCommand { StandardCommandString = IrCommandConstants.HdRadio });
                result.Add(StandardCommandsEnum.Spotify, new StandardCommand { StandardCommandString = IrCommandConstants.Spotify });
                result.Add(StandardCommandsEnum.YouTube, new StandardCommand { StandardCommandString = IrCommandConstants.YouTube });
                result.Add(StandardCommandsEnum.YouTubeTv, new StandardCommand { StandardCommandString = IrCommandConstants.YouTubeTv });
                result.Add(StandardCommandsEnum.Netflix, new StandardCommand { StandardCommandString = IrCommandConstants.Netflix });
                result.Add(StandardCommandsEnum.Hulu, new StandardCommand { StandardCommandString = IrCommandConstants.Hulu });
                result.Add(StandardCommandsEnum.DirecTvNow, new StandardCommand { StandardCommandString = IrCommandConstants.DirecTvNow });
                result.Add(StandardCommandsEnum.AmazonVideo, new StandardCommand { StandardCommandString = IrCommandConstants.AmazonVideo });
                result.Add(StandardCommandsEnum.PlayStationVue, new StandardCommand { StandardCommandString = IrCommandConstants.PlaystationVue });
                result.Add(StandardCommandsEnum.SlingTv, new StandardCommand { StandardCommandString = IrCommandConstants.SlingTv });

                result.Add(StandardCommandsEnum.TunerFrequencyUp, new StandardCommand { StandardCommandString = IrCommandConstants.TuneUp });
                result.Add(StandardCommandsEnum.TunerFrequencyDown, new StandardCommand { StandardCommandString = IrCommandConstants.TuneDown });
                result.Add(StandardCommandsEnum.AirPlay, new StandardCommand { StandardCommandString = IrCommandConstants.Airplay });
                result.Add(StandardCommandsEnum.GoogleCast, new StandardCommand { StandardCommandString = IrCommandConstants.GoogleCast });
                result.Add(StandardCommandsEnum.Dlna, new StandardCommand { StandardCommandString = IrCommandConstants.DLNA });
                result.Add(StandardCommandsEnum.Tidal, new StandardCommand { StandardCommandString = IrCommandConstants.Tidal });
                result.Add(StandardCommandsEnum.Deezer, new StandardCommand { StandardCommandString = IrCommandConstants.Deezer });
                result.Add(StandardCommandsEnum.Crackle, new StandardCommand { StandardCommandString = IrCommandConstants.Crackle });
                result.Add(StandardCommandsEnum.OnDemand, new StandardCommand { StandardCommandString = IrCommandConstants.OnDemand });
                result.Add(StandardCommandsEnum.Tivo, new StandardCommand { StandardCommandString = IrCommandConstants.Tivo });
                result.Add(StandardCommandsEnum.FSkip, new StandardCommand { StandardCommandString = IrCommandConstants.FSkip });
                result.Add(StandardCommandsEnum.RSkip, new StandardCommand { StandardCommandString = IrCommandConstants.RSkip });

                //TODO JASON L
                //result.Add(StandardCommandsEnum.Settings, new StandardCommand { StandardCommandString = IrCommandConstants.Settings });

                result.Add(StandardCommandsEnum.Optical1, new StandardCommand { StandardCommandString = IrCommandConstants.Optical1 });
                result.Add(StandardCommandsEnum.Optical2, new StandardCommand { StandardCommandString = IrCommandConstants.Optical2 });
                result.Add(StandardCommandsEnum.Optical3, new StandardCommand { StandardCommandString = IrCommandConstants.Optical3 });
                result.Add(StandardCommandsEnum.Optical4, new StandardCommand { StandardCommandString = IrCommandConstants.Optical4 });
                result.Add(StandardCommandsEnum.Optical5, new StandardCommand { StandardCommandString = IrCommandConstants.Optical5 });
                result.Add(StandardCommandsEnum.Optical6, new StandardCommand { StandardCommandString = IrCommandConstants.Optical6 });
                result.Add(StandardCommandsEnum.Optical7, new StandardCommand { StandardCommandString = IrCommandConstants.Optical7 });
                result.Add(StandardCommandsEnum.Optical8, new StandardCommand { StandardCommandString = IrCommandConstants.Optical8 });
                result.Add(StandardCommandsEnum.Optical9, new StandardCommand { StandardCommandString = IrCommandConstants.Optical9 });
                result.Add(StandardCommandsEnum.Optical10, new StandardCommand { StandardCommandString = IrCommandConstants.Optical10 });

                result.Add(StandardCommandsEnum.Coax1, new StandardCommand { StandardCommandString = IrCommandConstants.Coax1 });
                result.Add(StandardCommandsEnum.Coax2, new StandardCommand { StandardCommandString = IrCommandConstants.Coax2 });
                result.Add(StandardCommandsEnum.Coax3, new StandardCommand { StandardCommandString = IrCommandConstants.Coax3 });
                result.Add(StandardCommandsEnum.Coax4, new StandardCommand { StandardCommandString = IrCommandConstants.Coax4 });
                result.Add(StandardCommandsEnum.Coax5, new StandardCommand { StandardCommandString = IrCommandConstants.Coax5 });
                result.Add(StandardCommandsEnum.Coax6, new StandardCommand { StandardCommandString = IrCommandConstants.Coax6 });
                result.Add(StandardCommandsEnum.Coax7, new StandardCommand { StandardCommandString = IrCommandConstants.Coax7 });
                result.Add(StandardCommandsEnum.Coax8, new StandardCommand { StandardCommandString = IrCommandConstants.Coax8 });
                result.Add(StandardCommandsEnum.Coax9, new StandardCommand { StandardCommandString = IrCommandConstants.Coax9 });
                result.Add(StandardCommandsEnum.Coax10, new StandardCommand { StandardCommandString = IrCommandConstants.Coax10 });

                result.Add(StandardCommandsEnum.AnalogAudio1, new StandardCommand { StandardCommandString = IrCommandConstants.AnalogAudio1 });
                result.Add(StandardCommandsEnum.AnalogAudio2, new StandardCommand { StandardCommandString = IrCommandConstants.AnalogAudio2 });
                result.Add(StandardCommandsEnum.AnalogAudio3, new StandardCommand { StandardCommandString = IrCommandConstants.AnalogAudio3 });
                result.Add(StandardCommandsEnum.AnalogAudio4, new StandardCommand { StandardCommandString = IrCommandConstants.AnalogAudio4 });
                result.Add(StandardCommandsEnum.AnalogAudio5, new StandardCommand { StandardCommandString = IrCommandConstants.AnalogAudio5 });
                result.Add(StandardCommandsEnum.AnalogAudio6, new StandardCommand { StandardCommandString = IrCommandConstants.AnalogAudio6 });
                result.Add(StandardCommandsEnum.AnalogAudio7, new StandardCommand { StandardCommandString = IrCommandConstants.AnalogAudio7 });
                result.Add(StandardCommandsEnum.AnalogAudio8, new StandardCommand { StandardCommandString = IrCommandConstants.AnalogAudio8 });
                result.Add(StandardCommandsEnum.AnalogAudio9, new StandardCommand { StandardCommandString = IrCommandConstants.AnalogAudio9 });
                result.Add(StandardCommandsEnum.AnalogAudio10, new StandardCommand { StandardCommandString = IrCommandConstants.AnalogAudio10 });

                result.Add(StandardCommandsEnum.Bd1, new StandardCommand { StandardCommandString = IrCommandConstants.Bd1 });
                result.Add(StandardCommandsEnum.Catv1, new StandardCommand { StandardCommandString = IrCommandConstants.Catv1 });
                result.Add(StandardCommandsEnum.Game1, new StandardCommand { StandardCommandString = IrCommandConstants.Game1 });
                result.Add(StandardCommandsEnum.Pc1, new StandardCommand { StandardCommandString = IrCommandConstants.Pc1 });
                result.Add(StandardCommandsEnum.Bluetooth1, new StandardCommand { StandardCommandString = IrCommandConstants.Bluetooth1 });
                result.Add(StandardCommandsEnum.MediaPlayer1, new StandardCommand { StandardCommandString = IrCommandConstants.MediaPlayer1 });
                result.Add(StandardCommandsEnum.Ipod1, new StandardCommand { StandardCommandString = IrCommandConstants.Ipod1 });

                result.Add(StandardCommandsEnum.TunerPresetUp, new StandardCommand { StandardCommandString = IrCommandConstants.TunerPresetUp });
                result.Add(StandardCommandsEnum.TunerPresetDown, new StandardCommand { StandardCommandString = IrCommandConstants.TunerPresetDown });
                result.Add(StandardCommandsEnum.TunerFrequencyBandAm, new StandardCommand{ StandardCommandString = IrCommandConstants.BandAm});
                result.Add(StandardCommandsEnum.TunerFrequencyBandFm, new StandardCommand{ StandardCommandString = IrCommandConstants.BandFm});

                result.Add(StandardCommandsEnum.ToneBassLevelUp, new StandardCommand{ StandardCommandString = IrCommandConstants.BassLevelUp});
                result.Add(StandardCommandsEnum.ToneBassLevelDown, new StandardCommand{ StandardCommandString = IrCommandConstants.BassLevelDown});
                result.Add(StandardCommandsEnum.ToneTrebleLevelUp, new StandardCommand{ StandardCommandString = IrCommandConstants.TrebleLevelUp});
                result.Add(StandardCommandsEnum.ToneTrebleLevelDown, new StandardCommand{ StandardCommandString = IrCommandConstants.TrebleLevelDown});
                result.Add(StandardCommandsEnum.LoudnessOn, new StandardCommand{ StandardCommandString = IrCommandConstants.LoudnessOn});
                result.Add(StandardCommandsEnum.LoudnessOff, new StandardCommand{ StandardCommandString = IrCommandConstants.LoudnessOff});
                result.Add(StandardCommandsEnum.LoudnessToggle, new StandardCommand{ StandardCommandString = IrCommandConstants.LoudnessToggle});
                result.Add(StandardCommandsEnum.ToneControlOn, new StandardCommand{ StandardCommandString = IrCommandConstants.ToneControlOn});
                result.Add(StandardCommandsEnum.ToneControlOff, new StandardCommand{ StandardCommandString = IrCommandConstants.ToneControlOff});
                result.Add(StandardCommandsEnum.ToneControlToggle, new StandardCommand{ StandardCommandString = IrCommandConstants.ToneControlToggle});
                result.Add(StandardCommandsEnum.SurroundModeCycle, new StandardCommand{ StandardCommandString = IrCommandConstants.SurroundModeCycle});
                result.Add(StandardCommandsEnum.TunerAutoFrequencyUp, new StandardCommand{ StandardCommandString = IrCommandConstants.AutoFrequencyUp});
                result.Add(StandardCommandsEnum.TunerAutoFrequencyDown, new StandardCommand{ StandardCommandString = IrCommandConstants.AutoFrequencyDown});

                _standardEnumToCommandMapping = result;

                return _standardEnumToCommandMapping;
            }
        }

        public static List<StandardCommandsEnum> GetValues(StandardCommandsEnum enumeration)
        {
            var enumerations = new List<StandardCommandsEnum>();
            CType type = enumeration.GetType();
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo fieldInfo in fields)
            {
                enumerations.Add((StandardCommandsEnum)fieldInfo.GetValue(enumeration));
            }
            return enumerations;
        }

        private static void PopulateDictionary()
        {
            List<StandardCommandsEnum> enumArray = GetEnumList();

            List<FieldInfo> consts = GetStandardCommandsConstantsList();

            MapEnumsToStandardCommandsConstants(enumArray, consts);
        }

        private static void MapEnumsToStandardCommandsConstants(List<StandardCommandsEnum> enumArray, List<FieldInfo> consts)
        {
            for (int onEnum = 0; onEnum < enumArray.Count; onEnum++)
            {
                StandardCommandsEnum commandEnum = enumArray[onEnum];
                var match = consts.Find(c => c.Name.Equals(commandEnum.ToString()));
                if (match != null)
                {
                    if (!StandardEnumToCommandMapping.ContainsKey(commandEnum))
                    {
                        StandardEnumToCommandMapping.Add(commandEnum, new StandardCommand
                        {
                            StandardCommandString = match.GetValue(match.Name).ToString()
                        });
                    }
                    consts.Remove(match);
                }
            }
        }

        private static List<FieldInfo> GetStandardCommandsConstantsList()
        {
            CType type = new IrCommandConstants().GetType();
            List<FieldInfo> consts = new List<FieldInfo>(type.GetFields(BindingFlags.Public | BindingFlags.Static));
            return consts;
        }

        internal static List<string> GetStandardCommandsConstantNamesList()
        {
            CType type = new IrCommandConstants().GetType();
            List<FieldInfo> consts = new List<FieldInfo>(type.GetFields(BindingFlags.Public | BindingFlags.Static));
            List<string> constantNames = consts.Select(x => x.GetValue(type) as string).ToList();
            return constantNames;
        }

        private static List<StandardCommandsEnum> GetEnumList()
        {
            List<StandardCommandsEnum> enumArray = GetValues(new StandardCommandsEnum());
            return enumArray;
        }

        public static void PrintDictionary()
        {
            try
            {
                Crestron.SimplSharp.Stopwatch sw = new Crestron.SimplSharp.Stopwatch();
                sw.Start();
                PopulateDictionary();
                sw.Stop();
                Crestron.SimplSharp.CrestronConsole.PrintLine(StandardEnumToCommandMapping.Count.ToString());
                foreach (var entry in StandardEnumToCommandMapping)
                {
                    Crestron.SimplSharp.CrestronConsole.PrintLine(entry.Key.ToString() + "," + entry.Value.StandardCommandString);
                }
                Crestron.SimplSharp.CrestronConsole.PrintLine("Dictionary took " + sw.Elapsed.Milliseconds + " milliseconds to populate");
            }
            catch (Exception e)
            {
                Crestron.SimplSharp.CrestronConsole.PrintLine(e.Message);
                Crestron.SimplSharp.CrestronConsole.PrintLine(e.StackTrace);
            }
        }
    }

    internal static class SupportedCommandsMapping
    {
        private static Dictionary<int, object> _supportedFeatureToStandardCommandMapping;

        internal static Dictionary<int, object> SupportedFeatureToStandardCommandMapping
        {
            get
            {
                if (_supportedFeatureToStandardCommandMapping == null)
                {
                    PopulateValues();
                }
                return _supportedFeatureToStandardCommandMapping;
            }
        }

        private static void PopulateValues()
        {
            _supportedFeatureToStandardCommandMapping = new Dictionary<int, object>();

            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsAsterisk, StandardCommandsEnum.Asterisk);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsBack, StandardCommandsEnum.Back);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsChangeChannel, new List<StandardCommandsEnum> { StandardCommandsEnum.ChannelUp, StandardCommandsEnum.ChannelDown });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsChangeVolume, new List<StandardCommandsEnum> { StandardCommandsEnum.VolPlus, StandardCommandsEnum.VolMinus });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsClear, StandardCommandsEnum.Clear);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsDash, StandardCommandsEnum.Dash);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsDiscreteMute, new List<StandardCommandsEnum> { StandardCommandsEnum.MuteOn, StandardCommandsEnum.MuteOff });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsDiscretePower, new List<StandardCommandsEnum> { StandardCommandsEnum.PowerOn, StandardCommandsEnum.PowerOff });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsDvrCommand, StandardCommandsEnum.Dvr);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsEnter, StandardCommandsEnum.Enter);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsExit, StandardCommandsEnum.Exit);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsFavorite, StandardCommandsEnum.Favorite);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsForwardScan, StandardCommandsEnum.ForwardScan);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsForwardSkip, StandardCommandsEnum.ForwardSkip);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsGuide, StandardCommandsEnum.Guide);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsHome, StandardCommandsEnum.Home);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsInfo, StandardCommandsEnum.Info);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsKeypadBackSpace, StandardCommandsEnum.KeypadBackSpace);

            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsKeypadNumber,
                new List<StandardCommandsEnum> { 
                    StandardCommandsEnum._0, 
                    StandardCommandsEnum._1, 
                    StandardCommandsEnum._2, 
                    StandardCommandsEnum._3, 
                    StandardCommandsEnum._4, 
                    StandardCommandsEnum._5, 
                    StandardCommandsEnum._6, 
                    StandardCommandsEnum._7, 
                    StandardCommandsEnum._8, 
                    StandardCommandsEnum._9 });


            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsLast, StandardCommandsEnum.Last);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsLive, StandardCommandsEnum.Live);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsMenu, StandardCommandsEnum.Menu);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsMute, StandardCommandsEnum.Mute);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsPageChange, new List<StandardCommandsEnum> { StandardCommandsEnum.PageUp, StandardCommandsEnum.PageDown });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsPause, StandardCommandsEnum.Pause);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsPeriod, StandardCommandsEnum.Period);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsPlay, StandardCommandsEnum.Play);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsPound, StandardCommandsEnum.Octothorpe);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsRecord, StandardCommandsEnum.Record);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsRepeat, StandardCommandsEnum.Repeat);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsReplay, StandardCommandsEnum.Replay);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsReturn, StandardCommandsEnum.Return);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsReverseScan, StandardCommandsEnum.ReverseScan);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsReverseSkip, StandardCommandsEnum.ReverseSkip);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsSetChannel, StandardCommandsEnum.Channel);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsSetVolume, StandardCommandsEnum.Vol);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsSpeedSlow, StandardCommandsEnum.SpeedSlow);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsStop, StandardCommandsEnum.Stop);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsThumbsDown, StandardCommandsEnum.ThumbsDown);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsThumbsUp, StandardCommandsEnum.ThumbsUp);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsTogglePower, StandardCommandsEnum.Power);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsAudio, StandardCommandsEnum.Audio);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsDisplay, StandardCommandsEnum.Display);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsEject, StandardCommandsEnum.Eject);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsOptions, StandardCommandsEnum.Options);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsSubtitle, StandardCommandsEnum.Subtitle);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsToggleVideoMute, StandardCommandsEnum.VideoMute);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsToggleEnergyStar, StandardCommandsEnum.EnergyStar);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsDiscreteEnergyStar, new List<StandardCommandsEnum> { StandardCommandsEnum.EnergyStarOff, StandardCommandsEnum.EnergyStarOn });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsDiscreteVideoMute, new List<StandardCommandsEnum> { StandardCommandsEnum.VideoMuteOn, StandardCommandsEnum.VideoMuteOff });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsPopUpMenu, StandardCommandsEnum.PopUpMenu);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsTopMenu, StandardCommandsEnum.TopMenu);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsSelect, StandardCommandsEnum.Select);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsDiscreteToneControl, new List<StandardCommandsEnum> { StandardCommandsEnum.ToneControlOff, StandardCommandsEnum.ToneControlOn });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsToggleToneControl, StandardCommandsEnum.ToneControlToggle);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsSetBass, StandardCommandsEnum.ToneSetBass);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsChangeBass, new List<StandardCommandsEnum> { StandardCommandsEnum.ToneBassLevelDown, StandardCommandsEnum.ToneBassLevelUp });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsSetTreble, StandardCommandsEnum.ToneSetTreble);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsChangeTreble, new List<StandardCommandsEnum> { StandardCommandsEnum.ToneTrebleLevelDown, StandardCommandsEnum.ToneTrebleLevelUp });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsDiscreteLoudness, new List<StandardCommandsEnum> { StandardCommandsEnum.LoudnessOff, StandardCommandsEnum.LoudnessOn });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsToggleLoudness, StandardCommandsEnum.LoudnessToggle);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsSurroundModeCycle, StandardCommandsEnum.SurroundModeCycle);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsChangeFrequency, new List<StandardCommandsEnum> { StandardCommandsEnum.TunerFrequencyDown, StandardCommandsEnum.TunerFrequencyUp });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsSetFrequency, StandardCommandsEnum.TunerFrequency);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsAutoFrequency, new List<StandardCommandsEnum> { StandardCommandsEnum.TunerAutoFrequencyDown, StandardCommandsEnum.TunerAutoFrequencyUp });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsFrequencyBand, StandardCommandsEnum.TunerFrequencyBand);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsDiscreteFrequencyBand, new List<StandardCommandsEnum> { StandardCommandsEnum.TunerFrequencyBandAm, StandardCommandsEnum.TunerFrequencyBandFm });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsPresetRecall, StandardCommandsEnum.TunerPresetRecall);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsPresetStore, StandardCommandsEnum.TunerPresetStore);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsSearch, StandardCommandsEnum.Search);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsSirius, StandardCommandsEnum.Sirius);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsXm, StandardCommandsEnum.Xm);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsSiriusXm, StandardCommandsEnum.SiriusXm);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsHdRadio, StandardCommandsEnum.HdRadio);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsInternetRadio, StandardCommandsEnum.InternetRadio);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsLastFm, StandardCommandsEnum.LastFm);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsPandora, StandardCommandsEnum.Pandora);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsRhapsody, StandardCommandsEnum.Rhapsody);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsChangePreset, new List<StandardCommandsEnum> { StandardCommandsEnum.TunerPresetDown, StandardCommandsEnum.TunerPresetUp });
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsPlayPause, StandardCommandsEnum.PlayPause);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsSpotify, StandardCommandsEnum.Spotify);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsYouTube, StandardCommandsEnum.YouTube);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsYouTubeTv, StandardCommandsEnum.YouTubeTv);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsNetflix, StandardCommandsEnum.Netflix);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsHulu, StandardCommandsEnum.Hulu);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsDirectvNow, StandardCommandsEnum.DirecTvNow);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsAmazonVideo, StandardCommandsEnum.AmazonVideo);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsPlaystationVue, StandardCommandsEnum.PlayStationVue);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsSlingTv, StandardCommandsEnum.SlingTv);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsAirplay, StandardCommandsEnum.AirPlay);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsGoogleCast, StandardCommandsEnum.GoogleCast);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsDlna, StandardCommandsEnum.Dlna);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsTidal, StandardCommandsEnum.Tidal);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsDeezer, StandardCommandsEnum.Deezer);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsCrackle, StandardCommandsEnum.Crackle);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsOnDemand, StandardCommandsEnum.OnDemand);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsGooglePlay, StandardCommandsEnum.GooglePlay);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsBluetooth, StandardCommandsEnum.Bluetooth);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsTivo, StandardCommandsEnum.Tivo);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsRSkip, StandardCommandsEnum.ReverseSkip);
            _supportedFeatureToStandardCommandMapping.Add((int)CommonFeatureSupport.SupportsFSkip, StandardCommandsEnum.ForwardSkip);
        }
    }
}
