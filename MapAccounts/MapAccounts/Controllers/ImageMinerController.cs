using MapAccounts.Models;
using MapAccounts.Models.Imagery;
using MapAccounts.Models.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace MapAccounts.Controllers
{
    [RoutePrefix("api/ImageMiner")]
    public class ImageMinerController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost]
        [Route("ImagesFromStreet")]
        public IEnumerable<PictureDTO> GetImagesFromStreet([FromBody] StreetDTO Street, ImageProvider imageProvider = ImageProvider.Google)
        {
            var panoramaPoints = Street.Trechos.SelectMany(p => p).Where(p => p.PanoramaDTO != null && p.PanoramaDTO.pano != null);

            List<PictureDTO> Pictures = new List<PictureDTO>();

            foreach (var picNotNull in panoramaPoints)
            {
                var auxPic = picNotNull.PanoramaDTO.Pictures.ElementAt(0);
                auxPic.location = new PointDTO() { ID = picNotNull.ID, lat = picNotNull.lat, lng = picNotNull.lng };
                Pictures.Add(auxPic);
            }

            foreach (var picture in Pictures)
            {
                try
                {
                    IImageMiner imageMiner = null;
                    switch (imageProvider)
                    {
                        case ImageProvider.Google:
                            imageMiner = new Models.Imagery.Google.GSMiner();
                            break;
                        default:
                            imageMiner = new Models.Imagery.Google.GSMiner();
                            break;
                    }

                    picture.base64image = imageMiner.getImageBase64(picture.imageURI);
                    //(new Models.Imagery.Google.GSMiner()).DownloadBase64ImageFromURI();
                }
                catch (Exception)
                {
                    picture.base64image = null;
                }
                
            }
            Pictures.RemoveAll(p => p.base64image == null);
            return Pictures;
        }

        //[Authorize]
        //[HttpPost]
        //[Route("ImagesFromStreetDB")]
        //public StreetDTO GetImagesFromStreetDB(StreetDTO street)
        //{
        //    var streetName = street.Name;
        //    var streetModel = db.StreetModel.FirstOrDefault(s => s.Name.Equals(streetName));
        //    if (streetModel != null)
        //    {
        //        return new StreetDTO(streetModel);
        //    }

        //    return null;
        //}
    }
}
