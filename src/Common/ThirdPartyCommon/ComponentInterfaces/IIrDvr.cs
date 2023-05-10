// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IIrDvr
    {
        /// <summary>
        /// Method to send the DVR command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void DvrCommand(IrActions action);

        /// <summary>
        /// Method to send the Live command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Live(IrActions action);

        /// <summary>
        /// Method to send the Record command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void Record(IrActions action);

        /// <summary>
        /// Method to send the Speed Slow command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void SpeedSlow(IrActions action);

        /// <summary>
        /// Method to send the Thumbs Up command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void ThumbsUp(IrActions action);

        /// <summary>
        /// Method to send the Thumbs Down command to the Device.
        /// </summary>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void ThumbsDown(IrActions action);
    }
}
