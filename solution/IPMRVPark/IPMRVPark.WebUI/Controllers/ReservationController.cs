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
        IRepositoryBase<session> sessions;
        SessionService sessionService;

        public ReservationController(IRepositoryBase<reservation_view> reservations_view,
            IRepositoryBase<customer_view> customers,
            IRepositoryBase<session> sessions)
        {
            this.reservations_view = reservations_view;
            this.customers = customers;
            this.sessions = sessions;
            sessionService = new SessionService(this.sessions);
        }//end Constructor

        public ActionResult GetSessionGUID()
        {
            var result = sessionService.GetSession(this.HttpContext);
            string sessionSummary = "sessionID:" + result.ID +
                " sessionGUID:" + result.sessionGUID;
            return Json(sessionSummary);
        }

        // Search results for autocomplete dropdown list
        public ActionResult SearchByNameOrPhoneResult(string query)
        {
            return Json(SearchByNameOrPhoneList(query).Select(c => new { label = c.Label, ID = c.ID }));
        }
        private List<SelectionOptionID> SearchByNameOrPhoneList(string searchString)
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
                    //Filter by phone name
                    allCustomers = allCustomers.Where(s => s.fullName.Contains(searchString));
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


        // New Reservation page
        public ActionResult Reservation()
        {
            return View();
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
    }
}
