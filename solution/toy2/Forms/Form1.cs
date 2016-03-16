using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.ObjectModel;
using GMap.NET.WindowsForms;
using FileReadNWrite;
using System.Xml.Linq;
using System.Globalization;
using System.Diagnostics;

namespace toy2
{
    public partial class IPM_Toy_Project : Form
    {
        //readonly GMapOverlay top = new GMapOverlay();
        //internal readonly GMapOverlay objects = new GMapOverlay("objects");
        GMapOverlay polygonOverlay = new GMapOverlay("polygons");

        List<site> sites = new List<site>();
        List<style> styles = new List<style>();

        coord _leftTop;         // Left Top point from Input
        coord _rightBottom;     // Right Bottom point from Input
        coord leftTop;          // New Left Top point 
        coord rightBottom;      // New Right Bottom point 

        public IPM_Toy_Project()
        {
            InitializeComponent();

            //gmap.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(MouseLeftButtonDown);

            gmap.MouseDoubleClick += new MouseEventHandler(gmap_MouseDoubleClick);
        }

        private void gmap_Load(object sender, EventArgs e)
        {

            //gmap.SetPositionByKeywords("Maputo, Mozambique");

            


            //TextIO txt = new TextIO("site.sql");
            gmap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;


            gmap.Position = new GMap.NET.PointLatLng(43.6633780521837, -81.3024251317227);
            gmap.Zoom = 16;

            readXML();
           
            // gmap.Position = new GMap.NET.PointLatLng((_leftTop._y + _rightBottom._y) / 2, (_leftTop._x + _rightBottom._x) / 2);




            //txt.WriteLine("## this is only for RVSite DML(Database Modification Language) file.");
            //txt.WriteLine("## Another site type has more than 4 points, that's why I omit those data.");
            //txt.WriteLine("");

            //foreach (var style in styles)
            //{
            //    StringBuilder sb = new StringBuilder();
            //    sb.Append("Insert Into 'StyleUrl' Values( ");
            //    string str = string.Format("'{0}', '{1}' );", style.id, style.poly_color);
            //    sb.Append(str);

            //    txt.WriteLine(sb.ToString());
            //}
            //txt.WriteLine("");
            //txt.WriteLine("");
            //txt.WriteLine("");

            foreach ( var siteItem in sites )
            {
                List<PointLatLng> points = new List<PointLatLng>();

                //for (int i = 0; i < siteItem.points.Count - 2; i = i + 2)
                //{
                //    points.Add(new PointLatLng(double.Parse(siteItem.points[i + 1]), double.Parse(siteItem.points[i])));
                //}

                for (int i = 0; i < siteItem.coords.Count - 1; i++)
                {
                    //points.Add(new PointLatLng(siteItem.coords[i]._y, siteItem.coords[i]._x));
                    points.Add(new PointLatLng(siteItem.coords[i].y, siteItem.coords[i].x));
                }


                ////hex to color
                GMapPolygon polygon = new GMapPolygon(points,  siteItem.name);

                Color clr = Color.FromArgb(Int32.Parse(siteItem.styleRef.poly_color, NumberStyles.HexNumber));

                polygon.Fill = new SolidBrush(Color.FromArgb(50, clr));

                polygon.Stroke = new Pen(Color.FromArgb(Int32.Parse(siteItem.styleRef.line_color, NumberStyles.HexNumber)), 1);
                polygonOverlay.Polygons.Add(polygon);

                //if (siteItem.styleRef.id == "#PolyStyle00")
                //if (siteItem.points.Count == 10 )
                //{
                //    StringBuilder sb = new StringBuilder();
                //    sb.Append("Insert Into 'RVSite' Values( ");
                //    string str = string.Format("'{0}', '{1}', '{2}', '{3}','{4}', '{5}', '{6}', '{7}', '{8}','{9}', '{10}' );", siteItem.id, siteItem.name, siteItem.styleId, 
                //        siteItem.points[1], siteItem.points[0], siteItem.points[3], siteItem.points[2],
                //        siteItem.points[5], siteItem.points[4], siteItem.points[7], siteItem.points[6]);
                //    sb.Append(str);

                //    //txt.WriteLine(sb.ToString());
                //}
            }

           

            gmap.Overlays.Add(polygonOverlay);

            gmap.Bearing = 0;
            vScrollBar1.Value = (Int32)gmap.Zoom * 10;
        }

        private void readXML( )
        {
            string FileName = KMLIO.ShowFileDialog();
            if (FileName == string.Empty)
                return;

            XDocument xDoc = System.Xml.Linq.XDocument.Load(FileName);
            string xNs = "{" + xDoc.Root.Name.Namespace.ToString() + "}";

            // style parsing
            var styleList = from s in xDoc.Descendants(xNs + "Style")
                            select s;

            foreach (var i in styleList)//v
            {
                style newStyle = new style();
                newStyle.id = "#" + i.Attribute("id").Value;
                newStyle.label_color = i.Element(xNs + "LabelStyle").Element(xNs + "color").Value;
                newStyle.line_color = i.Element(xNs + "LineStyle").Element(xNs + "color").Value;
                newStyle.poly_color = i.Element(xNs + "PolyStyle").Element(xNs + "color").Value;

                styles.Add(newStyle);
            }


            // site parsing
            var coordsStr = from f in xDoc.Descendants(xNs + "Placemark")
                                // where elementToFind.Contains(f.Parent.Element(xNs + "name").Value + f.Element(xNs + "name").Value)
                                //select f.Element(xNs + "LineString").Element(xNs + "coordinates");
                            select f;

            //int seq = 0;
            //Console.WriteLine(coordsStr);
            foreach (var i in coordsStr)
            {
                var y = i.Element(xNs + "MultiGeometry").Descendants(xNs + "Polygon").Descendants(xNs + "outerBoundaryIs").Descendants(xNs + "LinearRing").Descendants(xNs + "coordinates");
                char[] delemeters = { ',', ' ' };
                site newSite = new site();
                newSite.id = i.Attribute("id").Value;
                newSite.styleId = i.Element(xNs + "styleUrl").Value;
                newSite.name = i.Element(xNs + "name").Value;
                newSite.points = y.ElementAt(0).Value.ToString().TrimStart().Split(delemeters).ToList();
                while (newSite.points.Remove("0"))
                    ;

                newSite.styleRef = (from style in styles where style.id == newSite.styleId
                                   select style).ElementAt(0);
                // assert even 
                Debug.Assert( (newSite.points.Count % 2) == 0 );

                for (int j = 0; j < newSite.points.Count; j += 2 )
                {
                    coord c = new coord();
                    c._x = double.Parse(newSite.points[j]);
                    c._y = double.Parse(newSite.points[j+1]);
                    checkBoundary(c);
                    newSite.coords.Add(c);
                }
                    
               


                //Console.WriteLine("({0}/{1}) : {2} : {3} : {4}", ++seq, points.Count, i.Attribute("id").Value, i.Element(xNs + "name").Value, i.Element(xNs + "styleUrl").Value);

                sites.Add(newSite);
            }
            Console.WriteLine(coordsStr.Count());

            Console.WriteLine("{0}:{1}", (_leftTop._y + _rightBottom._y) / 2, (_leftTop._x + _rightBottom._x) / 2);

            foreach( var s in sites )
            {
                foreach( var i in s.coords )
                {
                    GPoint org = gmap.FromLatLngToLocal(gmap.Position);
                    GPoint p = gmap.FromLatLngToLocal(new GMap.NET.PointLatLng(i._y, i._x) );
                    GPoint rp = rotatePosition( p, 0, org );

                    PointLatLng f = gmap.FromLocalToLatLng((int)rp.X, (int)rp.Y);
                    i.x = f.Lng;
                    i.y = f.Lat;
                }
            }
        }

        GPoint rotatePosition( GPoint p, double bearing, GPoint org )
        {
            const double halfC = Math.PI / 180.0;
            double r = bearing * halfC;

            double sin = Math.Sin(r);
            double cos = Math.Cos(r);
            
            // translate point back to origin:
            p.X -= org.X;
            p.Y -= org.Y;


            double nx = (p.X * cos - p.Y * sin);
            double ny = (p.X * sin + p.Y * cos);

            // translate point back:
           return new GPoint( (long)Math.Round(nx)+ org.X, (long)Math.Round(ny) + org.Y );

        }

        //GPoint rotateCoord( double x, double y, double bearing, double pivot_x, double pivot_y )
        //{
        //    const double halfC = Math.PI / 180.0;
        //    double r = bearing * halfC;

        //    double sin = Math.Sin(r);
        //    double cos = Math.Cos(r);
        //    //double sin = Math.Sin(bearing);
        //    //double cos = Math.Cos(bearing);

        //    // translate point back to origin:
        //    c._x -= pivot_x;
        //    c._y -= pivot_y;

        //    // rotate point
        //    //double nx = bearing > 0 ? (c._x * cos + c._y * sin) : (c._x * cos - c._y * sin);
        //    //double ny = bearing > 0 ? (-1* c._x * sin + c._y * cos) : (c._x * sin + c._y * cos);
        //    //double nx = bearing > 0 ?  (c._x * cos - c._y * sin) :(c._x * cos + c._y * sin);
        //    //double ny = bearing > 0 ?  (c._x * sin + c._y * cos) : (-1 * c._x * sin + c._y * cos);
        //    double nx = (c._x * cos - c._y * sin);
        //    double ny = (c._x * sin + c._y * cos);
        //    // translate point back:
        //    c.x = nx*2 + pivot_x;
        //    c.y = ny + pivot_y;

        //}

        void checkBoundary( coord c )
        {
            if( _leftTop == null )
            {
                _leftTop = new coord();
                _rightBottom = new coord();

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

        void gmap_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            PointLatLng point = gmap.FromLocalToLatLng(e.X, e.Y);

            GMapPolygon site;
            foreach( var poly in polygonOverlay.Polygons )
            {
                if (poly.IsInside(point) == true)
                {
                    site = poly;

                    MessageBox.Show(site.Name);
                    break;
                }
            }
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            gmap.Zoom = e.NewValue/10.0f;
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            gmap.Bearing = e.NewValue;
            Console.WriteLine("Bearing : {0}", gmap.Bearing);
        }

        private void gmap_OnMapZoomChanged()
        {
            vScrollBar1.Value = (Int32)gmap.Zoom * 10;
        }
    }

    public class coord
    {
        public double x;
        public double y;
        public double _x;
        public double _y;
    }

    public class site
    {
        public string id;
        public string styleId;
        public string name;
        public List<string> points;
        public List<coord> coords = new List<coord>();
        public string color;
        public style styleRef; 
    }

    public class style
    {
        public string id;
        public string label_color;
        public string line_color;
        public string poly_color;
    }

}
