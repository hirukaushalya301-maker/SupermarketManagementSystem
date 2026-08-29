using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class CategoryRepository
    {
        public List<Category> GetAllCategories()
        {
            const string query = @"
                SELECT
                    category_id,
                    category_name,
                    description,
                    category_status,
                    created_at
                FROM categories
                ORDER BY category_name;";

            List<Category> categories =
                new List<Category>();

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                categories.Add(MapCategory(reader));
            }

            return categories;
        }

        public bool CategoryNameExists(
            string categoryName,
            int? excludedCategoryId = null)
        {
            const string query = @"
                SELECT COUNT(*)
                FROM categories
                WHERE category_name = @categoryName
                AND (
                    @excludedCategoryId IS NULL
                    OR category_id <> @excludedCategoryId
                );";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@categoryName",
                categoryName
            );

            command.Parameters.AddWithValue(
                "@excludedCategoryId",
                excludedCategoryId.HasValue
                    ? excludedCategoryId.Value
                    : DBNull.Value
            );

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            ) > 0;
        }

        public int CreateCategory(Category category)
        {
            const string query = @"
                INSERT INTO categories (
                    category_name,
                    description,
                    category_status
                )
                VALUES (
                    @categoryName,
                    @description,
                    @categoryStatus
                );

                SELECT LAST_INSERT_ID();";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, category);
            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            );
        }

        public bool UpdateCategory(Category category)
        {
            const string query = @"
                UPDATE categories
                SET
                    category_name = @categoryName,
                    description = @description,
                    category_status = @categoryStatus
                WHERE category_id = @categoryId;";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, category);

            command.Parameters.AddWithValue(
                "@categoryId",
                category.CategoryId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool DeleteCategory(int categoryId)
        {
            const string query = @"
                DELETE FROM categories
                WHERE category_id = @categoryId
                AND NOT EXISTS (
                    SELECT 1
                    FROM products
                    WHERE products.category_id =
                        categories.category_id
                );";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@categoryId",
                categoryId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        private static void AddParameters(
            MySqlCommand command,
            Category category)
        {
            command.Parameters.AddWithValue(
                "@categoryName",
                category.CategoryName.Trim()
            );

            command.Parameters.AddWithValue(
                "@description",
                string.IsNullOrWhiteSpace(
                    category.Description)
                    ? DBNull.Value
                    : category.Description.Trim()
            );

            command.Parameters.AddWithValue(
                "@categoryStatus",
                category.CategoryStatus
            );
        }

        private static Category MapCategory(
            MySqlDataReader reader)
        {
            return new Category
            {
                CategoryId =
                    reader.GetInt32("category_id"),

                CategoryName =
                    reader.GetString("category_name"),

                Description = GetNullableString(
                    reader,
                    "description"
                ),

                CategoryStatus =
                    reader.GetString("category_status"),

                CreatedAt =
                    reader.GetDateTime("created_at")
            };
        }

        private static string GetNullableString(
            MySqlDataReader reader,
            string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            return reader.IsDBNull(index)
                ? string.Empty
                : reader.GetString(index);
        }
    }
}