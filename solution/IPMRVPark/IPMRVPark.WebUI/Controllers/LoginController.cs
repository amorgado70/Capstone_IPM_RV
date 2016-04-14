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
    public class LoginController : Controller
    {
        IRepositoryBase<staff> users;
        IRepositoryBase<customer_view> customers;
        IRepositoryBase<ipmevent> ipmevents;
        IRepositoryBase<session> sessions;
        IRepositoryBase<placeinmap> placesinmap;
        IRepositoryBase<selecteditem> selecteditems;
        IRepositoryBase<reservationitem> reservationitems;
        IRepositoryBase<payment> payments;
        IRepositoryBase<person> persons;
        IRepositoryBase<paymentreservationitem> paymentsreservationitems;
        IRepositoryBase<rvsite_available_view> rvsites_available;
        IRepositoryBase<site_description_rate_view> sites_description_rate;
        SessionService sessionService;
        PaymentService paymentService;

        public LoginController(
            IRepositoryBase<staff> users,
            IRepositoryBase<customer_view> customers,
            IRepositoryBase<ipmevent> ipmevents,
            IRepositoryBase<placeinmap> placesinmap,
            IRepositoryBase<rvsite_available_view> rvsites_available,
            IRepositoryBase<selecteditem> selecteditems,
            IRepositoryBase<reservationitem> reservationitems,
            IRepositoryBase<payment> payments,
            IRepositoryBase<person> persons,
            IRepositoryBase<paymentreservationitem> paymentsreservationitems,
            IRepositoryBase<session> sessions,
            IRepositoryBase<site_description_rate_view> sites_description_rate
            )
        {
            this.users = users;
            this.customers = customers;
            this.ipmevents = ipmevents;
            this.payments = payments;
            this.persons = persons;
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

        const long IDnotFound = -1;

        public ActionResult Home()
        {
            return RedirectToAction("Login");
        }

        public ActionResult Menu()
        {
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);

            return View();
        }

        public ActionResult Logout()
        {
            // Clean selected items
            long sessionID = sessionService.GetSessionID(this.HttpContext);
            paymentService.CleanAllSelectedItems(sessionID);
            return View();
        }

        public ActionResult Login()
        {
            
            List<SelectListItem> items = new List<SelectListItem>();
            var _ipmevents = ipmevents.GetAll().OrderBy(y => y.year);
            long sessionID = sessionService.GetSessionID(this.HttpContext);
            long IPMEventID = sessionService.GetSessionIPMEventID(sessionID);

            // Clean selected items
            paymentService.CleanAllSelectedItems(sessionID);

            foreach (ipmevent _ipmevent in _ipmevents)
            {
                SelectListItem i = new SelectListItem();
                i.Value = _ipmevent.ID.ToString();
                i.Text = _ipmevent.year.ToString();
                items.Add(i);
                if (_ipmevent.ID == IPMEventID)
                {
                    i.Selected = true;
                }
            }
            ViewBag.IPMEventYear = items;
            return View();
        }

        [HttpPost]
        public ActionResult GetSessionEmail()
        {
            SelectionOptionID user = new SelectionOptionID(IDnotFound, "");
            var _session = sessionService.GetSession(this.HttpContext);
            if (_session.idStaff != null)
            {
                staff _user = users.GetById(_session.idStaff);
                if (_user != null)
                {
                    user.ID = _session.idStaff.Value;
                    user.Label = _user.person.email;
                };
            };
            return Json(user);
        }

        [HttpPost]
        public ActionResult GetSessionYear()
        {
            string year = ipmevents.GetById(sessionService.GetSession(this.HttpContext).idIPMEvent).year.ToString();
            return Json(year);
        }

        [HttpPost]
        public ActionResult SelectUser(string userEmail, string userPassword)
        {
            SelectionOptionID user = new SelectionOptionID(IDnotFound, "");
            if (userEmail != null && userPassword != null)
            {
                var _session = sessionService.GetSession(this.HttpContext);


                bool personFound = false;
                try //checks if person is in database
                {
                    var _person = persons.GetAll().Where(u => u.email == userEmail && u.password == userPassword).
                        FirstOrDefault();
                    personFound = !(_person.Equals(default(person)));
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: '{0}'", e);
                }
                // Person found in database
                if (personFound)
                {
                    user.ID = persons.GetAll().Where(q => q.email == userEmail && q.password == userPassword).First().ID;
                    user.Label = userEmail;
                    _session.idStaff = user.ID;
                }
                else
                {
                    _session.idStaff = null;

                }
                sessions.Update(sessions.GetById(_session.ID));
                sessions.Commit();
            }
            return Json(user);
        }

        [HttpPost]
        public ActionResult ChangeYear(string idIPMEvent)
        {
            var _session = sessionService.GetSession(this.HttpContext);
            _session.idIPMEvent = long.Parse(idIPMEvent);
            sessions.Update(sessions.GetById(_session.ID));
            sessions.Commit();

            return Json(idIPMEvent);
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

        public ActionResult GetSessionCustomer()
        {
            SelectionOptionID customer = new SelectionOptionID(-1, "");
            var _session = sessionService.GetSession(this.HttpContext);
            if (_session.idCustomer != null)
            {
                var _customer = customers.GetAll().Where(c => c.id == _session.idCustomer).First();
                if (_customer != null)
                {
                    customer.ID = _session.idCustomer.Value;
                    customer.Label = _customer.fullName + " - Phone: " + _customer.mainPhone;
                };
            };
            return Json(customer);
        }

        [HttpPost]
        public ActionResult SelectCustomer(long idCustomer)
        {
            session _session = sessions.GetById(sessionService.GetSession(this.HttpContext).ID);
            _session.idCustomer = idCustomer;

            long sessionID = _session.ID;
            var _selecteditems = selecteditems.GetAll().Where(s => s.idSession == sessionID);
            foreach (selecteditem item in _selecteditems)
            {
                item.idCustomer = idCustomer;
                selecteditems.Update(item);
            }
            sessions.Update(_session);

            selecteditems.Commit();
            sessions.Commit();
            return Json(idCustomer);
        }
    }

}
