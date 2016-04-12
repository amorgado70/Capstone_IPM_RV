using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Models;
using IPMRVPark.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPMRVPark.Contracts.Data;
using System.Text.RegularExpressions;

namespace IPMRVPark.WebUI.Controllers
{
    public class PaymentController : Controller
    {
        IRepositoryBase<payment> payments;
        IRepositoryBase<customer_view> customers;
        IRepositoryBase<reasonforpayment> reasonsforpayment;
        IRepositoryBase<paymentmethod> paymentmethods;
        IRepositoryBase<selecteditem> selecteditems;
        IRepositoryBase<reservationitem> reservationitems;
        IRepositoryBase<paymentreservationitem> paymentsreservationitems;
        IRepositoryBase<payment_view> payments_view;
        IRepositoryBase<session> sessions;
        SessionService sessionService;
        PaymentService paymentService;

        public PaymentController(IRepositoryBase<payment> payments,
            IRepositoryBase<customer_view> customers,
            IRepositoryBase<reasonforpayment> reasonsforpayment,
            IRepositoryBase<paymentmethod> paymentmethods,
            IRepositoryBase<selecteditem> selecteditems,
            IRepositoryBase<reservationitem> reservationitems,
            IRepositoryBase<paymentreservationitem> paymentsreservationitems,
            IRepositoryBase<payment_view> payments_view,
        IRepositoryBase<session> sessions
            )
        {
            this.sessions = sessions;
            this.payments = payments;
            this.customers = customers;
            this.reasonsforpayment = reasonsforpayment;
            this.paymentmethods = paymentmethods;
            this.selecteditems = selecteditems;
            this.reservationitems = reservationitems;
            this.paymentsreservationitems = paymentsreservationitems;
            this.payments_view = payments_view;
            sessionService = new SessionService(
                this.sessions,
                this.customers
                );
            paymentService = new PaymentService(
                this.selecteditems,
                this.reservationitems,
                this.payments,
                this.paymentsreservationitems
                );
        }//end Constructor

        #region Common        
        const long IDnotFound = -1;

        private class reasonForPayment
        {
            public enum E
            {
                NewReservation = 1,
                ExtendReservation = 2,
                ShortenReservation = 3
            }
        }
        private void updateReasonForPayment()
        {
            foreach (reasonForPayment.E e in Enum.GetValues(typeof(reasonForPayment.E)))
            {
                var r = reasonsforpayment.GetByKey("ID", (long)e);
                if (r == null)
                {
                    reasonforpayment new_r = new reasonforpayment();
                    new_r.ID = (long)e;
                    new_r.description = e.ToString();
                    reasonsforpayment.Insert(new_r);
                }
                else
                {
                    r.description = e.ToString();
                    reasonsforpayment.Update(r);
                }
            }
            reasonsforpayment.Commit();
        }

        // Configure dropdown list items
        private void reasonsForPayment(string defaultReason)
        {
            //var reasonforpayment = reasonsforpayment.GetAll().OrderBy(s => s.description);
            //List<SelectListItem> selectReasonForPayment = new List<SelectListItem>();
            //foreach (var item in reasonforpayment)
            //{
            //    SelectListItem selectListItem = new SelectListItem();
            //    selectListItem.Value = item.ID.ToString();
            //    selectListItem.Text = item.description;
            //    string selectedText = defaultReason;
            //    selectListItem.Selected =
            //             (selectListItem.Text == selectedText);
            //    selectReasonForPayment.Add(selectListItem);
            //}
            //ViewBag.ReasonForPayment = selectReasonForPayment;
        }
        // Configure dropdown list items
        private void paymentMethods(string defaultMethod)
        {
            var paymentmethod = paymentmethods.GetAll().OrderBy(s => s.description);
            List<SelectListItem> selectPaymentMethod = new List<SelectListItem>();
            List<SelectListItem> selectPayDocType = new List<SelectListItem>();
            foreach (var item in paymentmethod)
            {
                SelectListItem selectList1Item = new SelectListItem();
                selectList1Item.Value = item.ID.ToString();
                selectList1Item.Text = item.description;
                string selectedText = defaultMethod;
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
        }

        // For Partial View : Show Payments Per Customer
        public ActionResult ShowPaymentPerCustomer(long id = IDnotFound)
        {
            long sessionID = sessionService.GetSessionID(this.HttpContext);
            //long customerID = sessionService.GetSessionCustomerID(sessionID);
            long customerID = id;
            ViewBag.CustomerID = customerID;
            ViewBag.CustomerName = sessionService.GetSessionCustomerNamePhone(sessionID);

            if (customerID != IDnotFound)
            {
                decimal finalBalance = paymentService.CustomerAccountBalance(customerID);
                ViewBag.CustomerBalance = finalBalance;
                var _payments = payments.GetAll().
                    Where(s => s.idCustomer == customerID);
                // Populate reason for payment
                foreach (var _payment in _payments)
                {
                    _payment.reasonforpayment = reasonsforpayment.GetById(_payment.idReasonForPayment);
                }

                return PartialView("PaymentPerCustomer", _payments);
            };

            return PartialView("../Login/EmptyPartial");
        }

        public ActionResult PrintPayment(long id)
        {

            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);

            // Find all reservation items related to this payment
            var _payment = payments.GetById(id);
            ViewBag.Payment = _payment;

            var _paymentreservationitems = paymentsreservationitems.GetAll().
                Where(p => p.idPayment == id);
            var _reservationitems = new List<reservationitem>();
            foreach (var item in _paymentreservationitems)
            {
                reservationitem _reservationitem = new reservationitem();
                _reservationitem = reservationitems.GetById(item.idReservationItem);
                _reservationitems.Add(_reservationitem);
            }

            if (_payment.createDate != null)
            {
                ViewBag.PaymentDate = _payment.createDate.Value.ToString("R").Substring(0, 16);
            }
            ViewBag.PaymentMethod = paymentmethods.GetById(_payment.idPaymentMethod).description;

            var _customer = customers.GetByKey("id", _payment.idCustomer);

            ViewBag.CustomerName = (_customer.fullName + ", " + _customer.mainPhone);

            decimal previousBalance = paymentService.CustomerPreviousBalance(_payment.idCustomer, _payment.ID);
            ViewBag.PreviousBalance = previousBalance;
            decimal finalBalance = paymentService.CustomerAccountBalance(_payment.idCustomer);
            ViewBag.FinalBalance = finalBalance;

            // Tax Percentage
            ViewBag.ProvinceTax = paymentService.GetProvinceTax(
                sessionService.GetSessionID(this.HttpContext)
                );

            return View(_reservationitems);
        }
        #endregion
        #region Payment or Refund
        public ActionResult PaymentOrRefund(bool isCredit = true)
        {
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);

            long sessionID = sessionService.GetSessionID(this.HttpContext);
            long customerID = sessionService.GetSessionCustomerID(sessionID);
            string customerName = sessionService.GetSessionCustomerNamePhone(sessionID);

            // Check customer's account balance
            decimal customerBalance = paymentService.CustomerAccountBalance(customerID);

            // Retrieve totals for selected items in this session and transfer them to payment
            payment _payment = new payment();
            _payment = paymentService.CalculateEditSelectedTotal(sessionID, customerID);

            decimal owedAmount = _payment.selectionTotal
                + _payment.cancellationFee
                - customerBalance
                - _payment.primaryTotal;

            long reasonID = (long)reasonForPayment.E.NewReservation;
            string pageTitle = "Payment Or Refund";
            string owedText = "(a) Owed Amount";
            string balanceText = "(u) Account Balance";
            string primaryText = string.Empty;
            string feeText = string.Empty;
            string selectionText = string.Empty;
            string amountText = "(b) Customer Paid";
            string dueText = "(c) Due Amount |(b)-(a)|";

            // Payment for a New Reservation
            if (owedAmount > 0 && _payment.primaryTotal == 0)
            {
                isCredit = true;
                reasonID = (long)reasonForPayment.E.NewReservation;
                pageTitle = "Payment For New Reservation";
                owedText = "(a) Owed Amount (u)-(y)";
                balanceText = "(u) Account Balance";
                primaryText = string.Empty;
                feeText = string.Empty;
                selectionText = "(y) New Reservation Total";
                amountText = "(b) Customer Paid";
                dueText = "(c) Due Amount |(b)-(a)|";
            }
            // Payment for a Edit Reservation
            if (owedAmount > 0 && _payment.primaryTotal > 0)
            {
                isCredit = true;
                reasonID = (long)reasonForPayment.E.ExtendReservation;
                pageTitle = "Payment For Extend Reservation";
                owedText = "(a) Owed Amount (u)+(v)-(y)";
                balanceText = "(u) Account Balance";
                primaryText = "(v) Primary Reservation Total";
                feeText = string.Empty;
                selectionText = "(y) New Reservation Total";
                amountText = "(b) Customer Paid";
                dueText = "(c) Due Amount |(b)-(a)|";
            }
            // Refund for a Edit Reservation
            if (owedAmount < 0 && _payment.primaryTotal > 0)
            {
                isCredit = false;
                reasonID = (long)reasonForPayment.E.ShortenReservation;
                pageTitle = "Refund For Shorten Reservation";
                owedText = "(a) Refund Amount (u)+(v)-(x)-(y)";
                balanceText = "(u) Account Balance";
                primaryText = "(v) Primary Reservation Total";
                feeText = "(x) Cancellation Fee";
                selectionText = "(y) New Reservation Total";
                amountText = "(b) Customer Received";
                dueText = "(c) Credit Amount (b)-(a)";
            }

            // Set reason for payment
            updateReasonForPayment();

            // Texts for payment page
            ViewBag.IsCredit = isCredit;
            ViewBag.ReasonForPayment = reasonID;
            ViewBag.PageTitle = pageTitle;
            ViewBag.OwedText = owedText;
            ViewBag.BalanceText = balanceText;
            ViewBag.PrimaryText = primaryText;
            ViewBag.FeeText = feeText;
            ViewBag.SelectionText = selectionText;
            ViewBag.AmountText = amountText;
            ViewBag.DueText = dueText;
            // Tax Percentage
            ViewBag.ProvinceTax = paymentService.GetProvinceTax(sessionID);
            // Payment summary info
            ViewBag.OwedAmount = owedAmount;
            ViewBag.CustomerBalance = customerBalance;
            ViewBag.CustomerID = customerID;
            ViewBag.CustomerName = customerName;

            // Create ViewBag for Dropdownlist            
            reasonsForPayment(pageTitle);
            // Create ViewBag for Dropdownlist
            paymentMethods("VISA");

            return View(_payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PaymentOrRefund(payment _payment)
        {
            // Identify session
            long sessionID = sessionService.GetSessionID(this.HttpContext);

            // Create and insert payment
            _payment.idSession = sessionID;
            _payment.createDate = DateTime.Now;
            _payment.lastUpdate = DateTime.Now;

            payments.Insert(_payment);
            payments.Commit();
            long ID = _payment.ID;

            var _selecteditems = selecteditems.GetAll().Where(s => s.idSession == sessionID);
            foreach (selecteditem item in _selecteditems)
            {
                if (item.isSiteChecked)
                {
                    // Update existing reservation item - For edit mode
                    if (item.idReservationItem != null &&
                        item.idReservationItem != IDnotFound)
                    {
                        var old_reservationitem = reservationitems.GetById(item.idReservationItem);
                        old_reservationitem.isCancelled = true;
                        old_reservationitem.total = 0;
                        reservationitems.Update(old_reservationitem);
                    }
                    // Create and insert reservation items
                    var _reservationitem = new reservationitem();
                    _reservationitem.idRVSite = item.idRVSite.Value;
                    _reservationitem.idCustomer = item.idCustomer.Value;
                    _reservationitem.idStaff = item.idStaff.Value;
                    _reservationitem.checkInDate = item.checkInDate;
                    _reservationitem.checkOutDate = item.checkOutDate;
                    _reservationitem.site = item.site;
                    _reservationitem.siteType = item.siteType;
                    _reservationitem.weeks = item.weeks;
                    _reservationitem.weeklyRate = item.weeklyRate;
                    _reservationitem.days = item.days;
                    _reservationitem.dailyRate = item.dailyRate;
                    _reservationitem.total = item.total;
                    _reservationitem.createDate = DateTime.Now;
                    _reservationitem.lastUpdate = DateTime.Now;
                    _reservationitem.timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
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
                }
            }

            // Clean selected items
            paymentService.CleanAllSelectedItems(sessionID);
            // 
            sessionService.ResetSessionCustomer(sessionID);

            return RedirectToAction("PrintPayment", new { id = ID });
        }
        #endregion

        public ActionResult SearchPayment(string searchByCustomer, string searchBySite)
        {
            var _payments = payments_view.GetAll();

            if (searchBySite != null)
            {
                //Regex for site name
                Regex rgx = new Regex("[^a-zA-Z0-9]");

                //Remove characters from search string
                searchBySite = rgx.Replace(searchBySite, "").ToUpper();

                //Filter list
                foreach (var _payment in _payments)
                {
                    if (!(rgx.Replace(_payment.sites, "").ToUpper().Contains(searchBySite)))
                    {
                        _payments = _payments.Where(p => p.id != _payment.id);
                    }

                }
            }


            if (searchByCustomer != null)
            {
                //Regex for customer name and phone
                Regex rgx = new Regex("[^a-zA-Z]");

                //Remove characters from search string
                searchByCustomer = rgx.Replace(searchByCustomer, "").ToUpper();

                //Filter list
                foreach (var _payment in _payments)
                {
                    if (!(rgx.Replace(_payment.fullName, "").ToUpper().Contains(searchByCustomer)))
                    {
                        _payments = _payments.Where(p => p.id != _payment.id);
                    }

                }
            }

            return View(_payments);

        }
    }
}
