using MapAccounts.Managers;
using MapAccounts.Models;
using MapAccounts.Models.Primitives;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web.Http;


namespace MapAccounts.Controllers
{
    using CaracteristicType = FilterResultDTO.CaracteristicType;
    [RoutePrefix("api/ImageFilter")]
    public class ImageFilterController : ApiController
    {
        //private ApplicationDbContext db = new ApplicationDbContext();
        [Route("DetectFeaturesInSequence/{filterType}")]
        [HttpPost]
        public IEnumerable<FilterResultDTO> DetectFeaturesInSequence([FromBody] IEnumerable<PictureDTO> pictures, String filterType)
        {
            if (pictures == null) return null;
            foreach (var p in pictures)
            {
                p.base64image = (new Models.Imagery.Google.GSMiner()).DownloadBase64ImageFromURI(p.imageURI);
            }
            pictures = pictures.Where(p => p.base64image != null);
            ImageFilterManager.getInstance().detectFeatureInGSSequence(ref pictures, (CaracteristicType)Enum.Parse(typeof(CaracteristicType), filterType));

            try
            {
                ResultsStoreManager storage = new ResultsStoreManager();
                storage.StoreHeatmapPoints(pictures, (CaracteristicType)Enum.Parse(typeof(CaracteristicType), filterType));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);                
            }
            

            return pictures.Where(p => p.base64image != null).Select(p => p.filterResults).SelectMany(p => p);
        }

        [Route("GenericFilterTest")]
        [HttpPost]
        public IEnumerable<FilterResultDTO> GenericFilter([FromBody] IEnumerable<PictureDTO> pictures)
        {
            List<FilterResultDTO> ret = new List<FilterResultDTO>();
            if (pictures == null) return ret;
            foreach (var p in pictures)
            {
                p.base64image = (new Models.Imagery.Google.GSMiner()).DownloadBase64ImageFromURI(p.imageURI);
                ret.Add(new FilterResultDTO()
                {
                    base64image = p.base64image,
                    imageID = p.imageID,
                    panoID = p.panoID,
                    Type = FilterResultDTO.CaracteristicType.Generic
                });
            }
            return ret;
        }
    }
}
