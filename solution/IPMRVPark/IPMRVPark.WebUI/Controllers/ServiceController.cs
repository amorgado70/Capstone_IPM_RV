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
    public class ServiceController : Controller
    {
        IRepositoryBase<service> services;
        public ServiceController(IRepositoryBase<service> services)
        {
            this.services = services;
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            var service = services.GetAll().OrderBy(c => c.ID);

            if (!String.IsNullOrEmpty(searchString))
            {
                service = service.Where(s => s.description.Contains(searchString)).OrderBy(c => c.ID);
            }

            return View(service);
        }

        // GET: /Details/5
        public ActionResult ServiceDetails(int? id)
        {
            var service = services.GetById(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }

        public ActionResult ErrorMessage()
        {
            return View();
        }

        // GET: /Create
        public ActionResult CreateService()
        {
            var service = new service();
            return View(service);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateService(service service)
        {
            //validation check
            var name1 = services.GetAll().Where(s => s.description.ToUpper().Contains(service.description.ToUpper())).ToList();

            var _service = new service();
            _service.description = service.description;
            _service.createDate = DateTime.Now;
            _service.lastUpdate = DateTime.Now;

            //code and name validation

            if (_service.description == null)
            {
                return RedirectToAction("ErrorMessage");
            }
            else if (_service.description.Trim().Length > 20)
            {
                return RedirectToAction("ErrorMessage");
            }
            else if (name1.Count() > 0)
            {
                return RedirectToAction("ErrorMessage");
            }

            services.Insert(_service);
            services.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Edit/5
        public ActionResult EditService(int id)
        {
            service service = services.GetById(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditService(service service)
        {
            var _service = services.GetById(service.ID);

            _service.description = service.description;
            _service.lastUpdate = DateTime.Now;
            services.Update(_service);
            services.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Delete/5
        public ActionResult DeleteService(int id)
        {
            service service = services.GetById(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }

        [HttpPost, ActionName("DeleteService")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            services.Delete(services.GetById(id));
            services.Commit();
            return RedirectToAction("Index");
        }

    }
}
