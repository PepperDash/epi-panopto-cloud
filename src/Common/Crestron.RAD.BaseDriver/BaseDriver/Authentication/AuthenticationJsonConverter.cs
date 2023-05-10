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
using Crestron.RAD.Common;
using Newtonsoft.Json.Linq;

namespace Crestron.RAD.BaseDriver
{
    public class AuthenticationJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(AuthenticationNode));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            if (jo["Type"].Value<string>().Equals(AuthenticationTypes.NONE))
                return new NoAuthentication();
            else if (jo["Type"].Value<string>().Equals(AuthenticationTypes.USERNAME_PASSWORD))
                return new UsernamePasswordAuthentication()
                           {
                               UsernameRequired = jo["UsernameRequired"].Value<bool>(),
                               UsernameMask = jo["UsernameMask"].Value<string>(),
                               PasswordRequired = jo["PasswordRequired"].Value<bool>(),
                               PasswordMask = jo["PasswordMask"].Value<string>()
                           };
            return jo;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
