using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Data;
using IPMRVPark.Contracts.Repositories;

namespace IPMRVPark.WebUI.Controllers
{
    public class SiteSizeController : Controller
    {
        IRepositoryBase<sitesize> sitesizes;
        public SiteSizeController(IRepositoryBase<sitesize> sitesizes)
        {
            this.sitesizes = sitesizes;
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            var sitesize = sitesizes.GetAll().OrderBy(c => c.ID);

            if (!String.IsNullOrEmpty(searchString))
            {
                sitesize = sitesize.Where(s => s.description.Contains(searchString)).OrderBy(c => c.ID);
            }

            return View(sitesize);
        }

        // GET: /Details/5
        public ActionResult SiteSizeDetails(int? id)
        {
            var sitesize = sitesizes.GetById(id);
            if (sitesize == null)
            {
                return HttpNotFound();
            }
            return View(sitesize);
        }

        // GET: /Create
        public ActionResult CreateSiteSize()
        {
            var sitesize = new sitesize();
            return View(sitesize);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSiteSize(sitesize sitesize)
        {
            var _sitesize = new sitesize();
            _sitesize.description = sitesize.description;
            _sitesize.createDate = DateTime.Now;
            _sitesize.lastUpdate = DateTime.Now;
            sitesizes.Insert(_sitesize);
            sitesizes.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Edit/5
        public ActionResult EditSiteSize(int id)
        {
            sitesize sitesize = sitesizes.GetById(id);
            if (sitesize == null)
            {
                return HttpNotFound();
            }
            return View(sitesize);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSiteSize(sitesize sitesize)
        {
            var _sitesize = sitesizes.GetById(sitesize.ID);

            _sitesize.description = sitesize.description;
            _sitesize.lastUpdate = DateTime.Now;
            sitesizes.Update(_sitesize);
            sitesizes.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Delete/5
        public ActionResult DeleteSiteSize(int id)
        {
            sitesize sitesize = sitesizes.GetById(id);
            if (sitesize == null)
            {
                return HttpNotFound();
            }
            return View(sitesize);
        }

        [HttpPost, ActionName("DeleteSiteSize")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            sitesizes.Delete(sitesizes.GetById(id));
            sitesizes.Commit();
            return RedirectToAction("Index");
        }

    }
}
