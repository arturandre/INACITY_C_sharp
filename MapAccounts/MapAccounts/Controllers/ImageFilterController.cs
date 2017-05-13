using MapAccounts.Managers;
using MapAccounts.Models;
using MapAccounts.Models.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using static MapAccounts.Models.Primitives.FilterResultDTO;

namespace MapAccounts.Controllers
{
    [RoutePrefix("api/ImageFilter")]
    public class ImageFilterController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        [Route("DetectFeaturesInSequence/{filterType}")]
        [HttpPost]
        public IEnumerable<FilterResultDTO> DetectFeaturesInSequence([FromBody] IEnumerable<PictureDTO> pictures, String filterType)
        {
            if (pictures == null) return null;
            foreach (var p in pictures)
            {
                p.base64image = (new Models.Imagery.Google.GSMiner()).DownloadBase64ImageFromURI(p.imageURI);
           }
            ImageFilterManager.getInstance().detectFeatureInGSSequence(ref pictures, (CaracteristicType)Enum.Parse(typeof(CaracteristicType), filterType));

            return pictures.Select(p => p.filterResults).SelectMany(p => p);
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
                    imageID = p.ID,
                    Type = FilterResultDTO.CaracteristicType.Generic
                });
            }
            return ret;
        }
    }
}
