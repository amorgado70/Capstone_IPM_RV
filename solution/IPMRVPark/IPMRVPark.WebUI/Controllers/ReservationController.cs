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
        IRepositoryBase<reservation_view> reservations_view;
        IRepositoryBase<customer_view> customers;
        IRepositoryBase<ipmevent> ipmevents;
        IRepositoryBase<session> sessions;
        IRepositoryBase<selected> selecteds;
        IRepositoryBase<rvsite_available> rvsites_available;
        IRepositoryBase<total_per_site_view> totals_per_site_view;
        SessionService sessionService;

        public ReservationController(IRepositoryBase<reservation_view> reservations_view,
            IRepositoryBase<customer_view> customers,
            IRepositoryBase<ipmevent> ipmevents,
            IRepositoryBase<rvsite_available> rvsites_available,
            IRepositoryBase<selected> selecteds,
            IRepositoryBase<total_per_site_view> totals_per_site_view,
            IRepositoryBase<session> sessions)
        {
            this.reservations_view = reservations_view;
            this.customers = customers;
            this.ipmevents = ipmevents;
            this.selecteds = selecteds;
            this.sessions = sessions;
            this.totals_per_site_view = totals_per_site_view;
            this.rvsites_available = rvsites_available;
            sessionService = new SessionService(this.sessions);
        }//end Constructor

        // For Partial View : Selected
        public ActionResult UpdateSelectedList()
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _selected = totals_per_site_view.GetAll();
            _selected = _selected.Where(q => q.idSession == _session.ID);

            return PartialView("Selected", _selected);
        }

        public ActionResult GetSessionGUID()
        {
            var result = sessionService.GetSession(this.HttpContext);
            string sessionSummary = "sessionID:" + result.ID +
                " sessionGUID:" + result.sessionGUID;
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
                foreach (rvsite_available rvsite in allRVSites)
                {
                    string rvsiteShort = rgx.Replace(rvsite.site, "").ToUpper();
                    if (rvsiteShort.Contains(searchString))
                    {
                        results.Add(new SelectionOptionID(rvsite.id, rvsite.site));
                    }
                    if (results.Count() > 25)
                    {
                        results.Add(new SelectionOptionID(-1, "..."));
                        return results;
                    }
                }
            }

            return results;
        }

        // New Reservation page - In fact, this page creates a new "selected"
        public ActionResult Reservation()
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _IPMEvent = ipmevents.GetById(_session.idIPMEvent);

            ViewBag.startDate = DateTime.Parse(_IPMEvent.startDate.ToString()).ToString("yyyy-MM-dd");

            ViewBag.minDate = DateTime.Parse(_IPMEvent.startDate.ToString()).ToString("yyyy-MM-dd");
            ViewBag.maxDate = DateTime.Parse(_IPMEvent.endDate.ToString()).ToString("yyyy-MM-dd");

            var _selected = new selected();
            return View(_selected);
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
            var reservation_view = reservations_view.GetAll();

            if (!String.IsNullOrEmpty(searchString))
            {
                reservation_view = reservation_view.Where(s => s.fullName.Contains(searchString));
            }

            return View(reservation_view);
        }

        // GET: /Details/5
        public ActionResult Details(int? id)
        {
            var reservation_view = reservations_view.GetById(id);
            if (reservation_view == null)
            {
                return HttpNotFound();
            }
            return View(reservation_view);
        }

        // GET: /Create
        public ActionResult Create()
        {
            var reservation_view = new reservation_view();
            return View(reservation_view);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(reservation_view reservation_view)
        {
            reservations_view.Insert(reservation_view);
            reservations_view.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Edit/5
        public ActionResult Edit(int id)
        {
            reservation_view reservation_view = reservations_view.GetById(id);
            if (reservation_view == null)
            {
                return HttpNotFound();
            }
            return View(reservation_view);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(reservation_view reservation_view)
        {
            reservations_view.Update(reservation_view);
            reservations_view.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Delete/5
        public ActionResult Delete(int id)
        {
            reservation_view reservation_view = reservations_view.GetById(id);
            if (reservation_view == null)
            {
                return HttpNotFound();
            }
            return View(reservation_view);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            reservations_view.Delete(reservations_view.GetById(id));
            reservations_view.Commit();
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
