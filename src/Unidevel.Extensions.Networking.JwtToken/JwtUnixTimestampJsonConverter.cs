using System;
using Newtonsoft.Json;

namespace Unidevel.Extensions.Networking
{
    /// <summary>
    /// Used internally to convert UX timestamp dates according to JWT specification.
    /// </summary>
    class JwtUnixTimestampJsonConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var ts = serializer.Deserialize<long>(reader);

            return DateTimeOffset.FromUnixTimeSeconds(ts);
        }

        public override bool CanConvert(Type type) => typeof(DateTimeOffset).IsAssignableFrom(type);

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            var v = (DateTimeOffset)value;

            var ts = v.ToUnixTimeSeconds();
            writer.WriteRawValue(ts.ToString());
        }

        public override bool CanRead => true;

        public override bool CanWrite => true;
    }
}
