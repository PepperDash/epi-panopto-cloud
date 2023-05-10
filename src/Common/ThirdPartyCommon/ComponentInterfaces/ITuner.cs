// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.Panopto.Common.Enums;
namespace Crestron.Panopto.Common.Interfaces
{
    public interface ITuner
    {
        /// <summary>
        /// Property indicating the Feedback is supported
        /// </summary>
        bool SupportsTunerFeedback { get; }

        /// <summary>
        /// Gets the tuner frequency
        /// </summary>
        string Tuner { get; }

        /// <summary>
        /// Property indicating that the change frequency command is supported
        /// </summary>
        bool SupportsChangeFrequency { get; }

        /// <summary>
        /// Method to send the frequency down command to the device
        /// </summary>
        /// <param name="action"></param>
        void FrequencyDown(CommandAction action);

        /// <summary>
        /// Method to send the frequency up command to the device
        /// </summary>
        /// <param name="action"></param>
        void FrequencyUp(CommandAction action);

        /// <summary>
        /// Property indicating that the supports set frequency command is supported
        /// </summary>
        bool SupportsSetFrequency { get; }

        /// <summary>
        /// Method to tune the device to a specific frequency
        /// </summary>
        /// <param name="value"></param>
        void SetSpecificFrequency(string value);

        /// <summary>
        /// property indicating that the auto tune frequency up down commands are supported
        /// </summary>
        bool SupportsAutoFrequency { get; }

        /// <summary>
        /// Method to send the auto frequency down command to the device
        /// </summary>
        /// <param name="action"></param>
        void AutoFrequencyDown(CommandAction action);

        /// <summary>
        /// Method to send the auto frequency up command to the device
        /// </summary>
        /// <param name="action"></param>
        void AutoFrequencyUp(CommandAction action);

        /// <summary>
        /// Property indicating that the frequency band command is supported by the device
        /// </summary>
        bool SupportsFrequencyBand { get; }

        /// <summary>
        /// Method to send the frequency band command to the device
        /// </summary>
        void FrequencyBand();

        /// <summary>
        /// Property indicating that the discrete frequency AM and FM commands are supported by the device
        /// </summary>
        bool SupportsDiscreteFrequencyBand { get; }

        /// <summary>
        /// Method to send the discrete frequency band AM command to the device
        /// </summary>
        void FrequencyBandAm();

        /// <summary>
        /// Method to send the discrete frequency band FM command to the device
        /// </summary>
        void FrequencyBandFm();

        /// <summary>
        /// Property returning the maximum number of presets that the device supports
        /// </summary>
        uint MaxNumPresets { get; }

        /// <summary>
        /// Property used to indicate that the device supports preset up/down functionality
        /// </summary>
        bool SupportsChangePreset { get; }

        /// <summary>
        /// Method used to send the Preset Up command to the device
        /// </summary>
        void PresetUp(CommandAction action);

        /// <summary>
        /// Method used to send the Preset Down command to the device
        /// </summary>
        void PresetDown(CommandAction action);

        /// <summary>
        /// Property indicating that the device supports preset recall
        /// </summary>
        bool SupportsPresetRecall { get; }

        /// <summary>
        /// Method used to send the recall preset command to the device
        /// </summary>
        /// <param name="value"></param>
        void PresetRecall(uint value);

        /// <summary>
        /// Property indicating that the device suppports store preset command
        /// </summary>
        bool SupportsPresetStore { get; }

        /// <summary>
        /// Method used to send the store preset command to the device
        /// </summary>
        /// <param name="value"></param>
        void PresetStore(uint value);
    }
}