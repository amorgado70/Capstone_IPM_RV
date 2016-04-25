using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Services;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IPMRVPark.WebUI.Controllers
{
    public class ReservationController : Controller
    {
        IRepositoryBase<customer_view> customers;
        IRepositoryBase<staff_view> users;
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
            IRepositoryBase<staff_view> users,
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
            this.users = users;
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
                this.customers,
                this.users
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
            session _session = sessionService.GetSession(this.HttpContext, false);
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
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext, true);
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
            long userID = sessionService.GetSessionUserID(this.HttpContext, true);
            ViewBag.UserID = userID;
            // Clean items that are in selected table
            paymentService.CleanEditSelectedItems(
                sessionService.GetSessionID(this.HttpContext, false),
                userID
                );

            return View();
        }

        // Edit Selected Item
        public ActionResult EditSelected(long selectedID = newReservationMode)
        {
            ViewBag.SelectedID = selectedID;
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext, true);

            return View();
        }

        // Update session's check-in and check-out dates
        [HttpPost]
        public ActionResult SelectCheckInOutDates(DateTime checkInDate, DateTime checkOutDate)
        {
            var _session = sessionService.GetSession(this.HttpContext, true);
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
            var _session = sessionService.GetSession(this.HttpContext, true);

            // Check if site is already in the list (it was previously cancelled)
            // If it is, remove it
            var old_selecteditems = selecteditems.GetAll().Where(osi => osi.idRVSite == idRVSite).ToList();
            foreach (var old_selecteditem in old_selecteditems)
            {
                selecteditems.Delete(old_selecteditem.ID);
            };

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
        [HttpPost]
        public ActionResult SelectSiteOnMap(long id)
        {
            var _session = sessionService.GetSession(this.HttpContext, true);
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
            long sessionID = sessionService.GetSessionID(this.HttpContext, true);
            long IPMEventID = sessionService.GetSessionIPMEventID(sessionID);
            // Discard selected items from other sessions
            // Discard selected items from other years
            // Discard selected items unchecked
            var _selecteditems = selecteditems.GetAll().
                Where(q => q.idSession == sessionID && q.idIPMEvent == IPMEventID && q.isSiteChecked == true).
                OrderByDescending(o => o.ID);

            CreateViewBagForSelectedTotal(sessionID);

            return PartialView("SelectedList", _selecteditems);
        }

        // For Partial View : Show Selection Summary
        public ActionResult ShowSelectionSummary()
        {
            long sessionID = sessionService.GetSessionID(this.HttpContext, false);
            long sessionCustomerID = sessionService.GetSessionCustomerID(sessionID);
            long sessionIPMEventID = sessionService.GetSessionIPMEventID(sessionID);

            // Data to be presented on the view

            CreatePaymentViewBags(sessionID, sessionIPMEventID, sessionCustomerID);

            var _selecteditems = selecteditems.GetAll().
                Where(si => si.idSession == sessionID && si.idCustomer == sessionCustomerID &&
                 (si.reservationAmount != 0 || si.amount != 0 )).
                OrderBy(order => order.site);

            if (_selecteditems.Count() > 0)
            {
                CreatePaymentViewBags(sessionID, sessionIPMEventID, sessionCustomerID);
                return PartialView("SelectionSummary", _selecteditems);
            }
            else
            {
                return PartialView("../Login/EmptyPartial");
            }
        }


        // For Partial View : Show Reservation Summary
        public ActionResult ShowReservationSummary()
        {
            long sessionID = sessionService.GetSessionID(this.HttpContext, true);
            long IPMEventID = sessionService.GetSessionIPMEventID(sessionID);
            var _selecteditem = selecteditems.GetAll().
                Where(q => q.idSession == sessionID).OrderByDescending(o => o.ID);

            CreateViewBagForSelectedTotal(sessionID);

            ViewBag.Customer = sessionService.GetSessionCustomerNamePhone(sessionID);

            if (_selecteditem.Count() > 0)
            {
                long sessionCustomerID = sessionService.GetSessionCustomerID(sessionID);
                CreatePaymentViewBags(sessionID, IPMEventID, sessionCustomerID);
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
            long sessionID = sessionService.GetSessionID(this.HttpContext, true);
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
            var _session = sessionService.GetSession(this.HttpContext, true);
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
        public ActionResult RemoveSelected(long id)
        {
            long sessionID = sessionService.GetSessionID(this.HttpContext, true);
            long userID = sessionService.GetSessionUserID(this.HttpContext, false);
            paymentService.CleanSelectedItem(sessionID, userID, id);
            return RedirectToAction("NewReservation");
        }

        // Delete all selected sites
        public ActionResult RemoveAllSelected()
        {
            long sessionID = sessionService.GetSessionID(this.HttpContext, true);
            long userID = sessionService.GetSessionUserID(this.HttpContext, false);
            var allSelected = selecteditems.GetAll().
                Where(q => q.idSession == sessionID).OrderByDescending(o => o.ID).ToList();

            if (allSelected.Count() > 0)
            {
                foreach (var _selected in allSelected)
                {
                    paymentService.CleanSelectedItem(sessionID, userID, _selected.ID);
                }
            }

            return RedirectToAction("NewReservation");
        }
        #endregion

        #region Edit Reservation - Site Reserved

        // Update Reserved Site
        public ActionResult UpdateReserved(int id)
        {
            var _session = sessionService.GetSession(this.HttpContext, true);
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
            long sessionID = sessionService.GetSessionID(this.HttpContext, true);
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
            var _session = sessionService.GetSession(this.HttpContext, true);
            var _selecteditem = selecteditems.GetById(id);
            _selecteditem.isSiteChecked = false;
            _selecteditem.timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            _selecteditem.total = 0;
            selecteditems.Update(selecteditems.GetById(id));
            selecteditems.Commit();
            return RedirectToAction("EditReservation");
        }

        // Remove All Reserved Sites
        public ActionResult RemoveAllReserved()
        {
            var _session = sessionService.GetSession(this.HttpContext, true);
            var allSelected = selecteditems.GetAll().
                Where(q => q.idSession == _session.ID).OrderByDescending(o => o.ID);

            if (allSelected.Count() > 0)
            {
                foreach (var i in allSelected)
                {
                    var _selecteditem = selecteditems.GetById(i.ID);
                    _selecteditem.isSiteChecked = false;
                    _selecteditem.timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
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
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext, true);

            return View();
        }

        // Partial View for CRUD of Reserved Site
        public ActionResult CRUDReservedItem(long selectedID = newReservationMode)
        {
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext, true);
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
            long userID = sessionService.GetSessionUserID(this.HttpContext, true);
            ViewBag.UserID = userID;
            // Clean items that are in selected table
            paymentService.CleanNewSelectedItems(
                sessionService.GetSessionID(this.HttpContext, false),
                userID
                );

            return View();
        }

        public ActionResult SearchReservationBySite(string searchBySite)
        {
            long sessionID = sessionService.GetSessionID(this.HttpContext, true);
            var _reservationitems = reservationitems.GetAll();

            // Discard reserved items from other years
            long IPMEventID = sessionService.GetSessionIPMEventID(sessionID);
            foreach (var _reservationitem in _reservationitems)
            {
                var _site = placesinmap.GetById(_reservationitem.idRVSite);
                if (_site.idIPMEvent != IPMEventID)
                {
                    _reservationitems = _reservationitems.Where(r => r.idRVSite != _site.ID);
                }
            }

            _reservationitems = _reservationitems.OrderByDescending(o => o.ID);

            if (searchBySite != null)
            {
                //Regex for site name
                Regex rgx = new Regex("[^a-zA-Z0-9]");

                //Remove characters from search string
                searchBySite = rgx.Replace(searchBySite, "").ToUpper();

                //Filter list
                foreach (var _reservationitem in _reservationitems)
                {
                    if (!(rgx.Replace(_reservationitem.site, "").ToUpper().Contains(searchBySite)))
                    {
                        _reservationitems = _reservationitems.Where(p => p.ID != _reservationitem.ID);
                    }

                }
            }

            return View(_reservationitems);
        }

        // For Partial View : Reserved Site List
        public ActionResult UpdateReservedList()
        {
            long sessionID = sessionService.GetSessionID(this.HttpContext, true);
            long customerID = sessionService.GetSessionCustomerID(sessionID);
            var _reserveditems = reservationitems.GetAll().
                Where(q => q.idCustomer == customerID && q.isCancelled != true );

            // Discard reserved items from other years
            long IPMEventID = sessionService.GetSessionIPMEventID(sessionID);
            foreach (var _reserveditem in _reserveditems)
            {
                var _site = placesinmap.GetById(_reserveditem.idRVSite);
                if (_site.idIPMEvent != IPMEventID)
                {
                    _reserveditems = _reserveditems.Where(r => r.idRVSite != _site.ID);
                }
            }

            _reserveditems = _reserveditems.OrderBy(o => o.site);

            decimal reservedTotal = paymentService.CalculateReservedTotal(customerID);

            if (reservedTotal > 0)
            {
                ViewBag.totalAmount = reservedTotal.ToString("N2");
            }

            return PartialView("../Reservation/ReservedList", _reserveditems);
        }

        public ActionResult GoToEditReservation()
        {
            long userID = sessionService.GetSessionUserID(this.HttpContext, true);
            // Clean items that are in selected table
            paymentService.CleanNewSelectedItems(
                sessionService.GetSessionID(this.HttpContext, false),
                userID
                );
            return RedirectToAction("EditReservation");
        }

        public ActionResult EditReservation()
        {
            long sessionUserID = sessionService.GetSessionUserID(this.HttpContext, true);
            ViewBag.UserID = sessionUserID;

            long sessionID = sessionService.GetSessionID(this.HttpContext, false);
            long sessionCustomerID = sessionService.GetSessionCustomerID(sessionID);
            ViewBag.Customer = sessionService.GetSessionCustomerNamePhone(sessionID);

            long sessionIPMEventID = sessionService.GetSessionIPMEventID(sessionID);

            var _reserveditems = reservationitems.GetAll();
            if (sessionCustomerID != IDnotFound)
            {
                _reserveditems = _reserveditems.
                    Where(q => q.idCustomer == sessionCustomerID && q.total != 0).
                    OrderByDescending(o => o.idRVSite);
            }

            // Discard reserved items from other years
            long IPMEventID = sessionService.GetSessionIPMEventID(sessionID);
            foreach (var _reserveditem in _reserveditems)
            {
                var _site = placesinmap.GetById(_reserveditem.idRVSite);
                if (_site.idIPMEvent != IPMEventID)
                {
                    _reserveditems = _reserveditems.Where(r => r.idRVSite != _site.ID);
                }
            }

            foreach (var item in _reserveditems)
            {
                // If reserved item is not in the selected item table
                var _checkitem = selecteditems.GetAll().Where(s => s.idRVSite == item.idRVSite && s.reservationAmount != 0).FirstOrDefault();
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

            CreatePaymentViewBags(sessionID, IPMEventID, sessionCustomerID);

            var _selecteditems = selecteditems.GetAll().
                Where(si => si.idSession == sessionID && si.idCustomer == sessionCustomerID && si.reservationAmount != 0).
                OrderBy(order => order.site);

            return View(_selecteditems);
        }

        // Data to be presented on the view
        private void CreatePaymentViewBags(long sessionID, long IPMEventID, long sessionCustomerID)
        {
            // Data to be presented on the view
            payment _payment = new payment();
            _payment = paymentService.CalculateEditSelectedTotal(sessionID, IPMEventID, sessionCustomerID);

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
            long sessionID = sessionService.GetSessionID(this.HttpContext, true);
            long sessionCustomerID = sessionService.GetSessionCustomerID(sessionID);
            long IPMEventID = sessionService.GetSessionIPMEventID(sessionID);

            // Data to be presented on the view
            var _selecteditems = selecteditems.GetAll().
                Where(s => s.idSession == sessionID && s.idCustomer == sessionCustomerID);

            payment _payment = paymentService.CalculateEditSelectedTotal(sessionID, IPMEventID, sessionCustomerID);

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
