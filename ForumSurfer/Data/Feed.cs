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


        public static void CreateDatabase()
        {
            if (!File.Exists(Repository.DatabasePath))
                SQLiteConnection.CreateFile(Repository.DatabasePath);

            String sqlCreateTableHosts = @"
                CREATE TABLE IF NOT EXISTS Hosts (
                    host_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    uri TEXT NOT NULL UNIQUE
                )
            ";

            String sqlCreateTableFeeds = @"
                CREATE TABLE IF NOT EXISTS Feeds (
                    feed_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    uri TEXT NOT NULL UNIQUE,
                    last_update TEXT,
                    title TEXT NOT NULL,
                    host_id INTEGER NOT NULL REFERENCES Hosts(host_id)
                )
            ";

            String sqlCreateTableArticles = @"
                CREATE TABLE IF NOT EXISTS Articles (
                    article_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    uri TEXT NOT NULL UNIQUE,
                    published_date TEXT,
                    title TEXT NOT NULL,
                    unread INTEGER NOT NULL,
                    feed_id INTEGER NOT NULL REFERENCES Feeds(feed_id)
                )
            ";

            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                m_dbConnection.Open();
                SQLiteTransaction tran = m_dbConnection.BeginTransaction();
                try
                {
                    SQLiteCommand command = new SQLiteCommand(sqlCreateTableHosts, m_dbConnection);
                    command.ExecuteNonQuery();
                    command = new SQLiteCommand(sqlCreateTableFeeds, m_dbConnection);
                    command.ExecuteNonQuery();
                    command = new SQLiteCommand(sqlCreateTableArticles, m_dbConnection);
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


        public static void UpdateAll()
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
                        feed.UpdateFromUri();
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
            }
        }


        public Feed() : base()
        {

        }

        public Feed(Feed f) : base(f)
        {

        }


        public void Save()
        {
            String sqlInsertHosts = @"
                INSERT INTO Hosts (uri) 
                SELECT $uri
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

            String sqlGetFeed = @"
                SELECT feed_id
                FROM Feeds 
                WHERE uri = $uri
            ";


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
                    command.ExecuteNonQuery();

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

        }


    }
}
