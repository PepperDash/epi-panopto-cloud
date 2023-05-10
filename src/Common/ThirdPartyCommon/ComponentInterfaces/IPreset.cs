// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.ThirdPartyCommon.ComponentInterfaces
{
    public enum PresetEvent
    {
        Saved,
        Recalled,
        Cleared
    }

    /// <summary>
    /// Interface to define Abstract repsentation of a preset from codec.
    /// The Preset class implements this.
    /// </summary>
    public interface IPreset
    {
        /// <summary>
        /// Method to recall the preset.
        /// </summary>
        void Recall();

        /// <summary>
        /// Save the preset values to Codec
        /// </summary>
        void Save(string presetName);

        /// <summary>
        /// NumberKeypad of the Preset
        /// </summary>
        int Number { get; }
        /// <summary>
        /// Name of preset
        /// </summary>
        string Name { get; }
    }
}
