using Emgu.CV;
using Emgu.CV.Structure;
using MapAccounts.Models.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace TreeFilterMicroService.Controllers
{
    public class TreeFilterController : ApiController
    {
        public FilterResultDTO FilterTree([FromBody] PictureDTO picture)
        {
            var bmp = picture.getImage();
            var img = new Image<Bgr, byte>(bmp);

            var ret = new FilterResultDTO();
            ret.

            return null;
        }
        public IEnumerable<FilterResultDTO> FilterTree([FromBody] IEnumerable<PictureDTO> pictures)
        {
            return null;
        }
    }
}
