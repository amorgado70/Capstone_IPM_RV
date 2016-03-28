using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Services;



namespace IPMRVPark.WebUI.Controllers
{
    public class ReservationController : Controller
    {
        IRepositoryBase<customer_view> customers;
        IRepositoryBase<ipmevent> ipmevents;
        IRepositoryBase<session> sessions;
        IRepositoryBase<placeinmap> placesinmap;
        IRepositoryBase<selecteditem> selecteditems;
        IRepositoryBase<rvsite_available_view> rvsites_available;
        IRepositoryBase<total_per_site_view> totals_per_site;
        SessionService sessionService;

        public ReservationController(
            IRepositoryBase<customer_view> customers,
            IRepositoryBase<ipmevent> ipmevents,
            IRepositoryBase<placeinmap> placesinmap,
            IRepositoryBase<rvsite_available_view> rvsites_available,
            IRepositoryBase<selecteditem> selecteditems,
            IRepositoryBase<total_per_site_view> totals_per_site,
            IRepositoryBase<session> sessions)
        {
            this.customers = customers;
            this.ipmevents = ipmevents;
            this.sessions = sessions;
            this.placesinmap = placesinmap;
            this.selecteditems = selecteditems;            
            this.totals_per_site = totals_per_site;
            this.rvsites_available = rvsites_available;
            sessionService = new SessionService(this.sessions);
        }//end Constructor

        const long newReservationMode = -1;

        // Main reservation page
        public ActionResult Reservation( long selectedID = newReservationMode)
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
                ViewBag.SelectedID = selectedID;
                ViewBag.SiteID = _selecteditem.idRVSite;
                placeinmap _placeinmap  = placesinmap.GetById(_selecteditem.idRVSite);
                ViewBag.SiteName = _placeinmap.site;                
                checkInDate = _selecteditem.checkInDate;
                checkOutDate = _selecteditem.checkOutDate;
            }
            else
            {
                ViewBag.SiteID = newReservationMode;
            }

            if (checkInDate == DateTime.MinValue) {
                checkInDate = _session.checkInDate.Value; };
            if (checkOutDate == DateTime.MinValue) {
                checkOutDate = _session.checkOutDate.Value; };

            if (!(checkInDate >= start && checkInDate <= end))
            {
                checkInDate = start;
            };
            if (!(checkOutDate >= checkInDate && checkOutDate <= end))
            {
                checkOutDate = end;
            };

            TimeSpan min = start - now;
            TimeSpan max = end - now;
            TimeSpan checkIn = checkInDate - now;
            TimeSpan checkOut = checkOutDate - now;

            ViewBag.checkInDate = (int)checkIn.TotalDays + 1;
            ViewBag.checkOutDate = (int)checkOut.TotalDays + 1;
            ViewBag.minDate = (int)min.TotalDays - 7;
            ViewBag.maxDate = (int)max.TotalDays + 1;

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
        // Select site button, add site to table
        [HttpPost]
        public ActionResult SelectSite(long idRVSite, DateTime checkInDate, DateTime checkOutDate)
        {
            var _session = sessionService.GetSession(this.HttpContext);

            // Add selected item to database
            var _selecteditem = new selecteditem();
            _selecteditem.checkInDate = checkInDate;
            _selecteditem.checkOutDate = checkOutDate;
            _selecteditem.idRVSite = idRVSite;
            _selecteditem.idSession = _session.ID;
            _selecteditem.idIPMEvent = _session.idIPMEvent;
            _selecteditem.idStaff = _session.idStaff;
            _selecteditem.idCustomer = _session.idCustomer;
            _selecteditem.isSiteChecked = true;
            _selecteditem.createDate = DateTime.Now;
            _selecteditem.lastUpdate = DateTime.Now;

            selecteditems.Insert(_selecteditem);
            selecteditems.Commit();

            return Json(idRVSite);
        }

        // Calculate total for site selected on the dropdown list
        [HttpPost]
        public ActionResult GetSiteTotal(long idRVSite, DateTime checkInDate, DateTime checkOutDate)
        {
            double amount = 0;
            var site = rvsites_available.GetAll().Where(s => s.id == idRVSite).First();
            if (site != null)
            {
                int duration = (int)(checkOutDate - checkInDate).TotalDays;
                int weeks = duration / 7;
                int days = duration % 7;
                amount = Convert.ToDouble(site.weeklyRate) * weeks +
                    Convert.ToDouble(site.dailyRate) * days;
            }
            string result = amount.ToString("C");
            return Json(result);
        }

        // For Partial View : Selected Site List
        public ActionResult UpdateSelectedList()
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _selecteditem = totals_per_site.GetAll();
            _selecteditem = _selecteditem.Where(q => q.idSession == _session.ID).OrderByDescending(o => o.idSelected);

            if (_selecteditem.Count() > 0)
            {
                ViewBag.totalAmount = _selecteditem.Sum(s => s.amount).Value.ToString("C");
            }
            return PartialView("Selected", _selecteditem);
        }

        // For Partial View : Show Reservation Summary
        public ActionResult ShowReservationSummary()
        {
            session _session = sessionService.GetSession(this.HttpContext);
            var _selecteditem = totals_per_site.GetAll();
            _selecteditem = _selecteditem.Where(q => q.idSession == _session.ID).OrderByDescending(o => o.idSelected);

            if (_selecteditem.Count() > 0)
            {
                ViewBag.totalAmount = _selecteditem.Sum(s => s.amount).Value.ToString("C");
            }

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
                ViewBag.Customer = _customer.fullName + ", " + _customer.mainPhone;
            };

            return PartialView("Summary", _selecteditem);
        }

        // Selected sites total
        public ActionResult GetReservationTotal()
        {
            string total = string.Empty;
            var _session = sessionService.GetSession(this.HttpContext);
            var _selecteditem = totals_per_site.GetAll();
            _selecteditem = _selecteditem.Where(q => q.idSession == _session.ID).OrderByDescending(o => o.idSelected);

            if (_selecteditem.Count() > 0)
            {
                total = "( " + _selecteditem.Count() + " ) ";
                string totalAmount = _selecteditem.Sum(s => s.amount).Value.ToString("C");
                total = total + "CAD" + totalAmount;
            };

            return Json(total);
        }

        // Update on selected site
        public ActionResult UpdateSelected(int id)
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _selecteditem = selecteditems.GetById(id);

            _selecteditem.checkInDate = _session.checkInDate.Value;
            _selecteditem.checkOutDate = _session.checkOutDate.Value;
            _selecteditem.lastUpdate = DateTime.Now;

            selecteditems.Update(_selecteditem);
            selecteditems.Commit();

            return RedirectToAction("Reservation");
        }

        // Delete on selected site
        public ActionResult RemoveSelected(int id)
        {
            selecteditems.Delete(selecteditems.GetById(id));
            selecteditems.Commit();
            return RedirectToAction("Reservation");
        }

        // Delete all selected sites
        public ActionResult RemoveAllSelected()
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var allSelected = totals_per_site.GetAll();
            allSelected = allSelected.Where(q => q.idSession == _session.ID).OrderByDescending(o => o.idSelected);

            if (allSelected.Count() > 0)
            {
                foreach (var _selected in allSelected)
                {
                    selecteditems.Delete(selecteditems.GetById(_selected.idSelected));
                }
                selecteditems.Commit();
            }

            return RedirectToAction("Reservation");
        }
    }
}
