// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Collections.Generic;
using Crestron.RAD.Common.Enums;

namespace Crestron.RAD.Common.StandardCommands
{
    public class CodecStandardCommandsConstants
    {
        public const string ArrowDown = "DN_ARROW";
        public const string ArrowLeft = "LEFT_ARROW";
        public const string ArrowRight = "RIGHT_ARROW";
        public const string ArrowUp = "UP_ARROW";
        public const string Aspect1 = "AspectRatio1";
        public const string Aspect2 = "AspectRatio2";
        public const string Aspect3 = "AspectRatio3";
        public const string Aspect4 = "AspectRatio4";
        public const string Aspect5 = "AspectRatio5";
        public const string Aspect6 = "AspectRatio6";
        public const string Aspect7 = "AspectRatio7";
        public const string Aspect8 = "AspectRatio8";
        public const string AspectPoll = "AspectRatioPoll";
        public const string Asterisk = "*";
        public const string AudioMute = "MUTE";
        public const string AudioMuteOff = "MUTE_OFF";
        public const string AudioMuteOn = "MUTE_ON";
        public const string AudioMutePoll = "AudioMutePoll";
        public const string Auto = "AUTO";
        public const string Aux1 = "AUX1";
        public const string Aux2 = "AUX2";
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
        public const string InputPoll = "InputPoll";
        public const string Mute = "MUTE";
        public const string MuteOff = "MUTE_OFF";
        public const string MuteOn = "MUTE_ON";
        public const string MutePoll = "MutePoll";
        public const string Osd = "OSD";
        public const string OsdOff = "OSD_OFF";
        public const string OsdOn = "OSD_ON";
        public const string OsdPoll = "OsdPoll";
        public const string VideoMuteOn = "VIDEOMUTE_ON";
        public const string VideoMuteOff = "VIDEOMUTE_OFF";
        public const string VideoMute = "VIDEOMUTE_TOGGLE";
        public const string VideoMutePoll = "VideoMutePoll";
        public const string VolMinus = "VOL-";
        public const string VolPlus = "VOL+";
        public const string Vol = "VOL";
        public const string VolumePoll = "VolumePoll";
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
        //added for Codecs
        public const string MicMuteOn = "MicMuteOn";
        public const string MicMuteOff = "MicMuteOff";
        public const string MicMute = "MicMute";
        public const string MicMutePoll = "MicMutePoll";
        public const string DialMeeting = "DialMeeting";
        public const string DialAddressBook = "DialAddressBook";
        public const string DialManual = "DialManual";
        public const string SelfViewOn = "SelfViewOn";
        public const string SelfViewOff = "SelfViewOff";
        public const string SelfViewAuto = "SelfViewAuto";
        public const string SelfView = "SelfView";
        public const string SelfViewPoll = "SelfViewPoll";
        public const string SelfViewPipLocation = "SelfViewPipLocation";
        public const string SelfViewMonitor = "SelfViewMonitor";
        public const string PictureMode = "PictureMode";
        public const string PresentationStart = "PresentationStart";
        public const string PresentationStop = "PresentationStop";
        public const string PipLocation = "PipLocation";
        public const string PipLocationPoll = "PipLocationPoll";
        public const string MainVideoSource = "MainVideoSource";
        public const string MainVideoSourcePoll = "MainVideoSourcePoll";
        public const string CameraFarEndPanLeft = "CameraFarEndPanLeft";
        public const string CameraFarEndPanRight = "CameraFarEndPanRight";
        public const string CameraFarEndTiltUp = "CameraFarEndTiltUp";
        public const string CameraFarEndTiltDown = "CameraFarEndTiltDown";
        public const string CameraFarEndZoomIn = "CameraFarEndZoomIn";
        public const string CameraFarEndZoomOut = "CameraFarEndZoomOut";
        public const string CameraFarEndFocusNear = "CameraFarEndFocusNear";
        public const string CameraFarEndFocusFar = "CameraFarEndFocusFar";
        public const string CameraFarEndStop = "CameraFarEndStop";
        public const string CameraFarEndStopPan = "CameraFarEndStopPan";
        public const string CameraFarEndStopTilt = "CameraFarEndStopTilt";
        public const string CameraFarEndStopZoom = "CameraFarEndStopZoom";
        public const string CameraFarEndStopFocus = "CameraFarEndStopFocus";
        public const string CameraFarEndAutoFocus = "CameraFarEndAutoFocus";
        public const string HangUp = "HangUp";
        public const string Dtmf = "Dtmf";
        public const string Hold = "Hold";
        public const string Resume = "Resume";
        public const string Transfer = "Transfer";
        public const string Join = "Join";
        public const string Answer = "Answer";
        public const string Reject = "Reject";
        public const string AllowFecc = "AllowFecc";
        public const string DoNotAllowFecc = "DoNotAllowFecc";
        public const string MuteMicsAutoAnswerOn = "MuteMicsAutoAnswerOn";
        public const string MuteMicsAutoAnswerOff = "MuteMicsAutoAnswerOff";
        public const string DoNotDisturbOn = "DoNotDisturbOn";
        public const string DoNotDisturbOff = "DoNotDisturbOff";
        public const string AutoAnswerOn = "AutoAnswerOn";
        public const string AutoAnswerOff = "AutoAnswerOff";
        public const string StandbyOn = "StandbyOn";
        public const string StandbyOff = "StandbyOff";
        public const string EncryptionOn = "EncryptionOn";
        public const string EncryptionOff = "EncryptionOff";
        public const string CameraTrackingOn = "CameraTrackingOn";
        public const string CameraTrackingOff = "CameraTrackingOff";
        public const string CamRecallTrackingPreset = "CamRecallTrackingPreset";
        public const string CamStoreTrackingPreset = "CamStoreTrackingPreset";
        public const string MessageResponse = "MessageResponse";
        public const string MessageClear = "MessageClear";
        public const string AlertClear = "AlertClear";
        //added for Cameras
        public const string CameraNearEndPanLeft = "CameraNearEndPanLeft";
        public const string CameraNearEndPanRight = "CameraNearEndPanRight";
        public const string CameraNearEndTiltUp = "CameraNearEndTiltUp";
        public const string CameraNearEndTiltDown = "CameraNearEndTiltDown";
        public const string CameraNearEndZoomIn = "CameraNearEndZoomIn";
        public const string CameraNearEndZoomOut = "CameraNearEndZoomOut";
        public const string CameraNearEndFocusNear = "CameraNearEndFocusNear";
        public const string CameraNearEndFocusFar = "CameraNearEndFocusFar";
        public const string CameraNearEndStop = "CameraNearEndStop";
        public const string CameraNearEndStopPan = "CameraNearEndStopPan";
        public const string CameraNearEndStopTilt = "CameraNearEndStopTilt";
        public const string CameraNearEndStopZoom = "CameraNearEndStopZoom";
        public const string CameraNearEndStopFocus = "CameraNearEndStopFocus";
        public const string CameraNearEndAutoFocus = "CameraNearEndAutoFocus";
        public const string CameraNearEndResetPosition = "CameraNearEndResetPosition";
        public const string CameraNearEndRecallPreset = "CameraNearEndRecallPreset";
        public const string CameraNearEndStorePreset = "CameraNearEndStorePreset";
        public const string Reboot = "Reboot";
        public const string CameraFarEndRecallPreset = "CameraFarEndRecallPreset";
        public const string CameraFarEndStorePreset = "CameraFarEndStorePreset";
        public const string Input4Dvi = "Input4Dvi";
        public const string Input4SVideoComposite = "Input4SVideoComposite";
        public const string FarEndPresentationSource = "FarEndPresentationSource";
        public const string SelfViewFullScreenOn = "SelfViewFullScreenOn";
        public const string SelfViewFullScreenOff = "SelfViewFullScreenOff";
        public const string MultipointAutoAnswerOn = "MultipointAutoAnswerOn";
        public const string MultipointAutoAnswerOff = "MultipointAutoAnswerOff";
        public const string MultipointAutoAnswerDoNotDisturb = "MultipointAutoAnswerDoNotDisturb";
        public const string MultipointModeAuto = "MultipointModeAuto";
        public const string MultipointModePresentation = "MultipointModePresentation";
        public const string MultipointModeDiscussion = "MultipointModeDiscussion";
        public const string MultipointModeFullScreen = "MultipointModeFullScreen";
        public const string Monitor1PresentationFar = "Monitor1PresentationFar";
        public const string Monitor1PresentationNearOrFar = "Monitor1PresentationNearOrFar";
        public const string Monitor1PresentationContentOrFar = "Monitor1PresentationContentOrFar";
        public const string Monitor1PresentationAll = "Monitor1PresentationAll";
        public const string Monitor2PresentationNear = "Monitor2PresentationNear";
        public const string Monitor2PresentationFar = "Monitor2PresentationFar";
        public const string Monitor2PresentationContent = "Monitor2PresentationContent";
        public const string Monitor2PresentationNearOrFar = "Monitor2PresentationNearOrFar";
        public const string Monitor2PresentationContentOrNear = "Monitor2PresentationContentOrNear";
        public const string Monitor2PresentationContentOrFar = "Monitor2PresentationContentOrFar";
        public const string Monitor2PresentationAll = "Monitor2PresentationAll";
        public const string Monitor3PresentationNear = "Monitor3PresentationNear";
        public const string Monitor3PresentationFar = "Monitor3PresentationFar";
        public const string Monitor3PresentationContent = "Monitor3PresentationContent";
        public const string Monitor3PresentationRecordNearOrFar = "Monitor3PresentationRecordNearOrFar";
        public const string Monitor3PresentationRecordAll = "Monitor3PresentationRecordAll";
        public const string IrRemoteEmulationKeyPress = "IrRemoteEmulationKeyPress";
        public const string IrRemoteEmulationKeyRelease = "IrRemoteEmulationKeyRelease";
        public const string IrRemoteEmulationKeyClick = "IrRemoteEmulationKeyClick";
    }

    public static class CodecStandardCommands
    {
        public static List<StandardCommandsEnum> CommandCollection
        {
            get
            {
                return new List<StandardCommandsEnum>
                {
                    StandardCommandsEnum.Hdmi1,
                    StandardCommandsEnum.Hdmi10,
                    StandardCommandsEnum.Hdmi2,
                    StandardCommandsEnum.Hdmi3,
                    StandardCommandsEnum.Hdmi4,
                    StandardCommandsEnum.Hdmi5,
                    StandardCommandsEnum.Hdmi6,
                    StandardCommandsEnum.Hdmi7,
                    StandardCommandsEnum.Hdmi8,
                    StandardCommandsEnum.Hdmi9,
                    StandardCommandsEnum.Dvi1,
                    StandardCommandsEnum.Dvi10,
                    StandardCommandsEnum.Dvi2,
                    StandardCommandsEnum.Dvi3,
                    StandardCommandsEnum.Dvi4,
                    StandardCommandsEnum.Dvi5,
                    StandardCommandsEnum.Dvi6,
                    StandardCommandsEnum.Dvi7,
                    StandardCommandsEnum.Dvi8,
                    StandardCommandsEnum.Dvi9,
                    StandardCommandsEnum.Input1,
                    StandardCommandsEnum.Input10,
                    StandardCommandsEnum.Input2,
                    StandardCommandsEnum.Input3,
                    StandardCommandsEnum.Input4,
                    StandardCommandsEnum.Input5,
                    StandardCommandsEnum.Input6,
                    StandardCommandsEnum.Input7,
                    StandardCommandsEnum.Input8,
                    StandardCommandsEnum.Input9,
                    StandardCommandsEnum.Input11,
                    StandardCommandsEnum.Input12,
                    StandardCommandsEnum.Input13,
                    StandardCommandsEnum.Input14,
                    StandardCommandsEnum.Input15,
                    //aspect ratios
                    StandardCommandsEnum.AspectSideBar,
                    StandardCommandsEnum.AspectStrech,
                    StandardCommandsEnum.AspectZoom,
                    StandardCommandsEnum.AspectNormal,
                    StandardCommandsEnum.AspectDotByDot,
                    StandardCommandsEnum.AspectFullScreen,
                    StandardCommandsEnum.AspectAuto,
                    StandardCommandsEnum.AspectOriginal,
                    StandardCommandsEnum.Aspect16By9,
                    StandardCommandsEnum.AspectWideZoom,
                    StandardCommandsEnum.Aspect4By3,
                    StandardCommandsEnum.AspectSubTitle,
                    StandardCommandsEnum.AspectJust,
                    StandardCommandsEnum.AspectZoom2,
                    StandardCommandsEnum.AspectZoom3,
                    StandardCommandsEnum.AspectRatio1,
                    StandardCommandsEnum.AspectRatio2,
                    StandardCommandsEnum.AspectRatio3,
                    StandardCommandsEnum.AspectRatio4,
                    StandardCommandsEnum.AspectRatio5,
                    StandardCommandsEnum.AspectRatio6,
                    StandardCommandsEnum.AspectRatio7,
                    StandardCommandsEnum.AspectRatio8,
                    StandardCommandsEnum.AspectRatio9,
                    StandardCommandsEnum.AspectRatio10,
                    StandardCommandsEnum.AspectRatio11,
                    StandardCommandsEnum.AspectRatioPoll,
                    //others
                    StandardCommandsEnum.Mute,
                    StandardCommandsEnum.MuteOff,
                    StandardCommandsEnum.MuteOn,
                    StandardCommandsEnum.VideoMuteOff,
                    StandardCommandsEnum.VideoMuteOn,
                    StandardCommandsEnum.VideoMutePoll,
                    StandardCommandsEnum.VideoMute,
                    StandardCommandsEnum.Vol,
                    StandardCommandsEnum.VolumePoll,
                    //Codecs
                    StandardCommandsEnum.MicMuteOn,
                    StandardCommandsEnum.MicMuteOff,
                    StandardCommandsEnum.MicMute,
                    StandardCommandsEnum.MicMutePoll,
                    StandardCommandsEnum.DialMeeting,
                    StandardCommandsEnum.DialAddressBook,
                    StandardCommandsEnum.DialManual,
                    StandardCommandsEnum.SelfViewOn,
                    StandardCommandsEnum.SelfViewOff,
                    StandardCommandsEnum.SelfViewAuto,
                    StandardCommandsEnum.SelfView,
                    StandardCommandsEnum.SelfViewPoll,
                    StandardCommandsEnum.PictureMode,
                    StandardCommandsEnum.PresentationStart,
                    StandardCommandsEnum.PresentationStop,
                    StandardCommandsEnum.PipLocation,
                    StandardCommandsEnum.PipLocationPoll,
                    StandardCommandsEnum.MainVideoSource,
                    StandardCommandsEnum.MainVideoSourcePoll,
                    StandardCommandsEnum.CameraFarEndPanLeft,
                    StandardCommandsEnum.CameraFarEndPanRight,
                    StandardCommandsEnum.CameraFarEndTiltUp,
                    StandardCommandsEnum.CameraFarEndTiltDown,
                    StandardCommandsEnum.CameraFarEndZoomIn,
                    StandardCommandsEnum.CameraFarEndZoomOut,
                    StandardCommandsEnum.CameraFarEndFocusNear,
                    StandardCommandsEnum.CameraFarEndFocusFar,
                    StandardCommandsEnum.CameraFarEndStop,
                    StandardCommandsEnum.CameraFarEndStopPan,
                    StandardCommandsEnum.CameraFarEndStopTilt,
                    StandardCommandsEnum.CameraFarEndStopZoom,
                    StandardCommandsEnum.CameraFarEndStopFocus,
                    StandardCommandsEnum.CameraFarEndAutoFocus,
                    StandardCommandsEnum.HangUp,
                    StandardCommandsEnum.Dtmf,
                    StandardCommandsEnum.Hold,
                    StandardCommandsEnum.Resume,
                    StandardCommandsEnum.Transfer,
                    StandardCommandsEnum.Join,
                    StandardCommandsEnum.Answer,
                    StandardCommandsEnum.Reject,
                    StandardCommandsEnum.AllowFecc,
                    StandardCommandsEnum.DoNotAllowFecc,
                    StandardCommandsEnum.MuteMicsAutoAnswerOn,
                    StandardCommandsEnum.MuteMicsAutoAnswerOff,
                    StandardCommandsEnum.DoNotDisturbOn,
                    StandardCommandsEnum.DoNotDisturbOff,
                    StandardCommandsEnum.AutoAnswerOn,
                    StandardCommandsEnum.AutoAnswerOff,
                    StandardCommandsEnum.StandbyOn,
                    StandardCommandsEnum.StandbyOff,
                    StandardCommandsEnum.EncryptionOn,
                    StandardCommandsEnum.EncryptionOff,
                    StandardCommandsEnum.CameraTrackingOn,
                    StandardCommandsEnum.CameraTrackingOff,
                    StandardCommandsEnum.CameraRecallTrackingPreset,
                    StandardCommandsEnum.CameraStoreTrackingPreset,
                    StandardCommandsEnum.MessageResponse,
                    StandardCommandsEnum.MessageClear,
                    StandardCommandsEnum.AlertClear,
                    //Cameras
                    StandardCommandsEnum.CameraNearEndPanLeft,
                    StandardCommandsEnum.CameraNearEndPanRight,
                    StandardCommandsEnum.CameraNearEndTiltUp,
                    StandardCommandsEnum.CameraNearEndTiltDown,
                    StandardCommandsEnum.CameraNearEndZoomIn,
                    StandardCommandsEnum.CameraNearEndZoomOut,
                    StandardCommandsEnum.CameraNearEndFocusNear,
                    StandardCommandsEnum.CameraNearEndFocusFar,
                    StandardCommandsEnum.CameraNearEndStop,
                    StandardCommandsEnum.CameraNearEndStopPan,
                    StandardCommandsEnum.CameraNearEndStopTilt,
                    StandardCommandsEnum.CameraNearEndStopZoom,
                    StandardCommandsEnum.CameraNearEndStopFocus,
                    StandardCommandsEnum.CameraNearEndAutoFocus,
                    StandardCommandsEnum.CameraNearEndResetPosition,
                    StandardCommandsEnum.CameraNearEndRecallPreset,
                    StandardCommandsEnum.CameraNearEndStorePreset,
                    StandardCommandsEnum.SelfViewPipLocation,
                    StandardCommandsEnum.SelfViewMonitor,
                    StandardCommandsEnum.Reboot,
                    StandardCommandsEnum.CameraFarEndStorePreset,
                    StandardCommandsEnum.CameraFarEndRecallPreset,
                    StandardCommandsEnum.Input4Dvi,
                    StandardCommandsEnum.Input4SVideoComposite,
                    StandardCommandsEnum.FarEndPresentationSource,
                    StandardCommandsEnum.SelfViewFullScreenOn,
                    StandardCommandsEnum.SelfViewFullScreenOff,
                    StandardCommandsEnum.MultipointAutoAnswerOn,
                    StandardCommandsEnum.MultipointAutoAnswerOff,
                    StandardCommandsEnum.MultipointAutoAnswerDoNotDisturb,
                    StandardCommandsEnum.MultipointModeAuto,
                    StandardCommandsEnum.MultipointModePresentation,
                    StandardCommandsEnum.MultipointModeDiscussion,
                    StandardCommandsEnum.MultipointModeFullScreen,
                    StandardCommandsEnum.Monitor1PresentationFar,
                    StandardCommandsEnum.Monitor1PresentationNearOrFar,
                    StandardCommandsEnum.Monitor1PresentationContentOrFar,
                    StandardCommandsEnum.Monitor1PresentationAll,
                    StandardCommandsEnum.Monitor2PresentationNear,
                    StandardCommandsEnum.Monitor2PresentationFar,
                    StandardCommandsEnum.Monitor2PresentationContent,
                    StandardCommandsEnum.Monitor2PresentationNearOrFar,
                    StandardCommandsEnum.Monitor2PresentationContentOrNear,
                    StandardCommandsEnum.Monitor2PresentationContentOrFar,
                    StandardCommandsEnum.Monitor2PresentationAll,
                    StandardCommandsEnum.Monitor3PresentationNear,
                    StandardCommandsEnum.Monitor3PresentationFar,
                    StandardCommandsEnum.Monitor3PresentationContent,
                    StandardCommandsEnum.Monitor3PresentationRecordNearOrFar,
                    StandardCommandsEnum.Monitor3PresentationRecordAll,
                    StandardCommandsEnum.IrRemoteEmulationKeyPress,
                    StandardCommandsEnum.IrRemoteEmulationKeyRelease,
                    StandardCommandsEnum.IrRemoteEmulationKeyClick
                };
            }
        }
    }
}
