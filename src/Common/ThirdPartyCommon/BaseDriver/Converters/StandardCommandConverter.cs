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
using Crestron.Panopto.Common.Enums;
using Crestron.SimplSharp;

namespace Crestron.Panopto.Common
{
    public class StandardCommandConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Dictionary<StandardCommandsEnum, Commands>));
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var standardCommands = new Dictionary<StandardCommandsEnum, Commands>();
            foreach (var commandPair in jo)
            {
                StandardCommandsEnum key = StandardCommandsEnum.NotAStandardCommand;
                Commands value = new Commands();
                try
                {
                    key = (StandardCommandsEnum)Enum.Parse(typeof(StandardCommandsEnum), commandPair.Key, true);
                    value = JsonConvert.DeserializeObject<Commands>(commandPair.Value.ToString());
                    standardCommands[key] = value;
                }
                catch
                {
                    ErrorLog.Notice("Invalid StndardCommadnsEnum found in JSON. Skipping {0}", commandPair.Key);
                }
            }
            return standardCommands;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
