using MapAccounts.Extensions;
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
            String polygonQuery = "POLYGON((" +
                region.West.ToString(true) + " " + region.North.ToString(true) + "," +
                region.West.ToString(true) + " " + region.South.ToString(true) + "," +
                region.East.ToString(true) + " " + region.South.ToString(true) + "," +
                region.East.ToString(true) + " " + region.North.ToString(true) + "," +
                region.West.ToString(true) + " " + region.North.ToString(true) + "))";

            var poly = DbGeography.PolygonFromText(polygonQuery, 4326);
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
