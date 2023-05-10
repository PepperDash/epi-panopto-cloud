// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IAuthentication
    {
        /// <summary>
        /// Indicates that a username is supported.
        /// </summary>
        bool SupportsUsername { get; }

        /// <summary>
        /// Deprecated.
        /// RegEx defining any mask to be applied to the username input.
        /// </summary>
        [System.Obsolete("This is a deprecated method.", false)]
        string UsernameMask { get; }

        /// <summary>
        /// String to be used as the username key in the SIMPL# DataStore.
        /// </summary>
        string UsernameKey { set; }

        /// <summary>
        /// Indcates that a password is supported.
        /// </summary>
        bool SupportsPassword { get; }

        /// <summary>
        /// Deprecated.
        /// RegEx defining any mask to be applied to the password input.
        /// </summary>
        [System.Obsolete("This is a deprecated method.", false)]
        string PasswordMask { get; }

        /// <summary>
        /// String to be used as the password key in the SIMPL# DataStore.
        /// </summary>
        string PasswordKey { set; }

        /// <summary>
        /// Method to store username.
        /// </summary>
        /// <param name="username">Username to store.  Empty values supported.</param>
        /// <returns>true if successful.</returns>
        bool StoreUsername(string username);

        /// <summary>
        /// Method to store password.
        /// </summary>
        /// <param name="password">Password to store.  Empty values supported.</param>
        /// <returns>true if successful.</returns>
        bool StorePassword(string password);
    }
}