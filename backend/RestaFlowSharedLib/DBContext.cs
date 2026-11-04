using System;
using System.Data;
using System.Data.SqlClient;

namespace RestaurantSharedLib
{
    public class DBContext : IDisposable
    {
        private const string ConnectionStringVariable = "RESTAFLOW_DB_CONNECTION";

        // Read from the environment so credentials never live in source control.
        public static string connectionString
        {
            get
            {
                var value = Environment.GetEnvironmentVariable(ConnectionStringVariable);
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException(
                        $"Environment variable '{ConnectionStringVariable}' is not set. See backend/README.md.");
                }
                return value;
            }
        }

        public readonly IDbConnection _connection;

        public DBContext()
        {
            _connection = new SqlConnection(connectionString);
            _connection.Open();
        }

        public static bool DBServerIsConnected()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
