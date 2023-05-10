using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json.Converters;
using Crestron.Panopto.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Crestron.Panopto.Common
{
    public class VideoOutputDetailConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(VideoOutputDetail));
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var detail = new VideoOutputDetail();
            if (jo["type"] != null)
            {
                try
                {
                    var enumString = jo["type"].Value<string>();
                    detail.type = (VideoConnections)Enum.Parse(typeof(VideoConnections), enumString, false);
                    
                }
                catch (Exception)
                {
                    detail.type = VideoConnections.Unknown;
                }
            }
            if (jo["connector"] != null)
            {
                try
                {
                    var enumString = jo["connector"].Value<string>();
                    detail.connector = (VideoConnectionTypes)Enum.Parse(typeof(VideoConnectionTypes), enumString, false);
                }
                catch (Exception)
                {
                    detail.connector = VideoConnectionTypes.Unknown;
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
            if (jo["friendlyName"] != null)
            {
                try
                {
                    detail.friendlyName = jo["friendlyName"].Value<string>();
                }
                catch (Exception)
                {
                    detail.friendlyName = string.Empty;
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