// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.Panopto.Common.Enums;
using Crestron.Panopto.Common.Interfaces;

namespace Crestron.Panopto.Common
{
    public class MediaServiceSupportedFeature : IMediaServiceSupportedFeature
    {
        public System.Type ComponentInterface { get; set; }

        public System.Collections.Generic.IList<CommonFeatureSupport> SupportStatements { get; set; }
    }
}