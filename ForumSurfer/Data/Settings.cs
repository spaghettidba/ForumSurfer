using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.Data
{
    public class Settings : Model.Settings
    {

        public Settings() : base()
        {

        }

        public Settings(Settings s) : base(s)
        {
            
        }


        public void Load()
        {
            String sql = @"
                SELECT *
                FROM GlobalOptions
            ";

            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        String _name = reader["name"].ToString();
                        String _value = reader["value"].ToString();
                        switch (_name)
                        {
                            case "RetentionDays":
                                RetentionDays = Int32.Parse(_value);
                                break;
                            case "AutoUpdateMinutes":
                                AutoUpdateMinutes = Int32.Parse(_value);
                                break;
                        }
                    }
                }
            }
        }


        public void Save()
        {
            SaveKeyValuePair("AutoUpdateMinutes", AutoUpdateMinutes.ToString());
            SaveKeyValuePair("RetentionDays", RetentionDays.ToString());
        }


        private void SaveKeyValuePair(String _name, String _value)
        {
            String sqlInsert = @"
                INSERT INTO GlobalOptions (name, value)
                SELECT $name, $value
                WHERE NOT EXISTS (
                    SELECT *
                    FROM GlobalOptions
                    WHERE name = $name
                );
            ";


            String sqlUpdate = @"
                UPDATE GlobalOptions 
                SET value = $value
                WHERE name = $name;
            ";

            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                m_dbConnection.Open();
                SQLiteTransaction tran = m_dbConnection.BeginTransaction();
                try
                {
                    SQLiteCommand command = new SQLiteCommand(sqlInsert, m_dbConnection);
                    command.Parameters.AddWithValue("$name", _name);
                    command.Parameters.AddWithValue("value", _value);
                    int inserted = command.ExecuteNonQuery();

                    if(inserted <= 0){
                        command = new SQLiteCommand(sqlUpdate, m_dbConnection);
                        command.Parameters.AddWithValue("$name", _name);
                        command.Parameters.AddWithValue("$value", _value);
                        command.ExecuteNonQuery();
                    }
                    
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
