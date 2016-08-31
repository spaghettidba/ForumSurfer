using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
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
                        String lastUpd = reader["last_update"].ToString();
                        if (String.IsNullOrEmpty(lastUpd)) lastUpd = DateTime.Now.ToString("s");
                        feed.LastUpdate = DateTime.Parse(lastUpd);
                        feed.Host = feed.Location.Host;
                        feed.Title = reader["title"].ToString();
                        feed.Id = (long)reader["feed_id"];
                        results.Add(feed);
                    }
                }
            }

            foreach(Feed f in results)
            {
                f.Articles = new List<Model.Article>(Article.LoadByFeed(f));
            }

            return results.ToList();
        }



        public static void UpdateAll(int retentionDays)
        {
            List<Feed> feeds = LoadAll();
            HashSet<Uri> existingArticles = new HashSet<Uri>();
            foreach(Feed f in feeds)
            {
                foreach(Article a in f.Articles)
                {
                    existingArticles.Add(a.Location);
                }
            }
            List<Task> TaskList = new List<Task>();
            foreach (Feed feed in feeds)
            {
                var LastTask = new Task(() => {
                    feed.Articles.Clear();
                    try
                    {
                        feed.UpdateFromUri(false, retentionDays);
                    }
                    catch(Exception e)
                    {
                        //TODO: log error somewhere
                        Debug.Print(e.Message);
                    }
                });
                LastTask.Start();
                TaskList.Add(LastTask);
            }
            Task.WaitAll(TaskList.ToArray());
            foreach(Feed feed in feeds)
            {
                foreach (Model.Article a in feed.Articles)
                {
                    if(!existingArticles.Contains(a.Location))
                    {
                        Article art = new Article(a);
                        art.Save();
                    }
                }
                feed.Save();
            }
        }


        public Feed() : base()
        {

        }

        public Feed(Model.Feed f) : base(f)
        {

        }


        public void Save(Boolean SaveArticles = false)
        {
            String sqlInsertHosts = @"
                INSERT INTO Hosts (uri, zoom) 
                SELECT $uri, 100
                WHERE NOT EXISTS (
                    SELECT *
                    FROM Hosts 
                    WHERE uri = $uri
                );
            ";

            String sqlGetHost = @"
                SELECT host_id
                FROM Hosts 
                WHERE uri = $uri
            ";

            String sqlInsertFeeds = @"
                INSERT INTO Feeds (
                    uri, 
                    last_update, 
                    title, 
                    host_id
                ) 
                SELECT 
                    $uri,
                    $last_update,
                    $title,
                    $host_id
                WHERE NOT EXISTS (
                    SELECT *
                    FROM Feeds
                    WHERE uri = $uri
                );
            ";

            String sqlUpdateFeed = @"
                UPDATE Feeds
                SET last_update = $last_update,
                    title = $title
                WHERE uri = $uri;
            ";

            String sqlGetFeed = @"
                SELECT feed_id
                FROM Feeds 
                WHERE uri = $uri
            ";


            if (Host == null)
                Host = Location.Host;

            if (Title == null)
                Title = Location.ToString();

            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                m_dbConnection.Open();
                SQLiteTransaction tran = m_dbConnection.BeginTransaction();
                try
                {
                    SQLiteCommand command = new SQLiteCommand(sqlInsertHosts, m_dbConnection);
                    command.Parameters.AddWithValue("$uri", Host);
                    command.ExecuteNonQuery();

                    command = new SQLiteCommand(sqlGetHost, m_dbConnection);
                    command.Parameters.AddWithValue("$uri", Host);
                    var host_id = command.ExecuteScalar();

                    command = new SQLiteCommand(sqlInsertFeeds, m_dbConnection);
                    command.Parameters.AddWithValue("$uri", Location.ToString());
                    command.Parameters.AddWithValue("$last_update", LastUpdate.ToString("s"));
                    command.Parameters.AddWithValue("$title", Title);
                    command.Parameters.AddWithValue("$host_id", host_id);
                    int inserted = command.ExecuteNonQuery();

                    if(inserted <= 0)
                    {
                        command = new SQLiteCommand(sqlUpdateFeed, m_dbConnection);
                        command.Parameters.AddWithValue("$uri", Location.ToString());
                        command.Parameters.AddWithValue("$last_update", LastUpdate.ToString("s"));
                        command.Parameters.AddWithValue("$title", Title);
                        command.ExecuteNonQuery();
                    }

                    command = new SQLiteCommand(sqlGetFeed, m_dbConnection);
                    command.Parameters.AddWithValue("$uri", Location.ToString());
                    Id = (long)command.ExecuteScalar();
                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }

            if (SaveArticles)
            {
                foreach(Model.Article a in Articles)
                {
                    Article dataArt = new Article(a);
                    dataArt.Save();
                }
            }

        }


        public void Delete()
        {
            String sqlDeleteHosts = @"
                DELETE 
                FROM Hosts
                WHERE host_id NOT IN (
                    SELECT host_id
                    FROM Feeds
                );
            ";

            String sqlDeleteArticles = @"
                DELETE
                FROM Articles
                WHERE feed_id = (
                    SELECT feed_id
                    FROM Feeds 
                    WHERE uri = $uri
                );
            ";

            String sqlDeleteFeed = @"
                DELETE
                FROM Feeds
                WHERE uri = $uri
            ";



            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                m_dbConnection.Open();
                SQLiteTransaction tran = m_dbConnection.BeginTransaction();
                try
                {
                    SQLiteCommand command = new SQLiteCommand(sqlDeleteArticles, m_dbConnection);
                    command.Parameters.AddWithValue("$uri", Location);
                    command.ExecuteNonQuery();

                    command = new SQLiteCommand(sqlDeleteFeed, m_dbConnection);
                    command.Parameters.AddWithValue("$uri", Location);
                    command.ExecuteNonQuery();

                    command = new SQLiteCommand(sqlDeleteHosts, m_dbConnection);
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
                WHERE feed_id = (
                    SELECT feed_id
                    FROM Feeds 
                    WHERE uri = $uri
                );
            ";



            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                m_dbConnection.Open();
                SQLiteTransaction tran = m_dbConnection.BeginTransaction();
                try
                {
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    command.Parameters.AddWithValue("$uri", Location);
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
