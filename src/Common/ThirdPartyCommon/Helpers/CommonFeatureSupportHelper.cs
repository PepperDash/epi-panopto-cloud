// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Linq;
using System.Collections.Generic;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Helpers
{
    public static class CommonFeatureSupportHelper
    {
        private static Dictionary<CommonFeatureSupport, string> FeatureSupportDescriptions
        {
            get
            {
                var dictionary = new Dictionary<CommonFeatureSupport, string>();
                dictionary.Add(CommonFeatureSupport.SupportsArrowKeys, "arrow keys, directions up/down/left/right ");
                dictionary.Add(CommonFeatureSupport.SupportsAsterisk, "Asterisk key function");
                dictionary.Add(CommonFeatureSupport.SupportsBack, "Back key to go back");
                dictionary.Add(CommonFeatureSupport.SupportsChangeChannel, "changing of channel");
                dictionary.Add(CommonFeatureSupport.SupportsChangeVolume, "changing of volume");
                dictionary.Add(CommonFeatureSupport.SupportsChannelFeedback, "feedback for the device");
                dictionary.Add(CommonFeatureSupport.SupportsClear, "Clear key");
                dictionary.Add(CommonFeatureSupport.SupportsColorButtons, "Color buttons");
                dictionary.Add(CommonFeatureSupport.SupportsCoolDownTime, "Cool down time");
                dictionary.Add(CommonFeatureSupport.SupportsDash, "Dash key");
                dictionary.Add(CommonFeatureSupport.SupportsDisconnect, "Disconnect");
                dictionary.Add(CommonFeatureSupport.SupportsDiscreteMute, "Discrete mute");
                dictionary.Add(CommonFeatureSupport.SupportsDiscretePower, "Discrete power");
                dictionary.Add(CommonFeatureSupport.SupportsDvrCommand, "DVR command");
                dictionary.Add(CommonFeatureSupport.SupportsEnter, "Enter");
                dictionary.Add(CommonFeatureSupport.SupportsExit, "Exit");
                dictionary.Add(CommonFeatureSupport.SupportsFavorite, "Favorite");
                dictionary.Add(CommonFeatureSupport.SupportsForwardScan, "Forward scan");
                dictionary.Add(CommonFeatureSupport.SupportsForwardSkip, "Forward skip");
                dictionary.Add(CommonFeatureSupport.SupportsGuide, "Guide");
                dictionary.Add(CommonFeatureSupport.SupportsHome, "Home");
                dictionary.Add(CommonFeatureSupport.SupportsInfo, "Info");
                dictionary.Add(CommonFeatureSupport.SupportsKeypadBackSpace, "Backspace");
                dictionary.Add(CommonFeatureSupport.SupportsKeypadNumber, "Number");
                dictionary.Add(CommonFeatureSupport.SupportsLast, "Last");
                dictionary.Add(CommonFeatureSupport.SupportsLetterKeys, "Letter keys");
                dictionary.Add(CommonFeatureSupport.SupportsLive, "Live");
                dictionary.Add(CommonFeatureSupport.SupportsMenu, "Menu");
                dictionary.Add(CommonFeatureSupport.SupportsMute, "Mute");
                dictionary.Add(CommonFeatureSupport.SupportsMuteFeedback, "Mute feedback");
                dictionary.Add(CommonFeatureSupport.SupportsPageChange, "Page change");
                dictionary.Add(CommonFeatureSupport.SupportsPause, "Pause");
                dictionary.Add(CommonFeatureSupport.SupportsPeriod, "Period");
                dictionary.Add(CommonFeatureSupport.SupportsPlay, "Play");
                dictionary.Add(CommonFeatureSupport.SupportsPound, "Pound");
                dictionary.Add(CommonFeatureSupport.SupportsPowerFeedback, "Power feedback");
                dictionary.Add(CommonFeatureSupport.SupportsReconnect, "Reconnect");
                dictionary.Add(CommonFeatureSupport.SupportsRecord, "Record");
                dictionary.Add(CommonFeatureSupport.SupportsRepeat, "Repeat");
                dictionary.Add(CommonFeatureSupport.SupportsReplay, "Replay");
                dictionary.Add(CommonFeatureSupport.SupportsReturn, "Return");
                dictionary.Add(CommonFeatureSupport.SupportsReverseScan, "Reverse scan");
                dictionary.Add(CommonFeatureSupport.SupportsReverseSkip, "Reverse skip");
                dictionary.Add(CommonFeatureSupport.SupportsSelect, "Select");
                dictionary.Add(CommonFeatureSupport.SupportsSetChannel, "Channel set");
                dictionary.Add(CommonFeatureSupport.SupportsSetVolume, "Volume set");
                dictionary.Add(CommonFeatureSupport.SupportsSpeedSlow, "Slow speed");
                dictionary.Add(CommonFeatureSupport.SupportsStop, "Stop");
                dictionary.Add(CommonFeatureSupport.SupportsThumbsDown, "Thumbs down");
                dictionary.Add(CommonFeatureSupport.SupportsThumbsUp, "Thumbs up");
                dictionary.Add(CommonFeatureSupport.SupportsTogglePower, "Power toggle");
                dictionary.Add(CommonFeatureSupport.SupportsVolumePercentFeedback, "Volume percent feedback");
                dictionary.Add(CommonFeatureSupport.SupportsWarmUpTime, "Warm up time");
                dictionary.Add(CommonFeatureSupport.SupportsFeedback, "Feedback");
                dictionary.Add(CommonFeatureSupport.SupportsPlayBackStatusFeedback, "Playback status feedback");
                dictionary.Add(CommonFeatureSupport.SupportsTrackFeedback, "Track feedback");
                dictionary.Add(CommonFeatureSupport.SupportsChapterFeedback, "Chapter feedback");
                dictionary.Add(CommonFeatureSupport.SupportsTrackElapsedTimeFeedback, "Track elapsed time feedback");
                dictionary.Add(CommonFeatureSupport.SupportsTrackRemainingTimeFeedback, "Track remaining time feedback");
                dictionary.Add(CommonFeatureSupport.SupportsChapterElapsedTimeFeedback, "Chapter elapsed time feedback");
                dictionary.Add(CommonFeatureSupport.SupportsChapterRemainingTimeFeedback, "Chapter remaining time feedback");
                dictionary.Add(CommonFeatureSupport.SupportsTotalElapsedTimeFeedback, "Total elapsed time feedback");
                dictionary.Add(CommonFeatureSupport.SupportsTotalRemainingTimeFeedback, "Total remaining time feedback");
                dictionary.Add(CommonFeatureSupport.SupportsAudio, "Audio");
                dictionary.Add(CommonFeatureSupport.SupportsDisplay, "Display");
                dictionary.Add(CommonFeatureSupport.SupportsEject, "Eject");
                dictionary.Add(CommonFeatureSupport.SupportsOptions, "Options");
                dictionary.Add(CommonFeatureSupport.SupportsSubtitle, "Subtitle");
                dictionary.Add(CommonFeatureSupport.SupportsInputFeedback, "Input feedback");
                dictionary.Add(CommonFeatureSupport.SupportsLampHours, "Lamp hours");
                dictionary.Add(CommonFeatureSupport.SupportsSetInputSource, "Input source set");
                dictionary.Add(CommonFeatureSupport.SupportsPanAndTilt, "Pan and tilt");
                dictionary.Add(CommonFeatureSupport.SupportsZoom, "Zoom");
                dictionary.Add(CommonFeatureSupport.SupportsManualFocus, "Manual focus");
                dictionary.Add(CommonFeatureSupport.SupportsAutoFocus, "Auto focus");
                dictionary.Add(CommonFeatureSupport.SupportsIrRemoteEmulation, "IR remote emulation");
                dictionary.Add(CommonFeatureSupport.SupportsMeetings, "Meetings");
                dictionary.Add(CommonFeatureSupport.SupportsSelfView, "Self view");
                dictionary.Add(CommonFeatureSupport.SupportsSelfViewPosition, "Self view position");
                dictionary.Add(CommonFeatureSupport.SupportsDualVideo, "Dual video");
                dictionary.Add(CommonFeatureSupport.SupportsPresentationPip, "Presentation PIP");
                dictionary.Add(CommonFeatureSupport.SupportsPictureMode, "Picture mode");
                dictionary.Add(CommonFeatureSupport.SupportsLocality, "Locality");
                dictionary.Add(CommonFeatureSupport.SupportsToggleVideoMute, "Video mute toggle");
                dictionary.Add(CommonFeatureSupport.SupportsSwitching, "Switching");
                dictionary.Add(CommonFeatureSupport.SupportsToggleEnergyStar, "Energy star toggle");
                dictionary.Add(CommonFeatureSupport.SupportsDiscreteEnergyStar, "Energy star discrete");
                dictionary.Add(CommonFeatureSupport.SupportsEnergyStarFeedback, "Energy star feedback");
                dictionary.Add(CommonFeatureSupport.SupportsDiscreteVideoMute, "Video mute discrete");
                dictionary.Add(CommonFeatureSupport.SupportsVideoMuteFeedback, "Video mute feedback");
                dictionary.Add(CommonFeatureSupport.SupportsOnScreenDisplayFeedback, "On Screen Display feedback");
                dictionary.Add(CommonFeatureSupport.SupportsFarEndCameraPresetStore, "Far end camera preset store");
                dictionary.Add(CommonFeatureSupport.SupportsInput4Connector, "Input4 connector");
                dictionary.Add(CommonFeatureSupport.SupportsFarEndPresentationSourceSelect, "Far end presentation source select");
                dictionary.Add(CommonFeatureSupport.SupportsMultipointControl, "Multipoint control");
                dictionary.Add(CommonFeatureSupport.SupportsIREmulation, "IR emulation");
                dictionary.Add(CommonFeatureSupport.SupportsMonitorPresentation, "Monitor presentation");
                dictionary.Add(CommonFeatureSupport.SupportsPictureLayout, "Monitor presentation");
                dictionary.Add(CommonFeatureSupport.SupportsPan, "Pan");
                dictionary.Add(CommonFeatureSupport.SupportsTilt, "Tilt");
                dictionary.Add(CommonFeatureSupport.SupportsMicMute, "Mic Mute");
                dictionary.Add(CommonFeatureSupport.SupportsPopUpMenu, "Popup Menu");
                dictionary.Add(CommonFeatureSupport.SupportsTopMenu, "Top Menu");
                dictionary.Add(CommonFeatureSupport.SupportsSelect, "Select");
                dictionary.Add(CommonFeatureSupport.SupportsToneControlFeedback, "Tone Control Feedback");
                dictionary.Add(CommonFeatureSupport.SupportsDiscreteToneControl, "Discrete Tone Control");
                dictionary.Add(CommonFeatureSupport.SupportsToggleToneControl, "Toggle Tone Control");
                dictionary.Add(CommonFeatureSupport.SupportsBassFeedback, "Bass Feedback");
                dictionary.Add(CommonFeatureSupport.SupportsBassDbFeedback, "Bass Db Feedback");
                dictionary.Add(CommonFeatureSupport.SupportsTrebleFeedback, "Treble Feedback");
                dictionary.Add(CommonFeatureSupport.SupportsTrebleDbFeedback, "Treble Db Feedback");
                dictionary.Add(CommonFeatureSupport.SupportsSetBass, "Set Bass");
                dictionary.Add(CommonFeatureSupport.SupportsChangeBass, "Change Bass");
                dictionary.Add(CommonFeatureSupport.SupportsSetTreble, "Set Treble");
                dictionary.Add(CommonFeatureSupport.SupportsChangeTreble, "Change Treble");
                dictionary.Add(CommonFeatureSupport.SupportsLoudnessFeedback, "Loudness Feedback");
                dictionary.Add(CommonFeatureSupport.SupportsDiscreteLoudness, "Discrete Loudness");
                dictionary.Add(CommonFeatureSupport.SupportsToggleLoudness, "Toggle Loudness");
                dictionary.Add(CommonFeatureSupport.SupportsSurroundModeFeedback, "Surround Mode Feedback");
                dictionary.Add(CommonFeatureSupport.SupportsSurroundModeCycle, "Surround Mode Cycle");
                dictionary.Add(CommonFeatureSupport.SupportsTunerFeedback, "Tuner Feedback");
                dictionary.Add(CommonFeatureSupport.SupportsChangeFrequency, "Change Frequency");
                dictionary.Add(CommonFeatureSupport.SupportsSetFrequency, "Set Frequency");
                dictionary.Add(CommonFeatureSupport.SupportsAutoFrequency, "Auto Frequency");
                dictionary.Add(CommonFeatureSupport.SupportsFrequencyBand, "Frequency Band");
                dictionary.Add(CommonFeatureSupport.SupportsDiscreteFrequencyBand, "Discrete Frequency Band");
                dictionary.Add(CommonFeatureSupport.SupportsPresetRecall, "Preset Recall");
                dictionary.Add(CommonFeatureSupport.SupportsPresetStore, "Preset Store");
                dictionary.Add(CommonFeatureSupport.SupportsSearch, "Search");
                dictionary.Add(CommonFeatureSupport.SupportsSetAudioInputSource, "Set Audio Input Source");
                dictionary.Add(CommonFeatureSupport.SupportsSetVideoInputSource, "Set Video Input Source");
                dictionary.Add(CommonFeatureSupport.SupportsAudioInputFeedback, "Audio Input Feedback");
                dictionary.Add(CommonFeatureSupport.SupportsVideoInputFeedback, "Video Input Feedback");
                dictionary.Add(CommonFeatureSupport.SupportsSetAudioOutputSource, "Set Audio Output Source");
                dictionary.Add(CommonFeatureSupport.SupportsSetVideoOutputSource, "Set Video Output Source");
                dictionary.Add(CommonFeatureSupport.SupportsAudioOutputFeedback, "Audio Output Feedback");
                dictionary.Add(CommonFeatureSupport.SupportsVideoOutputFeedback, "Video Output Feedback");
                dictionary.Add(CommonFeatureSupport.SupportsUsername, "User Name");
                dictionary.Add(CommonFeatureSupport.SupportsPassword, "Password");
                dictionary.Add(CommonFeatureSupport.SupportsSirius, "Sirius");
                dictionary.Add(CommonFeatureSupport.SupportsXm, "Xm");
                dictionary.Add(CommonFeatureSupport.SupportsSiriusXm, "SiriusXm");
                dictionary.Add(CommonFeatureSupport.SupportsHdRadio, "HD Radio");
                dictionary.Add(CommonFeatureSupport.SupportsInternetRadio, "Internet Radio");
                dictionary.Add(CommonFeatureSupport.SupportsLastFm, "Last FM");
                dictionary.Add(CommonFeatureSupport.SupportsPandora, "Pandora");
                dictionary.Add(CommonFeatureSupport.SupportsRhapsody, "Rhapsody");
                dictionary.Add(CommonFeatureSupport.SupportsChangePreset, "Change Preset");
                dictionary.Add(CommonFeatureSupport.SupportsPlayPause, "PlayPause");
                dictionary.Add(CommonFeatureSupport.SupportsSpotify, "Spotify");
                dictionary.Add(CommonFeatureSupport.SupportsYouTube, "You Tube");
                dictionary.Add(CommonFeatureSupport.SupportsYouTubeTv, "You Tube Tv");
                dictionary.Add(CommonFeatureSupport.SupportsNetflix, "Netflix");
                dictionary.Add(CommonFeatureSupport.SupportsHulu, "Hulu");
                dictionary.Add(CommonFeatureSupport.SupportsDirectvNow, "Directv Now");
                dictionary.Add(CommonFeatureSupport.SupportsAmazonVideo, "Amazon Video");
                dictionary.Add(CommonFeatureSupport.SupportsPlaystationVue, "Playstation Vue");
                dictionary.Add(CommonFeatureSupport.SupportsSlingTv, "Sling Tv");
                dictionary.Add(CommonFeatureSupport.SupportsAirplay, "Airplay");
                dictionary.Add(CommonFeatureSupport.SupportsGoogleCast, "Google Cast");
                dictionary.Add(CommonFeatureSupport.SupportsDlna, "DLNA");
                dictionary.Add(CommonFeatureSupport.SupportsTidal, "Tidal");
                dictionary.Add(CommonFeatureSupport.SupportsDeezer, "Deezer");
                dictionary.Add(CommonFeatureSupport.SupportsCrackle, "Crackle");
                dictionary.Add(CommonFeatureSupport.SupportsOnDemand, "On Demand");
                dictionary.Add(CommonFeatureSupport.SupportsGooglePlay, "Google Play");
                dictionary.Add(CommonFeatureSupport.SupportsBluetooth, "Bluetooth");
 
                return dictionary;
            }
        }
        public static string GetDescription(CommonFeatureSupport supportEnum)
        {
            var desc = supportEnum.ToString().Split('.').Last();
            if (FeatureSupportDescriptions.ContainsKey(supportEnum) &&
                !string.IsNullOrEmpty(FeatureSupportDescriptions[supportEnum]))
            {
                desc = FeatureSupportDescriptions[supportEnum];
            }

            return string.Format("Enables {0}", desc);
        }
    }
}
