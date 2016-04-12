using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;
using System.Web;
using IPMRVPark.Services;
using System.Xml;
using System.Xml.Linq;
using IPMRVPark.Contracts.Data;
using System.Data.Objects;
using System.Collections.Generic;

namespace IPMRVPark.WebUI.Controllers
{
    public class EventMapController : Controller
    {
        IRepositoryBase<ipmevent> events;
        IRepositoryBase<placeinmap> places;
        IRepositoryBase<coordinate> coords;
        IRepositoryBase<styleurl> styles;
        IRepositoryBase<sitetype> types;
        IRepositoryBase<sitesize> sizes;
        IRepositoryBase<service> services;
        IRepositoryBase<sitetype_service_rate_view> typeRates;
        IRepositoryBase<reservationitem> reservations;
        IRepositoryBase<siterate> rates;
        IRepositoryBase<rvsite_status_view> status;
        IRepositoryBase<customer_view> customers;
        IRepositoryBase<session> sessions;


        public EventMapController(IRepositoryBase<ipmevent> events,
            IRepositoryBase<placeinmap> places,
            IRepositoryBase<coordinate> coords,
            IRepositoryBase<styleurl> styles,
            IRepositoryBase<sitetype> types,
            IRepositoryBase<sitesize> sizes,
            IRepositoryBase<service> services,
            IRepositoryBase<sitetype_service_rate_view> typeRates,
            IRepositoryBase<reservationitem> reservations,
            IRepositoryBase<siterate> rates,
            IRepositoryBase<rvsite_status_view> status,
            IRepositoryBase<customer_view> customers,
            IRepositoryBase<session> sessions)
        {
            this.events = events;
            this.places = places;
            this.coords = coords;
            this.styles = styles;
            this.types = types;
            this.sizes = sizes;
            this.services = services;
            this.typeRates = typeRates;
            this.reservations = reservations;
            this.rates = rates;
            this.status = status;
            this.customers = customers;
            this.sessions = sessions;
        }

        public ActionResult IPMEventMap()
        {
            SessionService sessionService = new SessionService(
               this.sessions,
               this.customers
               );

            long sessionID = sessionService.GetSessionID(this.HttpContext);
            long eventId = sessionService.GetSessionIPMEventID(sessionID);

            //long eventId = events.GetQueryable().Select(x => x.ID).DefaultIfEmpty().Max();
            var year = events.GetQueryable().Where(x => x.ID == eventId).FirstOrDefault<ipmevent>().year;
            Polygons poly = Polygons.GetInstance();
            resetPolygons(poly, eventId, year);
            // in case of no event
            if( poly._leftTop != null)
            {
                ViewBag.Lat = (poly._leftTop._x + poly._rightBottom._x) / 2;
                ViewBag.Lng = (poly._leftTop._y + poly._rightBottom._y) / 2;
            }
            else
            {
                ViewBag.Lat = 0;
                ViewBag.Lng = 0;
            }
                
            return View(poly);
        }
        public void resetPolygons(Polygons poly, long eventId, long year)
        {
            if (poly.eventId != eventId || !poly.Initialized)
            {
                poly.AddSize(sizes.GetAll());
                poly.AddService(services.GetAll());
                poly.AddStyle(styles.GetQueryable().Where(x => x.idIPMEvent == eventId).OrderByDescending(x => x.ID).ToList());
                poly.AddType(types.GetQueryable().Where(x => x.idIPMEvent == eventId).ToList());
                poly.AddSite(places.GetQueryable().Where(x => x.idIPMEvent == eventId).ToList());
                poly.AddCoordinates(coords.GetQueryable().Where(x => x.idIPMEvent == eventId).ToList());
                poly.UpdateSite(status.GetQueryable().Where(x => x.Year == year).ToList());
                
                poly.Initialized = true;
                poly.eventId = eventId;
            }
        }
        public ActionResult IPMEventInfo()
        {
            var model = events.GetAll();
            return View(model);
        }
        // partial view of IPMEventInfo()
        public ActionResult SiteTypes(long year, long eventId )
        {
            var tRates = typeRates.GetQueryable().Where(x => x.eventId == eventId).ToList();

            // Try to get in memory sitetype_service_rate_view from poly for staff to continue to edit type
            Polygons poly = Polygons.GetInstance();
            poly.GetTypeRates(tRates, eventId);
            ViewBag.eventId = eventId;

            // send size, service
            ViewBag.sizes = sizes.GetAll();
            ViewBag.services = services.GetAll();
            ViewBag.eventStarted = isEventStarted(year);

            return PartialView("SiteTypes", tRates);
        }
        // method for temporary type & rate
        public ActionResult AddTypeRate(long eventId, long serviceId, string service, long sizeId, string size, decimal week,
            decimal day)
        {
            bool ret = true;
            string errMsg = "";
            Polygons poly = Polygons.GetInstance();

            // check validation in terms of key
            //  1. check with table records
            var _type = types.GetQueryable().Where(x => x.idIPMEvent == eventId && x.idService == serviceId && x.idSiteSize == sizeId).FirstOrDefault<sitetype>();
            if (_type != null ||
                poly.TypeRates.Where(x => x.eventId == eventId && x.serviceId == serviceId && x.sizeId == sizeId).FirstOrDefault<sitetype_service_rate_view>() != null)
            {
                ret = false;
                errMsg = string.Format("Same siteType with service({0}) and size({1}) is already exists!!", service, size);
            }

            if (ret)
                poly.AddTypeRate(eventId, serviceId, service, sizeId, size, week, day);

            return Json(new
            {
                success = ret,
                msg = errMsg
            }, JsonRequestBehavior.AllowGet);
        }
        // method for delete type & rate
        public ActionResult DeleteTypeRate(long eventId, long serviceId, string service, long sizeId, string size)
        {
            bool ret = true;
            string errMsg = "";

            Polygons poly = Polygons.GetInstance();
            if (!poly.DeleteTypeRate(eventId, serviceId, sizeId))
            {
                var _type = types.GetQueryable().Where(x => x.idIPMEvent == eventId && x.idService == serviceId && x.idSiteSize == sizeId).FirstOrDefault<sitetype>();
                if (_type == null || !delete_sitetype_dependants(_type.ID )
                    )
                {
                    ret = false;
                    errMsg = string.Format("Type( service{0}, size{1}) : deletion Failed", service, size);
                }
            }

            return Json(new
            {
                success = ret,
                msg = errMsg
            }, JsonRequestBehavior.AllowGet);
        }
        // method for database saving type & rate
        public ActionResult VerifyAndSaveTypes(string year, string city, string province, string street,
            DateTime startDate, DateTime endDate, DateTime lastDateRefund)
        {
            bool ret = true;
            string errMsg = "";
            long _year = 0;
            long.TryParse(year, out _year);

            /// parameter validation
            // dates check
            if (startDate == null || endDate == null || lastDateRefund == null ||
                startDate > endDate || lastDateRefund > endDate)
            {
                ret = false;
                errMsg = string.Format("Please check Event Period");
            }
            // city, street check
            else if (city == string.Empty || street == string.Empty)
            {
                ret = false;
                errMsg = string.Format("Please check city or street, it needs input");
            }

            /// event validation  
            var _event = events.GetQueryable().Where(x => x.year == _year).FirstOrDefault<ipmevent>();
            var _found = _event != null ? true : false;

            if( ret && _found)
            {
                if (isEventStarted(_year))
                {
                    ret = false;
                    errMsg = string.Format("Could not update this event which has been started");
                }
            }

            // validation of types are not needed due to AddTypeRate()
            if (ret == true)
            {
                if (!_found)
                    _event = new ipmevent();

                // update event object
                _event.year = _year;
                _event.street = street;
                _event.city = city;
                _event.provinceCode = province;
                _event.startDate = startDate;
                _event.endDate = endDate;
                _event.lastUpdate = DateTime.Now;
                _event.lastDateRefund = lastDateRefund;

                if (_found)
                    events.Update(_event);
                else
                    events.Insert(_event);
                
                long eventId = _event.ID;

                // commit events
                events.Commit();
                // save all type and rate information
                Polygons poly = Polygons.GetInstance();

                foreach (var t in poly.TypeRates)
                {   // save new type
                    var _type = new sitetype
                    {  // get eventId from event object not from TypeRates
                        idIPMEvent = _event.ID,
                        idSiteSize = t.sizeId,
                        idService = t.serviceId,
                        createDate = DateTime.Now,
                        lastUpdate = DateTime.Now
                    };
                    types.Insert(_type);
                    types.Commit();
                    // save two rate with new type id
                    rates.Insert(new siterate
                    {
                        idSiteType = _type.ID,
                        period = "Weekly",
                        rate = (decimal)t.week,
                        createDate = DateTime.Now,
                        lastUpdate = DateTime.Now,
                        idIPMEvent = _event.ID
                    });
                    rates.Insert(new siterate
                    {
                        idSiteType = _type.ID,
                        period = "Daily",
                        rate = (decimal)t.day,
                        createDate = DateTime.Now,
                        lastUpdate = DateTime.Now,
                        idIPMEvent = _event.ID
                    });
                    rates.Commit();
                }

                // clear poly's TypeRates
                poly.TypeRates.Clear();
            }

            // return 
            return Json(new
            {
                success = ret,
                msg = errMsg
            }, JsonRequestBehavior.AllowGet);
        }
        // check for event status
        private bool isEventStarted(long year)
        {
            // check this later : some conflict between this view and reservationitem table in terms of 'isAvailable'
            var site = status.GetQueryable().Where(x => x.Year == year && x.isAvaialable == 0 ).FirstOrDefault<rvsite_status_view>();

            if (site != null)
                return true;
            return false;
        }

        // view for mapping between type and styleurl
        public ActionResult DigitizeMap()
        {
            Polygons poly = Polygons.GetInstance();
            poly.Reset();

            var model = events.GetAll();
            var year = model.Max(x => x.year);
            ViewBag.year = year;
            ViewBag.eventId = model.Where(x => x.year == year).First<ipmevent>().ID;
            return View(model);
        }
        // partial view of DigitizeMap()
        public ActionResult SiteUrl(long eventId)
        {
            Polygons poly = Polygons.GetInstance();
            if( poly.Styles.Count == 0 )
                poly.AddStyle(styles.GetQueryable().Where(x => x.idIPMEvent == eventId).OrderByDescending(x => x.ID).ToList());
            var _urls = poly.Styles.Where(x => x.eventId == eventId).ToList();
            ViewBag.styleCount = _urls.Count;
            ViewBag.eventId = eventId;
            ViewBag.types = typeRates.GetQueryable().Where(x => x.eventId == eventId).ToList();

            return PartialView("SiteUrl", _urls);
        }

        // method for KML file upoload & parse
        [HttpPost]
        public ActionResult DigitizeMap(HttpPostedFileBase file, long eventId)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    XmlReader reader = XmlReader.Create(file.InputStream);
                    XDocument xDoc = System.Xml.Linq.XDocument.Load(reader);
                    KMLParser kmlParser = new KMLParser();
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

            var model = events.GetAll();
            ViewBag.year = model.Max(x => x.year);
            ViewBag.eventId = eventId;
            return View(model);
        }
        // syleInfo string -> comma separated parameter (styleUrl, idService, idSize ), e.g., "#ffffffff,1,2"
        private bool verifyTypeAndStyle(List<string> styleInfo, out string errMsg )
        {
            errMsg = "";

            // check if a staff did not upload KML file
            Polygons poly = Polygons.GetInstance();
            if (poly.Sites.Count == 0 ||
                poly.Coords.Count == 0)
            {
                errMsg = "Upload KML File before Saving!!";
                return false;
            }
            // no matching check
            if ( styleInfo == null )
            {
                errMsg = "Insert all matching information";
                return false;
            }
           
            // duplication check & 0 id
            List<string> dupcheck = new List<string>();

            foreach (var s in styleInfo)
            {
                string size_service = s.Split(',')[1] + s.Split(',')[2];
                // data duplication check
                if (dupcheck.Exists(x => x == size_service))
                {
                    errMsg = "Same type is choosed";
                    return false;
                }
                // valid id check
                if (s.Contains(",0"))
                {
                    errMsg = "Type was not choosed";
                    return false;
                }
                dupcheck.Add(size_service);
            }

            return true;
        }

        // syleInfo string -> comma separated parameter (styleUrl, idService, idSize ), e.g., "#ffffffff,1,2"
        [HttpPost]
        public ActionResult VerifyTypeAndStyle(int eventId, List<string> styleInfo)
        {
            string errMsg = "";
            bool ret = verifyTypeAndStyle(styleInfo, out errMsg);

            // return 
            return Json(new
            {
                success = ret,
                msg = errMsg
            }, JsonRequestBehavior.AllowGet);
        }
        // syleInfo string -> comma separated parameter (styleUrl, idService, idSize ), e.g., "#ffffffff,1,2"
        private void updateType_idStyleUrl(int eventId, List<string> styleInfo)
        {
            foreach (var s in styleInfo)
            {
                string[] type_ref = s.Split(',');
                var styleUrl = type_ref[0];
                var styleId = styles.GetQueryable().Where(x => x.idIPMEvent == eventId && x.styleUrl1 == styleUrl).First<styleurl>().ID;
                long idService = 0;
                long idSize = 0;
                long.TryParse(type_ref[1], out idService);
                long.TryParse(type_ref[2], out idSize);
                var type = types.GetQueryable().Where(x => x.idIPMEvent == eventId && x.idService == idService && x.idSiteSize == idSize).FirstOrDefault<sitetype>();
                type.idStyleUrl = styleId;

                types.Update(type);
            }
            types.Commit();
        }
        // Post Method to save object from KML Parser 
        [HttpPost]
        public ActionResult SaveParsedObjects(int eventId, List<string> styleInfo )
        {
            string errMsg = "Please check input information";
            bool ret = styleInfo == null ? false :
                        styleInfo.Count == 0 ? false:
                        verifyTypeAndStyle(styleInfo, out errMsg);

            // if succed deletion, process insertion.
            if ( ret && DeleteEventObjects(eventId))
            {
                Polygons poly = Polygons.GetInstance();
                // save styleUrl
                MySqlBulkInsert.InsertStyleUrl(poly, eventId);

                //  -> retrieve styleUrl to link type's styleUrl
                poly.AddStyle(styles.GetQueryable().Where(x => x.idIPMEvent == eventId).ToList());

                // set type's styleUrl
                updateType_idStyleUrl(eventId, styleInfo);

                //  -> retrieve siteType to link placeinmap's siteType
                poly.AddType(types.GetQueryable().Where(x => x.idIPMEvent == eventId).ToList());

                //  -> build site's siteType
                poly.MakeSiteRelation(eventId);

                // save placeinmap
                MySqlBulkInsert.InsertPlaceInMap(poly, eventId);

                //  -> retrieve placeinmap to link coordinate's placeinmap
                poly.AddSite(places.GetQueryable().Where(x => x.idIPMEvent == eventId).ToList());

                // set coord's placeId
                poly.SetPlaceIdInCoords();

                // save coordinates
                MySqlBulkInsert.InsertCoordinates(poly);

                //  -> retrieve coordinate to change init=true
                poly.AddCoordinates(coords.GetQueryable().Where(x => x.idIPMEvent == eventId).ToList());

                // update placeinmap with isRVSite
                MySqlBulkInsert.UpdateIsRVSite_PlaceInMap(poly);

                //poly.Initialized = true;
                //poly.eventId = eventId;

                // reset msg with no error
                errMsg = "";
            }

            // return 
            return Json(new
            {
                success = ret,
                msg = errMsg
            }, JsonRequestBehavior.AllowGet);
        }
        // save with Entity Framework : not currently used
        private bool saveCoordinatesWithEF()
        {
            //coords.SetAutoDetectChange(false);
            Polygons poly = Polygons.GetInstance();

            for (int i = 0; i < poly.Coords.Count; i++)
            {
                coordinate c = new coordinate();
                c.idIPMEvent = poly.Coords[i].eventId;
                c.idPlaceInMap = poly.Coords[i].placeId;
                c.seqCoordinate = poly.Coords[i].seqCoordinate;
                c.longitude = poly.Coords[i].y;
                c.latitude = poly.Coords[i].x;
                c.createDate = DateTime.Now;
                c.lastUpdate = DateTime.Now;

                coords.Insert(c);
                if ((i + 1) % 20 == 0)
                    coords.Commit();
                //break;
            }
            coords.Commit();

            //coords.SetAutoDetectChange(true);

            return true;
        }
        // save with EF context procedure : not currently used
        private bool saveCoordinatesWithProcedure()
        {
            DataContext contextForProcedure = new DataContext();
            contextForProcedure.Configuration.AutoDetectChangesEnabled = false;
            Polygons poly = Polygons.GetInstance();

            for (int i = 0; i < poly.Coords.Count; i++)
            {
                ObjectParameter success = new ObjectParameter("success", typeof(bool));
                ObjectParameter errMsg = new ObjectParameter("errMsg", typeof(string));

                //ObjectResult<sp_insert_coordinates_Result> rslt = contextForProcedure.sp_insert_coordinates(poly.Coords[i].eventId,
                //    (int)poly.Coords[i].placeId, poly.Coords[i].seqCoordinate, poly.Coords[i].y, poly.Coords[i].x,
                //    success, errMsg);

                //rslt.Dispose();
                //rslt.ToList();
                if (Convert.ToBoolean(success.Value) == true)
                    return false;
            }

            contextForProcedure.Configuration.AutoDetectChangesEnabled = false;

            return true;
        }
        // delete with EF context stored procedure
        public bool DeleteEventObjects(int eventId)
        {
            // sp_reset_event_derivatives wll do clear event related tables
            DataContext contextForProcedure = new DataContext();

            ObjectParameter success = new ObjectParameter("success", typeof(bool));
            ObjectParameter errMsg = new ObjectParameter("errMsg", typeof(string));

            ObjectResult<sp_reset_event_derivatives_Result> rslt = contextForProcedure.sp_reset_event_derivatives(eventId, success, errMsg);
            rslt.ToList();

            // can pass errMsg to html, if error condition
            success.Value.GetType();

            return Convert.ToBoolean(success.Value);
        }
        // delete with EF context stored procedure
        private bool delete_sitetype_dependants(long typeId)
        {
            // sp_typeId wll do delete type and depandants( rate, place, coord )
            DataContext contextForProcedure = new DataContext();

            ObjectParameter success = new ObjectParameter("success", typeof(bool));
            ObjectParameter errMsg = new ObjectParameter("errMsg", typeof(string));

            ObjectResult<sp_delete_sitetype_dependants_Result> rslt = contextForProcedure.sp_delete_sitetype_dependants(typeId, success, errMsg);
            rslt.ToList();

            // can pass errMsg to html, if error condition
            success.Value.GetType();

            return Convert.ToBoolean(success.Value);
        }
        // Fetch latest site update information => core part of map display
        public ActionResult GetSiteUpdate(string lastUpdate)
        {
            Polygons poly = Polygons.GetInstance();
            return Json(poly.GetSiteUpdate(lastUpdate), JsonRequestBehavior.AllowGet);
        }
        // JQuery Request Handler for ipmevent with specific year
        public ActionResult GetEventInfo(long year)
        {
            var obj = events.GetQueryable().Where(x => x.year == year).FirstOrDefault<ipmevent>();

            if( obj == null )
            {
                return Json(new
                {   id = 0,
                    city ="",
                    province = "",
                    street = "",
                    startdate = "yyyy-MM-dd",
                    enddate = "yyyy-MM-dd",
                    lastDateRefund = "yyyy-MM-dd"
                }, JsonRequestBehavior.AllowGet);
            }

            return Json( new{ id=obj.ID, city=obj.city, province = obj.provinceCode, street=obj.street, startdate= String.Format("{0:yyyy-MM-dd}", obj.startDate),
                enddate = String.Format("{0:yyyy-MM-dd}", obj.endDate),
                lastDateRefund = String.Format("{0:yyyy-MM-dd}", obj.lastDateRefund)
            }, JsonRequestBehavior.AllowGet);
        }
    }
}