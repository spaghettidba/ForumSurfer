using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.Model
{
    public class Host : RSSNode
    {
        public Uri Location { get; set; }
        public string Title { get; set; }
        public List<Feed> Feeds { get; set; }

        public Object SortKey
        {
            get
            {
                return Title;
            }
        }

        public Host()
        {
            Feeds = new List<Feed>();
        }

        public Host(Host h) : this()
        {
            this.Location = h.Location;
            this.Title = h.Title;
            this.Feeds = h.Feeds;
        }
    }
}
