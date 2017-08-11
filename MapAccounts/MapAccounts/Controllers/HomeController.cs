using MapAccounts.Managers;
using MapAccounts.Models;
using MapAccounts.Models.Primitives;
using MapAccounts.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MapAccounts.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }

        //[Authorize]
        //public ActionResult UserIndex(int regionId)
        //{
        //    var region = db.RegionModel.Find(regionId);
        //    RegionDTO regionDTO = new RegionDTO(region);
        //    return View("Index", regionDTO);
        //}

        //public async Task<ActionResult> SaveSection(RegionDTO _Region)
        //{
        //    var Logged = this.IsUserLogged(_Region);
        //    if (Logged != null) return Logged;
        //    RegionDTO sessionRegion = ((RegionDTO)Session["Model"] ?? _Region);
        //    RegionModel region = null;
        //    if (sessionRegion.ID > -1) region = await db.RegionModel.FindAsync(sessionRegion.ID);
        //    if (region != null)
        //    {
        //        region.Bounds = sessionRegion.Bounds;
        //        db.Entry(region).State = System.Data.Entity.EntityState.Modified;
        //    }
        //    else
        //    {
        //        region = new RegionModel()
        //        {
        //            Bounds = sessionRegion.Bounds
        //        };
        //        ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
        //        region.ApplicationUserID = user.Id;
        //        region.StreetModel = new List<StreetModel>();
        //        foreach (var street in sessionRegion.StreetDTO)
        //        {
        //            var dbStreet = db.StreetModel.FirstOrDefault(p => p.Name.Equals(street.Name));
        //            if (dbStreet != null)
        //            {
        //                region.StreetModel.Add(dbStreet);
        //            }
        //            else
        //            {
        //                StreetModel streetmodel = new StreetModel();
        //                streetmodel.Name = street.Name;
        //                streetmodel.StreetTrechosModel = (ICollection<ICollection<StreetPointModel>>)new List<List<StreetPointModel>>();
        //                foreach (var trecho in street.Trechos)
        //                {
        //                    List<StreetPointModel> streetModelTrecho = new List<StreetPointModel>();
        //                    streetmodel.StreetTrechosModel.Add(streetModelTrecho);
        //                    foreach (var point in trecho)
        //                    {
        //                        StreetPointModel streetpoint = new StreetPointModel()
        //                        {
        //                            lat = point.lat,
        //                            lng = point.lng,
        //                            StreetModel = streetmodel
        //                        };
        //                        if (point.PanoramaDTO != null && point.PanoramaDTO.pano != null)
        //                            streetpoint.GSPanorama = new GSPanorama(point.PanoramaDTO);

        //                        streetModelTrecho.Add(streetpoint);
        //                    }
        //                }
        //                region.StreetModel.Add(streetmodel);
        //            }
        //        }

        //        db.RegionModel.Add(region);
        //        db.Entry(region).State = System.Data.Entity.EntityState.Added;
        //    }
        //    db.SaveChanges();
        //    sessionRegion.ID = region.ID;
        //    Task.Run(() =>
        //    {
        //        var imageManager = ImageManager.getInstance();
        //        for (var i = 0; i < sessionRegion.StreetDTO.Count; i++)
        //        {
        //            String streetName = sessionRegion.StreetDTO[i].Name;
        //            var dbStreet = db.StreetModel.FirstOrDefault(p => p.Name.Equals(streetName));
        //            imageManager.getStreetPictures(ref dbStreet);
        //            db.SaveChanges();
        //        }
        //    });
        //    return View("Index", sessionRegion);
        //}

        public ActionResult About()
        {

            ViewBag.Message = Global.PJT_NAME;
            //ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}