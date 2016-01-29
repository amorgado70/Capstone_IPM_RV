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
    public partial class IPM_Toy_Project : Form
    {
        //readonly GMapOverlay top = new GMapOverlay();
        //internal readonly GMapOverlay objects = new GMapOverlay("objects");
        GMapOverlay polygonOverlay = new GMapOverlay("polygons");

        List<site> sites = new List<site>();
        List<style> styles = new List<style>();

        public IPM_Toy_Project()
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

            TextIO txt = new TextIO("site.sql");

            readXML();

            txt.WriteLine("## this is only for RVSite DML(Database Modification Language) file.");
            txt.WriteLine("## Another site type has more than 4 points, that's why I omit those data.");
            txt.WriteLine("");

            foreach (var style in styles)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Insert Into 'StyleUrl' Values( ");
                string str = string.Format("'{0}', '{1}' );", style.id, style.poly_color);
                sb.Append(str);

                txt.WriteLine(sb.ToString());
            }
            txt.WriteLine("");
            txt.WriteLine("");
            txt.WriteLine("");

            foreach ( var siteItem in sites )
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

                if (siteItem.styleRef.id == "#PolyStyle00")
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Insert Into 'RVSite' Values( ");
                    string str = string.Format("'{0}', '{1}', '{2}', '{3}','{4}', '{5}', '{6}', '{7}', '{8}','{9}', '{10}' );", siteItem.id, siteItem.name, siteItem.styleId, 
                        siteItem.points[1], siteItem.points[0], siteItem.points[3], siteItem.points[2],
                        siteItem.points[5], siteItem.points[4], siteItem.points[7], siteItem.points[6]);
                    sb.Append(str);

                    txt.WriteLine(sb.ToString());
                }
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

            int seq = 0;
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

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            gmap.Zoom = e.NewValue/10.0f;
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            gmap.Bearing = e.NewValue;
        }

        private void gmap_OnMapZoomChanged()
        {
            vScrollBar1.Value = (Int32)gmap.Zoom * 10;
        }
    }

    public class site
    {
        public string id;
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
