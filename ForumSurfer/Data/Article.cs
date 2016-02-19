using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.Data
{
    public class Article : Model.Article
    {
        public static List<Article> LoadByFeed(Feed f)
        {
            List<Article> results = new List<Article>();

            String sqlArticles = @"
                SELECT * 
                FROM Articles
                WHERE feed_id = (
                    SELECT feed_id
                    FROM Feeds
                    WHERE uri = $uri
                );
            ";

            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand(sqlArticles, m_dbConnection);
                command.Parameters.AddWithValue("$uri", f.Location.ToString());
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Article art = new Article();
                        art.Location = new Uri(reader["uri"].ToString());
                        art.PublishDate = DateTime.Parse(reader["published_date"].ToString());
                        art.Title = reader["title"].ToString();
                        art.Unread = !(reader["unread"].ToString().Equals("0"));

                        results.Add(art);
                    }
                }
            }

            return results.OrderByDescending(o => o.PublishDate).ToList();
        }


        public Article() : base()
        {

        }

        public Article(Article a) : base(a)
        {

        }

    }
}
