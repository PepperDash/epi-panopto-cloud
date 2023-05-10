// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IIrKeypad
    {
        /// <summary>
        /// Sends a keypard number to the device.
        /// </summary>
        /// <param name="number">Number to be sent to the device.</param>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void KeypadNumber(uint number, IrActions action);

        /// <summary>
        /// Method to send the enter command to the Device.
        /// Typically, Enter is used to force a channel number change
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Enter(IrActions action);

        /// <summary>
        /// Method to send a "#" to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Pound(IrActions action);

        /// <summary>
        /// Method to send a "*" to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Asterisk(IrActions action);

        /// <summary>
        /// Method to send a "." to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Period(IrActions action);

        /// <summary>
        /// Method to send a "-" to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Dash(IrActions action);
    }
}
