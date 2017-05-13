using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using MapAccounts.Models;
using Microsoft.AspNet.Identity;
using MapAccounts.Models.Primitives;

namespace MapAccounts.Controllers
{
    [Authorize]
    public class UserMapController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        

        // GET: UserMap
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var regionModel = db.RegionModel.Include(r => r.ApplicationUser).Where(p => p.ApplicationUserID == userId);
            var rmodelList = await regionModel.ToListAsync();
            var ret = rmodelList.Select(p => new RegionDTO(p)).ToList();
            return View(ret);
        }

        // GET: UserMap/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RegionModel regionModel = await db.RegionModel.FindAsync(id);
            if (regionModel == null)
            {
                return HttpNotFound();
            }
            return View(regionModel);
        }

        // GET: UserMap/Create
        public ActionResult Create()
        {
            ViewBag.ApplicationUserID = new SelectList(db.Users, "Id", "Email");
            return View();
        }

        // POST: UserMap/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,ApplicationUserID,Bounds")] RegionModel regionModel)
        {
            if (ModelState.IsValid)
            {
                db.RegionModel.Add(regionModel);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.ApplicationUserID = new SelectList(db.Users, "Id", "Email", regionModel.ApplicationUserID);
            return View(regionModel);
        }

        // GET: UserMap/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RegionModel regionModel = await db.RegionModel.FindAsync(id);
            if (regionModel == null)
            {
                return HttpNotFound();
            }
            ViewBag.ApplicationUserID = new SelectList(db.Users, "Id", "Email", regionModel.ApplicationUserID);
            return View(regionModel);
        }

        // POST: UserMap/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,ApplicationUserID,Bounds")] RegionModel regionModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(regionModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ApplicationUserID = new SelectList(db.Users, "Id", "Email", regionModel.ApplicationUserID);
            return View(regionModel);
        }

        // GET: UserMap/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RegionModel regionModel = await db.RegionModel.FindAsync(id);
            if (regionModel == null)
            {
                return HttpNotFound();
            }
            return View(regionModel);
        }

        // POST: UserMap/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            RegionModel regionModel = await db.RegionModel.FindAsync(id);
            db.RegionModel.Remove(regionModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
