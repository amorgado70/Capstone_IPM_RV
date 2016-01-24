using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using GMap.NET.ObjectModel;

using FileReadNWrite;
using Microsoft.Win32;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;


namespace toy1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GMapMarker currentMarker;
        // polygons
        GMapPolygon polygon;

       internal readonly GMapOverlay polygons = new GMapOverlay("polygons");

        public MainWindow()
        {
            InitializeComponent();

            gmap.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(MouseLeftButtonDown);

            currentMarker = new GMapMarker(gmap.Position);
            {
                currentMarker.Shape = new customMarkerRed(this, currentMarker, "custom position marker");
                currentMarker.Offset = new System.Windows.Point(-15, -15);
                currentMarker.ZIndex = int.MaxValue;
                gmap.Markers.Add(currentMarker);
            }
        }

        private void GMapControl_Loaded(object sender, RoutedEventArgs e)
        {
            gmap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            //gmap.SetPositionByKeywords("Maputo, Mozambique");

            // somewhere in conestoga college
            gmap.Position = new GMap.NET.PointLatLng(43.388928, -80.407194);
            gmap.Zoom = 18;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            gmap.Width = this.Width - 50;
            gmap.Height = this.Height - 50;
        }

        private new void MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(gmap);

            currentMarker.Position = gmap.FromLocalToLatLng((int)p.X, (int)p.Y);
            Console.WriteLine("x={0}, y={1}", currentMarker.Position.Lat, currentMarker.Position.Lng);

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                addMarker();
            }
        }

        private void addMarker()
        {
            GMapMarker m1 = new GMapMarker(currentMarker.Position);
            {
                Placemark? pm = null;
                GeoCoderStatusCode status;
                var plret = GMapProviders.GoogleMap.GetPlacemark(currentMarker.Position, out status);
                if (status == GeoCoderStatusCode.G_GEO_SUCCESS && plret != null)
                {
                    pm = plret;
                }

                string ToolTipText;
                if (pm != null)
                {
                    ToolTipText = pm.Value.Address;
                }
                else
                {
                    ToolTipText = currentMarker.Position.ToString();
                }

                m1.Shape = new pointMarker(this, m1, ToolTipText);
                m1.ZIndex = 55;
            }
            gmap.Markers.Add(m1);
        }

        private void btn_MakePoint_Click(object sender, RoutedEventArgs e)
        {
            addMarker();
        }

        private void btn_ReportPoint_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach(GMapMarker m in gmap.Markers )
            {
                if(m.Shape is pointMarker)
                {   // screen point from latlng
                    GPoint pt = gmap.FromLatLngToLocal(m.Position);
                    sb.Append(string.Format("local:{0},{1} latlng:{2},{3}\n", pt.X, pt.Y, m.Position.Lat, m.Position.Lng));
                }
                    
            }
            MessageBox.Show(sb.ToString());
        }

        private void removeMarker()
        {
            // remove all marker except current marker
            var clear = gmap.Markers.Where(p => p != null && p != currentMarker);
            if (clear != null)
            {
                for (int i = 0; i < clear.Count(); i++)
                {
                    gmap.Markers.Remove(clear.ElementAt(i));
                    i--;
                }
            }
        }

        private void btn_RemovePoint_Click(object sender, RoutedEventArgs e)
        {
            removeMarker();
        }


        private void readXML2()
        {
            string FileName = KMLIO.ShowFileDialog();
            if (FileName != "")
            {
                KMLIO io = new KMLIO(FileName);

                XmlDocument xml = io.Read();


                //Create an XmlNamespaceManager for resolving namespaces.
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("id", "Layers");

                //Select and display the value of id values.
                XmlNodeList nodeList;
                XmlElement root = xml.DocumentElement;
                //nodeList = root.SelectNodes("/kml/Document/Folder/Placemark/@id", nsmgr);
                nodeList = root.SelectNodes("/kml");


                foreach (XmlNode Node in nodeList)
                {
                    Console.WriteLine(Node.Value);


                    //RetailItem item = new RetailItem();
                    //item.Description = Node.SelectSingleNode(ItemField.Description.ToString()).InnerText;
                    //item.UnitsOnHand = int.Parse(Node.SelectSingleNode(ItemField.UnitsOnHand.ToString()).InnerText);
                    //item.Price = float.Parse(Node.SelectSingleNode(ItemField.Price.ToString()).InnerText);
                    //Items.Add(item);
                }
            }
        }

        private void readXML()
        {
            string FileName = KMLIO.ShowFileDialog();
            if (FileName != "")
            {
                XDocument xDoc = System.Xml.Linq.XDocument.Load(FileName);
                string xNs = "{" + xDoc.Root.Name.Namespace.ToString() + "}";

                var coordsStr = from f in xDoc.Descendants(xNs + "Placemark")
                                    // where elementToFind.Contains(f.Parent.Element(xNs + "name").Value + f.Element(xNs + "name").Value)
                                    //select f.Element(xNs + "LineString").Element(xNs + "coordinates");
                                select f;

                int seq = 0;
                //Console.WriteLine(coordsStr);
                foreach( var i in coordsStr)
                {
                    var y = i.Element(xNs + "MultiGeometry").Descendants(xNs + "Polygon").Descendants(xNs + "outerBoundaryIs").Descendants(xNs + "LinearRing").Descendants(xNs + "coordinates");
                    char[] delemeters = { ',', ' ' };
                    List<string> points = y.ElementAt(0).Value.ToString().TrimStart().Split(delemeters).ToList();
                    while(points.Remove("0"))
                        ;

                    Console.WriteLine("({0}/{1}) : {2} : {3} : {4}", ++seq, points.Count, i.Attribute("id").Value, i.Element(xNs + "name").Value, i.Element(xNs + "styleUrl").Value);
                }

                Console.WriteLine(coordsStr.Count());
            }
        }
/*
        -81.31210058949804,43.66881697093099,0 
        -81.31201943741262,43.66878181360178,0 
        -81.31194193728339,43.66887607310042,0 
        -81.31202308945122,43.66891123048376,0 
        -81.31210058949804,43.66881697093099,0
        */

        private void btn_LoadKML_Click(object sender, RoutedEventArgs e)
        {
            readXML();
        }

        void RegeneratePolygon()
        {
            List<PointLatLng> polygonPoints = new List<PointLatLng>();

            foreach (GMapMarker m in gmap.Markers)
            {
                if (m is GMapMarkerRect)
                {
                    m.Tag = polygonPoints.Count;
                    polygonPoints.Add(m.Position);
                }
            }

            if (polygon == null)
            {
                polygon = new GMapPolygon(polygonPoints  /*, "polygon test" */);
                polygon.IsHitTestVisible = true;
                polygons.Polygons.Add(polygon);
            }
            else
            {
                polygon.Points.Clear();
                polygon.Points.AddRange(polygonPoints);

                if (polygons.Polygons.Count == 0)
                {
                    polygons.Polygons.Add(polygon);
                }
                else
                {
                    gmap.UpdatePolygonLocalPosition(polygon);
                }
            }
        }

        private void btn_DrawPolygon_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

