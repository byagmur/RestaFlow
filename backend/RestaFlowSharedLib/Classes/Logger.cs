using RestaurantSharedLib;
using System;
using System.Data;
using System.Data.SqlClient;

public static class Logger
{

    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Critical
    }

    public static void Log(LogLevel level, string message, Exception ex = null, string userName = null, string additionalInfo = null)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(DBContext.connectionString))
            {
                // SQL komutunu hazırla
                string sql = @"
                    INSERT INTO LogEntries (LogLevel, Message, StackTrace, Source, UserName) 
                    VALUES (@LogLevel, @Message, @StackTrace, @Source, @UserName )";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                   
                    command.Parameters.Add("@LogLevel", SqlDbType.VarChar, 20).Value = level.ToString();
                    command.Parameters.Add("@Message", SqlDbType.NVarChar).Value = message;
                    command.Parameters.Add("@StackTrace", SqlDbType.NVarChar).Value = (object)(ex?.StackTrace ?? "") ?? DBNull.Value;
                    command.Parameters.Add("@Source", SqlDbType.VarChar, 255).Value = (object)(ex?.Source ?? "") ?? DBNull.Value;
                    command.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = (object)userName ?? DBNull.Value;

                    
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception logEx)
        {
        
            Console.WriteLine($"Log yazma hatası: {logEx.Message}");

           
            try
            {
                string errorLogPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log_errors.txt");
                System.IO.File.AppendAllText(errorLogPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB Log Error: {logEx.Message}\r\n" +
                    $"Original message: {message}\r\n\r\n");
            }
            catch
            {
                
            }
        }
    }

    public static void LogInfo(string message, string userName = null, string additionalInfo = null)
    {
        Log(LogLevel.Info, message, null, userName, additionalInfo);
    }

    public static void LogWarning(string message, string userName = null, string additionalInfo = null)
    {
        Log(LogLevel.Warning, message, null, userName, additionalInfo);
    }

    public static void LogError(string message, Exception ex = null, string userName = null, string additionalInfo = null)
    {
        Log(LogLevel.Error, message, ex, userName, additionalInfo);
    }

    public static void LogCritical(string message, Exception ex = null, string userName = null, string additionalInfo = null)
    {
        Log(LogLevel.Critical, message, ex, userName, additionalInfo);
    }

    public static void Log(LogLevel error, string v, Exception ex, string username, object value)
    {
        throw new NotImplementedException();
    }
}