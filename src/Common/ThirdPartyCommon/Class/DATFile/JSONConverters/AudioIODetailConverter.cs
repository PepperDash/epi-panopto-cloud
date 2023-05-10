using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json.Converters;
using Crestron.RAD.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Crestron.RAD.Common
{
    public class AudioIODetailConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(AudioIODetail));
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            AudioIODetail detail = new AudioIODetail();
            if (jo["type"] != null)
            {
                try
                {
                    var enumString = jo["type"].Value<string>();
                    detail.type = (AudioConnections)Enum.Parse(typeof(AudioConnections), enumString, false);
                }
                catch (Exception)
                {
                    detail.type = AudioConnections.Unknown;
                }
            }
            if (jo["connector"] != null)
            {
                try
                {
                    var enumString = jo["connector"].Value<string>();
                    detail.connector = (AudioConnectionTypes)Enum.Parse(typeof(AudioConnectionTypes), enumString, false);
                }
                catch (Exception)
                {
                    detail.connector = AudioConnectionTypes.Unknown;
                }
            }
            if (jo["description"] != null)
            {
                try
                {
                    detail.description = jo["description"].Value<string>();
                }
                catch (Exception)
                {
                    detail.description = string.Empty;
                }
            }
            return detail;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}