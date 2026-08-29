using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class SupplierRepository
    {
        public List<Supplier> GetAllSuppliers()
        {
            List<Supplier> suppliers = new();

            const string query = """
                SELECT
                    supplier_id,
                    user_id,
                    supplier_code,
                    supplier_name,
                    COALESCE(contact_person, '')
                        AS contact_person,
                    COALESCE(phone, '') AS phone,
                    COALESCE(email, '') AS email,
                    COALESCE(address, '') AS address,
                    supplier_status,
                    created_at
                FROM suppliers
                ORDER BY supplier_name;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                suppliers.Add(MapSupplier(reader));
            }

            return suppliers;
        }

        public bool SupplierCodeExists(
            string supplierCode,
            int? excludedSupplierId = null)
        {
            const string query = """
                SELECT COUNT(*)
                FROM suppliers
                WHERE supplier_code = @supplierCode
                  AND (
                      @excludedSupplierId IS NULL
                      OR supplier_id <> @excludedSupplierId
                  );
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@supplierCode",
                supplierCode
            );

            command.Parameters.AddWithValue(
                "@excludedSupplierId",
                excludedSupplierId.HasValue
                    ? excludedSupplierId.Value
                    : DBNull.Value
            );

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            ) > 0;
        }

        public int CreateSupplier(Supplier supplier)
        {
            const string query = """
                INSERT INTO suppliers
                (
                    user_id,
                    supplier_code,
                    supplier_name,
                    contact_person,
                    phone,
                    email,
                    address,
                    supplier_status
                )
                VALUES
                (
                    @userId,
                    @supplierCode,
                    @supplierName,
                    @contactPerson,
                    @phone,
                    @email,
                    @address,
                    @supplierStatus
                );

                SELECT LAST_INSERT_ID();
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, supplier);

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            );
        }

        public bool UpdateSupplier(Supplier supplier)
        {
            const string query = """
                UPDATE suppliers
                SET
                    user_id = @userId,
                    supplier_code = @supplierCode,
                    supplier_name = @supplierName,
                    contact_person = @contactPerson,
                    phone = @phone,
                    email = @email,
                    address = @address,
                    supplier_status = @supplierStatus
                WHERE supplier_id = @supplierId;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, supplier);

            command.Parameters.AddWithValue(
                "@supplierId",
                supplier.SupplierId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool BlockSupplier(int supplierId)
        {
            const string query = """
                UPDATE suppliers
                SET supplier_status = 'BLOCKED'
                WHERE supplier_id = @supplierId;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@supplierId",
                supplierId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        private static void AddParameters(
            MySqlCommand command,
            Supplier supplier)
        {
            command.Parameters.AddWithValue(
                "@userId",
                supplier.UserId.HasValue
                    ? supplier.UserId.Value
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@supplierCode",
                supplier.SupplierCode
            );

            command.Parameters.AddWithValue(
                "@supplierName",
                supplier.SupplierName
            );

            command.Parameters.AddWithValue(
                "@contactPerson",
                ToDatabaseValue(
                    supplier.ContactPerson
                )
            );

            command.Parameters.AddWithValue(
                "@phone",
                ToDatabaseValue(supplier.Phone)
            );

            command.Parameters.AddWithValue(
                "@email",
                ToDatabaseValue(supplier.Email)
            );

            command.Parameters.AddWithValue(
                "@address",
                ToDatabaseValue(supplier.Address)
            );

            command.Parameters.AddWithValue(
                "@supplierStatus",
                supplier.SupplierStatus
            );
        }

        private static object ToDatabaseValue(
            string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? DBNull.Value
                : value;
        }

        private static Supplier MapSupplier(
            MySqlDataReader reader)
        {
            int userIdIndex =
                reader.GetOrdinal("user_id");

            return new Supplier
            {
                SupplierId =
                    reader.GetInt32("supplier_id"),

                UserId =
                    reader.IsDBNull(userIdIndex)
                        ? null
                        : reader.GetInt32(userIdIndex),

                SupplierCode =
                    reader.GetString("supplier_code"),

                SupplierName =
                    reader.GetString("supplier_name"),

                ContactPerson =
                    reader.GetString("contact_person"),

                Phone =
                    reader.GetString("phone"),

                Email =
                    reader.GetString("email"),

                Address =
                    reader.GetString("address"),

                SupplierStatus =
                    reader.GetString("supplier_status"),

                CreatedAt =
                    reader.GetDateTime("created_at")
            };
        }
    }
}