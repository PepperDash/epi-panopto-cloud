// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IBasicInformation2
    {
        /// <summary>
        /// Sets a user attribute to the specified value.
        /// </summary>
        /// <param name="attributeId">The attribute ID specified by UserAttribute.ParameterId</param>
        /// <param name="attributeValue">The value this attribute should be set to</param>
        void SetUserAttribute(string attributeId, string attributeValue);

        /// <summary>
        /// Sets a user attribute to the specified value.
        /// </summary>
        /// <param name="attributeId">The attribute ID specified by UserAttribute.ParameterId</param>
        /// <param name="attributeValue">The value this attribute should be set to</param>
        void SetUserAttribute(string attributeId, ushort attributeValue);

        /// <summary>
        /// Sets a user attribute to the specified value.
        /// </summary>
        /// <param name="attributeId">The attribute ID specified by UserAttribute.ParameterId</param>
        /// <param name="attributeValue">The value this attribute should be set to</param>
        void SetUserAttribute(string attributeId, bool attributeValue);

        /// <summary>
        /// Retrieves a list of all User Attributes the current driver supports
        /// </summary>
        /// <returns></returns>
        List<UserAttribute> RetrieveUserAttributes();
    }
}
