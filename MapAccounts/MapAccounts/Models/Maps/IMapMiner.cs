using MapAccounts.Models.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MapAccounts.Models.Maps
{
    public interface IMapMiner
    {
        Task<List<StreetDTO>> getStreets(Bound region);
        Task<List<AmenityDTO>> getAmenities(Bound region, AmenityType type);
    }
}