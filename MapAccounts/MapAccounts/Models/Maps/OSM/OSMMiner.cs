using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using MapAccounts.Models.Primitives;
using System.Data.Entity.Spatial;
using System.Globalization;

namespace MapAccounts.Models.Maps.OSM
{
    public class OSMMiner : IMapMiner
    {
        private String overpassUrl = "http://overpass-api.de/api/interpreter?data=";

        public async Task<List<AmenityDTO>> getAmenities(Bound region, AmenityType type)
        {
            List<AmenityDTO> ret = new List<AmenityDTO>();
            OverpassQueryBuilder oqb = new OverpassQueryBuilder(region);
            var sampleQuery = "";
            //Caso de excessão para pontos de ônibus
            if (type.Equals(AmenityType.bus_station))
            {
                sampleQuery = oqb.busStopsOverpassQuery();
            }
            else
            {
                sampleQuery = oqb.amenityOverpassQuery(type);
            }
            try
            {

                string encodedURL = overpassUrl + HttpUtility.UrlEncode(sampleQuery);
                using (HttpClient client = new HttpClient())
                {

                    var responseString = await client.GetStringAsync(encodedURL);
                    OSMResult osm = JsonConvert.DeserializeObject<OSMResult>(responseString);
                    if (osm.GetNodesAsDictionary != null && osm.GetNodesAsDictionary.Count() > 0)
                    {
                        var nodes = osm.GetNodesAsDictionary;
                        foreach (var node in nodes)
                        {
                            ret.Add(new AmenityDTO()
                            {
                                lat = (float)node.Value.lat,
                                lng = (float)node.Value.lon,
                                name = node.Value.tags.ContainsKey("name") ? node.Value.tags["name"] : null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ret;
        }

        public async Task<List<StreetDTO>> getStreets(Bound region)
        {
            List<StreetDTO> ruasDetectadas = new List<StreetDTO>();
            OverpassQueryBuilder oqb = new OverpassQueryBuilder(region);
            var sampleQuery = oqb.highwayOverpassQuery();

            try
            {

                string encodedURL = overpassUrl + HttpUtility.UrlEncode(sampleQuery);
                using (HttpClient client = new HttpClient())
                {

                    var responseString = await client.GetStringAsync(encodedURL);
                    OSMResult osm = JsonConvert.DeserializeObject<OSMResult>(responseString);
                    if (osm.GetWaysAsDictionary != null && osm.GetWaysAsDictionary.Count() > 0)
                    {
                        foreach (var way in osm.GetWaysAsDictionary)
                        {
                            if (!way.Value.tags.ContainsKey("name") || String.IsNullOrWhiteSpace(way.Value.tags["name"]))
                            {
                                continue;
                            }
                            var name = way.Value.tags["name"];
                            var ruaExistente = ruasDetectadas.FirstOrDefault(p => p.Name.Equals(name));
                            var novoTrecho = new List<PointDTO>();
                            if (ruaExistente != null)
                            {
                                foreach (var idNode in way.Value.nodes)
                                {
                                    var ponto = osm.GetNodesAsDictionary[idNode];
                                    novoTrecho.Add(new PointDTO() { ID = idNode.ToString(), lat = (float)ponto.lat, lng = (float)ponto.lon});

                                }
                                ruaExistente.Trechos.Add(novoTrecho);
                                //ruaExistente.Trechos = ruaExistente.Trechos.Distinct(new PointComparer()).ToList();
                                continue;
                            }

                            StreetDTO rua = new StreetDTO();
                            rua.Name = name;
                            foreach (var idNode in way.Value.nodes)
                            {
                                var ponto = osm.GetNodesAsDictionary[idNode];
                                novoTrecho.Add(new PointDTO() { ID = idNode.ToString(), lat = (float)ponto.lat, lng = (float)ponto.lon});
                            }
                            rua.Trechos.Add(novoTrecho);
                            //rua.Trechos = rua.Trechos.Distinct(new PointComparer()).ToList();
                            ruasDetectadas.Add(rua);
                        }
                    }
                    
                    

                    return ruasDetectadas;
                }
            }
            catch (Exception ex)
            {
                return new List<StreetDTO>() { new StreetDTO() { Name = "Erro! " + ex.Message } };
            }
        }
    }
}