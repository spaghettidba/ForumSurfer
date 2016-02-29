using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
                Host feedHost = results.FirstOrDefault(el => el.Location.Equals(new Uri("http://" + feed.Host)));
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

        public Host(Model.Host h) : base(h)
        {

        }


        public void MarkAllRead()
        {
            String sql = @"
                UPDATE Articles
                SET unread = 0
                WHERE feed_id IN (
                    SELECT feed_id
                    FROM Feeds 
                    WHERE host_id = (
                        SELECT host_id
                        FROM Hosts
                        WHERE uri = $uri
                    )
                );
            ";



            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                m_dbConnection.Open();
                SQLiteTransaction tran = m_dbConnection.BeginTransaction();
                try
                {
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    command.Parameters.AddWithValue("$uri", Title);
                    command.ExecuteNonQuery();

                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }
        }
    }
}
