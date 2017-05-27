using MapAccounts.Models;
using MapAccounts.Models.Primitives;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MapAccounts.Controllers
{
    [RoutePrefix("api/DBHeatMap")]
    public class DBHeatMapController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost]
        [Route("GetFeaturesInRegion")]
        public IEnumerable<HeatMapPointDTO> GetFeaturesInRegion([FromBody] Bound region)
        {
            var poly = DbGeography.PolygonFromText("POLYGON((" +
                region.West + " " + region.North + "," +
                region.West + " " + region.South + "," +
                region.East + " " + region.South + "," +
                region.East + " " + region.North + "," +
                region.West + " " + region.North + "))", 4326);
            var points = db.HeatmapPointModel.Where(p => poly.Intersects(p.coordinates))
                .Select(p => new HeatMapPointDTO()
                {
                    location = new PointDTO()
                    {
                        ID = p.ID,
                        lat = p.coordinates.Latitude.Value,
                        lng = p.coordinates.Longitude.Value
                    },
                    CracksDensity = p.CracksDensity,
                    TreesDensity = p.TreesDensity
                });

            return points;
        }
    }
}
