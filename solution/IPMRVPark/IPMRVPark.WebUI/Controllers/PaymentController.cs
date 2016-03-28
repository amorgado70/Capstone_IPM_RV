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
        IRepositoryBase<total_per_session_view> totals_per_session;
        IRepositoryBase<reasonforpayment> reasonforpayments;
        IRepositoryBase<paymentmethod> paymentmethods;
        IRepositoryBase<selecteditem> selecteditems;
        IRepositoryBase<reservationitem> reservationitems;
        IRepositoryBase<total_per_site_view> totals_per_site;
        IRepositoryBase<paymentreservationitem> paymentsreservationitems;
        IRepositoryBase<session> sessions;
        SessionService sessionService;

        public PaymentController(IRepositoryBase<payment> payments,
            IRepositoryBase<customer_view> customers,
            IRepositoryBase<total_per_session_view> totals_per_session,
            IRepositoryBase<reasonforpayment> reasonforpayments,
            IRepositoryBase<paymentmethod> paymentmethods,
            IRepositoryBase<selecteditem> selecteditems,
            IRepositoryBase<reservationitem> reservationitems,
            IRepositoryBase<total_per_site_view> totals_per_site,
            IRepositoryBase<paymentreservationitem> paymentsreservationitems,
        IRepositoryBase<session> sessions)
        {
            this.sessions = sessions;
            this.payments = payments;
            this.customers = customers;
            this.totals_per_session = totals_per_session;
            this.reasonforpayments = reasonforpayments;
            this.paymentmethods = paymentmethods;
            this.selecteditems = selecteditems;
            this.reservationitems = reservationitems;
            this.totals_per_site = totals_per_site;
            this.paymentsreservationitems = paymentsreservationitems;
            sessionService = new SessionService(this.sessions);
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
                _customer = customers.GetAll().Where(c => c.id == _session.idCustomer).FirstOrDefault();
                tryResult = !(_customer.Equals(default(session)));
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }

            if (tryResult)//customer found in database
            {
                ViewBag.CustomerID = _customer.id;
                ViewBag.CustomerName = _customer.fullName + ", " + _customer.mainPhone;
            };
            ViewBag.CustomerBalance = 0.01;

            // Read total for session
            total_per_session_view _total_per_session = new total_per_session_view();
            tryResult = false;
            try //checks if total is in database
            {
                _total_per_session = totals_per_session.GetAll().Where(c => c.idSession == _session.ID).FirstOrDefault();
                tryResult = !(_total_per_session.Equals(default(session)));
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }

            if (tryResult)//customer found in database
            {
                ViewBag.Total = _total_per_session.total_amount;
            };

            // Tax Percentage
            ViewBag.ProvinceTax = 13.00;

            // Configure dropdown list items
            var reasonforpayment = reasonforpayments.GetAll().OrderBy(s => s.description);
            List<SelectListItem> selectReasonForPayment = new List<SelectListItem>();
            foreach (var item in reasonforpayment)
            {
                SelectListItem selectListItem = new SelectListItem();
                selectListItem.Value = item.ID.ToString();
                selectListItem.Text = item.description;
                string selectedText = "New Reservation";
                selectListItem.Selected =
                 (selectListItem.Text == selectedText);
                selectReasonForPayment.Add(selectListItem);
            }
            ViewBag.ReasonForPayment = selectReasonForPayment;

            // Configure dropdown list items
            var paymentmethod = paymentmethods.GetAll().OrderBy(s => s.description);
            List<SelectListItem> selectPaymentMethod = new List<SelectListItem>();
            List<SelectListItem> selectPayDocType = new List<SelectListItem>();
            foreach (var item in paymentmethod)
            {
                SelectListItem selectList1Item = new SelectListItem();
                selectList1Item.Value = item.ID.ToString();
                selectList1Item.Text = item.description;
                string selectedText = "VISA";
                selectList1Item.Selected =
                 (selectList1Item.Text.Contains(selectedText));
                selectPaymentMethod.Add(selectList1Item);
                SelectListItem selectList2Item = new SelectListItem();
                selectList2Item.Value = item.ID.ToString();
                selectList2Item.Text = item.doctype;
                selectPayDocType.Add(selectList2Item);
            }
            ViewBag.PaymentMethod = selectPaymentMethod;
            ViewBag.PayDocType = selectPayDocType;

            var payment = new payment();
            return View(payment);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePayment(payment _payment)
        {
            // Identify session
            session _session = sessionService.GetSession(this.HttpContext);
            long sessionID = _session.ID;

            // Create and insert payment
            _payment.idSession = sessionID;
            _payment.isCredit = true;
            _payment.createDate = DateTime.Now;
            _payment.lastUpdate = DateTime.Now;
            payments.Insert(_payment);
            payments.Commit();

            var _selecteditems = totals_per_site.GetAll().Where(s => s.idSession == sessionID);
            foreach (total_per_site_view item in _selecteditems)
            {
                // Create and insert reservation items
                var _reservationitem = new reservationitem();
                _reservationitem.idRVSite = item.idRVSite.Value;
                _reservationitem.idCustomer = item.idCustomer.Value;
                _reservationitem.idStaff = item.idStaff.Value;
                _reservationitem.checkInDate = item.checkInDate;
                _reservationitem.checkOutDate = item.checkOutDate;
                _reservationitem.totalAmount = item.amount;
                _reservationitem.createDate = DateTime.Now;
                _reservationitem.lastUpdate = DateTime.Now;
                reservationitems.Insert(_reservationitem);
                reservationitems.Commit();
                // Create link between payment and reservation
                // *****_reservationitem.idPayment
                var _paymentreservationitem = new paymentreservationitem();
                _paymentreservationitem.idPayment = _payment.ID;
                _paymentreservationitem.idReservationItem = _reservationitem.ID;
                _paymentreservationitem.createDate = DateTime.Now;
                _paymentreservationitem.lastUpdate = DateTime.Now;
                paymentsreservationitems.Insert(_paymentreservationitem);
                paymentsreservationitems.Commit();
                // Remove items from selected table
                selecteditems.Delete(selecteditems.GetById(item.idSelected));
                selecteditems.Commit();
            }

            // Reset customer
            _session.idCustomer = null;
            sessions.Update(_session);
            sessions.Commit();

            return RedirectToAction("PrintPayment",_payment);
        }

        public ActionResult PrintPayment(payment _payment)
        {
            return View(_payment);
        }
    }
}
