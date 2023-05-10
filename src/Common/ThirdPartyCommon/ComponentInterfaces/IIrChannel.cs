// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IIrChannel
    {
        /// <summary>
        /// Method to send the Channel Down command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void ChannelDown(IrActions action);

        /// <summary>
        /// Method to send the Channel Up command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void ChannelUp(IrActions action);

        /// <summary>
        /// Method to send the Guide command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Guide(IrActions action);

        /// <summary>
        /// Method to send the Page Down command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void PageDown(IrActions action);

        /// <summary>
        /// Method to send the Page Up command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void PageUp(IrActions action);
    }

    public interface IIrChannel2
    {
        /// <summary>
        /// Method to set the Channel Sequence Properties
        /// </summary>
        /// <param name="configuration"></param>
        void SetChannelSequenceConfiguration(ChannelSequenceConfig configuration);
    }
}
