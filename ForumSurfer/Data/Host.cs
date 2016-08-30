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
                    feedHost = Load(feed.Host);
                    feed.ParentHost = feedHost;
                    results.Add(feedHost);
                }

                if (feedHost == null)
                {
                    feedHost = new Host();
                    feedHost.Title = feed.Host;
                    feedHost.Location = new Uri("http://" + feed.Host);
                    feedHost.Zoom = 100;
                    feed.ParentHost = feedHost;
                    results.Add(feedHost);
                }
                if (feed.ParentHost == null)
                    feed.ParentHost = feedHost;

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


        public static Host Load(string uri)
        {
            String sql = @"
                SELECT *
                FROM Hosts 
                WHERE uri = $uri;
            ";

            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.Parameters.AddWithValue("$uri", uri);

                Host host = new Host();

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        host.Location = new Uri("http://" + reader["uri"].ToString());
                        host.Zoom = 100;
                        if (!(reader["zoom"] is DBNull)) 
                        {
                            if (!(reader["zoom"] == null))
                                host.Zoom = (int)Int64.Parse(reader["zoom"].ToString());
                        }
                        host.Title = reader["uri"].ToString();
                    }
                    else
                        return null;
                }

                return host;
            }
        }


        public void Save()
        {
            String sql = @"
                UPDATE Hosts
                SET zoom = $zoom
                WHERE uri = $uri;
            ";

            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                m_dbConnection.Open();
                SQLiteTransaction tran = m_dbConnection.BeginTransaction();
                try
                {
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    command.Parameters.AddWithValue("$uri", Title);
                    command.Parameters.AddWithValue("$zoom", Zoom);
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
