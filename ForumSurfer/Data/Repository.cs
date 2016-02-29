using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.Data
{
    public class Repository
    {
        public const String DatabaseName = "ForumSurfer.sqlite";
        public static String DatabaseFolder
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(@"%AppData%\ForumSurfer");
            }
        }
        
        public static String DatabasePath
        {
            get
            {
                return DatabaseFolder + "\\" + DatabaseName;
            }
        }

        public static String ConnectionString
        {
            get
            {
                return "Data Source=" + DatabasePath + ";Version=3;";
            }
        }

        public static void CreateDatabase()
        {
            if (!File.Exists(DatabasePath))
            {
                Directory.CreateDirectory(DatabaseFolder);
                SQLiteConnection.CreateFile(DatabasePath);
            }

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

            using (SQLiteConnection m_dbConnection = new SQLiteConnection(ConnectionString))
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
    }
}
