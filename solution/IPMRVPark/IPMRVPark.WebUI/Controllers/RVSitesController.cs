using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Data;

namespace IPMRVPark.WebUI.Controllers
{
    public class RVSitesController : Controller
    {
        private DataContext db = new DataContext();

        //
        // GET: /RVSites/

        public ActionResult Index()
        {
            return View(db.rvsite_coord_view.ToList());
        }

        //
        // GET: /RVSites/Details/5

        public ActionResult Details(string id = null)
        {
            rvsite_coord_view rvsite_coord_view = db.rvsite_coord_view.Find(id);
            if (rvsite_coord_view == null)
            {
                return HttpNotFound();
            }
            return View(rvsite_coord_view);
        }

        //
        // GET: /RVSites/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /RVSites/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(rvsite_coord_view rvsite_coord_view)
        {
            if (ModelState.IsValid)
            {
                db.rvsite_coord_view.Add(rvsite_coord_view);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(rvsite_coord_view);
        }

        //
        // GET: /RVSites/Edit/5

        public ActionResult Edit(string id = null)
        {
            rvsite_coord_view rvsite_coord_view = db.rvsite_coord_view.Find(id);
            if (rvsite_coord_view == null)
            {
                return HttpNotFound();
            }
            return View(rvsite_coord_view);
        }

        //
        // POST: /RVSites/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(rvsite_coord_view rvsite_coord_view)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rvsite_coord_view).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(rvsite_coord_view);
        }

        //
        // GET: /RVSites/Delete/5

        public ActionResult Delete(string id = null)
        {
            rvsite_coord_view rvsite_coord_view = db.rvsite_coord_view.Find(id);
            if (rvsite_coord_view == null)
            {
                return HttpNotFound();
            }
            return View(rvsite_coord_view);
        }

        //
        // POST: /RVSites/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            rvsite_coord_view rvsite_coord_view = db.rvsite_coord_view.Find(id);
            db.rvsite_coord_view.Remove(rvsite_coord_view);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}