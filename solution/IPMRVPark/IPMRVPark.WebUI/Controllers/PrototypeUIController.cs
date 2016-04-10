using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;
using System.IO;
using System.Web;
using IPMRVPark.Services;
using System.Xml;
using System.Xml.Linq;

namespace IPMRVPark.WebUI.Controllers
{
    public class PrototypeUIController : Controller
    {
        public KMLParser kmlParser;

        public PrototypeUIController()
        {
            kmlParser = new KMLParser();
        }
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
        public ActionResult DigitizeMap_parse()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DigitizeMap_parse(HttpPostedFileBase file, long eventId)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    XmlReader reader = XmlReader.Create(file.InputStream);
                    XDocument xDoc = System.Xml.Linq.XDocument.Load(reader);
                    kmlParser.Parse(xDoc, eventId);
                    ViewBag.Message = "File is uploaded and parsed successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }

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
