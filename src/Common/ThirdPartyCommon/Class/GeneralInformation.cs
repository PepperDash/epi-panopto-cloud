// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;

namespace Crestron.Panopto.Common
{
    public class GeneralInformation
    {
        public string DeviceType { get; set; }
        public string Manufacturer { get; set; }
        public string BaseModel { get; set; }
        public DateTime VersionDate { get; set; }
        public string DriverVersion { get; set; }
        public string SdkVersion { get; set; }
        public string Description { get; set; }
        public Guid Guid { get; set; }
        public List<string> SupportedSeries { get; set; }
        public List<string> SupportedModels { get; set; }

        public GeneralInformation()
        {
            DeviceType = string.Empty;
            Manufacturer = string.Empty;
            BaseModel = string.Empty;
            VersionDate = new DateTime();
            DriverVersion = string.Empty;
            SdkVersion = string.Empty;
            Description = string.Empty;
            Guid = new Guid();
            SupportedSeries = new List<string>();
            SupportedModels = new List<string>();
        }
    }
}
