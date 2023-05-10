// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Collections.Generic;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IIrNavigation
    {
        /// <summary>
        /// Method to send an arrow key to the Device.
        /// </summary>
        /// <param name="direction">Direction of arrow to be send to the device.</param>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void ArrowKey(ArrowDirections direction, IrActions action);

        /// <summary>
        /// Method to send the Select command to the Device.
        /// Typically, Select is used when calling the center button of a directional pad
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Select(IrActions action);

        /// <summary>
        /// Method to send the clear command to the device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Clear(IrActions action);

        /// <summary>
        /// Method to send the exit command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Exit(IrActions action);

        /// <summary>
        /// Method to send the home command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Home(IrActions action);

        /// <summary>
        /// Method to send the menu command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Menu(IrActions action);

        /// <summary>
        /// Method used to send the search command to the device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Search(IrActions action);
    }
}
