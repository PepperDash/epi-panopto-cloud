// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IBasicAvr7 : IBasicAvr6, ICustomCommandCollection2
    { }
    public interface IBasicAvr6 : IBasicAvr5, ICustomCommandCollection, ICustomCommand
    { }

    public interface IBasicAvr5 : IBasicAvr4, ISurround2, IAudio2
    { }
    public interface IBasicAvr4 : IBasicAvr3, IModelFileSupport
    { }
    public interface IBasicAvr3: IBasicAvr2, ISupportedCommandsHelper
    { }

    public interface IBasicAvr2 : IBasicAvr, INavigation2
    { }

    public interface IBasicAvr
            : IBasicInformation, IConnection, IPower, IOutputs, 
                IInputs2, IAudio, IVolume, ISurround, ITuner, 
                INavigation, IBasicLogger, IDisposable,
                IBasicInformation2
    {
        /// <summary>
        /// Alerts of a change in state
        /// </summary>
        event Action<AvrStateObjects, IBasicAvr, byte> StateChangedEvent;
    }
}