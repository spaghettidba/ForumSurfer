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
                        art.ParentFeed = f;
                        results.Add(art);
                    }
                }
            }

            return results.ToList();
        }



        public static Article LoadByUri(Uri u)
        {
            Article result = null;
            String sqlArticles = @"
                SELECT * 
                FROM Articles
                WHERE uri = $uri;
            ";

            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand(sqlArticles, m_dbConnection);
                command.Parameters.AddWithValue("$uri", u.ToString());
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = new Article();
                        result.Location = new Uri(reader["uri"].ToString());
                        result.PublishDate = DateTime.Parse(reader["published_date"].ToString());
                        result.Title = reader["title"].ToString();
                        result.Unread = !(reader["unread"].ToString().Equals("0"));
                    }
                }
            }
            return result;
        }


        public Article() : base()
        {

        }

        public Article(Model.Article a) : base(a)
        {

        }


        public void Save()
        {
            String sqlUpdate = @"
                UPDATE Articles
                SET published_date = $published_date,
                    title = $title,
                    unread = $unread
                WHERE uri = $uri;
            ";

            String sqlInsert = @"
                INSERT INTO Articles (
                    feed_id,
                    published_date,
                    title,
                    uri,
                    unread
                )
                SELECT
                    $feed_id,
                    $published_date,
                    $title,
                    $uri,
                    $unread
                WHERE NOT EXISTS (
                    SELECT *
                    FROM Articles
                    WHERE uri = $uri
                );
            ";

            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand(sqlUpdate, m_dbConnection);
                command.Parameters.AddWithValue("$published_date", PublishDate.ToString("s"));
                command.Parameters.AddWithValue("$title", Title);
                command.Parameters.AddWithValue("$unread", Unread);
                command.Parameters.AddWithValue("$uri", Location.ToString());
                int rowcount = command.ExecuteNonQuery();

                if(rowcount == 0)
                {
                    command = new SQLiteCommand(sqlInsert, m_dbConnection);
                    command.Parameters.AddWithValue("$feed_id", ParentFeed.Id);
                    command.Parameters.AddWithValue("$published_date", PublishDate.ToString("s"));
                    command.Parameters.AddWithValue("$title", Title);
                    command.Parameters.AddWithValue("$unread", Unread);
                    command.Parameters.AddWithValue("$uri", Location.ToString());
                    rowcount = command.ExecuteNonQuery();

                    if(rowcount == 0)
                    {
                        //something is wrong here: article already exists
                        //I could reaload it from database
                    }
                }
            }

        }

    }
}
