using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class ProductBatchRepository
    {
        public List<ProductBatch> GetAll()
        {
            List<ProductBatch> batches = new();

            const string query = """
                SELECT
                    b.batch_id,
                    b.product_id,
                    p.product_name,
                    p.barcode,
                    b.batch_number,
                    b.manufactured_date,
                    b.expiry_date,
                    b.received_quantity,
                    b.available_quantity,
                    b.cost_price,
                    b.batch_status,
                    b.created_at
                FROM product_batches b
                INNER JOIN products p
                    ON b.product_id = p.product_id
                ORDER BY b.created_at DESC;
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
                batches.Add(MapBatch(reader));
            }

            return batches;
        }

        public bool BatchNumberExists(
            int productId,
            string batchNumber,
            int? excludedBatchId = null)
        {
            const string query = """
                SELECT COUNT(*)
                FROM product_batches
                WHERE product_id = @productId
                  AND batch_number = @batchNumber
                  AND (
                      @excludedBatchId IS NULL
                      OR batch_id <> @excludedBatchId
                  );
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@productId",
                productId
            );

            command.Parameters.AddWithValue(
                "@batchNumber",
                batchNumber
            );

            command.Parameters.AddWithValue(
                "@excludedBatchId",
                excludedBatchId.HasValue
                    ? excludedBatchId.Value
                    : DBNull.Value
            );

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            ) > 0;
        }

        public int Add(ProductBatch batch)
        {
            const string query = """
                INSERT INTO product_batches
                (
                    product_id,
                    batch_number,
                    manufactured_date,
                    expiry_date,
                    received_quantity,
                    available_quantity,
                    cost_price,
                    batch_status
                )
                VALUES
                (
                    @productId,
                    @batchNumber,
                    @manufacturedDate,
                    @expiryDate,
                    @receivedQuantity,
                    @availableQuantity,
                    @costPrice,
                    @batchStatus
                );

                SELECT LAST_INSERT_ID();
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, batch);

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            );
        }

        public bool Update(ProductBatch batch)
        {
            const string query = """
                UPDATE product_batches
                SET
                    product_id = @productId,
                    batch_number = @batchNumber,
                    manufactured_date = @manufacturedDate,
                    expiry_date = @expiryDate,
                    received_quantity = @receivedQuantity,
                    available_quantity = @availableQuantity,
                    cost_price = @costPrice,
                    batch_status = @batchStatus
                WHERE batch_id = @batchId;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, batch);

            command.Parameters.AddWithValue(
                "@batchId",
                batch.BatchId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool Block(int batchId)
        {
            const string query = """
                UPDATE product_batches
                SET batch_status = 'BLOCKED'
                WHERE batch_id = @batchId;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@batchId",
                batchId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public int UpdateExpiredBatches()
        {
            const string query = """
                UPDATE product_batches
                SET batch_status = 'EXPIRED'
                WHERE expiry_date IS NOT NULL
                  AND expiry_date < CURDATE()
                  AND batch_status = 'ACTIVE';
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            return command.ExecuteNonQuery();
        }

        private static void AddParameters(
            MySqlCommand command,
            ProductBatch batch)
        {
            command.Parameters.AddWithValue(
                "@productId",
                batch.ProductId
            );

            command.Parameters.AddWithValue(
                "@batchNumber",
                batch.BatchNumber
            );

            command.Parameters.AddWithValue(
                "@manufacturedDate",
                batch.ManufacturedDate.HasValue
                    ? batch.ManufacturedDate.Value.Date
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@expiryDate",
                batch.ExpiryDate.HasValue
                    ? batch.ExpiryDate.Value.Date
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@receivedQuantity",
                batch.ReceivedQuantity
            );

            command.Parameters.AddWithValue(
                "@availableQuantity",
                batch.AvailableQuantity
            );

            command.Parameters.AddWithValue(
                "@costPrice",
                batch.CostPrice
            );

            command.Parameters.AddWithValue(
                "@batchStatus",
                batch.BatchStatus
            );
        }

        private static ProductBatch MapBatch(
            MySqlDataReader reader)
        {
            int manufacturedDateIndex =
                reader.GetOrdinal("manufactured_date");

            int expiryDateIndex =
                reader.GetOrdinal("expiry_date");

            return new ProductBatch
            {
                BatchId =
                    reader.GetInt32("batch_id"),

                ProductId =
                    reader.GetInt32("product_id"),

                ProductName =
                    reader.GetString("product_name"),

                Barcode =
                    reader.GetString("barcode"),

                BatchNumber =
                    reader.GetString("batch_number"),

                ManufacturedDate =
                    reader.IsDBNull(manufacturedDateIndex)
                        ? null
                        : reader.GetDateTime(
                            manufacturedDateIndex
                        ),

                ExpiryDate =
                    reader.IsDBNull(expiryDateIndex)
                        ? null
                        : reader.GetDateTime(
                            expiryDateIndex
                        ),

                ReceivedQuantity =
                    reader.GetInt32("received_quantity"),

                AvailableQuantity =
                    reader.GetInt32("available_quantity"),

                CostPrice =
                    reader.GetDecimal("cost_price"),

                BatchStatus =
                    reader.GetString("batch_status"),

                CreatedAt =
                    reader.GetDateTime("created_at")
            };
        }
    }
}