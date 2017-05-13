using MapAccounts.Comparers;
using MapAccounts.Extensions;
using MapAccounts.Models.Maps;
using MapAccounts.Models.Maps.OSM;
using MapAccounts.Models.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapAccounts.Managers
{
    public class MapManager
    {
        private static ICollection<IMapMiner> MapMiners { get; set; }
        private static MapManager instance = null;

        private MapManager()
        {
            MapMiners = new List<IMapMiner>()
            { new OSMMiner() };
        }

        public static MapManager getInstance()
        {
            if (instance == null) instance = new MapManager();
            return instance;
        }

        public async Task<IEnumerable<StreetDTO>> getStreetsInRegion(Bound regions)
        {
            //TODO: Implementar um meio de mesclar informações de diferentes fontes
            List<StreetDTO> ret = new List<StreetDTO>();
            foreach (var miner in MapMiners)
            {
                var Streets = await miner.getStreets(regions);
                ret.AddRange(Streets);
            }
            return ret.Distinct(new StreetComparer());
        }

        public async Task<List<AmenityDTO>> getAmenitiesInRegion(String type, Bound regions)
        {
            //TODO: Implementar um meio de mesclar informações de diferentes fontes
            List<AmenityDTO> ret = new List<AmenityDTO>();
            foreach (var miner in MapMiners)
            {
                var Amenities = await miner.getAmenities(regions, StringToEnumParser.ParseEnum<AmenityType>(type));
                ret.AddRange(Amenities);
            }
            //return ret.Distinct(new StreetComparer());
            return ret;
        }

    }
}