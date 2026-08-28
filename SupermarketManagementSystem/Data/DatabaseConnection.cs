using MySql.Data.MySqlClient;

namespace SupermarketManagementSystem.Data
{
    public static class DatabaseConnection
    {
        public static MySqlConnection GetConnection()
        {
            string? password = Environment.GetEnvironmentVariable(
                "SUPERMARKET_DB_PASSWORD"
            );

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "MySQL password environment variable is not configured."
                );
            }

            MySqlConnectionStringBuilder builder = new()
            {
                Server = "localhost",
                Port = 3306,
                Database = "supermarket_management",
                UserID = "root",
                Password = password
            };

            return new MySqlConnection(builder.ConnectionString);
        }
    }
}