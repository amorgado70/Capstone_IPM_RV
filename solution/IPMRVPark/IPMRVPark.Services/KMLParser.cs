using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IPMRVPark.Services
{
    public class coord
    {
        public double _x;
        public double _y;
        public string x;
        public string y;
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

    public class KMLParser
    {
        public List<site> Sites { get; set; }
        public List<style> Styles { get; set; }
        coord _leftTop;         // Left Top point from Input
        coord _rightBottom;     // Right Bottom point from Input

        public KMLParser()
        {
            Sites = new List<site>();
            Styles = new List<style>();
        }

        public void Parse( XDocument xDoc )
        {
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

                Styles.Add(newStyle);
            }

            // site parsing
            var coordsStr = from f in xDoc.Descendants(xNs + "Placemark")
                                // where elementToFind.Contains(f.Parent.Element(xNs + "name").Value + f.Element(xNs + "name").Value)
                                //select f.Element(xNs + "LineString").Element(xNs + "coordinates");
                            select f;

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

                newSite.styleRef = (from style in Styles
                                    where style.id == newSite.styleId
                                    select style).ElementAt(0);
                // assert even
                Debug.Assert((newSite.points.Count % 2) == 0);

                for (int j = 0; j < newSite.points.Count; j += 2)
                {
                    coord c = new coord();
                    c._x = double.Parse(newSite.points[j]);
                    c._y = double.Parse(newSite.points[j + 1]);
                    c.x = newSite.points[j];
                    c.y = newSite.points[j+1];
                    checkBoundary(c);
                    newSite.coords.Add(c);
                }

                Sites.Add(newSite);
            }
            Console.WriteLine(coordsStr.Count());

            Console.WriteLine("{0}:{1}", (_leftTop._y + _rightBottom._y) / 2, (_leftTop._x + _rightBottom._x) / 2);
        }

        public void checkBoundary(coord c)
        {
            if (_leftTop == null)
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
    }
}
