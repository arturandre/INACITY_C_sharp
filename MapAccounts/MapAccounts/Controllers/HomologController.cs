using Emgu.CV;
using Emgu.CV.Structure;
using MapAccounts.ComputerVision.ImageProcessing;
using MapAccounts.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MapAccounts.Controllers
{
    [RoutePrefix("api/Homolog")]
    public class HomologController : Controller
    {
        //[Route("bgr2hsl")]
        //[HttpPost]
        //public async Task<HttpResponseMessage> bgr2hsl()
        //{
        //    // Check if the request contains multipart/form-data.
        //    //if (!Request.Content.IsMimeMultipartContent())
        //    //{
        //    //    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //    //}

        //    string root = HttpContext.Current.Server.MapPath("~/App_Data");
        //    var provider = new MultipartFormDataStreamProvider(root);

        //    try
        //    {
        //        // Read the form data.
        //        await Request.Content.ReadAsMultipartAsync(provider);

        //        var image_filename = provider.FileData.First().LocalFileName;

        //         var image = new Image<Bgr, byte>(image_filename);
        //        var xpto = Bgr2HslConverter.Bgr2Hsl(image);

        //        using (MemoryStream ms = new MemoryStream(xpto.Bytes))
        //        {
        //            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
        //            result.Content = new ByteArrayContent(ms.ToArray());
        //            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        //            return result;
        //        }

        //    }
        //    catch (System.Exception e)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }

        //}


        [Route("treesFilter")]
        [HttpPost]
        public ActionResult treesFilter()
        {
            //string root = HttpContext.Current.Server.MapPath("~/App_Data");
            //var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                // Read the form data.
                //await Request.Content.ReadAsMultipartAsync(provider);

                //var image_filename = provider.FileData.First().LocalFileName;
                var image_web = Request.Files.Get(0);
                Bitmap image_bitmap = new Bitmap(Image.FromStream(Request.Files.Get(0).InputStream));
                

                var image = new Image<Bgr, byte>(image_bitmap);
                var xpto = new TreesFilter().MaskTest(image);
                //return xpto;
                string path = Server.MapPath("~/App_Data/temp.png");
                xpto.Save(path);
                return base.File(path, "image/png");
                //using (MemoryStream ms = new MemoryStream(xpto.Bytes))
                //{
                //    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                //    result.Content = new ByteArrayContent(ms.ToArray());
                //    result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                //    return base.File(xpto.Bytes, "image/png");
                //}

            }
            catch (System.Exception e)
            {
                return null;//Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }

        }
    }
}
