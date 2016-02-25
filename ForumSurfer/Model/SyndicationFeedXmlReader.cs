using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ForumSurfer.Model
{
    public class SyndicationFeedXmlReader : XmlTextReader
    {
        readonly string[] Rss20DateTimeHints = { "pubDate" };
        readonly string[] Atom10DateTimeHints = { "updated", "published", "lastBuildDate" };
        private bool isRss2DateTime = false;
        private bool isAtomDateTime = false;
        static DateTimeFormatInfo dtfi = CultureInfo.CurrentCulture.DateTimeFormat;

        public SyndicationFeedXmlReader(Stream stream) : base(stream) { }

        public override bool IsStartElement(string localname, string ns)
        {
            isRss2DateTime = false;
            isAtomDateTime = false;

            if (Rss20DateTimeHints.Contains(localname)) isRss2DateTime = true;
            if (Atom10DateTimeHints.Contains(localname)) isAtomDateTime = true;

            return base.IsStartElement(localname, ns);
        }

        public override string ReadString()
        {
            string dateVal = base.ReadString();
            
            try
            {
                //if ((isRss2DateTime) || (isAtomDateTime))
                //{
                //    Debug.Print("Prima: " + dateVal);
                //    String prima = (String)dateVal.Clone();
                //    dateVal = DateTimeOffset.Parse(dateVal).ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                //}
                //if (isRss2DateTime)
                //{
                //    MethodInfo objMethod = typeof(Rss20FeedFormatter).GetMethod("DateFromString",
                //                                                                 BindingFlags.NonPublic |
                //                                                                 BindingFlags.Static);
                //    Debug.Assert(objMethod != null);
                //    objMethod.Invoke(null, new object[] { dateVal, this });

                //}
                //if (isAtomDateTime)
                //{
                //    MethodInfo objMethod = typeof(Atom10FeedFormatter).GetMethod("DateFromString",
                //                                                                  BindingFlags.NonPublic |
                //                                                                  BindingFlags.Instance);
                //    Debug.Assert(objMethod != null);
                //    objMethod.Invoke(new Atom10FeedFormatter(), new object[] { dateVal, this });
                //}
            }
            catch (TargetInvocationException)
            {
                try
                {
                    return DateTime.Parse(dateVal).ToString(dtfi.RFC1123Pattern);
                }
                catch (FormatException)
                {
                    return DateTimeOffset.UtcNow.ToString(dtfi.RFC1123Pattern);
                }
            }
            if ((isRss2DateTime) || (isAtomDateTime))
            {
                Debug.Print("Dopo: " + dateVal);
            }
            return dateVal;
        }
    }
}
