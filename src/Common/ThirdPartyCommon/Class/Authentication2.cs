// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using Crestron.Panopto.Common.Enums;
using Crestron.Panopto.Common.Interfaces;
using Crestron.SimplSharp.CrestronDataStore;

namespace Crestron.Panopto.Common
{
    public class Authentication2 : IAuthentication2
    {
        private bool _isRequired = false;
        private string _defaultPassword = string.Empty;
        private string _defaultUsername = string.Empty;

        public Authentication2(AuthenticationNode node)
        {
            if(node is UsernamePasswordAuthentication)
            {
                var auth = node as UsernamePasswordAuthentication;

                _defaultPassword = auth.DefaultPassword;
                _defaultUsername = auth.DefaultUsername;
                _isRequired = auth.Required;
            }
        }

        #region IAuthentication2 Members

        public void OverrideUsername(string username)
        {
        }

        public void OverridePassword(string password)
        {
        }

        public string DefaultUsername
        {
            get { return _defaultUsername; }
        }

        public string DefaultPassword
        {
            get { return _defaultPassword; }
        }

        public bool Required
        {
            get { return _isRequired; }
        }

        public bool IsAuthenticated { get; set; }

        #endregion 

    }
}