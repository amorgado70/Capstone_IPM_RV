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
        IRepositoryBase<staff_view> users;
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
            IRepositoryBase<staff_view> users,
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

        const long IDnotFound = -1;

        public ActionResult Home()
        {
            return RedirectToAction("Login");
        }

        public ActionResult Menu()
        {
            // Identify session
            long sessionID = sessionService.GetSessionID(this.HttpContext, true, false);
            long IPMEventID = sessionService.GetSessionIPMEventID(sessionID);
            long userID = sessionService.GetSessionUserID(this.HttpContext, false, false);

            ViewBag.UserID = userID;
            paymentService.CleanOldSelectedItem(IPMEventID, userID);

            return View();
        }

        public ActionResult Logout()
        {
            // Clean selected items
            long sessionID = sessionService.GetSessionID(this.HttpContext, false, false);
            paymentService.CleanAllSelectedItems(sessionID, IDnotFound);
            return View();
        }

        public ActionResult Login()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            var _ipmevents = ipmevents.GetAll().OrderBy(y => y.year);
            long sessionID = sessionService.GetSessionID(this.HttpContext, false, false);
            long userID = sessionService.GetSessionUserID(this.HttpContext, false, false);
            long IPMEventID = sessionService.GetSessionIPMEventID(sessionID);

            // Clean selected items
            paymentService.CleanAllSelectedItems(sessionID, userID);

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

            // This is used to chek password
            var _session = sessionService.GetSession(this.HttpContext, false, false);
            ViewBag.P1 = _session.sessionGUID.Substring(0, 12);
            ViewBag.P2 = _session.sessionGUID.Substring(11, 12);

            return View();
        }

        public ActionResult ChangePassword()
        {
            long userID = sessionService.GetSessionUserID(this.HttpContext, true, false);

            // This is used to chek password
            var _session = sessionService.GetSession(this.HttpContext, false, false);
            ViewBag.P1 = _session.sessionGUID.Substring(0, 12);
            ViewBag.P2 = _session.sessionGUID.Substring(11, 12);

            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(string userEmail, string userPassword,
            string P1, string P2, string enterPassword)
        {
            person _person = new person();

            if (userEmail != null && userPassword != null)
            {
                var _session = sessionService.GetSession(this.HttpContext, false, false);
                bool personFound = false;
                bool userAuthor = false;

                string xP1 = _session.sessionGUID.Substring(0, 12);
                string xP2 = _session.sessionGUID.Substring(11, 12);

                if (P1 == xP1 && P2 == xP2)
                {
                    try //checks if person is in database
                    {
                        _person = persons.GetAll().Where(u => u.email == userEmail).
                            FirstOrDefault();
                        personFound = !(_person.Equals(default(person)));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("An error occurred: '{0}'", e);
                    }
                }
                // Person found in database
                if (personFound)
                {
                    userAuthor = sessionService.VerifyHash(_person.ID, userPassword);
                }
                // User is authorized
                if (userAuthor)
                {
                    _person.password = sessionService.GetHash(enterPassword);
                    persons.Update(_person);
                    persons.Commit();
                    _session.idStaff = null;
                    sessions.Update(_session);
                    sessions.Commit();
                    return Json(new { ID = _person.ID });
                }
            }
            return Json(new { ID = IDnotFound });
        }

        public ActionResult ResetPassword(long personID)
        {
            var sessionID = sessionService.GetSessionID(this.HttpContext, true, true);
            person _person = new person();
            bool personFound = false;

            try //checks if person is in database
            {
                _person = persons.GetById(personID);
                personFound = !(_person.Equals(default(person)));
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }

            // Person has been found, reset password
            if (personFound)
            {
                _person.password = sessionService.GetHash("012345");
                persons.Update(_person);
                persons.Commit();
            }

            return RedirectToAction("ResetPassword","Staff");
        }

        [HttpPost]
        public ActionResult GetSessionEmail()
        {
            SelectionOptionID user = new SelectionOptionID(IDnotFound, "");
            var _session = sessionService.GetSession(this.HttpContext, false, false);
            if (_session.idStaff != null)
            {
                long userID = _session.idStaff.Value;
                var _user = users.GetByKey("id", userID);
                if (_user != null)
                {
                    user.ID = _session.idStaff.Value;
                    user.Label = _user.email;
                };
            };
            return Json(user);
        }

        [HttpPost]
        public ActionResult GetSessionYear()
        {
            string year = ipmevents.GetById(sessionService.GetSession(this.HttpContext, false, false).idIPMEvent).year.ToString();
            return Json(year);
        }

        [HttpPost]
        public ActionResult SelectUser(string userEmail, string userPassword, string P1, string P2)
        {

            SelectionOptionID user = new SelectionOptionID(IDnotFound, "");
            person _person = new person();

            if (userEmail != null && userPassword != null)
            {
                var _session = sessionService.GetSession(this.HttpContext, false, false);
                bool personFound = false;
                bool userAuthor = false;

                string xP1 = _session.sessionGUID.Substring(0, 12);
                string xP2 = _session.sessionGUID.Substring(11, 12);

                if (P1 == xP1 && P2 == xP2)
                {
                    try //checks if person is in database
                    {
                        _person = persons.GetAll().Where(u => u.email == userEmail).
                            FirstOrDefault();
                        personFound = !(_person.Equals(default(person)));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("An error occurred: '{0}'", e);
                    }
                }
                // Person found in database
                if (personFound)
                {
                    userAuthor = sessionService.VerifyHash(_person.ID, userPassword);
                }
                // User is authorized
                if (userAuthor)
                {
                    user.ID = _person.ID;
                    user.Label = userEmail;
                    _session.idStaff = user.ID;
                }
                else
                {
                    user.ID = IDnotFound;
                    user.Label = string.Empty;
                    _session.idStaff = null;
                }
                sessions.Update(_session);
                sessions.Commit();
            }
            return Json(user);
        }

        [HttpPost]
        public ActionResult ChangeYear(string idIPMEvent)
        {
            var _session = sessionService.GetSession(this.HttpContext, true, false);
            _session.idIPMEvent = long.Parse(idIPMEvent);
            sessions.Update(sessions.GetById(_session.ID));
            sessions.Commit();

            return Json(idIPMEvent);
        }

        public ActionResult GetSessionGUID()
        {
            string sessionSummary = string.Empty;
            return Json(sessionSummary);
        }

        public ActionResult GetSessionCustomer()
        {
            SelectionOptionID customer = new SelectionOptionID(-1, "");
            var _session = sessionService.GetSession(this.HttpContext, true, false);
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
            session _session = sessions.GetById(sessionService.GetSession(this.HttpContext, true, false).ID);
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
            long UserID = sessionService.GetSessionUserID(this.HttpContext, true, false);
            long sessionID = sessionService.GetSessionID(this.HttpContext, false, false);
            long IPMEventID = sessionService.GetSessionIPMEventID(sessionID);
            ViewBag.IPMEventYear = ipmevents.GetById(IPMEventID).year;
            return View();
        }
        [HttpPost, ActionName("CleanIPMEventData")]
        [ValidateAntiForgeryToken]
        public ActionResult CleanIPMEventDataConfirm()
        {
            long UserID = sessionService.GetSessionUserID(this.HttpContext, true, false);
            long sessionID = sessionService.GetSessionID(this.HttpContext, false, false);
            long IPMEventID = sessionService.GetSessionIPMEventID(sessionID);

            paymentsreservationitems.BulkDelete("paymentreservationitem", "idIPMEvent", IPMEventID);
            selecteditems.BulkDelete("selecteditem", "idIPMEvent", IPMEventID);
            reservationitems.BulkDelete("reservationitem", "idIPMEvent", IPMEventID);
            coordinates.BulkDelete("coordinates", "idIPMEvent", IPMEventID);
            placesinmap.BulkDelete("placeinmap", "idIPMEvent", IPMEventID);
            siterates.BulkDelete("siterate", "idIPMEvent", IPMEventID);
            sitetypes.BulkDelete("sitetype", "idIPMEvent", IPMEventID);
            stylesurl.BulkDelete("styleurl", "idIPMEvent", IPMEventID);
            payments.BulkDelete("payment", "idIPMEvent", IPMEventID);

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

            return RedirectToAction("Home", "Login");
        }

        public ActionResult Admin()
        {
            sessionService.GetSessionUserID(this.HttpContext, true, true);

            return View();
        }
    }

}
