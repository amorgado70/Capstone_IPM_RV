using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;

namespace IPMRVPark.WebUI.Controllers
{
    public class PrototypeUIController : Controller
    {
        public ActionResult Home()
        {
            return View();
        }
        public ActionResult Menu()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult IPMEventMap()
        {
            return View();
        }        
        public ActionResult IPMEventInfo()
        {
            return View();
        }
        public ActionResult DigitizeMap()
        {
            return View();
        }
        public ActionResult Reservation()
        {
            return View();
        }
        public ActionResult ReservationSearch()
        {
            return View();
        }
        public ActionResult Customer()
        {
            return View();
        }
        public ActionResult CustomerSearch()
        {
            return View();
        }
        public ActionResult Payment()
        {
            return View();
        }
        public ActionResult Refund()
        {
            return View();
        }
        public ActionResult PaymentSearch()
        {
            return View();
        }
        public ActionResult ReservationOrderIndex()
        {
            return View();
        }
        public ActionResult ReservationOrderEdit()
        {
            return View();
        }
    }
}
