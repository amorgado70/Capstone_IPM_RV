using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Models.View;
using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Services;
using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace IPMRVPark.WebUI.Controllers
{
    public class ReservationController : Controller
    {
        IRepositoryBase<reservation_view> reservations;
        IRepositoryBase<customer_view> customers;
        IRepositoryBase<ipmevent> ipmevents;
        IRepositoryBase<session> sessions;
        IRepositoryBase<selected> selecteds;
        IRepositoryBase<rvsite_available_view> rvsites_available;
        IRepositoryBase<total_per_site_view> totals_per_site;
        IRepositoryBase<site_description_rate_view> sites_description_rate;
        SessionService sessionService;

        public ReservationController(IRepositoryBase<reservation_view> reservations,
            IRepositoryBase<customer_view> customers,
            IRepositoryBase<ipmevent> ipmevents,
            IRepositoryBase<rvsite_available_view> rvsites_available,
            IRepositoryBase<selected> selecteds,
            IRepositoryBase<total_per_site_view> totals_per_site,
            IRepositoryBase<site_description_rate_view> sites_description_rate,
            IRepositoryBase<session> sessions)
        {
            this.reservations = reservations;
            this.customers = customers;
            this.ipmevents = ipmevents;
            this.selecteds = selecteds;
            this.sessions = sessions;
            this.totals_per_site = totals_per_site;
            this.rvsites_available = rvsites_available;
            this.sites_description_rate = sites_description_rate;
            sessionService = new SessionService(this.sessions);
        }//end Constructor

        // For Partial View : Selected
        public ActionResult UpdateSelectedList()
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _selected = totals_per_site.GetAll();
            _selected = _selected.Where(q => q.idSession == _session.ID).OrderByDescending(o => o.idSelected);

            return PartialView("Selected", _selected);
        }

        public ActionResult GetSessionGUID()
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _IPMEvent = ipmevents.GetById(_session.idIPMEvent);
            string sessionSummary = "sessionID: " + _session.ID +
                " sessionGUID: " + _session.sessionGUID +
                " IPMEvent: " + _IPMEvent.year;
            return Json(sessionSummary);
        }

        // Search results for autocomplete dropdown list
        public ActionResult SearchCustomerByNameOrPhoneResult(string query)
        {
            return Json(SearchCustomerByNameOrPhoneList(query).Select(c => new { label = c.Label, ID = c.ID }));
        }
        private List<SelectionOptionID> SearchCustomerByNameOrPhoneList(string searchString)
        {
            //Return value
            List<SelectionOptionID> results = new List<SelectionOptionID>();
            if (searchString != null)
            {
                //Regex for phone number
                Regex rgx = new Regex("[^0-9]");

                //Read customer data
                var allCustomers = customers.GetAll();

                //Check if search is by phone number or by customer name
                if (searchString.Any(char.IsDigit))
                {
                    searchString = rgx.Replace(searchString, "");
                    //Filter by phone number
                    foreach (customer_view customer in allCustomers)
                    {
                        string phoneNumber = rgx.Replace(customer.mainPhone, "");
                        if (phoneNumber.Contains(searchString))
                        {
                            results.Add(new SelectionOptionID(customer.id, customer.fullName + " - Phone: " + customer.mainPhone));
                        }
                        if (results.Count() > 5)
                        {
                            results.Add(new SelectionOptionID(-1, "..."));
                            return results;
                        }
                    };
                }
                else
                {
                    //Filter by customer name
                    allCustomers = allCustomers.Where(s => s.fullName.ToUpper().Contains(searchString));
                    if (allCustomers != null)
                        foreach (customer_view customer in allCustomers)
                        {
                            {
                                results.Add(new SelectionOptionID(customer.id, customer.fullName + " - Phone: " + customer.mainPhone));
                            }
                            if (results.Count() > 5)
                            {
                                results.Add(new SelectionOptionID(-1, "..."));
                                return results;
                            }
                        };
                }
            }
            return results;
        }

        // Search results for autocomplete dropdown list
        public ActionResult SearchSiteByNameResult(string query)
        {
            return Json(SearchSiteByName(query).Select(c => new { label = c.Label, ID = c.ID }));
        }
        private List<SelectionOptionID> SearchSiteByName(string searchString)
        {
            //Return value
            List<SelectionOptionID> results = new List<SelectionOptionID>();

            //Regex for site name
            Regex rgx = new Regex("[^a-zA-Z0-9]");

            //Read RVSite available
            var allRVSites = rvsites_available.GetAll();

            //Remove characters from search string
            searchString = rgx.Replace(searchString, "").ToUpper();

            if (searchString != null)
            {
                //Filter by RV Site
                foreach (rvsite_available_view rvsite in allRVSites)
                {
                    string rvsiteShort = rgx.Replace(rvsite.site, "").ToUpper();
                    if (rvsiteShort.Contains(searchString))
                    {
                        results.Add(new SelectionOptionID(rvsite.id, rvsite.site));
                    }
                    if (results.Count() > 25)
                    {
                        results.OrderBy(q => q.Label).ToList();
                        results.Add(new SelectionOptionID(-1, "..."));
                        return results;
                    }
                }
            }

            return results.OrderBy(q => q.Label).ToList();
        }

        // New Reservation page - In fact, this page creates a new "selected"
        public ActionResult Reservation()
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _IPMEvent = ipmevents.GetById(_session.idIPMEvent);

            var start = _IPMEvent.startDate.Value;
            var end = _IPMEvent.endDate.Value;
            var now = DateTime.Now;
            var min = start - now;
            var max = end - now;

            ViewBag.startDate = (int)min.TotalDays + 1;
            ViewBag.minDate = ViewBag.startDate - 7;
            ViewBag.maxDate = (int)max.TotalDays + 1;

            var _selected = new selected();
            return View(_selected);
        }

        [HttpPost]
        public ActionResult GetSiteTotal(long idRVSite, DateTime checkInDate, DateTime checkOutDate)
        {
            double amount = 0;
            var site = rvsites_available.GetAll().Where(s => s.id == idRVSite).First();
            if (site != null)
            {                
                int duration = (int)(checkOutDate - checkInDate).TotalDays + 1;
                int weeks = duration / 7;
                int days = duration % 7;
                amount = Convert.ToDouble(site.weeklyRate) * weeks +
                    Convert.ToDouble(site.dailyRate) * days;
            }
            string result = amount.ToString("C");
            return Json(result);
        }

        [HttpPost]
        public ActionResult Reservation(long idRVSite, DateTime checkInDate, DateTime checkOutDate)
        {
            var _selected = new selected();

            _selected.checkInDate = checkInDate;
            _selected.checkOutDate = checkOutDate;
            _selected.idRVSite = idRVSite;
            var _session = sessionService.GetSession(this.HttpContext);
            _selected.idSession = _session.ID;
            _selected.idIPMEvent = _session.idIPMEvent;
            _selected.idStaff = _session.idStaff;
            _selected.idCustomer = _session.idCustomer;
            _selected.isSiteChecked = true;
            _selected.createDate = DateTime.Now;
            _selected.lastUpdate = DateTime.Now;

            selecteds.Insert(_selected);
            selecteds.Commit();

            return Json(idRVSite);
        }

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            var reservation = reservations.GetAll();

            if (!String.IsNullOrEmpty(searchString))
            {
                reservation = reservation.Where(s => s.fullName.Contains(searchString));
            }

            return View(reservation);
        }

        // GET: /Details/5
        public ActionResult Details(int? id)
        {
            var reservation = reservations.GetById(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }

        // GET: /Create
        public ActionResult Create()
        {
            var reservation = new reservation_view();
            return View(reservation);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(reservation_view reservation)
        {
            reservations.Insert(reservation);
            reservations.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Edit/5
        public ActionResult Edit(int id)
        {
            reservation_view reservation = reservations.GetById(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(reservation_view reservation)
        {
            reservations.Update(reservation);
            reservations.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Delete/5
        public ActionResult Delete(int id)
        {
            reservation_view reservation = reservations.GetById(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            reservations.Delete(reservations.GetById(id));
            reservations.Commit();
            return RedirectToAction("Index");
        }

        public ActionResult RemoveSelected(int id)
        {
            selecteds.Delete(selecteds.GetById(id));
            selecteds.Commit();
            return RedirectToAction("Reservation");
        }

    }
}
