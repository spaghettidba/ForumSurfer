using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ForumSurfer.Model
{
    public class OPML
    {
        public List<Feed> Import(string uri)
        {
            XDocument doc = XDocument.Load(uri);
            List<Feed> t = doc
                                   .Descendants("outline")
                                   .Elements("outline")
                                   .Select(o => new Feed
                                   {
                                       Location = new Uri(o.Attribute("xmlUrl").Value)
                                   })
                                   .ToList();
            return t;
        }


        public XDocument Export(List<Data.Host> allData)
        {
            var doc = new XDocument(
                new XElement("opml",
                    new XAttribute("version", "1.0"),
                    new XElement("head",
                        new XElement("title", "Feeds subscribed in ForumSurfer")
                        ),
                    new XElement("body",
                        allData.Select(x => new XElement("outline",
                            new XAttribute("text", x.Location),
                            new XAttribute("title", x.Title),
                                x.Feeds.Select(y => new XElement("outline",
                                        new XAttribute("type", "rss"),
                                        new XAttribute("text", y.Title),
                                        new XAttribute("title", y.Title),
                                        new XAttribute("xmlUrl", y.Location)
                                    ) // XElement outline feed
                                ) // Select
                            ) //XEelement outline host
                        ) // Select
                    ) //XElement body
               ) // XEelement opml
            ); // XDocument

            return doc;
        }
    }
}
