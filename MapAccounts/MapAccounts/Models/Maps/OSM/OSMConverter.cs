using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MapAccounts.Models.Maps.OSM
{
    public class OSMConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(OSMResult));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            OSMResult osmresult = new OSMResult();
            osmresult.version = (string)jo["version"];
            osmresult.generator = (string)jo["generator"];
            osm3s osm3s = jo["osm3s"].ToObject<osm3s>();
            osmresult.nodes = new List<node>();
            osmresult.ways = new List<way>();
            foreach(JObject obj in jo["elements"])
            {
                String type = (string)obj["type"];
                switch(type)
                {
                    case "node":
                        osmresult.nodes.Add(obj.ToObject<node>());
                        break;
                    case "way":
                        osmresult.ways.Add(obj.ToObject<way>());
                        break;
                    default:
                        break;
                }
            }
            return osmresult;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}