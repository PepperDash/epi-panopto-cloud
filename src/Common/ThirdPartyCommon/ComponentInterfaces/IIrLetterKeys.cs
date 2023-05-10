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
    public interface IIrLetterKeys
    {
        /// <summary>
        /// Method to send a letter key to the device.
        /// </summary>
        /// <param name="letterKeys">Letter to be sent.</param>
        /// <param name="action">Indicates if command should be pressed, held, or released.</param>
        void LetterKeys(LetterButtons letterKeys, IrActions action);
    }
}
