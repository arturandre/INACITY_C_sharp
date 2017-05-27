using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MapAccounts.Models.Primitives;
using System.Globalization;
using System.Data.Entity.Spatial;
using MapAccounts.Models;

namespace MapAccounts.Managers
{
    public class ResultsStoreManager
    {
        internal void StoreHeatmapPoints(IEnumerable<PictureDTO> pictures, FilterResultDTO.CaracteristicType filterType)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            foreach (var picture in pictures.GroupBy(x => x.panoID).Select(g => g.First()).ToList())
            {
                var lat = picture.location.lat.ToString(CultureInfo.InvariantCulture);
                var lon = picture.location.lng.ToString(CultureInfo.InvariantCulture);
                var geo = DbGeography.PointFromText(string.Format("POINT({0} {1})", lon, lat), 4326);
                var density = (float?)(picture.filterResults.FirstOrDefault(r => r.Type == filterType).Density);
                var newPoint = new HeatmapPointModel()
                {
                    ID = picture.panoID,
                    coordinates = geo,
                    TreesDensity = filterType == FilterResultDTO.CaracteristicType.Trees ? density : null,
                    CracksDensity = filterType == FilterResultDTO.CaracteristicType.Cracks ? density : null
                };
                var oldPoint = db.HeatmapPointModel.FirstOrDefault(p => p.ID == newPoint.ID);
                if (oldPoint != null)
                {
                    oldPoint.TreesDensity = newPoint.TreesDensity ?? oldPoint.TreesDensity;
                    oldPoint.CracksDensity = newPoint.CracksDensity ?? oldPoint.TreesDensity;
                    db.Entry<HeatmapPointModel>(oldPoint).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    db.HeatmapPointModel.Add(newPoint);
                }
            }
            db.SaveChanges();
            db.Dispose();
        }

    }
}