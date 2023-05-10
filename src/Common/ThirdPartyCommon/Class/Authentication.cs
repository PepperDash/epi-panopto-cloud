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
    public class Authentication : IAuthentication
    {
        private bool _supportsUsername;
        private bool _supportsPassword;
        private string _usernameMask;
        private string _passwordMask;
        private string _usernameKey;
        private string _passwordKey;
        private IDataStore _dataStore;

        public Authentication(AuthenticationNode node, IDataStore dataStore)
        {
            if(node is UsernamePasswordAuthentication)
            {
                var auth = node as UsernamePasswordAuthentication;
                _supportsUsername = auth.UsernameRequired;
                _supportsPassword = auth.PasswordRequired;
                _usernameMask = auth.UsernameMask;
                _passwordMask = auth.PasswordMask;
            }
            _dataStore = dataStore;
        }

        private bool StoreField(string field, string mask, string key)
        {
            bool fieldStored = false;
            if (!String.IsNullOrEmpty(key))
            {
                var resultCode = _dataStore.SetLocalValue(key, field as object);
                if (resultCode == CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
                {
                    fieldStored = true;
                }
            }
            return fieldStored;
        }

        #region IAuthentication Members

        public bool SupportsUsername
        {
            get {return _supportsUsername; }
        }

        public string UsernameMask
        {
            get { return _usernameMask; }
        }

        public string UsernameKey
        {
            set { this._usernameKey = value; }
        }

        public bool SupportsPassword
        {
            get { return _supportsPassword; }
        }

        public string PasswordMask
        {
            get { return _passwordMask; }
        }

        public string PasswordKey
        {
            set { this._passwordKey = value; }
        }

        public bool StoreUsername(string username)
        {
            return StoreField(username, this._usernameMask, this._usernameKey);
        }

        public bool StorePassword(string password)
        {
            return StoreField(password, this._passwordMask, this._passwordKey);
        }

        #endregion 
    }
}