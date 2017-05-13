using System;
using System.Collections.Generic;

namespace MapAccounts.Models.Primitives
{
    public class StreetDTO
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public StreetDTO()
        {
            Trechos = new List<List<PointDTO>>();
        }

        public StreetDTO(StreetModel StreetModel)
        {
            Trechos = new List<List<PointDTO>>();
            this.ID = StreetModel.ID;
            this.Name = StreetModel.Name;
            foreach (var trecho in StreetModel.StreetTrechosModel)
            {
                var nt = new List<PointDTO>();
                foreach (var point in trecho)
                {
                    var np = new PointDTO(point);
                    if (point.GSPanorama != null)
                    {
                        var panoramaDTO = new PanoramaDTO(point.GSPanorama);
                        np.PanoramaDTO = panoramaDTO;
                    }

                    nt.Add(np);
                }
                Trechos.Add(nt);
            }
        }
        public int ID { get; set; }
        public String Name { get; set; }
        //public List<Segment> Segments { get; set; }
        public List<List<PointDTO>> Trechos { get; set; }
    }
}