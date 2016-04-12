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
    public class PayDocTypeController : Controller
    {
        IRepositoryBase<paydoctype> paydoctypes;
        public PayDocTypeController(IRepositoryBase<paydoctype> paydoctypes)
        {
            this.paydoctypes = paydoctypes;
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            var paydoctype = paydoctypes.GetAll().OrderBy(c => c.ID);

            if (!String.IsNullOrEmpty(searchString))
            {
                paydoctype = paydoctype.Where(s => s.description.Contains(searchString)).OrderBy(c => c.ID);
            }

            return View(paydoctype);
        }

        // GET: /Details/5
        public ActionResult PayDocTypeDetails(int? id)
        {
            var paydoctype = paydoctypes.GetById(id);
            if (paydoctype == null)
            {
                return HttpNotFound();
            }
            return View(paydoctype);
        }

        public ActionResult ErrorMessage()
        {
            return View();
        }

        // GET: /Create
        public ActionResult CreatePayDocType()
        {
            var paydoctype = new paydoctype();
            return View(paydoctype);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePayDocType(paydoctype paydoctype)
        {
            //validation check
            var name1 = paydoctypes.GetAll().Where(s => s.description.ToUpper().Contains(paydoctype.description.ToUpper())).ToList();

            var _paydoctype = new paydoctype();
            _paydoctype.ID = paydoctype.ID;
            _paydoctype.description = paydoctype.description;
            _paydoctype.createDate = DateTime.Now;
            _paydoctype.lastUpdate = DateTime.Now;

            //code and name validation

            if (_paydoctype.description == null)
            {
                return RedirectToAction("ErrorMessage");
            }
            else if (_paydoctype.description.Trim().Length > 45)
            {
                return RedirectToAction("ErrorMessage");
            }
            else if (name1.Count() > 0)
            {
                return RedirectToAction("ErrorMessage");
            }

            paydoctypes.Insert(_paydoctype);
            paydoctypes.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Edit/5
        public ActionResult EditPayDocType(int id)
        {
            paydoctype paydoctype = paydoctypes.GetById(id);
            if (paydoctype == null)
            {
                return HttpNotFound();
            }
            return View(paydoctype);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPayDocType(paydoctype paydoctype)
        {
            var _paydoctype = paydoctypes.GetById(paydoctype.ID);

            _paydoctype.description = paydoctype.description;
            _paydoctype.lastUpdate = DateTime.Now;
            paydoctypes.Update(_paydoctype);
            paydoctypes.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Delete/5
        public ActionResult DeletePayDocType(int id)
        {
            paydoctype paydoctype = paydoctypes.GetById(id);
            if (paydoctype == null)
            {
                return HttpNotFound();
            }
            return View(paydoctype);
        }

        [HttpPost, ActionName("DeletePayDocType")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            paydoctypes.Delete(paydoctypes.GetById(id));
            paydoctypes.Commit();
            return RedirectToAction("Index");
        }

    }
}
