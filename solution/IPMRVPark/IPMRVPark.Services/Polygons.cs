using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPMRVPark.Models;
using System.Diagnostics;
using System.Threading;

namespace IPMRVPark.Services
{
    public class _siteUpdate
    {
        public long id;
        public string fillColor;
        public string lastUpdate;           // UTC time string
        public DateTime lastUpdateTime;     // UTC DateTime
    }

    public class _site
    {
        public _site() {
            coords = new List<_coord>();
        }
        public long id;
        public long eventId;
        public string name;
        public string tag;  // kml tag
        public long typeId;
        public string json_coord;
        public bool isSite;
        public string poly_color;

        // references
        public _type type;
        public _style style;
        public _size size;
        public _service service;
        public List<_coord> coords { get; set; }
        public string styleUrl;     // temporary when KML parsed
    }

    public class _coord
    {
        public long id;
        public long eventId;
        public long placeId;
        public int seqCoordinate;
        public double _x;
        public double _y;
        public string x;
        public string y;
        public string siteTag;   // temporary when KML parsed
    }

    public class _type
    {
        public long id;
        public long eventId;
        public long sizeId;
        public long serviceId;
        public Nullable<long> styleId;
        public string styleUrl;     // temporary when KML parsed
    }

    public class _style
    {
        public long id;
        public Nullable<long> eventId;
        public string styleUrl; // name
        public string label_color;
        public string line_color;
        public string poly_color;
        public int siteCount;
    }

    public class _size
    {
        public long id;
        public string description;
    }

    public class _service
    {
        public long id;
        public string description;
    }

    public class Polygons : IDisposable
    {
        static private Polygons _instance = null;

        // for color of reservation
        const string _GrayRGB = "ff808080";
        const string _darkGrayRGB = "ffA9A9A9";
        const string _dimGrayRGB = "ff696969";
        const string _steelBlueRGB = "ffB0C4DE";
        const string _redRGB = "ffFF0000";

        const string _reserved = _GrayRGB;
        const string _selected = _redRGB;

        // data from KML Parse, Relation Input from Admin, and Database
        public List<_site> Sites { get; set; }
        public List<_coord> Coords { get; set; }
        public List<_type> Types { get; set; }
        public List<_style> Styles { get; set; }
        public List<_size> Sizes { get; set; }
        public List<_service> Services { get; set; }

        // temporary sitetype_service_rate_view list for sitetype save.
        public List<sitetype_service_rate_view> TypeRates { get; set; }
       
        // for the position of google map
        public _coord _leftTop { get; set; }         // Left Top point from Input
        public _coord _rightBottom { get; set; }     // Right Bottom point from Input

        // for working thread of fetching
        private Thread UpdateFetchThread = null;
        private bool locked = false, stopFromParent = false;
        private const int updateKeepSec = 60;
        private const int updateInterval = 6*1000;
        private TimeSpan UpdateKeepSpan { get; set; }
        private List<_siteUpdate> Updates { get; set; } // updates for pages

        private DateTime lastUpdateTime;

        // for starting fetching 
        public long eventId;

        private bool _Initialized;
        public bool Initialized
        {
            get { return _Initialized; }
            set
            {
                _Initialized = value;
                if (_Initialized == true)
                    startFetchUpdate();
                else
                    stopFetchUpdate();
            }
        }

        // test use
        private int firstSiteId=int.MaxValue;
        private int lastSiteId=0;

#region public_method

        private Polygons()
        {
            Initialized = false;
            Sites = new List<_site>();
            Coords = new List<_coord>();
            Types = new List<_type>();
            Styles = new List<_style>();
            Sizes = new List<_size>();
            Services = new List<_service>();

            TypeRates = new List<sitetype_service_rate_view>();
            Updates = new List<_siteUpdate>();

            UpdateKeepSpan = TimeSpan.FromSeconds(updateKeepSec);
            lastUpdateTime = DateTime.UtcNow;
        }

        static public Polygons GetInstance()
        {
            if (_instance == null)
                _instance = new Polygons();

            return _instance;
        }

        public void Dispose()
        {
            stopFetchUpdate();
        }

        public void Reset()
        {
            Initialized = false;
            eventId = 0;

            lastUpdateTime = DateTime.UtcNow;

            Sites.Clear();
            Coords.Clear();
            Types.Clear();
            Styles.Clear();
            Sizes.Clear();
            Services.Clear();
            Updates.Clear();
        }

        public List<_siteUpdate> GetSiteUpdate(string start)
        {
            List<_siteUpdate> updates = new List<_siteUpdate>();

            DateTime startTime;
            DateTime.TryParse(start, out startTime);

            // update Updates List with lock & monitor
            lock (Updates)
            {
                // Block this thread Until other thread call pulse
                while (locked == true)
                    Monitor.Wait(Updates);  // Stay as WaitSleepJoin State

                locked = true;

                // do something
                foreach (var i in Updates)
                {
                    if (i.lastUpdateTime > startTime)
                        updates.Add(new _siteUpdate { id = i.id, fillColor = i.fillColor, lastUpdate = i.lastUpdate, lastUpdateTime = i.lastUpdateTime });
                }

                // Wake other thread 
                locked = false;
                Monitor.Pulse(Updates);
            }
            // return update list
            return updates;
        }

        public void GetTypeRates( List<sitetype_service_rate_view> tRates, long eventId )
        {
            foreach( var i in TypeRates )
            {
                if( i.eventId == eventId )
                {
                    var c = new sitetype_service_rate_view();
                    c.eventId = i.eventId;
                    c.typeid = i.typeid;
                    c.styleId = i.styleId;
                    c.sizeId = i.sizeId;
                    c.size = i.size;
                    c.service = i.service;
                    c.serviceId = i.serviceId;
                    c.week = i.week;
                    c.day = i.day;

                    tRates.Add(c);
                }
            }
        }

        public bool DeleteTypeRate( long eventId, long serviceId, long sizeId)
        {
            for( int i=0; i<TypeRates.Count; i++ )
            {
                if (TypeRates[i].eventId == eventId &&
                    TypeRates[i].serviceId == serviceId &&
                    TypeRates[i].sizeId == sizeId)
                {
                    TypeRates.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void AddTypeRate(long eventId, long serviceId, string service, long sizeId, string size, decimal week,
            decimal day)
        {
            var c = new sitetype_service_rate_view();
            c.eventId = eventId;
            c.sizeId = sizeId;
            c.size = size;
            c.serviceId = serviceId;
            c.service = service;
            c.week = week;
            c.day = day;
            TypeRates.Add(c);
        }

        public _style GetStyle( string name )
        {
            foreach( var i in Styles )
            {
                if (i.styleUrl == name)
                    return i;
            }
            return null;
        }

        public void AddStyle(List<styleurl> styles)
        {
            // Reset Styles
            Styles.Clear();

            // Add each style
            foreach (var style in styles)
            {
                _style s = new _style();
                s.id = style.ID;
                s.eventId = style.idIPMEvent;
                s.poly_color = style.backgroundColor;
                s.styleUrl = style.styleUrl1;
                Styles.Add(s);
            }
        }

        public void AddType(List<sitetype> types)
        {
            // Reset Types
            Types.Clear();

            // Add each type
            foreach (var type in types)
            {
                _type t = new _type();
                t.id = type.ID;
                t.eventId = type.idIPMEvent;
                t.sizeId = type.idSiteSize;
                t.serviceId = type.idService;
                t.styleId = type.idStyleUrl;
                // check this later
                if(type.idStyleUrl != null )
                    t.styleUrl = getStyle(type.idStyleUrl).styleUrl;

                Types.Add(t);
            }
        }

        public void AddSite(List<placeinmap> places)
        {
            // reset site id;
            firstSiteId = int.MaxValue;
            lastSiteId = 0;

            // Reset Sites
            Sites.Clear();

            // Add each Site
            foreach (var place in places)
            {
                _site s = new _site();
                s.id = place.ID;
                s.eventId = (int)place.idIPMEvent;
                s.name = place.site;
                s.tag = place.tag;
                s.typeId = place.idSiteType;

                // Adjust Site's style and type
                // check this later 
                // type is more than style ????
                // get type
                s.type = getType(s.typeId);
                // assert site is valid
                Debug.Assert(s.type != null);
                // get style
                s.style = getStyle(s.type.styleId);
                // assert site's style is valid
                Debug.Assert(s.style != null);

                s.poly_color = s.style.poly_color;
                s.size = getSize(s.type.sizeId);
                s.service = getService(s.type.serviceId);

                // increase styleurl's sites count
                s.style.siteCount++;

                // add to Sites
                Sites.Add(s);
                // for test
                lastSiteId = lastSiteId < (int)s.id ? (int)s.id : lastSiteId;
                firstSiteId = firstSiteId > (int)s.id ? (int)s.id : firstSiteId;
            }
        }

        public void UpdateSite(List<rvsite_status_view> status)
        {
            // update site poly_color
            foreach( var s in status )
            {
                _site site = getSite(s.id);     // to optimize search, you can use sort and search algo
                // assert site is valid
                //Debug.Assert(site != null);

                if (s.isAvaialable==0)
                {
                    if(s.ReservedFrom != null)
                        site.poly_color = _reserved;
                    else
                        site.poly_color = _selected;

                }

                // if you want to expand information coverage of _site, you can add here.
                //   e.g., reservation info, out of service, etc..
            }
        }

        public void AddCoordinates(List<coordinate> coords)
        {
            // Reset Coords
            Coords.Clear();

            // Add each Site
            foreach (var coord in coords)
            {
                _coord c = new _coord();
                c.id = coord.ID;
                c.placeId = coord.idPlaceInMap;
                double.TryParse(coord.longitude, out c._x);
                double.TryParse(coord.latitude, out c._y);
                c.x = coord.longitude.ToString();
                c.y = coord.latitude.ToString();
                // add to Coords
                Coords.Add(c);

                // Adjust Site's Coord List
                _site site = getSite(coord.idPlaceInMap);
                // assert site is valid
                //Debug.Assert(site != null);
                site.coords.Add(c);
            }

            // prepare Json String for Coordinations
            prepare_Coord_JsonString();
        }

        private void prepare_Coord_JsonString()
        {
            // prepare json string for sites array
            StringBuilder sb = new StringBuilder();
            foreach (var s in Sites)
            {
                sb.Append("[");
                int cnt = 0, max = s.coords.Count;
                s.isSite = max == 5;
                foreach (var c in s.coords)
                {
                    sb.Append("{\"X\":").Append(c.x).Append(",\"Y\":").Append(c.y).Append("}");
                    if (++cnt < max)
                        sb.Append(",");

                    // check boundary
                    if (s.isSite)
                        checkBoundary(c);
                }
                s.json_coord = sb.Append("]").ToString();
                sb.Clear();
            }
        }

        public void SetPlaceIdInCoords()
        {
            foreach (var c in Coords)
            {
                _site site = getSite(c.siteTag);
                // assert site is valid
                Debug.Assert(site != null);

                // update site id
                c.placeId = site.id;
            }
        }

        public void AddSize(IQueryable<sitesize> sizes)
        {
            // Reset Sizes
            Sizes.Clear();

            // Add each size
            foreach (var size in sizes)
            {
                _size s = new _size();
                s.id = size.ID;
                s.description = size.description;
                Sizes.Add(s);
            }
        }

        public void AddService(IQueryable<service> services)
        {
            // Reset Services
            Services.Clear();

            // Add each size
            foreach (var service in services)
            {
                _service s = new _service();
                s.id = service.ID;
                s.description = service.description;
                Services.Add(s);
            }
        }

        public void MakeSiteRelation(long eventId)
        {
            foreach (var s in Sites)
            {
                s.typeId = getType(s.styleUrl).id;
            }
        }
        #endregion

        #region private_method

        private void startFetchUpdate()
        {
            UpdateFetchThread = new Thread(new ThreadStart(doFetchUpdate));
            UpdateFetchThread.Start();
        }

        private void stopFetchUpdate()
        {
            if (UpdateFetchThread != null)
            {
                stopFromParent = true;
                Thread.Sleep(100);

                UpdateFetchThread.Join();
                UpdateFetchThread = null;
                stopFromParent = false;
            }
        }

        private void doFetchUpdate()
        {
            while (stopFromParent == false)
            {
                // get list of _reserve_selection from database
                List<_reserve_selection> updates;
                MySqlReader.GetSiteUpdate(lastUpdateTime, out updates);

                // prepare to remove old _siteUpdate object
                DateTime now = DateTime.UtcNow;

                // update Updates List with lock & monitor
                lock (Updates)
                {
                    while (locked == true)     // Block this thread Until other thread call pulse
                        Monitor.Wait(Updates); // Stay as WaitSleepJoin State

                    locked = true;

                    // pop updates after UpdateKeepSpan from tail 
                    for (int i = 0; i < Updates.Count;)
                    {
                        if (now - Updates[0].lastUpdateTime > UpdateKeepSpan)
                            Updates.RemoveAt(0);
                        else
                            break;
                    }

                    // insert new update
                    foreach (var u in updates)
                    {
                        _site s = getSite(u.id);
                        string site_color = "";
                        if (s != null)
                        {
                            if (u.removed)
                                site_color = s.style.poly_color;
                            else if (u.type == _reserve_selection._type.reservation)
                                site_color = _reserved;
                            else
                                site_color = _selected;
                            // insert new _siteUpdate 
                            Updates.Add(new _siteUpdate { id = u.id, fillColor = site_color, lastUpdate = u.lastUpdateString, lastUpdateTime = u.lastUpdate });
                            // update site poly_color
                            s.poly_color = site_color;
                        }
                        // update last check time
                        lastUpdateTime = u.lastUpdate;
                    }

                    // Wake other thread 
                    locked = false;
                    Monitor.Pulse(Updates);
                }

                Thread.Sleep(updateInterval);
            }
        }

        //private void doFetchUpdate2()
        //{
        //    while (stopFromParent == false)
        //    {
        //        // fetch from database
        //        Random rnd = new Random();
        //        int id = rnd.Next(firstSiteId, lastSiteId);
        //        int id2 = rnd.Next(firstSiteId, lastSiteId);

        //        _site s = getSite(id);
        //        _site s2 = getSite(id2);
        //        if (s != null && s2 != null)
        //        {
        //            s.poly_color = _dimGrayRGB;
        //            s2.poly_color = _dimGrayRGB;


        //            DateTime now = DateTime.Now;
        //            //now = DateTime.Now;

        //            // update Updates List with lock & monitor
        //            lock (Updates)
        //            {
        //                while (locked == true)     // Block this thread Until other thread call pulse
        //                    Monitor.Wait(Updates); // Stay as WaitSleepJoin State

        //                locked = true;

        //                // pop updates after UpdateKeepSpan from tail 
        //                for (int i = 0; i < Updates.Count;)
        //                {
        //                    if (now - Updates[0].lastUpdateTime > UpdateKeepSpan)
        //                        Updates.RemoveAt(0);
        //                    else
        //                        break;
        //                }

        //                // push new updates to head
        //                Updates.Add(new _siteUpdate { id = id, fillColor = s.poly_color, lastUpdate = DateTime.Now.ToString(), lastUpdateTime = DateTime.Now });
        //                Updates.Add(new _siteUpdate { id = id2, fillColor = s2.poly_color, lastUpdate = DateTime.Now.ToString(), lastUpdateTime = DateTime.Now });

        //                // Wake other thread 
        //                locked = false;
        //                Monitor.Pulse(Updates);
        //            }
        //        }

        //        Thread.Sleep(updateInterval);
        //    }
        //}

        private _style getStyle(Nullable<long> id)
        {
            foreach (var i in Styles)
            {
                if (i.id == id)
                    return i;
            }
            return null;
        }

        private _type getType(long id)
        {
            foreach (var i in Types)
            {
                if (i.id == id)
                    return i;
            }
            return null;
        }

        private _type getType(string styleUrl)
        {
            foreach (var i in Types)
            {
                if (i.styleUrl == styleUrl)
                    return i;
            }
            return null;
        }

        private _site getSite(long id)
        {
            foreach (var i in Sites)
            {
                if (i.id == id)
                    return i;
            }
            return null;
        }

        private _site getSite(string tag)
        {
            foreach (var i in Sites)
            {
                if (i.tag == tag)
                    return i;
            }
            return null;
        }

        private _size getSize(long id)
        {
            foreach (var i in Sizes)
            {
                if (i.id == id)
                    return i;
            }
            return null;
        }

        private _service getService(long id)
        {
            foreach (var i in Services)
            {
                if (i.id == id)
                    return i;
            }
            return null;
        }

        private void checkBoundary(_coord c)
        {
            if (_leftTop == null)
            {
                _leftTop = new _coord();
                _rightBottom = new _coord();

                _leftTop._x = c._x;
                _leftTop._y = c._y;
                _rightBottom._x = c._x;
                _rightBottom._y = c._y;
            }
            else
            {
                _leftTop._x = c._x < _leftTop._x ? c._x : _leftTop._x;
                _leftTop._y = c._y < _leftTop._y ? c._y : _leftTop._y;
                _rightBottom._x = c._x > _rightBottom._x ? c._x : _rightBottom._x;
                _rightBottom._y = c._y > _rightBottom._y ? c._y : _rightBottom._y;
            }
        }

#endregion

#region test_implemenatation

        private void makeFakeSize(long eventId)
        {
            _size s = new _size();
            s.id = 1;     // not meaningful
            s.description = "";
            Sizes.Add(s);
        }

        public void makeFakeType(long eventId)
        {
            foreach (var s in Styles)
            {
                Types.Add(new _type()
                {
                    id = 0,
                    eventId = eventId,
                    sizeId = 1,         // check this later
                    serviceId = 1,       // check this later
                    styleId = s.id
                });
            }
        }

        private void resetUpdate()
        {
            // fetch from database
            Random rnd = new Random();
            int id = rnd.Next(firstSiteId, lastSiteId);
            int id2 = rnd.Next(firstSiteId, lastSiteId);

            _site s = getSite(id);
            if (s != null)
            {
                s.poly_color = "ffff0000";
                _site s2 = getSite(id2);
                s2.poly_color = "ffff0000";

                DateTime now = DateTime.Now;

                // pop updates after UpdateKeepSpan from tail 
                for (int i = 0; i < Updates.Count;)
                {
                    if (now - Updates[0].lastUpdateTime > UpdateKeepSpan)
                        Updates.RemoveAt(0);
                    else
                        break;
                }

                // push new updates to head
                Updates.Add(new _siteUpdate { id = id, fillColor = s.poly_color, lastUpdate = DateTime.Now.ToString(), lastUpdateTime = now });
                Updates.Add(new _siteUpdate { id = id2, fillColor = s2.poly_color, lastUpdate = DateTime.Now.ToString(), lastUpdateTime = now });
            }
        }

        #endregion
    }
}
