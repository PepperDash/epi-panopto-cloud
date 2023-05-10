// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Events
{
    /// <summary>
    /// Argument for authentication events
    /// </summary>
    public sealed class AuthenticationEventArgs : EventArgs
    {
        public AuthenticationEventArgs(bool isAuthenticated, string authenticationError)
        {
            IsAuthenticated = isAuthenticated;
            AuthenticationError = authenticationError;
        }

        /// <summary>
        /// The new authenticated state of the device
        /// </summary>
        public bool IsAuthenticated { get; private set; }

        /// <summary>
        /// The error reported by the device explaining why authentication failed
        /// </summary>
        public string AuthenticationError { get; private set; }
    }
}