using MapAccounts.Managers;
using MapAccounts.Models;
using MapAccounts.Models.Primitives;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace MapAccounts.Controllers
{
    [RoutePrefix("api/MapMiner")]
    public class MapMinerController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Route("StreetsInRegion")]
        [HttpPost]
        public async Task<IEnumerable<StreetDTO>> GetStreetsInRegion([FromBody] Bound region)
        {
            return await MapManager.getInstance().getStreetsInRegion(region);
        }

        [Route("AmenitiesInRegion/{type}")]
        [HttpPost]
        public async Task<List<AmenityDTO>> getAmenity(String type, [FromBody]Bound region)
        {
            if (region.East <= -46.1854091776976 &&
                region.West >= -46.9862327944605 &&
                region.North <= -23.1953866709405 &&
                region.South >= -23.9111536551291 
                && type == "bus_station")
            {
                Models.Maps.GeoSampa.GeoSampaMiner miner = new Models.Maps.GeoSampa.GeoSampaMiner();
                var ret = new List<AmenityDTO>();
                foreach (var bs in miner.busStopNodes)
                {
                    if (bs.X <= region.East &&
                bs.X >= region.West &&
                bs.Y >= region.South &&
                bs.Y <= region.North)
                        ret.Add(new AmenityDTO()
                    {
                        ID = -1,
                        lat = bs.Y,
                        lng = bs.X,
                        name = "Nome: "+  bs.pt_nome + "</br>" + "Endereço: " + bs.pt_enderec + "</br>" + "Descrição: " + bs.pt_descric,
                        address = bs.pt_nome + "," + bs.pt_enderec,
                        PanoramaDTO = null
                    });
                }
                return ret;
            }
            return await MapManager.getInstance().getAmenitiesInRegion(type, region);

        }

        [Route("SPBusStationsTest")]
        [HttpGet]
        public Object getAmenitySPBusStation()
        {
            Models.Maps.GeoSampa.GeoSampaMiner miner = new Models.Maps.GeoSampa.GeoSampaMiner();
            return miner.busStopNodes;

        }
    }
}
