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
    public class MaintenanceController : Controller
    {
        IRepositoryBase<provincecode> provincecodes;
        IRepositoryBase<countrycode> countrycodes;

        public MaintenanceController(IRepositoryBase<provincecode> provincecodes,
            IRepositoryBase<countrycode> countrycodes)
        {
            this.provincecodes = provincecodes;
            this.countrycodes = countrycodes;
        }//end Constructor

        // GET: list with filter
        public ActionResult ProvinceIndex(string searchString)
        {
            var provincecode = provincecodes.GetAll();

            if (!String.IsNullOrEmpty(searchString))
            {
                provincecode = provincecode.Where(s => s.name.Contains(searchString));
            }

            return View(provincecode);
        }

        // GET: /Create Standard Solution
        public ActionResult Province1Create()
        {
            var provincecode = new provincecode();
            return View(provincecode);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Province1Create(provincecode provincecode)
        {
            provincecodes.Insert(provincecode);
            provincecodes.Commit();

            return RedirectToAction("ProvinceIndex");
        }

        // GET: /Create with ViewBag for Dropdown
        public ActionResult Province2Create()
        {
            //Viewbag for Dropdownlist
            var country = countrycodes.GetAll();
            ViewBag.Country = country.OrderBy(q => q.name);

            var provincecode = new provincecode();
            return View(provincecode);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Province2Create(provincecode provincecode)
        {
            provincecodes.Insert(provincecode);
            provincecodes.Commit();

            return RedirectToAction("ProvinceIndex");
        }


        // GET: /Create with jQuery for Dropbdown
        public ActionResult Province3Create()
        {
            var provincecode = new provincecode();
            return View(provincecode);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Province3Create(provincecode provincecode)
        {
            provincecodes.Insert(provincecode);
            provincecodes.Commit();

            return RedirectToAction("ProvinceIndex");
        }

        // Search results for autocomplete dropdown list
        public ActionResult SearchByNameResult(string query)
        {
            return Json(SearchByName(query).Select(c => new { label = c.Label, ID = c.ID }));
        }
        private List<SelectionOptionCode> SearchByName(string searchString)
        {
            //Return value
            List<SelectionOptionCode> results = new List<SelectionOptionCode>();
            if (searchString != null)
            {
                //Read country data
                var allCountryCodes = countrycodes.GetAll();

                //Filter by name and return a list of ID (Code) and Name
                allCountryCodes = allCountryCodes.Where(s => s.name.Contains(searchString));

                if (allCountryCodes != null)
                    foreach (countrycode country in allCountryCodes)
                    {
                        {
                            results.Add(new SelectionOptionCode(country.code, country.name));
                        }
                        if (results.Count() > 5)
                        {
                            results.Add(new SelectionOptionCode("", "..."));
                            return results;
                        }
                    };
            };
            return results;
        }


    }
}
