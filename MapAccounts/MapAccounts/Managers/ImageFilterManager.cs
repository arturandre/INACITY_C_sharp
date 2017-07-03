using Emgu.CV;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV.Structure;
using MapAccounts.Models;
using MapAccounts.Models.Primitives;
using MapAccounts.ComputerVision.ImageProcessing;

namespace MapAccounts.Managers
{
    public class ImageFilterManager
    {
        private static ICollection<ImageFilter> ImageFilters { get; set; }
        private static ImageFilterManager instance = new ImageFilterManager();
        public static ImageFilterManager getInstance() { return instance; }

        private ImageFilterManager()
        {
            ImageFilterManager.ImageFilters = new List<ImageFilter>();
            ImageFilterManager.ImageFilters.Add(new TreesFilter());
            ImageFilterManager.ImageFilters.Add(new CrackFilter());
        }

        public void detectFeatureInSequence(ref IEnumerable<PictureDTO> pictures, FilterResultDTO.CaracteristicType filterType)
        {
            var filter = ImageFilters.FirstOrDefault(p => p.FilterType.Equals(filterType));
            if (filter == null)
                return;
            foreach (var picture in pictures)
            {
                var pictureBitmap = picture.getImage();
                var img = new Image<Bgr, byte>(pictureBitmap);
                var result = filter.filterImage(img);

                picture.filterResults.Add(result);

                img.Dispose();
                pictureBitmap.Dispose();
            }
        }

        internal IEnumerable<PictureDTO> detectFeatureInGSSequence(IEnumerable<GSPicture> gspictures, FilterResultDTO.CaracteristicType filterType)
        {
            var pictures = gspictures.Select(p => new PictureDTO(p));
            detectFeatureInGSSequence(ref pictures, filterType);
            return pictures;
        }
        internal void detectFeatureInGSSequence(ref IEnumerable<PictureDTO> pictures, FilterResultDTO.CaracteristicType filterType)
        {
            var filter = ImageFilters.FirstOrDefault(p => p.FilterType.Equals(filterType));
            if (filter == null)
                return;
            //SQL_GIS_T1Entities gist1 = new SQL_GIS_T1Entities();
            foreach (var picture in pictures)
            {
                var pictureBitmap = picture.getImage();
                var img = new Image<Bgr, byte>(pictureBitmap);
                var result = filter.filterImage(img);
                result.imageID = picture.imageID;
                result.panoID = picture.panoID;
                if (picture.filterResults == null) picture.filterResults = new List<FilterResultDTO>();
                picture.filterResults.Add(result);
                //var lat = picture.location.lat.ToString(CultureInfo.InvariantCulture);
                //var lon = picture.location.lng.ToString(CultureInfo.InvariantCulture);
                //var geo = DbGeography.PointFromText(string.Format("POINT({0} {1})", lon, lat), 4326);
                //var newPoint = new Point()
                //{
                //    idNode = picture.panoID,
                //    coordinate = geo,
                //    treesDensity = filterType == FilterResultDTO.CaracteristicType.Trees ? result.Density : null,
                //    cracksDensity = filterType == FilterResultDTO.CaracteristicType.Cracks ? result.Density : null
                //};
                //var oldPoint = gist1.Point.FirstOrDefault(p => p.idNode == newPoint.idNode);
                //if (oldPoint != null)
                //{
                //    oldPoint.treesDensity = newPoint.treesDensity ?? oldPoint.treesDensity;
                //    oldPoint.cracksDensity = newPoint.cracksDensity ?? oldPoint.treesDensity;
                //    gist1.Entry<Point>(oldPoint).State = System.Data.Entity.EntityState.Modified;
                //}
                //else
                //{
                //    gist1.Point.Add(newPoint);
                //}


                img.Dispose();
                pictureBitmap.Dispose();
            }
            //gist1.SaveChanges();
            //gist1.Dispose();
        }
    }
}