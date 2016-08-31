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
        public Host ParentHost { get; set; }
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
            this.ParentHost = ParentHost;
            this.Articles = Articles;
        }


        public void UpdateFromUri(Boolean deriveAttributes = false, int retentionDays = -1)
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

                        // Title is not set
                        if (Title.StartsWith("http"))
                        {
                            Title = feed.Title.Text;
                        }


                        //Date unchanged
                        if(LastUpdate.Equals(feed.LastUpdatedTime.DateTime))
                        {
                            LastUpdate = DateTime.Now;
                        }
                        else
                        {
                            LastUpdate = feed.LastUpdatedTime.DateTime;
                        }

                        foreach (SyndicationItem item in feed.Items)
                        {
                            Article art = new Article();
                            art.ParentFeed = this;
                            art.Location = item.Links[0].Uri;
                            art.PublishDate = item.PublishDate.ToLocalTime().DateTime;
                            art.Title = item.Title.Text;
                            art.Unread = true;

                            // Some dates don't parse correctly (connect.microsoft.com for instance)
                            if (art.PublishDate.Date.Equals(new DateTime(1, 1, 1)))
                            { 
                                art.PublishDate = item.LastUpdatedTime.ToLocalTime().DateTime;
                                if (art.PublishDate.Date.Equals(new DateTime(1, 1, 1)))
                                {
                                    art.PublishDate = LastUpdate;
                                }
                            }

                            // load article from feed only when within retention
                            if (retentionDays > 0 && art.PublishDate >= DateTime.Now.AddDays(-1 * retentionDays))
                                Articles.Add(art);


                            // Dates older than 180 days are likely parsing errors too
                            // I can't evaluate the retention policy after setting
                            // the date to the feed's last update, because some
                            // sites "resurrect" older posts and put them in the feed.
                            if (art.PublishDate < DateTime.Now.AddDays(-180))
                            {
                                art.PublishDate = item.LastUpdatedTime.ToLocalTime().DateTime;

                                art.PublishDate = LastUpdate;
                            }
                        }

                    } // using XmlReader

                } // using Stream

            } // using HttpWebResponse
        }
    }
}
