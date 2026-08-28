using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class UserRepository
    {
        public User? FindByUsername(string username)
        {
            const string query = @"
                SELECT
                    u.user_id,
                    u.username,
                    u.password_hash,
                    u.full_name,
                    u.role_id,
                    r.role_name,
                    u.account_status,
                    u.created_at
                FROM users u
                INNER JOIN roles r
                    ON u.role_id = r.role_id
                WHERE u.username = @username
                LIMIT 1;";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@username",
                username
            );

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            return reader.Read()
                ? MapUser(reader)
                : null;
        }

        public List<User> GetAllUsers()
        {
            const string query = @"
                SELECT
                    u.user_id,
                    u.username,
                    u.password_hash,
                    u.full_name,
                    u.role_id,
                    r.role_name,
                    u.account_status,
                    u.created_at
                FROM users u
                INNER JOIN roles r
                    ON u.role_id = r.role_id
                ORDER BY u.user_id DESC;";

            List<User> users = new List<User>();

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                users.Add(MapUser(reader));
            }

            return users;
        }

        public List<Role> GetAllRoles()
        {
            const string query = @"
                SELECT role_id, role_name
                FROM roles
                ORDER BY role_name;";

            List<Role> roles = new List<Role>();

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                roles.Add(new Role
                {
                    RoleId = reader.GetInt32("role_id"),
                    RoleName = reader.GetString("role_name")
                });
            }

            return roles;
        }

        public bool UsernameExists(
            string username,
            int? excludedUserId = null)
        {
            const string query = @"
                SELECT COUNT(*)
                FROM users
                WHERE username = @username
                AND (
                    @excludedUserId IS NULL
                    OR user_id <> @excludedUserId
                );";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@username",
                username
            );

            command.Parameters.AddWithValue(
                "@excludedUserId",
                excludedUserId.HasValue
                    ? excludedUserId.Value
                    : DBNull.Value
            );

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            ) > 0;
        }

        public int CreateUser(User user)
        {
            const string query = @"
                INSERT INTO users (
                    username,
                    password_hash,
                    full_name,
                    role_id,
                    account_status
                )
                VALUES (
                    @username,
                    @passwordHash,
                    @fullName,
                    @roleId,
                    @accountStatus
                );

                SELECT LAST_INSERT_ID();";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddUserParameters(command, user);
            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            );
        }

        public bool UpdateUser(User user)
        {
            const string query = @"
                UPDATE users
                SET
                    username = @username,
                    full_name = @fullName,
                    role_id = @roleId,
                    account_status = @accountStatus
                WHERE user_id = @userId;";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddUserParameters(command, user);

            command.Parameters.AddWithValue(
                "@userId",
                user.UserId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool UpdateAccountStatus(
            int userId,
            string accountStatus)
        {
            const string query = @"
                UPDATE users
                SET account_status = @accountStatus
                WHERE user_id = @userId;";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@accountStatus",
                accountStatus
            );

            command.Parameters.AddWithValue(
                "@userId",
                userId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        private static void AddUserParameters(
            MySqlCommand command,
            User user)
        {
            command.Parameters.AddWithValue(
                "@username",
                user.Username
            );

            command.Parameters.AddWithValue(
                "@passwordHash",
                user.PasswordHash
            );

            command.Parameters.AddWithValue(
                "@fullName",
                user.FullName
            );

            command.Parameters.AddWithValue(
                "@roleId",
                user.RoleId
            );

            command.Parameters.AddWithValue(
                "@accountStatus",
                user.AccountStatus
            );
        }

        private static User MapUser(
            MySqlDataReader reader)
        {
            return new User
            {
                UserId = reader.GetInt32("user_id"),
                Username = reader.GetString("username"),
                PasswordHash =
                    reader.GetString("password_hash"),
                FullName = reader.GetString("full_name"),
                RoleId = reader.GetInt32("role_id"),
                RoleName = reader.GetString("role_name"),
                AccountStatus =
                    reader.GetString("account_status"),
                CreatedAt =
                    reader.GetDateTime("created_at")
            };
        }
    }
}