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
        private static Dictionary<ImageProvider, IImageMiner> ImageMiners { get; set; }
        private static ImageManager instance = null;

        private ImageManager()
        {
            ImageMiners = new Dictionary<ImageProvider, IImageMiner>();
            ImageMiners.Add(ImageProvider.Google, new GSMiner());
            //{ ImageProvider.Google, new GSMiner() };
        }

        public static ImageManager getInstance()
        {
            if (instance == null) instance = new ImageManager();
            return instance;
        }

        public static IImageMiner getImageMiner(ImageProvider imgProvider)
        {
            if (ImageMiners.ContainsKey(imgProvider))
            {
                return ImageMiners[imgProvider];
            }
            return null;
            
        }
    }
}