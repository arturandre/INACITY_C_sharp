using MapAccounts.Models;
using MapAccounts.Models.Imagery;
using MapAccounts.Models.Imagery.Google;
using System.Collections.Generic;
using System.Linq;

namespace MapAccounts.Managers
{
    public class ImageManager
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private static ICollection<IImageMiner> ImageMiners { get; set; }
        private static ImageManager instance = null;

        private ImageManager()
        {
            ImageMiners = new List<IImageMiner>()
            { new GSMiner() };
        }

        public static ImageManager getInstance()
        {
            if (instance == null) instance = new ImageManager();
            return instance;
        }
        /*
        public void getStreetPictures(ref StreetDTO Street)
        {
            if (Street.Points[0].PanoramaDTO == null)
            {
                foreach (var miner in ImageMiners)
                {
                    miner.getImagesForPoints(Street.Points);
                }
            }

            
        }*/

        internal void getStreetPictures(ref StreetModel dbStreet)
        {
            //if (dbStreet.StreetPointModel.ElementAt(0).GSPanorama != null)
            {
                foreach (var miner in ImageMiners)
                {
                    miner.getImagesForPoints(dbStreet.StreetTrechosModel.SelectMany(p => p).ToList());
                }
            }
        }
    }
}