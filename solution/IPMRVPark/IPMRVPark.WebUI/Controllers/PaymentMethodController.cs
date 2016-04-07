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
    public class PaymentMethodController : Controller
    {
        IRepositoryBase<paymentmethod> paymentmethods;
        public PaymentMethodController(IRepositoryBase<paymentmethod> paymentmethods)
        {
            this.paymentmethods = paymentmethods;
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            var paymentmethod = paymentmethods.GetAll().OrderBy(c => c.description);

            if (!String.IsNullOrEmpty(searchString))
            {
                paymentmethod = paymentmethod.Where(s => s.description.Contains(searchString)).OrderBy(c => c.description);
            }

            return View(paymentmethod);
        }

        // GET: /Details/5
        public ActionResult PaymentMethodDetails(int? id)
        {
            var paymentmethod = paymentmethods.GetById(id);
            if (paymentmethod == null)
            {
                return HttpNotFound();
            }
            return View(paymentmethod);
        }

        // GET: /Create
        public ActionResult CreatePaymentMethod()
        {
            var paymentmethod = new paymentmethod();
            return View(paymentmethod);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePaymentMethod(paymentmethod paymentmethod)
        {
            var _paymentmethod = new paymentmethod();
            _paymentmethod.description = paymentmethod.description;
            _paymentmethod.doctype = paymentmethod.doctype;
            _paymentmethod.createDate = DateTime.Now;
            _paymentmethod.lastUpdate = DateTime.Now;
            paymentmethods.Insert(_paymentmethod);
            paymentmethods.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Edit/5
        public ActionResult EditPaymentMethod(int id)
        {
            paymentmethod paymentmethod = paymentmethods.GetById(id);
            if (paymentmethod == null)
            {
                return HttpNotFound();
            }
            return View(paymentmethod);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPaymentMethod(paymentmethod paymentmethod)
        {
            var _paymentmethod = paymentmethods.GetById(paymentmethod.ID);

            _paymentmethod.description = paymentmethod.description;
            _paymentmethod.doctype = paymentmethod.doctype;
            _paymentmethod.lastUpdate = DateTime.Now;
            paymentmethods.Update(_paymentmethod);
            paymentmethods.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Delete/5
        public ActionResult DeletePaymentMethod(int id)
        {
            paymentmethod paymentmethod = paymentmethods.GetById(id);
            if (paymentmethod == null)
            {
                return HttpNotFound();
            }
            return View(paymentmethod);
        }

        [HttpPost, ActionName("DeletePaymentMethod")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            paymentmethods.Delete(paymentmethods.GetById(id));
            paymentmethods.Commit();
            return RedirectToAction("Index");
        }

    }
}
