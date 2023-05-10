// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Crestron.Panopto.Common;
using Newtonsoft.Json.Linq;

namespace Crestron.Panopto.Common
{
    public class AuthenticationJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(AuthenticationNode));
        }

        private bool TokensExist(JObject jsonObject, List<string> tokenNames)
        {
            bool exists = true;
            foreach(string token in tokenNames)
            {
                if(!TokenExists(jsonObject, token))
                {
                    exists = false;
                    break;
                }
            }
            return exists;
        }

        private bool TokenExists(JObject jsonObject, string tokenName)
        {
            bool isNotNull = false;
            if(jsonObject[tokenName] != null)
            {
                isNotNull = true;
            }
            return isNotNull;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object result;
            JObject jo = JObject.Load(reader);
            result = jo;
            if (TokenExists(jo, "Type"))
            {
                if (jo["Type"].Value<string>().Equals(AuthenticationTypes.NONE))
                {
                    result = CreateNoAuthentication(jo);
                }
                if (jo["Type"].Value<string>().Equals(AuthenticationTypes.USERNAME_PASSWORD))
                {
                    result = CreateUsernamePassworthAuth(jo);
                }
            }
            return result;
        }

        private object CreateNoAuthentication(JObject jo)
        {
            var authentication = new NoAuthentication();
            if (TokenExists(jo, "Required"))
            {
                return new NoAuthentication { Required = jo["Required"].Value<bool>() };
            }
            return authentication;
        }

        private object CreateUsernamePassworthAuth(JObject jo)
        {
            var usernamePasswordAuth = new UsernamePasswordAuthentication();
            if(TokensExist(jo, new List<string>{"UsernameRequired", "UsernameMask", "PasswordRequired", "PasswordMask"}))
            {
                usernamePasswordAuth.UsernameRequired = jo["UsernameRequired"].Value<bool>();
                usernamePasswordAuth.UsernameMask = jo["UsernameMask"].Value<string>();
                usernamePasswordAuth.PasswordRequired = jo["PasswordRequired"].Value<bool>();
                usernamePasswordAuth.PasswordMask = jo["PasswordMask"].Value<string>();
            }
            if (TokensExist(jo, new List<string>{"DefaultUsername", "DefaultPassword", "Required"}))
            {
                usernamePasswordAuth.DefaultUsername = jo["DefaultUsername"].Value<string>();
                usernamePasswordAuth.DefaultPassword = jo["DefaultPassword"].Value<string>();
                usernamePasswordAuth.Required = jo["Required"].Value<bool>();
            }
            return usernamePasswordAuth;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
