// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.Panopto.Common.Enums;
namespace Crestron.Panopto.Common.Interfaces
{
    public interface IIrAudio
    {
        /// <summary>
        /// Method used to send the Tone Control On command to the device
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void ToneControlOn(IrActions action);

        /// <summary>
        /// Method used to send the Tone Control Off command to the device
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void ToneControlOff(IrActions action);

        /// <summary>
        /// Method used to send the Tone Control Toggle command to the device
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void ToneControlToggle(IrActions action);

        /// <summary>
        /// Method used to send the Bass Up command to the device
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void BassUp(IrActions action);

        /// <summary>
        /// Method used to send the Bass Down command to the device
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void BassDown(IrActions action);

        /// <summary>
        /// Method used to send the Treble Up command to the device
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void TrebleUp(IrActions action);

        /// <summary>
        /// Method used to send the Treble Down command to the device
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void TrebleDown(IrActions action);

        /// <summary>
        /// Method used to send the Loudness On command to the device
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void LoudnessOn(IrActions action);

        /// <summary>
        /// Method used to send the Loudness Off command to the device
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void LoudnessOff(IrActions action);

        /// <summary>
        /// Method used to send the Toggle Loudness command to the device
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void ToggleLoudness(IrActions action);
    }
}