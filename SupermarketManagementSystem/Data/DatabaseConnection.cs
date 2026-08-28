using MySql.Data.MySqlClient;

namespace SupermarketManagementSystem.Data
{
    public static class DatabaseConnection
    {
        private const string ConnectionString =
            "Server=localhost;" +
            "Port=3306;" +
            "Database=supermarket_management;" +
            "Uid=root;" +
            "Pwd=Hk@2056;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}