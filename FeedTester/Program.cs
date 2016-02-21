using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FeedTester
{
    class Program
    {
        static String DatabaseFolder = @"%AppData%\ForumSurfer";
        static String DatabaseName = "ForumSurfer.sqlite";
        static String DatabasePath;

        static Program()
        {
            DatabaseFolder = Environment.ExpandEnvironmentVariables(DatabaseFolder);
            Directory.CreateDirectory(DatabaseFolder);
            DatabasePath = DatabaseFolder + "\\" + DatabaseName;
        }

        static void Main(string[] args)
        {
            //CreateDatabase();
            //InsertFeed("http://dba.stackexchange.com/feeds/tag/sql-server%20OR%20sql-server-2008%20OR%20sql-server-2012%20OR%20sql-server-2008-r2%20OR%20sql-server-2014");
            //UpdateAllFeeds();
            ParseOpml();
        }

        private static void ParseOpml()
        {
            XDocument doc = XDocument.Load("file://d:/temp/feeds.opml");
            var descendants = doc.Descendants("outline");
            var elements = descendants.Elements("outline");
            List<Outline> t = doc
                                   .Descendants("outline")
                                   .Elements("outline")
                                   .Select(o => new Outline
                                   {
                                       Text = o.Attribute("text").Value,
                                       URL = o.Attribute("xmlUrl").Value
                                   })
                                   .ToList();
            foreach(Outline o in t)
            {
                try
                {
                    Debug.Print(o.Text);
                    InsertFeed(o.URL);
                }
                catch(Exception e)
                {
                    Debug.Print("Errore!!! " + o.Text);
                    Debug.Print(o.URL);
                    Debug.Print(e.StackTrace);
                }
            }
        }


        class Outline
        {
            public string Text { get; set; }
            public string URL { get; set; }
        }


        static void InsertFeed(string url)
        {
            SyndicationFeed feed;

            using (XmlReader reader = XmlReader.Create(url))
            {
                feed = SyndicationFeed.Load(reader);
                feed.BaseUri = new Uri(url);
                reader.Close();
            }
            long id = InsertDBFeed(feed);
            Debug.Print("Inserted with Id = " + id);
        }



        static void CreateDatabase()
        {
            if (!File.Exists(DatabasePath))
                SQLiteConnection.CreateFile(DatabasePath);

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

            using (SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;"))
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


        static long InsertDBFeed(SyndicationFeed feed)
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


            String host = feed.Links[0].Uri.Host;
            long feed_id;

            using (SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;"))
            {
                m_dbConnection.Open();
                SQLiteTransaction tran = m_dbConnection.BeginTransaction();
                try
                {
                    SQLiteCommand command = new SQLiteCommand(sqlInsertHosts, m_dbConnection);
                    command.Parameters.AddWithValue("$uri", host);
                    command.ExecuteNonQuery();

                    command = new SQLiteCommand(sqlGetHost, m_dbConnection);
                    command.Parameters.AddWithValue("$uri", host);
                    var host_id = command.ExecuteScalar();

                    command = new SQLiteCommand(sqlInsertFeeds, m_dbConnection);
                    command.Parameters.AddWithValue("$uri", feed.BaseUri.ToString());
                    command.Parameters.AddWithValue("$last_update", feed.LastUpdatedTime.ToString("s"));
                    command.Parameters.AddWithValue("$title", feed.Title.Text);
                    command.Parameters.AddWithValue("$host_id", host_id);
                    command.ExecuteNonQuery();

                    command = new SQLiteCommand(sqlGetFeed, m_dbConnection);
                    command.Parameters.AddWithValue("$uri", feed.BaseUri.ToString());
                    feed_id = (long)command.ExecuteScalar();
                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }

            return feed_id;
        }



        static void UpdateAllFeeds()
        {
            String sqlFeeds = @"
                SELECT *
                FROM Feeds;
            ";

            List<SyndicationFeed> feeds = new List<SyndicationFeed>();

            using (SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;"))
            {
                m_dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand(sqlFeeds, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    SyndicationFeed feed = null;
                    try
                    {
                        string uri = reader["uri"].ToString();
                        using (XmlReader x = XmlReader.Create(uri))
                        {
                            feed = SyndicationFeed.Load(x);
                            x.Close();
                        }
                    }
                    catch (Exception)
                    {
                        //Ignore
                    }
                    if (feed != null)
                        feeds.Add(feed);
                }
            }

            foreach (SyndicationFeed feed in feeds)
            {
                UpdateFeed(feed);
            }
        }


        static void UpdateFeed(SyndicationFeed feed)
        {

            String sqlInsertArticle = @"
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

            long feed_id = InsertDBFeed(feed);

            using (SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;"))
            {
                m_dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand(sqlInsertArticle, m_dbConnection);

                foreach (SyndicationItem item in feed.Items)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("$feed_id",feed_id);
                    command.Parameters.AddWithValue("$published_date", item.PublishDate.ToString("s"));
                    command.Parameters.AddWithValue("$title", item.Title.Text);
                    command.Parameters.AddWithValue("$uri", item.Links[0].Uri);
                    command.Parameters.AddWithValue("$unread", 1);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}



