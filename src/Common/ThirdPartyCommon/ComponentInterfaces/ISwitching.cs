// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Collections.Generic;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface ISwitching
    {
        bool SupportsAudio { get; }
        bool SupportsVideo { get; }

        List<SwitchConnection> Inputs { get; }
        List<SwitchConnection> Outputs { get; }

        void RouteVideo(int inputId, int outputId);
        void RouteAudio(int inputId, int outputId);
    }
}
