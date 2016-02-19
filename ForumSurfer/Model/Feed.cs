using System;
using System.Collections.Generic;
using System.Linq;
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


        public void UpdateFromUri()
        {
            using (XmlReader x = XmlReader.Create(Location.ToString()))
            {
                SyndicationFeed feed = SyndicationFeed.Load(x);
                x.Close();
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
            }
        }
    }
}
