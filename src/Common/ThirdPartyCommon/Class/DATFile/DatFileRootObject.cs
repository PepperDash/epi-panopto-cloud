// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Collections.Generic;

namespace Crestron.Panopto.Common
{
    public class DatFileRootObject
    {
        public string manufacturer;
        public string driverId;
        public PowerWaitPeriod power;
        public string deviceType;
        public Crestron.Panopto.Common.Enums.DeviceTypes deviceTypeId;
        public string sdkVersion;
        public string driverVersion;
        public List<string> supportedSeries;
        public string description;
        public List<string> supportedModels;
        public List<DatFileInput> inputs;
        public List<DatFileFeature> features;
        public string baseModel;
        public ACommunication communication;
        public string driverVersionDate;
        public List<UserAttribute> userAttributes;
        public DatFileMultiPowerOff multiPowerOff;
    }
}