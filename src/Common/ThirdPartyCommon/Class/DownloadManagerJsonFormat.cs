// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.RAD.Common
{
    class DriverInfo
    {
        public int Count { get; set; }
        public DriverMetadata[] DriverMetadata { get; set; }
    }

    class DriverMetadata
    {
        public string DeviceType { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string TransportType { get; set; }
        public string Filename { get; set; }
        public string Description { get; set; }
        public string VersionDate { get; set; }
        public string Version { get; set; }
// ReSharper disable once InconsistentNaming
        public string SDKVersion { get; set; }
        public string Url { get; set; }
        public string AvfSupported { get; set; }
    }
}
