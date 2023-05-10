// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IBasicInformation
    {
        string Description { get; }

        Guid Guid { get; }

        string Manufacturer { get; }

        string BaseModel { get; }

        string DriverVersion { get; }

        List<string> SupportedSeries { get; }

        List<string> SupportedModels { get; }

        DateTime VersionDate { get; }
        
        /// <summary>
        /// Id of the Device
        /// </summary>
        byte Id { get; set; }
       
        /// <summary>
        /// Indicates if the Device supports feedback
        /// </summary>
        [EditorBrowsableAttribute(EditorBrowsableState.Never)] 
        [System.Obsolete("This has been replaced by IFeedback.SupportsFeedback", false)]
        bool SupportsFeedback { get; }
    }
}
