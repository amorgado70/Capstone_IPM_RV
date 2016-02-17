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
    public class CountryController : Controller
    {

        IRepositoryBase<countrycode> countrycodes;

        public CountryController(IRepositoryBase<countrycode> countrycodes)
        {
            this.countrycodes = countrycodes;
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            var countrycode = countrycodes.GetAll();

            if (!String.IsNullOrEmpty(searchString))
            {
                countrycode = countrycode.Where(s => s.name.Contains(searchString));
            }

            return View(countrycode);
        }

        // GET: /Details/5
        public ActionResult Details(int? id)
        {
            var countrycode = countrycodes.GetById(id);
            if (countrycode == null)
            {
                return HttpNotFound();
            }
            return View(countrycode);
        }

        // GET: /Create
        public ActionResult Create()
        {
            var countrycode = new countrycode();
            return View(countrycode);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(countrycode countrycode)
        {
            countrycodes.Insert(countrycode);
            countrycodes.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Edit/5
        public ActionResult Edit(int id)
        {
            countrycode countrycode = countrycodes.GetById(id);
            if (countrycode == null)
            {
                return HttpNotFound();
            }
            return View(countrycode);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(countrycode countrycode)
        {
            countrycodes.Update(countrycode);
            countrycodes.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Delete/5
        public ActionResult Delete(int id)
        {
            countrycode countrycode = countrycodes.GetById(id);
            if (countrycode == null)
            {
                return HttpNotFound();
            }
            return View(countrycode);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            countrycodes.Delete(countrycodes.GetById(id));
            countrycodes.Commit();
            return RedirectToAction("Index");
        }

    }
}
