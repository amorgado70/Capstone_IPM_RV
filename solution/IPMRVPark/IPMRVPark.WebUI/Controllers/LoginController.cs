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
        IRepositoryBase<coordinate> coordinates;
        IRepositoryBase<sitetype> sitetypes;
        IRepositoryBase<siterate> siterates;
        IRepositoryBase<styleurl> stylesurl;        
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
            IRepositoryBase<coordinate> coordinates,
            IRepositoryBase<sitetype> sitetypes,
            IRepositoryBase<siterate> siterates,
            IRepositoryBase<styleurl> stylesurl,
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
            this.coordinates = coordinates;
            this.sitetypes = sitetypes;
            this.siterates = siterates;
            this.stylesurl = stylesurl;
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

        public ActionResult CleanIPMEventData()
        {
            long UserID = sessionService.GetSessionUserID(this.HttpContext);
            long sessionID = sessionService.GetSessionID(this.HttpContext);
            long IPMEventID = sessionService.GetSessionIPMEventID(sessionID);
            ViewBag.IPMEventYear = ipmevents.GetById(IPMEventID).year;
            return View();
        }
        [HttpPost, ActionName("CleanIPMEventData")]
        [ValidateAntiForgeryToken]
        public ActionResult CleanIPMEventDataConfirm()
        {
            long UserID = sessionService.GetSessionUserID(this.HttpContext);
            long sessionID = sessionService.GetSessionID(this.HttpContext);
            long IPMEventID = sessionService.GetSessionIPMEventID(sessionID);

            // Payment Reservation Coordination
            //var _paymentreservationitems = paymentsreservationitems.GetAll().Where(pri => pri.idIPMEvent == IPMEventID).ToList();
            //foreach (var _paymentreservationitem in _paymentreservationitems)
            //{
            //    paymentsreservationitems.Delete(_paymentreservationitem.ID);
            //}
            //paymentsreservationitems.Commit();

            paymentsreservationitems.BulkDelete("paymentreservationitem","idIPMEvent", IPMEventID);

            //// Selected Item
            //var _selecteditems = selecteditems.GetAll().Where(si => si.idIPMEvent == IPMEventID).ToList();
            //foreach (var _selecteditem in _selecteditems)
            //{
            //    selecteditems.Delete(_selecteditem.ID);
            //}
            //selecteditems.Commit();

            selecteditems.BulkDelete("selecteditem", "idIPMEvent", IPMEventID);

            //// Reservation Item
            //var _reservationitems = reservationitems.GetAll().Where(ri => ri.idIPMEvent == IPMEventID).ToList();
            //foreach (var _reservationitem in _reservationitems)
            //{
            //    reservationitems.Delete(_reservationitem.ID);
            //}
            //reservationitems.Commit();

            reservationitems.BulkDelete("reservationitem", "idIPMEvent", IPMEventID);

            // Place In Map or RV Site
            //var _placesinmap = placesinmap.GetAll().Where(pim => pim.idIPMEvent == IPMEventID).ToList();
            //foreach (var _placeinmap in _placesinmap)
            //{
            // Coordinates
            //var _coordinates = coordinates.GetAll().Where(c => c.idPlaceInMap == _placeinmap.ID).ToList();
            //if (_coordinates != null)
            //{
            //foreach(var _coordinate in _coordinates)
            //{
            //    coordinates.Delete(_coordinate.ID);
            //}
            //}
            //placesinmap.Delete(_placeinmap.ID);
            //}
            //coordinates.Commit();
            //placesinmap.Commit();

            coordinates.BulkDelete("coordinates", "idIPMEvent", IPMEventID);
            placesinmap.BulkDelete("placeinmap", "idIPMEvent", IPMEventID);

            // Site Rates
            //var _siterates = siterates.GetAll().Where(sr => sr.idIPMEvent == IPMEventID).ToList();
            //if (_siterates != null)
            //{
            //    foreach (var _siterate in _siterates)
            //    {
            //        siterates.Delete(_siterate.ID);
            //    }
            //    siterates.Commit();
            //}
            siterates.BulkDelete("siterate", "idIPMEvent", IPMEventID);

            // Site Types
            //var _sitetypes = sitetypes.GetAll().Where(st => st.idIPMEvent == IPMEventID).ToList();
            //if (_sitetypes != null)
            //{
            //    foreach (var _sitetype in _sitetypes)
            //    {
            //        sitetypes.Delete(_sitetype.ID);
            //    }
            //    sitetypes.Commit();
            //}
            sitetypes.BulkDelete("sitetype", "idIPMEvent", IPMEventID);

            // Style Url
            //var _stylesurl = stylesurl.GetAll().Where(su => su.idIPMEvent == IPMEventID).ToList();
            //if (_stylesurl != null)
            //{
            //    foreach (var _styleurl in _stylesurl)
            //    {
            //        stylesurl.Delete(_styleurl.ID);
            //    }
            //    stylesurl.Commit();
            //}
            stylesurl.BulkDelete("styleurl", "idIPMEvent", IPMEventID);

            var _ipmevent = ipmevents.GetById(IPMEventID);
            _ipmevent.startDate = DateTime.MinValue;
            _ipmevent.lastDateRefund = DateTime.MaxValue;
            _ipmevent.endDate = DateTime.MaxValue;
            _ipmevent.lastUpdate = DateTime.Now;
            _ipmevent.mapFileUrl = null;
            _ipmevent.description = null;
            _ipmevent.local = null;
            _ipmevent.street = null;
            _ipmevent.city = null;
            _ipmevent.provinceCode = null;
            ipmevents.Update(_ipmevent);
            ipmevents.Commit();            

            return RedirectToAction("Home","Login");
        }

    }

}
