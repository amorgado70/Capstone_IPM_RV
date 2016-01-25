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




namespace toy2
{
    public partial class Form1 : Form
    {
        //readonly GMapOverlay top = new GMapOverlay();
        //internal readonly GMapOverlay objects = new GMapOverlay("objects");
        GMapOverlay polygonOverlay = new GMapOverlay("polygons");

        List<site> sites = new List<site>();
        List<style> styles = new List<style>();

        public Form1()
        {
            InitializeComponent();

            //gmap.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(MouseLeftButtonDown);

            gmap.MouseDoubleClick += new MouseEventHandler(gmap_MouseDoubleClick);
        }

        private void gmap_Load(object sender, EventArgs e)
        {
            gmap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            //gmap.SetPositionByKeywords("Maputo, Mozambique");

            gmap.Position = new GMap.NET.PointLatLng(43.66881697093099, -81.31210058949804);
            gmap.Zoom = 16;


            readXML();

            foreach( var siteItem in sites )
            {
                List<PointLatLng> points = new List<PointLatLng>();

                for( int i=0; i< siteItem.points.Count-2; i=i+2 )
                {
                    points.Add(new PointLatLng(double.Parse(siteItem.points[i+1]), double.Parse(siteItem.points[i])));
                }


                ////hex to color
                GMapPolygon polygon = new GMapPolygon(points,  siteItem.name);

                Color clr = Color.FromArgb(Int32.Parse(siteItem.styleRef.poly_color, NumberStyles.HexNumber));

                polygon.Fill = new SolidBrush(Color.FromArgb(50, clr));

                polygon.Stroke = new Pen(Color.FromArgb(Int32.Parse(siteItem.styleRef.line_color, NumberStyles.HexNumber)), 1);
                polygonOverlay.Polygons.Add(polygon);
                
            }
            gmap.Overlays.Add(polygonOverlay);

            gmap.Bearing = 0;
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

            foreach (var i in styleList)
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

            int seq = 0;
            //Console.WriteLine(coordsStr);
            foreach (var i in coordsStr)
            {
                var y = i.Element(xNs + "MultiGeometry").Descendants(xNs + "Polygon").Descendants(xNs + "outerBoundaryIs").Descendants(xNs + "LinearRing").Descendants(xNs + "coordinates");
                char[] delemeters = { ',', ' ' };
                site newSite = new site();
                newSite.styleId = i.Element(xNs + "styleUrl").Value;
                newSite.name = i.Element(xNs + "name").Value;
                newSite.points = y.ElementAt(0).Value.ToString().TrimStart().Split(delemeters).ToList();
                while (newSite.points.Remove("0"))
                    ;

                newSite.styleRef = (from style in styles where style.id == newSite.styleId
                                   select style).ElementAt(0);


                //Console.WriteLine("({0}/{1}) : {2} : {3} : {4}", ++seq, points.Count, i.Attribute("id").Value, i.Element(xNs + "name").Value, i.Element(xNs + "styleUrl").Value);

                sites.Add(newSite);
            }
            Console.WriteLine(coordsStr.Count());
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
    }

    public class site
    {
        public string styleId;
        public string name;
        public List<string> points;
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
