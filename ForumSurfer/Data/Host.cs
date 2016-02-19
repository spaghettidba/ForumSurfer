using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.Data
{
    public class Host : Model.Host
    {
        public static List<Host> LoadAll()
        {
            List<Host> results = new List<Host>();

            List<Feed> feeds = Feed.LoadAll();
            foreach(Feed feed in feeds)
            {
                Host feedHost = results.FirstOrDefault(el => el.Location.Equals(feed.Host));
                if (feedHost == null)
                {
                    feedHost = new Host();
                    feedHost.Title = feed.Host;
                    feedHost.Location = new Uri("http://" + feed.Host);
                    results.Add(feedHost);
                }
                feedHost.Feeds.Add(feed);
            }

            return results;
        }



        public Host() : base()
        {

        }

        public Host(Host h) : base(h)
        {

        }
    }
}
