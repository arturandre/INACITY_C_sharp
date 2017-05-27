using System;
using System.Collections.Generic;
using System.Linq;
using MapAccounts.Extensions;
using System.Net.Http;

namespace MapAccounts.Models.Imagery.Google
{
    public class GSMiner : IImageMiner
    {
        private String gsURL = "https://maps.googleapis.com/maps/api/streetview";
        private String key = System.Configuration.ConfigurationManager.AppSettings["googlestreet:key"];

        private static double Deg2Rad(double deg)
        {
            return (deg * Math.PI) / 180.0;
        }
        private static double Rad2Deg(double rad)
        {
            return (rad * 180.0) / Math.PI;
        }

        /// <summary>
        /// Ref: http://www.sunearthtools.com/tools/distance.php#contents
        /// </summary>
        /// <param name="A">Point A</param>
        /// <param name="B">Point B</param>
        /// <returns>Angle between two points in the sphere</returns>
        private static double AngleBetweenPoints(Vector2D A, Vector2D B)
        {
            var latA = GSMiner.Deg2Rad(A.Y);
            var lngA = GSMiner.Deg2Rad(A.X);
            var latB = GSMiner.Deg2Rad(B.Y);
            var lngB = GSMiner.Deg2Rad(B.X);
            var phi = Math.Log(Math.Tan((latB / 2.0) + (Math.PI / 4)) / Math.Tan((latA / 2.0) + (Math.PI / 4)));
            var lon = Math.Abs(lngA - lngB);
            /*Rolamento*/
            var theta = Math.Atan2(lon, phi);
            return (theta * 180.0) / Math.PI;
        }

        private static double OldAngleBetweenPoints(Vector2D vectorA, Vector2D vectorB)
        {
            var vectorNorth = new Vector2D(0, 1);
            var vectorAB = vectorB - vectorA;
            var estimatedAngle = (180.0 * Vector2D.angleBetween(vectorNorth, vectorAB)) / (Math.PI);
            return 360.0 - estimatedAngle;
        }

        public void getImagesForPoints(ICollection<StreetPointModel> points)
        {
            for (int i = 0; i < points.Count() - 1; i++)
            {
                var point = points.ElementAt(i);
                if (point.GSPanorama == null) continue;
                var nextPoint = points.ElementAt(i + 1);
                var vectorA = new Vector2D(point.lng, point.lat);
                var vectorB = new Vector2D(nextPoint.lng, nextPoint.lat);
                //if (point.GSPanorama.GSPicture != null && point.GSPanorama.GSPicture.Count > 0 && Math.Abs(point.GSPanorama.frontAngle - estimatedAngle) < 0.00000001) continue;
                //point.GSPanorama.frontAngle = GSMiner.OldAngleBetweenPoints(vectorA, vectorB);
                point.GSPanorama.frontAngle = GSMiner.AngleBetweenPoints(vectorA, vectorB);
                if (point.GSPanorama.GSPicture == null) point.GSPanorama.GSPicture = new List<GSPicture>();
                var pano = point.GSPanorama.panoID;
                if (pano != null && pano.Length == 22)
                {
                    GSQueryBuilder querybuilder = new GSQueryBuilder(pano, 640, 640, point.GSPanorama.frontAngle, point.GSPanorama.pitch, key);
                    var picture = new GSPicture();

                    string finalURL = gsURL + querybuilder.getQueryPanoId();
                    picture.imageURI = DownloadBase64ImageFromURI(finalURL);

                    //picture.imageURI = "Testando";
                    picture.heading = point.GSPanorama.frontAngle;
                    point.GSPanorama.GSPicture.Add(picture);
                }
            }
            var lastPoint = points.ElementAt(points.Count - 1);
            if (lastPoint.GSPanorama == null) return;
            var secondToLastPoint = points.LastOrDefault(p => p.GSPanorama != null && p != lastPoint);
            if (secondToLastPoint == null) return;
            var lastAngle = secondToLastPoint.GSPanorama.frontAngle;
            if (lastPoint.GSPanorama.GSPicture == null) lastPoint.GSPanorama.GSPicture = new List<GSPicture>();
            lastPoint.GSPanorama.frontAngle = lastAngle;
            var lastPano = lastPoint.GSPanorama.panoID;
            if (lastPano != null)
            {
                GSQueryBuilder querybuilder = new GSQueryBuilder(lastPano, 640, 640, lastPoint.GSPanorama.frontAngle, lastPoint.GSPanorama.pitch, key);
                var picture = new GSPicture();

                string finalURL = gsURL + querybuilder.getQueryPanoId();
                picture.imageURI = DownloadBase64ImageFromURI(finalURL);

                picture.heading = lastPoint.GSPanorama.frontAngle;
                lastPoint.GSPanorama.GSPicture.Add(picture);
            }
        }

        public string DownloadBase64ImageFromURI(String Uri)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = client.GetByteArrayAsync(Uri).Result;
                    return Convert.ToBase64String(response);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        /*
        public void getImagesForPoints(IEnumerable<PointDTO> points)
        {
            for (int i = 0; i < points.Count() - 1; i++)
            {
                var point = points.ElementAt(i);
                var nextPoint = points.ElementAt(i + 1);
                var vectorA = new Vector2D(point.lng, point.lat);
                var vectorB = new Vector2D(nextPoint.lng, nextPoint.lat);
                var vectorNorth = new Vector2D(0, 1);
                var vectorAB = vectorB - vectorA;
                var estimatedAngle = (180.0 * Vector2D.angleBetween(vectorNorth, vectorAB)) / (Math.PI);
                point.PanoramaDTO = point.PanoramaDTO ?? new PanoramaDTO();
                point.PanoramaDTO.frontAngle = estimatedAngle;
                point.PanoramaDTO.Pictures = new List<PictureDTO>();
                var pano = point.PanoramaDTO.pano;
                if (pano != null)
                {
                    GSQueryBuilder querybuilder = new GSQueryBuilder(pano, 640, 640, point.PanoramaDTO.frontAngle, point.PanoramaDTO.pitch, key);
                    using (HttpClient client = new HttpClient())
                    {
                        var picture = new PictureDTO();
                        
                        string finalURL = gsURL + querybuilder.getQueryPanoId();
                        var response = client.GetByteArrayAsync(finalURL).Result;
                        picture.base64image = Convert.ToBase64String(response);
                        
                        picture.heading = point.PanoramaDTO.frontAngle;
                        point.PanoramaDTO.Pictures.Add(picture);
                    }
                }
            }
        }*/
    }
}