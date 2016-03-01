using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ForumSurfer.Model
{
    public class Feed : RSSNode
    {
        public Uri Location { get; set; }
        public DateTime LastUpdate { get; set; }
        public String Title { get; set; }
        public String Host { get; set; }
        public List<Article> Articles { get; set; }
        public long Id { get; set; }

        public Object SortKey
        {
            get
            {
                return Title;
            }
        }

        public Feed()
        {
            Articles = new List<Article>();
        }

        public Feed(Feed f) : this()
        {
            this.Location = f.Location;
            this.LastUpdate = f.LastUpdate;
            this.Title = f.Title;
            this.Host = f.Host;
            this.Articles = Articles;
        }


        public void UpdateFromUri(Boolean deriveAttributes = false)
        {
            //WebClient client = new WebClient();
            //using (SyndicationFeedXmlReader x = new SyndicationFeedXmlReader(client.OpenRead(Location)))
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(Location.ToString());
            httpWebRequest.UserAgent = "Googlebot/1.0 (googlebot@googlebot.com http://googlebot.com/)";

            // Use The Default Proxy
            httpWebRequest.Proxy = System.Net.WebRequest.DefaultWebProxy;

            // Use The Thread's Credentials (Logged In User's Credentials)
            if (httpWebRequest.Proxy != null)
                httpWebRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;

            using (HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse())
            {
                using (Stream responseStream = httpWebResponse.GetResponseStream())
                {
                    using (XmlReader x = XmlReader.Create(responseStream))
                    {

                        SyndicationFeed feed = SyndicationFeed.Load(x);
                        x.Close();
                        if (deriveAttributes)
                        {
                            Title = feed.Title.Text;
                            LastUpdate = feed.LastUpdatedTime.DateTime;
                            Host = Location.Host;
                        }
                        foreach (SyndicationItem item in feed.Items)
                        {
                            Article art = new Article();
                            art.ParentFeed = this;
                            art.Location = item.Links[0].Uri;
                            art.PublishDate = item.PublishDate.ToLocalTime().DateTime;
                            art.Title = item.Title.Text;
                            art.Unread = true;

                            Articles.Add(art);
                        }

                    } // using XmlReader

                } // using Stream

            } // using HttpWebResponse
        }
    }
}
