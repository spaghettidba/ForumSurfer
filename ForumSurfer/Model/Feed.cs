using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
