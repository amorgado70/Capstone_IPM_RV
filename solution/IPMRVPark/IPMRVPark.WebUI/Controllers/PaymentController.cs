using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Models;
using IPMRVPark.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IPMRVPark.WebUI.Controllers
{
    public class PaymentController : Controller
    {
        IRepositoryBase<payment> payments;
        IRepositoryBase<customer_view> customers;
        IRepositoryBase<reasonforpayment> reasonforpayments;
        IRepositoryBase<paydoctype> paydoctypes;
        IRepositoryBase<paymentmethod> paymentmethods;
        IRepositoryBase<reservationitem> reservationitems;
        IRepositoryBase<session> sessions;
        SessionService sessionService;

        public PaymentController(IRepositoryBase<payment> payments, 
            IRepositoryBase<customer_view> customers, 
            IRepositoryBase<reasonforpayment> reasonforpayments,
            IRepositoryBase<paydoctype> paydoctypes,
            IRepositoryBase<paymentmethod> paymentmethods,
            IRepositoryBase<reservationitem> reservationitems)
        {
            this.payments = payments;
            this.customers = customers;
            this.reasonforpayments = reasonforpayments;
            this.paydoctypes = paydoctypes;
            this.paymentmethods = paymentmethods;
            this.reservationitems = reservationitems;
        }//end Constructor

        // GET: /Create
        public ActionResult CreatePayment()
        {
            session _session = sessionService.GetSession(this.HttpContext);

            // Read customer from session
            customer_view _customer = new customer_view();
            bool tryResult = false;
            try //checks if customer is in database
            {
                _customer = customers.GetAll(_session.idCustomer).FirstOrDefault();
                tryResult = !(_customer.Equals(default(session)));
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }

            if (tryResult)//customer found in database
            {
                ViewBag.Customer = _customer.fullName + ", " + _customer.mainPhone;
            };            

            var reasonforpayment = reasonforpayments.GetAll();
            ViewBag.ReasonForPayments = reasonforpayment.OrderBy(s => s.description);

            var paydoctype = paydoctypes.GetAll();
            ViewBag.PayDocType = paydoctype.OrderBy(s => s.description);

            var paymentmethod = paymentmethods.GetAll();
            ViewBag.PaymentMethod = paymentmethod.OrderBy(s => s.description);

            var reservationitem = reservationitems.GetAll();
            ViewBag.ReservationItem = reservationitem.OrderBy(s => s.ID);

            var payment = new payment();
            return View(payment);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePayment(payment payment)
        {
            payments.Insert(payment);
            payments.Commit();

            return RedirectToAction("Payment");
        }

    }
}
