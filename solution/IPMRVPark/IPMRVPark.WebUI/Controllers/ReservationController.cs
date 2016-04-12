using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Services;
using System.Collections.Generic;

namespace IPMRVPark.WebUI.Controllers
{
    public class ReservationController : Controller
    {
        IRepositoryBase<customer_view> customers;
        IRepositoryBase<ipmevent> ipmevents;
        IRepositoryBase<session> sessions;
        IRepositoryBase<placeinmap> placesinmap;
        IRepositoryBase<selecteditem> selecteditems;
        IRepositoryBase<reservationitem> reservationitems;
        IRepositoryBase<payment> payments;
        IRepositoryBase<paymentreservationitem> paymentsreservationitems;
        IRepositoryBase<rvsite_available_view> rvsites_available;
        IRepositoryBase<site_description_rate_view> sites_description_rate;
        SessionService sessionService;
        PaymentService paymentService;

        public ReservationController(
            IRepositoryBase<customer_view> customers,
            IRepositoryBase<ipmevent> ipmevents,
            IRepositoryBase<placeinmap> placesinmap,
            IRepositoryBase<rvsite_available_view> rvsites_available,
            IRepositoryBase<selecteditem> selecteditems,
            IRepositoryBase<reservationitem> reservationitems,
            IRepositoryBase<payment> payments,
            IRepositoryBase<paymentreservationitem> paymentsreservationitems,
            IRepositoryBase<session> sessions,
            IRepositoryBase<site_description_rate_view> sites_description_rate
            )
        {
            this.customers = customers;
            this.ipmevents = ipmevents;
            this.payments = payments;
            this.paymentsreservationitems = paymentsreservationitems;
            this.placesinmap = placesinmap;
            this.selecteditems = selecteditems;
            this.reservationitems = reservationitems;
            this.rvsites_available = rvsites_available;
            this.sites_description_rate = sites_description_rate;
            this.sessions = sessions;
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

        const long newReservationMode = -1;
        const long IDnotFound = -1;

        // Convert dates in number of days counting from today
        private void CreateViewBagsForDates(long selectedID = newReservationMode)
        {
            session _session = sessionService.GetSession(this.HttpContext);
            ipmevent _IPMEvent = ipmevents.GetById(_session.idIPMEvent);

            // Read and convert the dates to a value than can be used by jQuery Datepicker
            DateTime start = _IPMEvent.startDate.Value;
            DateTime end = _IPMEvent.endDate.Value;
            DateTime now = DateTime.Now;
            DateTime checkInDate = DateTime.MinValue;
            DateTime checkOutDate = DateTime.MinValue;

            // Parameters for Edit Reservation, NOT used for New Reservation
            if (selectedID != newReservationMode)
            {
                selecteditem _selecteditem = selecteditems.GetById(selectedID);
                checkInDate = _selecteditem.checkInDate;
                checkOutDate = _selecteditem.checkOutDate;
            }

            if (checkInDate == DateTime.MinValue)
            {
                if (_session.checkInDate != null)
                {
                    checkInDate = _session.checkInDate.Value;
                };
            };
            if (checkOutDate == DateTime.MinValue)
            {
                if (_session.checkOutDate != null)
                {
                    checkOutDate = _session.checkOutDate.Value;
                };
            };

            if (!(checkInDate >= start && checkInDate <= end))
            {
                checkInDate = start;
            };
            if (!(checkOutDate >= checkInDate && checkOutDate <= end))
            {
                checkOutDate = end;
            };

            int min = (int)(start - now).TotalDays + 1;
            int max = (int)(end - now).TotalDays + 1;
            int checkIn = (int)(checkInDate - now).TotalDays - 7;
            int checkOut = (int)(checkOutDate - now).TotalDays + 1;

            ViewBag.minDate = min;
            ViewBag.maxDate = max;
            ViewBag.checkInDate = checkIn;
            ViewBag.checkOutDate = checkOut;
        }
        #endregion

        #region New Reservation - Site Selected

        // Partial View for CRUD of Selected Item 
        public ActionResult CRUDSelectedItem(long selectedID = newReservationMode)
        {
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);
            CreateViewBagsForDates(selectedID);

            // Parameters for Edit Reservation, NOT used for New Reservation
            if (selectedID != newReservationMode)
            {
                selecteditem _selecteditem = selecteditems.GetById(selectedID);
                ViewBag.SelectedID = selectedID;
                ViewBag.SiteID = _selecteditem.idRVSite;
                placeinmap _placeinmap = placesinmap.GetById(_selecteditem.idRVSite);
                ViewBag.SiteName = _placeinmap.site;
            }
            else
            {
                ViewBag.SiteID = newReservationMode;
            }

            return PartialView();
        }

        // New Reservation Page - Site Selection
        public ActionResult NewReservation()
        {
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);
            // Clean items that are in selected table
            paymentService.CleanEditSelectedItems(
                sessionService.GetSessionID(this.HttpContext)
                );

            return View();
        }

        // Edit Selected Item
        public ActionResult EditSelected(long selectedID = newReservationMode)
        {
            ViewBag.SelectedID = selectedID;
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);

            return View();
        }

        // Update session's check-in and check-out dates
        [HttpPost]
        public ActionResult SelectCheckInOutDates(DateTime checkInDate, DateTime checkOutDate)
        {
            var _session = sessionService.GetSession(this.HttpContext);
            _session.checkInDate = checkInDate;
            _session.checkOutDate = checkOutDate;
            sessions.Update(sessions.GetById(_session.ID));
            sessions.Commit();

            return Json(checkInDate);
        }

        // Select site button, add site to selected table
        [HttpPost]
        public ActionResult SelectSite(long idRVSite, DateTime checkInDate, DateTime checkOutDate)
        {
            var _session = sessionService.GetSession(this.HttpContext);

            // Add selected item to the database
            var _selecteditem = new selecteditem();
            var type_rates = sites_description_rate.GetAll().
                Where(s => s.id == idRVSite).FirstOrDefault();

            _selecteditem.checkInDate = checkInDate;
            _selecteditem.checkOutDate = checkOutDate;
            _selecteditem.weeklyRate = type_rates.weeklyRate.Value;
            _selecteditem.dailyRate = type_rates.dailyRate.Value;
            _selecteditem.idRVSite = idRVSite;
            _selecteditem.idSession = _session.ID;
            _selecteditem.idIPMEvent = _session.idIPMEvent;
            _selecteditem.idStaff = _session.idStaff;
            _selecteditem.idCustomer = _session.idCustomer;
            _selecteditem.site = type_rates.RVSite;
            _selecteditem.siteType = type_rates.description;
            _selecteditem.isSiteChecked = true;
            CalcSiteTotal calcResults = new CalcSiteTotal(
                checkInDate,
                checkOutDate,
                type_rates.weeklyRate.Value,
                type_rates.dailyRate.Value,
                true);
            _selecteditem.duration = calcResults.duration;
            _selecteditem.weeks = calcResults.weeks;
            _selecteditem.days = calcResults.days;
            _selecteditem.amount = calcResults.amount;
            _selecteditem.total = calcResults.total;
            _selecteditem.createDate = DateTime.Now;
            _selecteditem.lastUpdate = DateTime.Now;
            _selecteditem.timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            selecteditems.Insert(_selecteditem);
            selecteditems.Commit();

            return Json(idRVSite);
        }

        // Select site button, add site to selected table
        public ActionResult SelectSiteOnMap(long id)
        {
            var _session = sessionService.GetSession(this.HttpContext);
            ipmevent _IPMEvent = ipmevents.GetById(_session.idIPMEvent);

            // Read dates from IPM Event
            DateTime checkInDate = _IPMEvent.startDate.Value;
            DateTime checkOutDate = checkInDate.AddDays(7);
            // Read dates from session
            if (_session.checkInDate != null)
            {
                checkInDate = _session.checkInDate.Value;
            };
            if (_session.checkOutDate != null)
            {
                checkOutDate = _session.checkOutDate.Value;
            };
                        
            // Add selected item to the database
            var _selecteditem = new selecteditem();
            var type_rates = sites_description_rate.GetAll().
                Where(s => s.id == id).FirstOrDefault();

            _selecteditem.checkInDate = checkInDate;
            _selecteditem.checkOutDate = checkOutDate;
            _selecteditem.weeklyRate = type_rates.weeklyRate.Value;
            _selecteditem.dailyRate = type_rates.dailyRate.Value;
            _selecteditem.idRVSite = id;
            _selecteditem.idSession = _session.ID;
            _selecteditem.idIPMEvent = _session.idIPMEvent;
            _selecteditem.idStaff = _session.idStaff;
            _selecteditem.idCustomer = _session.idCustomer;
            _selecteditem.site = type_rates.RVSite;
            _selecteditem.siteType = type_rates.description;
            _selecteditem.isSiteChecked = true;
            CalcSiteTotal calcResults = new CalcSiteTotal(
                checkInDate,
                checkOutDate,
                type_rates.weeklyRate.Value,
                type_rates.dailyRate.Value,
                true);
            _selecteditem.duration = calcResults.duration;
            _selecteditem.weeks = calcResults.weeks;
            _selecteditem.days = calcResults.days;
            _selecteditem.amount = calcResults.amount;
            _selecteditem.total = calcResults.total;
            _selecteditem.createDate = DateTime.Now;
            _selecteditem.lastUpdate = DateTime.Now;
            _selecteditem.timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            selecteditems.Insert(_selecteditem);
            selecteditems.Commit();

            return Json(id);
        }

        // Calculate total for site selected on the dropdown list
        [HttpPost]
        public ActionResult GetSiteData(long idRVSite, DateTime checkInDate, DateTime checkOutDate)
        {
            decimal amount = 0;
            decimal weeklyRate = 0;
            decimal dailyRate = 0;

            var site = sites_description_rate.GetAll().Where(s => s.id == idRVSite).FirstOrDefault();

            if (site != null)
            {
                weeklyRate = site.weeklyRate.Value;
                dailyRate = site.dailyRate.Value;

                CalcSiteTotal calcResults = new CalcSiteTotal(
                    checkInDate,
                    checkOutDate,
                    weeklyRate,
                    dailyRate,
                    true);
                amount = calcResults.amount;
            }

            return Json(new
            {
                amount = amount.ToString("N2"),
                type = site.description,
                weeklyRate = weeklyRate.ToString("N2"),
                dailyRate = dailyRate.ToString("N2")
            });
        }

        // Sum and Count for Selected Items
        private void CreateViewBagForSelectedTotal(long sessionID)
        {
            int count;
            decimal selectedTotal = paymentService.CalculateNewSelectedTotal(sessionID, out count);

            if (selectedTotal > 0)
            {
                ViewBag.totalAmount = selectedTotal.ToString("N2");
            }
        }

        // For Partial View : Selected Site List
        public ActionResult UpdateSelectedList()
        {
            long sessionID = sessionService.GetSessionID(this.HttpContext);
            var _selecteditems = selecteditems.GetAll().
                Where(q => q.idSession == sessionID).OrderByDescending(o => o.ID);

            CreateViewBagForSelectedTotal(sessionID);

            return PartialView("SelectedList", _selecteditems);
        }

        // For Partial View : Show Reservation Summary
        public ActionResult ShowSelectionSummary()
        {
            long sessionID = sessionService.GetSessionID(this.HttpContext);
            var _selecteditem = selecteditems.GetAll().
                Where(q => q.idSession == sessionID).OrderByDescending(o => o.ID);

            CreateViewBagForSelectedTotal(sessionID);

            ViewBag.Customer = sessionService.GetSessionCustomerNamePhone(sessionID);




            if (_selecteditem.Count() > 0)
            {
                long sessionCustomerID = sessionService.GetSessionCustomerID(sessionID);
                CreatePaymentViewBags(sessionID, sessionCustomerID);
                return PartialView("SelectionSummary", _selecteditem);
            }
            else
            {
                return PartialView("../Login/EmptyPartial");
            }
        }


        // For Partial View : Show Reservation Summary
        public ActionResult ShowReservationSummary()
        {
            long sessionID = sessionService.GetSessionID(this.HttpContext);
            var _selecteditem = selecteditems.GetAll().
                Where(q => q.idSession == sessionID).OrderByDescending(o => o.ID);

            CreateViewBagForSelectedTotal(sessionID);

            ViewBag.Customer = sessionService.GetSessionCustomerNamePhone(sessionID);




            if (_selecteditem.Count() > 0)
            {
                long sessionCustomerID = sessionService.GetSessionCustomerID(sessionID);
                CreatePaymentViewBags(sessionID, sessionCustomerID);
                return PartialView("ReservationSummary", _selecteditem);
            }
            else
            {
                return PartialView("../Login/EmptyPartial");
            }
        }

        // Selected sites total
        public ActionResult GetSelectionTotal()
        {
            long sessionID = sessionService.GetSessionID(this.HttpContext);
            int count;
            decimal sum = paymentService.CalculateNewSelectedTotal(sessionID, out count);

            string totalAmount = "";
            if (count > 0)
            {
                totalAmount = "( " + count + " )  $" + sum.ToString("N2");
            }

            return Json(totalAmount);
        }

        // Update on selected site
        public ActionResult UpdateSelected(int id)
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _selecteditem = selecteditems.GetById(id);
            _selecteditem.checkInDate = _session.checkInDate.Value;
            _selecteditem.checkOutDate = _session.checkOutDate.Value;
            CalcSiteTotal calcResults = new CalcSiteTotal(
                _selecteditem.checkInDate,
                _selecteditem.checkOutDate,
                _selecteditem.weeklyRate,
                _selecteditem.dailyRate,
                true);
            _selecteditem.duration = calcResults.duration;
            _selecteditem.weeks = calcResults.weeks;
            _selecteditem.days = calcResults.days;
            _selecteditem.amount = calcResults.amount;
            _selecteditem.total = calcResults.total;
            _selecteditem.lastUpdate = DateTime.Now;
            _selecteditem.timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            selecteditems.Update(_selecteditem);
            selecteditems.Commit();

            return RedirectToAction("NewReservation");
        }

        // Delete on selected site
        public ActionResult RemoveSelected(int id)
        {
            selecteditems.Delete(selecteditems.GetById(id));
            selecteditems.Commit();
            return RedirectToAction("NewReservation");
        }

        // Delete all selected sites
        public ActionResult RemoveAllSelected()
        {
            long sessionID = sessionService.GetSessionID(this.HttpContext);
            var allSelected = selecteditems.GetAll().
                Where(q => q.idSession == sessionID).OrderByDescending(o => o.ID);

            if (allSelected.Count() > 0)
            {
                foreach (var _selected in allSelected)
                {
                    selecteditems.Delete(selecteditems.GetById(_selected.ID));
                }
                selecteditems.Commit();
            }

            return RedirectToAction("NewReservation");
        }
        #endregion

        #region Edit Reservation - Site Reserved

        // Update Reserved Site
        public ActionResult UpdateReserved(int id)
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _selecteditem = selecteditems.GetById(id);
            _selecteditem.checkInDate = _session.checkInDate.Value;
            _selecteditem.checkOutDate = _session.checkOutDate.Value;
            CalcSiteTotal calcResults = new CalcSiteTotal(
                _selecteditem.checkInDate,
                _selecteditem.checkOutDate,
                _selecteditem.weeklyRate,
                _selecteditem.dailyRate,
                true);
            _selecteditem.duration = calcResults.duration;
            _selecteditem.weeks = calcResults.weeks;
            _selecteditem.days = calcResults.days;
            _selecteditem.amount = calcResults.amount;
            _selecteditem.total = calcResults.total;
            _selecteditem.lastUpdate = DateTime.Now;
            _selecteditem.timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            selecteditems.Update(_selecteditem);
            selecteditems.Commit();

            return RedirectToAction("EditReservation");
        }

        // Reinsert Reserved Site
        public ActionResult ReinsertReserved(int id)
        {
            var _selecteditem = selecteditems.GetById(id);
            var item = reservationitems.GetById(_selecteditem.idReservationItem);
            _selecteditem.checkInDate = item.checkInDate;
            _selecteditem.checkOutDate = item.checkOutDate;
            _selecteditem.duration = item.duration;
            _selecteditem.weeks = item.weeks;
            _selecteditem.weeklyRate = item.weeklyRate;
            _selecteditem.days = item.days;
            _selecteditem.dailyRate = item.dailyRate;
            _selecteditem.amount = item.total;
            _selecteditem.total = item.total;
            _selecteditem.isSiteChecked = true;
            _selecteditem.lastUpdate = DateTime.Now;
            _selecteditem.timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            selecteditems.Update(selecteditems.GetById(id));

            selecteditems.Commit();
            return RedirectToAction("EditReservation");
        }

        // Remove Reserved Site
        public ActionResult RemoveReserved(int id)
        {
            var _selecteditem = selecteditems.GetById(id);
            _selecteditem.isSiteChecked = false;
            _selecteditem.total = 0;
            selecteditems.Update(selecteditems.GetById(id));
            selecteditems.Commit();
            return RedirectToAction("EditReservation");
        }

        // Remove All Reserved Sites
        public ActionResult RemoveAllReserved()
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var allSelected = selecteditems.GetAll().
                Where(q => q.idSession == _session.ID).OrderByDescending(o => o.ID);

            if (allSelected.Count() > 0)
            {
                foreach (var i in allSelected)
                {
                    var _selecteditem = selecteditems.GetById(i.ID);
                    _selecteditem.isSiteChecked = false;
                    _selecteditem.total = 0;
                    selecteditems.Update(_selecteditem);
                }
                selecteditems.Commit();
            }

            return RedirectToAction("EditReservation");
        }


        // Edit Reserved Site
        public ActionResult EditReserved(long selectedID = newReservationMode)
        {
            ViewBag.SelectedID = selectedID;
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);

            return View();
        }

        // Partial View for CRUD of Reserved Site
        public ActionResult CRUDReservedItem(long selectedID = newReservationMode)
        {
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);
            CreateViewBagsForDates(selectedID);

            // Parameters for Edit Reservation, NOT used for New Reservation
            if (selectedID != newReservationMode)
            {
                selecteditem _selecteditem = selecteditems.GetById(selectedID);
                ViewBag.SelectedID = selectedID;
                ViewBag.SiteID = _selecteditem.idRVSite;
                placeinmap _placeinmap = placesinmap.GetById(_selecteditem.idRVSite);
                ViewBag.SiteName = _placeinmap.site;
            }
            else
            {
                ViewBag.SiteID = newReservationMode;
            }

            return PartialView();
        }

        // Search reservation page
        public ActionResult SearchReservation()
        {
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);
            // Clean items that are in selected table
            paymentService.CleanNewSelectedItems(
                sessionService.GetSessionID(this.HttpContext)
                );

            return View();
        }

        // For Partial View : Reserved Site List
        public ActionResult UpdateReservedList()
        {
            long sessionID = sessionService.GetSessionID(this.HttpContext);
            long customerID = sessionService.GetSessionCustomerID(sessionID);
            var _reserveditems = reservationitems.GetAll().
                Where(q => q.idCustomer == customerID).OrderByDescending(o => o.ID);

            decimal reservedTotal = paymentService.CalculateReservedTotal(customerID);

            if (reservedTotal > 0)
            {
                ViewBag.totalAmount = reservedTotal.ToString("N2");
            }

            return PartialView("../Reservation/ReservedList", _reserveditems);
        }

        public ActionResult GoToEditReservation()
        {
            // Clean items that are in selected table
            paymentService.CleanNewSelectedItems(
                sessionService.GetSessionID(this.HttpContext));
            return RedirectToAction("EditReservation");
        }

        public ActionResult EditReservation()
        {
            long sessionUserID = sessionService.GetSessionUserID(this.HttpContext);
            ViewBag.UserID = sessionUserID;

            long sessionID = sessionService.GetSessionID(this.HttpContext);
            long sessionCustomerID = sessionService.GetSessionCustomerID(sessionID);
            ViewBag.Customer = sessionService.GetSessionCustomerNamePhone(sessionID);

            long sessionIPMEventID = sessionService.GetSessionIPMEventID(sessionID);

            var _reserveditems = reservationitems.GetAll();
            if (sessionCustomerID != IDnotFound)
            {
                _reserveditems = _reserveditems.Where(q => q.idCustomer == sessionCustomerID).OrderByDescending(o => o.idRVSite);
            }

            foreach (var item in _reserveditems)
            {
                // If reserved item is not in the selected item table
                var _checkitem = selecteditems.GetAll().Where(s => s.idRVSite == item.idRVSite).FirstOrDefault();
                if (_checkitem == null)
                {
                    var _site_description_rate = sites_description_rate.GetByKey("id", item.idRVSite);
                    // Add reserved item as selected item                
                    selecteditem _selecteditem = new selecteditem();
                    _selecteditem.idRVSite = item.idRVSite;
                    _selecteditem.idSession = sessionID;
                    _selecteditem.idIPMEvent = sessionIPMEventID;
                    _selecteditem.idStaff = sessionUserID;
                    _selecteditem.idCustomer = item.idCustomer;
                    _selecteditem.checkInDate = item.checkInDate;
                    _selecteditem.checkOutDate = item.checkOutDate;
                    _selecteditem.site = _site_description_rate.RVSite;
                    _selecteditem.siteType = _site_description_rate.description;
                    _selecteditem.duration = item.duration;
                    _selecteditem.weeks = item.weeks;
                    _selecteditem.weeklyRate = item.weeklyRate;
                    _selecteditem.days = item.days;
                    _selecteditem.dailyRate = item.dailyRate;
                    _selecteditem.amount = item.total;
                    _selecteditem.isSiteChecked = true;
                    CalcSiteTotal calcResults = new CalcSiteTotal(
                        item.checkInDate,
                        item.checkOutDate,
                        _site_description_rate.weeklyRate.Value,
                        _site_description_rate.dailyRate.Value,
                        true);
                    _selecteditem.duration = calcResults.duration;
                    _selecteditem.weeks = calcResults.weeks;
                    _selecteditem.days = calcResults.days;
                    _selecteditem.amount = calcResults.amount;
                    _selecteditem.total = calcResults.total;
                    _selecteditem.createDate = DateTime.Now;
                    _selecteditem.lastUpdate = DateTime.Now;
                    _selecteditem.timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                    _selecteditem.idReservationItem = item.ID;
                    _selecteditem.reservationCheckInDate = item.checkInDate;
                    _selecteditem.reservationCheckOutDate = item.checkOutDate;
                    _selecteditem.reservationAmount = item.total;



                    selecteditems.Insert(_selecteditem);
                }
            }
            selecteditems.Commit();

            // Data to be presented on the view

            CreatePaymentViewBags(sessionID, sessionCustomerID);

            var _selecteditems = selecteditems.GetAll().
                Where(s => s.idSession == sessionID && s.idCustomer == sessionCustomerID);

            //payment _payment = paymentService.CalculateEditSelectedTotal(sessionID, sessionCustomerID);

            //// Value of previous reservation, just before edit reservation mode started
            //ViewBag.PrimaryTotal = _payment.primaryTotal.ToString("N2");
            //ViewBag.SelectionTotal = _payment.selectionTotal.ToString("N2");
            //ViewBag.CancellationFee = _payment.cancellationFee.ToString("N2");
            //// Suggested value for payment            
            //if (_payment.amount >= 0)
            //{
            //    ViewBag.dueAmount = _payment.amount.ToString("N2");
            //    ViewBag.refundAmount = "0.00";
            //}
            //else
            //{
            //    ViewBag.refundAmount = (_payment.amount * -1).ToString("N2");
            //    ViewBag.dueAmount = "0.00";
            //}

            return View(_selecteditems);
        }

        // Data to be presented on the view
        private void CreatePaymentViewBags(long sessionID, long sessionCustomerID)
        {
            // Data to be presented on the view
            payment _payment = new payment();
            _payment = paymentService.CalculateEditSelectedTotal(sessionID, sessionCustomerID);

            // Value of previous reservation, just before edit reservation mode started
            ViewBag.PrimaryTotal = _payment.primaryTotal.ToString("N2");
            ViewBag.SelectionTotal = _payment.selectionTotal.ToString("N2");
            ViewBag.CancellationFee = _payment.cancellationFee.ToString("N2");
            // Suggested value for payment            
            if (_payment.amount >= 0)
            {
                ViewBag.dueAmount = _payment.amount.ToString("N2");
                ViewBag.refundAmount = "0.00";
            }
            else
            {
                ViewBag.refundAmount = (_payment.amount * -1).ToString("N2");
                ViewBag.dueAmount = "0.00";
            }
        }

        public ActionResult EditReservationSummary()
        {
            long sessionID = sessionService.GetSessionID(this.HttpContext);
            long sessionCustomerID = sessionService.GetSessionCustomerID(sessionID);

            // Data to be presented on the view
            var _selecteditems = selecteditems.GetAll().
                Where(s => s.idSession == sessionID && s.idCustomer == sessionCustomerID);

            payment _payment = paymentService.CalculateEditSelectedTotal(sessionID, sessionCustomerID);

            // Value of previous reservation, just before edit reservation mode started
            ViewBag.PrimaryTotal = _payment.primaryTotal.ToString("N2");
            ViewBag.SelectionTotal = _payment.selectionTotal.ToString("N2");
            ViewBag.CancellationFee = _payment.cancellationFee.ToString("N2");
            // Suggested value for payment            
            if (_payment.amount >= 0)
            {
                ViewBag.dueAmount = _payment.amount.ToString("N2");
                ViewBag.refundAmount = "0.00";
            }
            else
            {
                ViewBag.refundAmount = (_payment.amount * -1).ToString("N2");
                ViewBag.duedAmount = "0.00";
            }

            return View(_selecteditems);
        }

        #endregion

        public ActionResult SiteName(int id)
        {
            var _site = sites_description_rate.GetByKey("id", id);

            string siteName = string.Empty;

            if (_site != null)
            {
                siteName = _site.RVSite;
            }

            return Content(siteName);
        }
    }
}
