using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IPMRVPark.Services
{
    public class KMLParser
    {
        // reference of Single Polygon Object
        private Polygons polys;

        public KMLParser()
        {
            polys = Polygons.GetInstance();
        }

        public void Parse(XDocument xDoc, long eventId)
        {
            // reset data lists
            polys.Reset();

            // get Root Namespace
            string xNs = "{" + xDoc.Root.Name.Namespace.ToString() + "}";

            // style parsing
            var styleList = from s in xDoc.Descendants(xNs + "Style")
                            select s;

            foreach (var i in styleList)
            {
                _style newStyle = new _style();
                newStyle.styleUrl = "#" + i.Attribute("id").Value;
                newStyle.eventId = eventId;
                newStyle.label_color = i.Element(xNs + "LabelStyle").Element(xNs + "color").Value;
                newStyle.line_color = i.Element(xNs + "LineStyle").Element(xNs + "color").Value;
                newStyle.poly_color = i.Element(xNs + "PolyStyle").Element(xNs + "color").Value;

                polys.Styles.Add(newStyle);
            }

            // _site parsing
            var coordsStr = from f in xDoc.Descendants(xNs + "Placemark")
                                // where elementToFind.Contains(f.Parent.Element(xNs + "name").Value + f.Element(xNs + "name").Value)
                                //select f.Element(xNs + "LineString").Element(xNs + "coordinates");
                            select f;
            int siteID = 1;
            foreach (var i in coordsStr)
            {
                var y = i.Element(xNs + "MultiGeometry").Descendants(xNs + "Polygon").Descendants(xNs + "outerBoundaryIs").Descendants(xNs + "LinearRing").Descendants(xNs + "coordinates");
                char[] delemeters = { ',', ' ' };
                // create new site
                _site newSite = new _site();
                newSite.style = polys.GetStyle(i.Element(xNs + "styleUrl").Value);
                newSite.styleUrl = newSite.style.styleUrl;
                newSite.eventId = eventId;
                newSite.id = siteID++;
                newSite.name = i.Element(xNs + "name").Value;
                newSite.tag = i.Attribute("id").Value;
                var tmpCoords = y.ElementAt(0).Value.ToString().TrimStart().Split(delemeters).ToList();
                while (tmpCoords.Remove("0"))
                    ;

                // assert even count 
                Debug.Assert((tmpCoords.Count % 2) == 0);

                for (int j = 0; j < tmpCoords.Count; j += 2)
                {
                    _coord c = new _coord();
                    c._x = double.Parse(tmpCoords[j]);
                    c._y = double.Parse(tmpCoords[j + 1]);
                    c.x = tmpCoords[j];
                    c.y = tmpCoords[j + 1];
                    c.placeId = newSite.id;
                    c.eventId = eventId;
                    c.seqCoordinate = (j / 2) + 1;
                    c.siteTag = newSite.tag;
                    newSite.coords.Add(c);
                    polys.Coords.Add(c);
                }

                polys.Sites.Add(newSite);
                // style site count add
                newSite.style.siteCount++;
            }
            Console.WriteLine(coordsStr.Count());

        }
       
    }
}
