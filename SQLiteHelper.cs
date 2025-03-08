using System;
using System.Data.SQLite;
using System.IO;

namespace SystemMonitor
{
    public class SQLiteHelper
    {
        public static SQLiteConnection CreateConnection()
        {
            //var dbPath = ConfigurationManager.GetDbPath();

            var dbPath = @"C:\Users\Muhammad\Desktop\c#_modul\github\SystemMonitor\SystemMonitor.db";

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
            var created_at = DateTime.Now.ToString();
            using (var connection = CreateConnection())
            {
                if (connection == null)
                {
                    return;
                }

                try
                {
                    using (var command = new SQLiteCommand("INSERT INTO JwtToken (token, created_at) VALUES (@token, @created_at )", connection))
                    {
                        command.Parameters.AddWithValue("@token", token);
                        command.Parameters.AddWithValue("@created_at", created_at);
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

        public static string GetJwtToken()
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
                    using (var command = new SQLiteCommand("SELECT token FROM JwtToken ORDER BY created_at DESC LIMIT 1", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                token = reader["token"].ToString();
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

        public static void LogMessage(string message)
        {
            using (var connection = CreateConnection())
            {
                if (connection == null)
                {
                    return;
                }

                try
                {
                    using (var command = new SQLiteCommand("INSERT INTO LogEntry (message) VALUES (@message)", connection))
                    {
                        command.Parameters.AddWithValue("@message", message);
                        command.Parameters.AddWithValue("@timestamp", DateTime.Now);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Xatolik: {ex.Message}");
                }
            }
        }
    }
}
