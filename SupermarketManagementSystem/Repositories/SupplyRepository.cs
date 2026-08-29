using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class SupplyRepository
    {
        public List<Supply> GetAllSupplies()
        {
            List<Supply> supplies = new();

            const string query = """
                SELECT
                    sp.supply_id,
                    sp.supplier_id,
                    s.supplier_name,
                    sp.product_id,
                    p.product_name,
                    p.barcode,
                    COALESCE(sp.supplier_product_code, '')
                        AS supplier_product_code,
                    sp.supplier_price,
                    sp.lead_time_days,
                    sp.supply_status
                FROM supplies sp
                INNER JOIN suppliers s
                    ON sp.supplier_id = s.supplier_id
                INNER JOIN products p
                    ON sp.product_id = p.product_id
                ORDER BY
                    s.supplier_name,
                    p.product_name;
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
                supplies.Add(MapSupply(reader));
            }

            return supplies;
        }

        public bool SupplierProductExists(
            int supplierId,
            int productId,
            int? excludedSupplyId = null)
        {
            const string query = """
                SELECT COUNT(*)
                FROM supplies
                WHERE supplier_id = @supplierId
                  AND product_id = @productId
                  AND (
                      @excludedSupplyId IS NULL
                      OR supply_id <> @excludedSupplyId
                  );
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@supplierId",
                supplierId
            );

            command.Parameters.AddWithValue(
                "@productId",
                productId
            );

            command.Parameters.AddWithValue(
                "@excludedSupplyId",
                excludedSupplyId.HasValue
                    ? excludedSupplyId.Value
                    : DBNull.Value
            );

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            ) > 0;
        }

        public int CreateSupply(Supply supply)
        {
            const string query = """
                INSERT INTO supplies
                (
                    supplier_id,
                    product_id,
                    supplier_product_code,
                    supplier_price,
                    lead_time_days,
                    supply_status
                )
                VALUES
                (
                    @supplierId,
                    @productId,
                    @supplierProductCode,
                    @supplierPrice,
                    @leadTimeDays,
                    @supplyStatus
                );

                SELECT LAST_INSERT_ID();
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, supply);

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            );
        }

        public bool UpdateSupply(Supply supply)
        {
            const string query = """
                UPDATE supplies
                SET
                    supplier_id = @supplierId,
                    product_id = @productId,
                    supplier_product_code =
                        @supplierProductCode,
                    supplier_price = @supplierPrice,
                    lead_time_days = @leadTimeDays,
                    supply_status = @supplyStatus
                WHERE supply_id = @supplyId;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, supply);

            command.Parameters.AddWithValue(
                "@supplyId",
                supply.SupplyId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool DeactivateSupply(int supplyId)
        {
            const string query = """
                UPDATE supplies
                SET supply_status = 'INACTIVE'
                WHERE supply_id = @supplyId;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@supplyId",
                supplyId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        private static void AddParameters(
            MySqlCommand command,
            Supply supply)
        {
            command.Parameters.AddWithValue(
                "@supplierId",
                supply.SupplierId
            );

            command.Parameters.AddWithValue(
                "@productId",
                supply.ProductId
            );

            command.Parameters.AddWithValue(
                "@supplierProductCode",
                string.IsNullOrWhiteSpace(
                    supply.SupplierProductCode)
                    ? DBNull.Value
                    : supply.SupplierProductCode
            );

            command.Parameters.AddWithValue(
                "@supplierPrice",
                supply.SupplierPrice
            );

            command.Parameters.AddWithValue(
                "@leadTimeDays",
                supply.LeadTimeDays
            );

            command.Parameters.AddWithValue(
                "@supplyStatus",
                supply.SupplyStatus
            );
        }

        private static Supply MapSupply(
            MySqlDataReader reader)
        {
            return new Supply
            {
                SupplyId =
                    reader.GetInt32("supply_id"),

                SupplierId =
                    reader.GetInt32("supplier_id"),

                SupplierName =
                    reader.GetString("supplier_name"),

                ProductId =
                    reader.GetInt32("product_id"),

                ProductName =
                    reader.GetString("product_name"),

                Barcode =
                    reader.GetString("barcode"),

                SupplierProductCode =
                    reader.GetString(
                        "supplier_product_code"
                    ),

                SupplierPrice =
                    reader.GetDecimal("supplier_price"),

                LeadTimeDays =
                    reader.GetInt32("lead_time_days"),

                SupplyStatus =
                    reader.GetString("supply_status")
            };
        }
    }
}