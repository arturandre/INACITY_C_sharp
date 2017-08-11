using MapAccounts.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace MapAccounts.Models.Maps.GeoSampa
{
    public class GeoSampaMiner
    {
        private static String DataFilePath = PathMap.MapPath(@"~/Models/Maps/GeoSampa/Data/SAD69-96_SHP_pontoonibus.csv");
        private const double offsetLng = -0.0004087117614;
        private const double offsetLat = -0.0004538259751;
        public List<BusStopNode> busStopNodes { get; set; }
        public GeoSampaMiner()
        {
            if (busStopNodes == null || busStopNodes.Count == 0)
            {
                busStopNodes = new List<BusStopNode>();
                using (StreamReader text = new StreamReader(GeoSampaMiner.DataFilePath))
                {
                    String line = "";
                    while (true)
                    {
                        line = text.ReadLine();
                        if (String.IsNullOrEmpty(line)) break;
                        String[] values = line.Split(',');
                        BusStopNode bs = new BusStopNode();
                        bs.X = Double.Parse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture) + offsetLng;
                        bs.Y = Double.Parse(values[1]) + offsetLat;
                        bs.pt_nome = values[2];
                        bs.pt_enderec = values[3];
                        bs.pt_descric = values[4];
                        busStopNodes.Add(bs);
                    }
                }
            }
        }
    }
}