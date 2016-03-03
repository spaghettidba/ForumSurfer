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
        public const String DatabaseVersion = "1.0.1";

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

            String sqlCreateTableOptions = @"
                CREATE TABLE IF NOT EXISTS GlobalOptions (
                    option_id INTEGER PRIMARY KEY,
                    name TEXT NOT NULL UNIQUE,
                    value TEXT NOT NULL
                )
            ";

            String sqlCreateTableBoilerplate = @"
                CREATE TABLE IF NOT EXISTS Boilerplate (
                    item_id INTEGER PRIMARY KEY,
                    title TEXT NOT NULL UNIQUE,
                    value TEXT NOT NULL
                )
            ";

            String sqlInsertDatabaseVersion = @"
                INSERT INTO GlobalOptions (option_id, name, value)
                SELECT 1, 'database_version', $version
                WHERE NOT EXISTS (
                    SELECT *
                    FROM GlobalOptions
                    WHERE option_id = 1
                );

                UPDATE GlobalOptions
                SET value = $version
                WHERE option_id = 1;
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
                    command = new SQLiteCommand(sqlCreateTableOptions, m_dbConnection);
                    command.ExecuteNonQuery();
                    command = new SQLiteCommand(sqlCreateTableBoilerplate, m_dbConnection);
                    command.ExecuteNonQuery();
                    command = new SQLiteCommand(sqlInsertDatabaseVersion, m_dbConnection);
                    command.Parameters.AddWithValue("$version", DatabaseVersion);
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
