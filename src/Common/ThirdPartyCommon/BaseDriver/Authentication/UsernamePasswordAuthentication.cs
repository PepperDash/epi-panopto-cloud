// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Text.RegularExpressions;

namespace Crestron.Panopto.Common
{
    public class UsernamePasswordAuthentication : AuthenticationNode
    {
        #region Properties

        public override string Type
        {
            get { return AuthenticationTypes.USERNAME_PASSWORD; }
        }

        public override bool Required { get; set; }

        public bool UsernameRequired;
        
        [Obsolete("This is deprecated.", false)]
        public string UsernameMask { get; set; }

        public bool PasswordRequired;
        
        [Obsolete("This is deprecated.", false)]
        public string PasswordMask { get; set; }

        public string DefaultUsername { get; set; }

        public string DefaultPassword { get; set; }

        #endregion

        public UsernamePasswordAuthentication()
        {
            DefaultPassword = string.Empty;
            DefaultUsername = string.Empty;
            UsernameMask = string.Empty;
            PasswordMask = string.Empty;
        }

        #region Methods

        [Obsolete("This method is deprecated because UsernameMask and PasswordMask are deprecated.", false)]
        private bool IsValidRegex(string pattern)
        {
            bool isValid = false;
            try
            {
                var temp = new Regex(pattern);
                isValid = true;
            }
            catch (ArgumentException)
            {
                //should alert user of invalid regex
                isValid = false;
            }
            return isValid;
        }

        #endregion
    }
}