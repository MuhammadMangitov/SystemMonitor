﻿using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace SystemMonitor
{
    public class SQLiteHelper
    {
        public static SQLiteConnection CreateConnection()
        {
            var dbPath = ConfigurationManager.GetDbPath();

            if (!File.Exists(dbPath))
            {
                Console.WriteLine("Baza mavjud emas!");
                return null;
            }

            var sqliteConnection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            sqliteConnection.Open();
            return sqliteConnection;
        }

        public static void InsertJwtToken(string token)
        {
            using (var connection = CreateConnection())
            {
                if (connection == null)
                {
                    return;
                }
                try
                {
                    using (var command = new SQLiteCommand(
                            "UPDATE Configurations SET Jwt_token = @jwt_token WHERE id = 1", connection))
                    { 
                        command.Parameters.AddWithValue("@jwt_token", token);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Xatolik: {ex.Message}");
                }
            }
        }

        public static void DeleteOldJwtToken()
        {
            using (var connection = CreateConnection())
            {
                if (connection == null)
                {
                    return;
                }

                try
                {
                    var command = new SQLiteCommand("DELETE FROM JwtToken WHERE Token IS NOT NULL", connection);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Xatolik: {ex.Message}");
                }
            }
        }

        public static async Task<string> GetJwtToken()
        {
            string token = null;

            using (var connection = CreateConnection())
            {
                if (connection == null)
                {
                    return null;
                }

                try
                {
                    using (var command = new SQLiteCommand("SELECT Jwt_token FROM Configurations LIMIT 1", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                token = reader["Jwt_token"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Xatolik: {ex.Message}");
                }
            }

            return token;
        }

        /*public static void LogMessage(string message)
        {
            using (var connection = CreateConnection())
            {
                if (connection == null)
                {
                    return;
                }

                try
                {
                    using (var command1 = new SQLiteCommand("INSERT INTO LogEntry (message) VALUES (@message)", connection))
                    {
                        command1.Parameters.AddWithValue("@message", message);
                        command1.Parameters.AddWithValue("@timestamp", DateTime.Now);
                        command1.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Xatolik: {ex.Message}");
                }
            }
        }*/

        public static void WriteLog(string module, string function, string message)
        {
            if (string.IsNullOrEmpty(module) || string.IsNullOrEmpty(function) || string.IsNullOrEmpty(message))
            {
                Console.WriteLine("Module, function yoki message bo‘sh bo‘lmasligi kerak");
                return;
            }

            var createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); 

            using (var connection = CreateConnection())
            {
                if (connection == null)
                {
                    return;
                }
                try
                {
                    using (var command = new SQLiteCommand(@"INSERT INTO ""LogEntry"" (""module"", ""function"", ""created_date"", ""message"") 
                          VALUES (@module, @function, @created_date, @message)", connection))
                    {
                        command.Parameters.AddWithValue("@module", module);
                        command.Parameters.AddWithValue("@function", function);
                        command.Parameters.AddWithValue("@created_date", createdDate);
                        command.Parameters.AddWithValue("@message", message);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Log yozishda xato: {ex.Message}");
                }
            }
        }
    }
}
