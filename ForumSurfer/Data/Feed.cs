using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.Data
{
    public class Feed : Model.Feed
    {
        public static List<Feed> LoadAll()
        {
            List<Feed> results = new List<Feed>();


            String sqlFeeds = @"
                SELECT * 
                FROM Feeds;
            ";

            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand(sqlFeeds, m_dbConnection);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Feed feed = new Feed();
                        feed.Location = new Uri(reader["uri"].ToString());
                        feed.LastUpdate = DateTime.Parse(reader["last_update"].ToString());
                        feed.Host = feed.Location.Host;
                        feed.Title = reader["title"].ToString();

                        results.Add(feed);
                    }
                }
            }

            foreach(Feed f in results)
            {
                f.Articles = new List<Model.Article>(Article.LoadByFeed(f));
            }

            return results.OrderBy(o => o.Host).ToList();
        }



        public Feed() : base()
        {

        }

        public Feed(Feed f) : base(f)
        {

        }
    }
}
