using MapAccounts.Models.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MapAccounts.Models.Primitives.Converters
{
    public class RegionDTOConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(RegionDTO));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var region = (RegionDTO)value;
            writer.Formatting = Formatting.Indented;
            writer.WriteStartObject();

            writer.WritePropertyName("Bounds");
            writer.WriteStartObject();
            writer.WritePropertyName("South");
            writer.WriteValue(region.Bounds.South);
            writer.WritePropertyName("West");
            writer.WriteValue(region.Bounds.West);
            writer.WritePropertyName("North");
            writer.WriteValue(region.Bounds.North);
            writer.WritePropertyName("East");
            writer.WriteValue(region.Bounds.East);
            writer.WriteEndObject();

            writer.WritePropertyName("streets");
            writer.WriteStartArray();

            writer.WritePropertyName("Segments");
            writer.WriteStartArray();

            writer.WriteEnd();
            
            writer.WriteEnd();

            writer.WriteEndObject();

        }
    }
}