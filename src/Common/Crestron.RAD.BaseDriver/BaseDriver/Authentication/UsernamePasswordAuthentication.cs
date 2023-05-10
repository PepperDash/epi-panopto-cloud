// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Text.RegularExpressions;

namespace Crestron.RAD.BaseDriver
{
    public class UsernamePasswordAuthentication : AuthenticationNode
    {
        private string _usernameMask = string.Empty;
        private string _passwordMask = string.Empty;

        public override string Type
        {
            get { return AuthenticationTypes.USERNAME_PASSWORD; }
        }

        public bool UsernameRequired;
        public string UsernameMask
        {
            get
            {
                return _usernameMask;
            }
            set
            {
                _usernameMask = value;
            }
        }
        public bool PasswordRequired;
        public string PasswordMask
        {
            get
            {
                return _passwordMask;
            }
            set
            {
                _passwordMask = value;
            }
        }

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
    }
}