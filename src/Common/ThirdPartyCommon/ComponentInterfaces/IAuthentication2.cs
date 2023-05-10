// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code. 

using System;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IAuthentication2
    {
        /// <summary>
        /// Method used to override the stored value. 
        /// This is useful when the application wants to manage the authentication 
        /// outside of the RAD framework and simply supply the value directly.
        /// 
        /// Note: Value is not stored in DataStore.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        void OverrideUsername(string username);

        /// <summary>
        /// Method used to override the stored value. 
        /// This is useful when the application wants to manage the authentication 
        /// outside of the RAD framework and simply supply the value directly.
        /// 
        /// Note: Value is not stored in DataStore.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        void OverridePassword(string password);

        /// <summary>
        /// Method used to retrieve the default username value.
        /// </summary>
        string DefaultUsername { get; }

        /// <summary>
        /// Method used to retrieve the default password value.
        /// </summary>
        string DefaultPassword { get; }

        /// <summary>
        /// Method used to retrieve the required value. 
        /// 
        /// Note: This is set true when authentication is not an 
        /// optional setting for a device.
        /// </summary>
        bool Required { get; }

        bool IsAuthenticated { get; }
    }
}