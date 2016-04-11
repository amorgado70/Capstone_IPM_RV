using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;
using System.Collections.Generic;

namespace IPMRVPark.WebUI.Controllers
{
    public class ProvinceController : Controller
    {
        IRepositoryBase<province_view> provinces_view;
        IRepositoryBase<provincecode> provincecodes;
        IRepositoryBase<countrycode> countrycodes;

        public ProvinceController(IRepositoryBase<province_view> provinces_view,
                IRepositoryBase<provincecode> provincecodes,
                IRepositoryBase<countrycode> countrycodes)
        {
            this.provinces_view = provinces_view;
            this.provincecodes = provincecodes;
            this.countrycodes = countrycodes;
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            var province_view = provinces_view.GetAll().OrderBy(q => q.provinceName);

            if (!String.IsNullOrEmpty(searchString))
            {
                province_view = province_view.Where(s => s.provinceName.Contains(searchString)).OrderBy(r => r.provinceName);
            }

            return View(province_view);
        }

        // GET: /Details/5
        public ActionResult ProvinceDetails(string id)
        {
            province_view province_view = provinces_view.GetAll().
                Where(c => c.provinceCode == id).FirstOrDefault();
            //var province_view = provinces_view.GetById(id);
            if (province_view == null)
            {
                return HttpNotFound();
            }
            return View(province_view);
        }

        // GET: /Create
        public ActionResult CreateProvince()
        {
            //Dropdown list for country
            var country = countrycodes.GetAll();
            ViewBag.Country = country.OrderBy(q => q.name);

            var province_view = new province_view();
            return View(province_view);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProvince(province_view province_form_page)
        {
            var _province = new provincecode();
            _province.code = province_form_page.provinceCode;
            _province.name = province_form_page.provinceName;
            _province.countryCode = province_form_page.countryCode;
            _province.createDate = DateTime.Now;
            _province.lastUpdate = DateTime.Now;
            provincecodes.Insert(_province);
            provincecodes.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Edit/5
        public ActionResult EditProvince(string id)
        {
            //Dropdown list for country
            var country = countrycodes.GetAll();
            ViewBag.Country = country.OrderBy(q => q.name);

            province_view province_view = provinces_view.GetAll().
                Where(c => c.provinceCode == id).FirstOrDefault();

            if (province_view == null)
            {
                return HttpNotFound();
            }
            return View(province_view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProvince(province_view province_form_page)
        {
            var _province = provincecodes.GetById(province_form_page.provinceCode);

            _province.code = province_form_page.provinceCode;
            _province.name = province_form_page.provinceName;
            _province.countryCode = province_form_page.countryCode;
            _province.lastUpdate = DateTime.Now;
            provincecodes.Update(_province);
            provincecodes.Commit();

            return RedirectToAction("Index");
        }
   }
}
