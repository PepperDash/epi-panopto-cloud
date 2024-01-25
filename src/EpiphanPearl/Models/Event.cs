using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.PanoptoCloud.EpiphanPearl.Models
{
    public class Event
    {
        [JsonProperty("start")]
        [JsonConverter(typeof(SecondEpochConverter))]
        public DateTime Start { get; set; }

        [JsonProperty("finish")]
        [JsonConverter(typeof(SecondEpochConverter))]
        public DateTime Finish { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("extra_data")]
        public EventExtraData ExtraData { get; set; }
    }

    public class SecondEpochConverter : DateTimeConverterBase
    {
        private static DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(((DateTime)value - _epoch).TotalSeconds.ToString(CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) { return null; }
            return _epoch.AddSeconds((long)reader.Value);
        }
    }
}