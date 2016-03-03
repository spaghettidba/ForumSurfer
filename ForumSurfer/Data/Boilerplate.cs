using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.Data
{
    public class Boilerplate
    {
        public long Id { get; private set; }
        public String Title { get; set; }
        public String Value { get; set; }

        public static List<Boilerplate> LoadAll()
        {
            List<Boilerplate> results = new List<Boilerplate>();

            String sqlBoilerplate = @"
                SELECT * 
                FROM Boilerplate;
            ";

            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand(sqlBoilerplate, m_dbConnection);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Boilerplate boilerplate = new Boilerplate();
                        boilerplate.Title = reader["title"].ToString();
                        boilerplate.Value = reader["value"].ToString();
                        boilerplate.Id = Int32.Parse(reader["item_id"].ToString());
                        results.Add(boilerplate);
                    }
                }
            }

            return results;
        }


        public void Save()
        {
            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {
                String sqlInsert = @"
                    INSERT INTO Boilerplate (title, value)
                    SELECT $title, $value
                    WHERE NOT EXISTS (
                        SELECT *
                        FROM Boilerplate
                        WHERE title = $title
                    );
                ";

                String sqlGetId = @"
                    SELECT item_id
                    FROM Boilerplate
                    WHERE title = $title;
                ";

                String sqlUpdate = @"
                    UPDATE Boilerplate 
                    SET value = $value
                    WHERE title = $title;
                ";


                m_dbConnection.Open();
                SQLiteTransaction tran = m_dbConnection.BeginTransaction();
                try
                {
                    SQLiteCommand command = new SQLiteCommand(sqlInsert, m_dbConnection);
                    command.Parameters.AddWithValue("$title", Title);
                    command.Parameters.AddWithValue("$value", Value);
                    int howMany = command.ExecuteNonQuery();

                    if(howMany > 0)
                    {
                        command = new SQLiteCommand(sqlGetId, m_dbConnection);
                        command.Parameters.AddWithValue("$title", Title);
                        var item_id = command.ExecuteScalar();
                        Id = (long)item_id;
                    } 
                    else
                    {
                        command = new SQLiteCommand(sqlUpdate, m_dbConnection);
                        command.Parameters.AddWithValue("$value", Value);
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


        public void Delete()
        {
            using (SQLiteConnection m_dbConnection = new SQLiteConnection(Repository.ConnectionString))
            {

                String sqlDelete = @"
                    DELETE 
                    FROM Boilerplate 
                    WHERE title = $title;
                ";

                SQLiteCommand command = new SQLiteCommand(sqlDelete, m_dbConnection);
                command.Parameters.AddWithValue("$title", Title);
                command.ExecuteNonQuery();
            }
        }

    }
}
