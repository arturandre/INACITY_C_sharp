using MapAccounts.Models;
using MapAccounts.Models.Imagery;
using MapAccounts.Models.Imagery.Google;
using System.Collections.Generic;
using System.Linq;

namespace MapAccounts.Managers
{
    public class ImageMinerManager
    {
        //private ApplicationDbContext db = new ApplicationDbContext();
        private static Dictionary<ImageProvider, IImageMiner> ImageMiners { get; set; }
        private static ImageMinerManager instance = null;

        private ImageMinerManager()
        {
            ImageMiners = new Dictionary<ImageProvider, IImageMiner>();
            ImageMiners.Add(ImageProvider.Google, new GSMiner());
            //{ ImageProvider.Google, new GSMiner() };
        }

        public static ImageMinerManager getInstance()
        {
            if (instance == null) instance = new ImageMinerManager();
            return instance;
        }

        public IImageMiner getImageMiner(ImageProvider imgProvider)
        {
            if (ImageMiners.ContainsKey(imgProvider))
            {
                return ImageMiners[imgProvider];
            }
            //TODO: Configurar heurística para determinar a plataforma de imageamento
            else
            {
                return ImageMiners.Values.FirstOrDefault();
            }
            
            
        }
    }
}