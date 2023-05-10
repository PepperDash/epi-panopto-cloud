// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.RAD.Common.ExtensionMethods;
using Crestron.SimplSharpPro;

namespace Crestron.RAD.ProTransports.ExtensionMethods
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Call to try unregister a GenericDevice and then try to dispose.
        /// </summary>
        /// <param name="deviceObj">GenericDevice object used in Simpl# Pro</param>
        /// <returns>Whether object exists and was able to dispose.</returns>
        public static bool TryDeviceUnregisterDispose(this GenericDevice deviceObj)
        {
            if (deviceObj.Exists())
            {
                if (deviceObj.Registered)
                {
                    deviceObj.UnRegister();
                }
                return deviceObj.TryDispose();
            }
            return false;
        }
    }
}