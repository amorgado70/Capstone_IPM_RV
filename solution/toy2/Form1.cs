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

namespace toy2
{
    public class site
    {
        public string id;
        public string name;
        public List<string> points;
        public string color;
    }


    public partial class Form1 : Form
    {
        //readonly GMapOverlay top = new GMapOverlay();
        //internal readonly GMapOverlay objects = new GMapOverlay("objects");
        GMapOverlay polygonOverlay = new GMapOverlay("polygons");

        // marker
        GMapMarker currentMarker;

        // polygons
        GMapPolygon polygon;

        public Form1()
        {
            InitializeComponent();

            //gmap.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(MouseLeftButtonDown);

            gmap.MouseDoubleClick += new MouseEventHandler(gmap_MouseDoubleClick);

            //currentMarker = new GMapMarker(gmap.Position);
            //{
            //    currentMarker.Shape = new customMarkerRed(this, currentMarker, "custom position marker");
            //    currentMarker.Offset = new System.Windows.Point(-15, -15);
            //    currentMarker.ZIndex = int.MaxValue;
            //    gmap.Markers.Add(currentMarker);
            //}
        }

        private void gmap_Load(object sender, EventArgs e)
        {
            gmap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            //gmap.SetPositionByKeywords("Maputo, Mozambique");

            gmap.Position = new GMap.NET.PointLatLng(43.66881697093099, -81.31210058949804);
            gmap.Zoom = 18;


            List<site> sites = new List<site>();

            readXML( ref sites );

            foreach( var siteItem in sites )
            {
                List<PointLatLng> points = new List<PointLatLng>();

                for( int i=0; i< siteItem.points.Count-2; i=i+2 )
                {
                    points.Add(new PointLatLng(double.Parse(siteItem.points[i+1]), double.Parse(siteItem.points[i])));
                }

                GMapPolygon polygon = new GMapPolygon(points,  siteItem.name);
                polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Red));
                polygon.Stroke = new Pen(Color.Black, 1);
                polygonOverlay.Polygons.Add(polygon);
                
            }
            gmap.Overlays.Add(polygonOverlay);
        }

        private void readXML( ref List< site > sites )
        {
            string FileName = KMLIO.ShowFileDialog();
            if (FileName == string.Empty)
                return;

            XDocument xDoc = System.Xml.Linq.XDocument.Load(FileName);
            string xNs = "{" + xDoc.Root.Name.Namespace.ToString() + "}";

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
                newSite.id = i.Value;
                newSite.name = i.Element(xNs + "name").Value;
                newSite.points = y.ElementAt(0).Value.ToString().TrimStart().Split(delemeters).ToList();
                while (newSite.points.Remove("0"))
                    ;

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
}
