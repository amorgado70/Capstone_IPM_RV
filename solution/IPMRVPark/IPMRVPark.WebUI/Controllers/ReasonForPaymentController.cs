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
    public class ReasonForPaymentController : Controller
    {
        IRepositoryBase<reasonforpayment> reasonforpayments;
        public ReasonForPaymentController(IRepositoryBase<reasonforpayment> reasonforpayments)
        {
            this.reasonforpayments = reasonforpayments;
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            var reasonforpayment = reasonforpayments.GetAll().OrderBy(c => c.ID);

            if (!String.IsNullOrEmpty(searchString))
            {
                reasonforpayment = reasonforpayment.Where(s => s.description.Contains(searchString)).OrderBy(c => c.ID);
            }

            return View(reasonforpayment);
        }

        // GET: /Details/5
        public ActionResult ReasonForPaymentDetails(int? id)
        {
            var reasonforpayment = reasonforpayments.GetById(id);
            if (reasonforpayment == null)
            {
                return HttpNotFound();
            }
            return View(reasonforpayment);
        }

        // GET: /Create
        public ActionResult CreateReasonForPayment()
        {
            var reasonforpayment = new reasonforpayment();
            return View(reasonforpayment);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateReasonForPayment(reasonforpayment reasonforpayment)
        {
            var _reasonforpayment = new reasonforpayment();
            _reasonforpayment.ID = reasonforpayment.ID;
            _reasonforpayment.description = reasonforpayment.description;
            _reasonforpayment.createDate = DateTime.Now;
            _reasonforpayment.lastUpdate = DateTime.Now;
            reasonforpayments.Insert(_reasonforpayment);
            reasonforpayments.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Edit/5
        public ActionResult EditReasonForPayment(int id)
        {
            reasonforpayment reasonforpayment = reasonforpayments.GetById(id);
            if (reasonforpayment == null)
            {
                return HttpNotFound();
            }
            return View(reasonforpayment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditReasonForPayment(reasonforpayment reasonforpayment)
        {
            var _reasonforpayment = reasonforpayments.GetById(reasonforpayment.ID);

            _reasonforpayment.description = reasonforpayment.description;
            _reasonforpayment.lastUpdate = DateTime.Now;
            reasonforpayments.Update(_reasonforpayment);
            reasonforpayments.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Delete/5
        public ActionResult DeleteReasonForPayment(int id)
        {
            reasonforpayment reasonforpayment = reasonforpayments.GetById(id);
            if (reasonforpayment == null)
            {
                return HttpNotFound();
            }
            return View(reasonforpayment);
        }

        [HttpPost, ActionName("DeleteReasonForPayment")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            reasonforpayments.Delete(reasonforpayments.GetById(id));
            reasonforpayments.Commit();
            return RedirectToAction("Index");
        }

    }
}
