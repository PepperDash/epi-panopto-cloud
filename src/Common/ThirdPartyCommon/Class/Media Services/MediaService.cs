// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.Panopto.Common.Enums;
using Crestron.Panopto.Common.Interfaces;
using System;
using System.Linq;
using Crestron.Panopto.Common.ExtensionMethods;

namespace Crestron.Panopto.Common
{
    public class MediaService : IMediaService
    {
        public string Id { get; set; }

        public string FriendlyName { get; set; }

        public System.Collections.Generic.IList<IMediaServiceSupportedFeature> SupportedFeatures { get; set; }

        public MediaServiceStates State { get; set; }

        public MediaServiceSubscriptionStates SubscriptionStatus { get; set; }

        public bool IsSelectable { get; set; }

        public bool IsBranded { get; set; }
    }
}