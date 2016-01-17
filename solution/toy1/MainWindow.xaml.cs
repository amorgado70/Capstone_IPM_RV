using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;

namespace toy1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GMapMarker currentMarker;

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

            gmap.Position = new GMap.NET.PointLatLng(43.388928, -80.407194);
            gmap.Zoom = 18;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            gmap.Width = this.Width - 50;
            gmap.Height = this.Height - 50;
        }

        private void btn_MakePoint_Click(object sender, RoutedEventArgs e)
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

        private new void MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(gmap);

            currentMarker.Position = gmap.FromLocalToLatLng((int)p.X, (int)p.Y);
            Console.WriteLine("x={0}, y={1}", currentMarker.Position.Lat, currentMarker.Position.Lng);

            GMapMarker m = new GMapMarker(currentMarker.Position);
            gmap.Markers.Add(m);

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
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
        }
    }
}
