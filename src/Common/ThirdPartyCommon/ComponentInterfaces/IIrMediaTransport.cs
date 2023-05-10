// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IIrMediaTransport
    {
        /// <summary>
        /// Method to send the forward scan command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void ForwardScan(IrActions action);

        /// <summary>
        /// Method to send the reverse scan command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void ReverseScan(IrActions action);

        /// <summary>
        /// Method to send the play command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Play(IrActions action);

        /// <summary>
        /// Method to send the pause command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Pause(IrActions action);

        /// <summary>
        /// Method to send the stop command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Stop(IrActions action);

        /// <summary>
        /// Method to send the forward skip command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void ForwardSkip(IrActions action);

        /// <summary>
        /// Method to send the reverse skip command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void ReverseSkip(IrActions action);

        /// <summary>
        /// Method to send the repeat command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Repeat(IrActions action);

        /// <summary>
        /// Method to send the return command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Return(IrActions action);

        /// <summary>
        /// Method to send the back command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Back(IrActions action);

        /// <summary>
        /// Method to send the Play/Pause command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void PlayPause(IrActions action);  
    }
}
