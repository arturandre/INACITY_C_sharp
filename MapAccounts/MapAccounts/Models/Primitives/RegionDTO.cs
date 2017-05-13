using System.Collections.Generic;
using System.Linq;

namespace MapAccounts.Models.Primitives
{
    public class RegionDTO
    {
        public RegionDTO() { }
        public RegionDTO(RegionModel region)
        {
            this.ID = region.ID;
            this.Bounds = region.Bounds;
            this.StreetDTO = region.StreetModel
                .Select(p =>
                new StreetDTO()
                {
                    Name = p.Name,
                    Trechos = p.StreetTrechosModel.Select(
                        q => q.Select(
                            r => new PointDTO()
                            {
                                lat = r.lat,
                                lng = r.lng
                            }).ToList()).ToList()
                }).ToList();
            //this.StreetDTO =
            //    region.StreetModel.
            //    Select(p => new StreetDTO()
            //    {
            //        Name = p.Name,
            //        Trechos = p.StreetTrechosModel.SelectMany(p => p).Select(r =>
            //            new PointDTO() { lat = r.lat, lng = r.lng }).ToList()
            //    }).ToList();
        }

        public int ID { get; set; }
        public Bound Bounds { get; set; }
        public PointDTO getCenter()
        {
            return (new PointDTO()
            {
                lat = (Bounds.North + Bounds.South) / 2.0,
                lng = (Bounds.East + Bounds.West) / 2.0
            });
        }
        public List<StreetDTO> StreetDTO { get; set; }
    }
}