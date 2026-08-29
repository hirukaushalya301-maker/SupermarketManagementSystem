using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class StockRepository
    {
        public List<StockItem> GetAllStock()
        {
            List<StockItem> stockItems = new();

            const string query = """
                SELECT
                    COALESCE(s.stock_id, 0) AS stock_id,
                    p.product_id,
                    p.barcode,
                    p.product_name,
                    c.category_name,
                    COALESCE(s.quantity_on_hand, 0)
                        AS quantity_on_hand,
                    COALESCE(s.reserved_quantity, 0)
                        AS reserved_quantity,
                    p.minimum_stock,
                    p.selling_price,
                    s.last_updated
                FROM products p
                INNER JOIN categories c
                    ON p.category_id = c.category_id
                LEFT JOIN stock s
                    ON p.product_id = s.product_id
                WHERE p.product_status <> 'DISCONTINUED'
                ORDER BY p.product_name;
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
                stockItems.Add(MapStockItem(reader));
            }

            return stockItems;
        }

        public List<StockMovement> GetMovements(
            int? productId = null)
        {
            List<StockMovement> movements = new();

            const string query = """
                SELECT
                    m.movement_id,
                    m.product_id,
                    p.product_name,
                    m.batch_id,
                    COALESCE(b.batch_number, '')
                        AS batch_number,
                    m.movement_type,
                    m.quantity,
                    COALESCE(m.reference_type, '')
                        AS reference_type,
                    m.reference_id,
                    COALESCE(m.notes, '') AS notes,
                    m.performed_by,
                    COALESCE(u.full_name, '')
                        AS performed_by_name,
                    m.created_at
                FROM stock_movements m
                INNER JOIN products p
                    ON m.product_id = p.product_id
                LEFT JOIN product_batches b
                    ON m.batch_id = b.batch_id
                LEFT JOIN users u
                    ON m.performed_by = u.user_id
                WHERE (
                    @productId IS NULL
                    OR m.product_id = @productId
                )
                ORDER BY m.created_at DESC;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@productId",
                productId.HasValue
                    ? productId.Value
                    : DBNull.Value
            );

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                movements.Add(MapMovement(reader));
            }

            return movements;
        }

        public bool AdjustStock(StockMovement movement)
        {
            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            using MySqlTransaction transaction =
                connection.BeginTransaction();

            try
            {
                EnsureStockRecord(
                    connection,
                    transaction,
                    movement.ProductId
                );

                int currentQuantity =
                    GetCurrentQuantity(
                        connection,
                        transaction,
                        movement.ProductId
                    );

                int change =
                    GetQuantityChange(movement);

                int newQuantity =
                    currentQuantity + change;

                if (newQuantity < 0)
                {
                    throw new InvalidOperationException(
                        "There is not enough stock " +
                        "for this operation."
                    );
                }

                UpdateStockQuantity(
                    connection,
                    transaction,
                    movement.ProductId,
                    newQuantity
                );

                InsertMovement(
                    connection,
                    transaction,
                    movement
                );

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static void EnsureStockRecord(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int productId)
        {
            const string query = """
                INSERT INTO stock
                (
                    product_id,
                    quantity_on_hand,
                    reserved_quantity
                )
                VALUES
                (
                    @productId,
                    0,
                    0
                )
                ON DUPLICATE KEY UPDATE
                    product_id = product_id;
                """;

            using MySqlCommand command =
                new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

            command.Parameters.AddWithValue(
                "@productId",
                productId
            );

            command.ExecuteNonQuery();
        }

        private static int GetCurrentQuantity(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int productId)
        {
            const string query = """
                SELECT quantity_on_hand
                FROM stock
                WHERE product_id = @productId
                FOR UPDATE;
                """;

            using MySqlCommand command =
                new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

            command.Parameters.AddWithValue(
                "@productId",
                productId
            );

            return Convert.ToInt32(
                command.ExecuteScalar()
            );
        }

        private static void UpdateStockQuantity(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int productId,
            int newQuantity)
        {
            const string query = """
                UPDATE stock
                SET quantity_on_hand = @newQuantity
                WHERE product_id = @productId;
                """;

            using MySqlCommand command =
                new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

            command.Parameters.AddWithValue(
                "@newQuantity",
                newQuantity
            );

            command.Parameters.AddWithValue(
                "@productId",
                productId
            );

            command.ExecuteNonQuery();
        }

        private static void InsertMovement(
            MySqlConnection connection,
            MySqlTransaction transaction,
            StockMovement movement)
        {
            const string query = """
                INSERT INTO stock_movements
                (
                    product_id,
                    batch_id,
                    movement_type,
                    quantity,
                    reference_type,
                    reference_id,
                    notes,
                    performed_by
                )
                VALUES
                (
                    @productId,
                    @batchId,
                    @movementType,
                    @quantity,
                    @referenceType,
                    @referenceId,
                    @notes,
                    @performedBy
                );
                """;

            using MySqlCommand command =
                new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

            command.Parameters.AddWithValue(
                "@productId",
                movement.ProductId
            );

            command.Parameters.AddWithValue(
                "@batchId",
                movement.BatchId.HasValue
                    ? movement.BatchId.Value
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@movementType",
                movement.MovementType
            );

            command.Parameters.AddWithValue(
                "@quantity",
                movement.Quantity
            );

            command.Parameters.AddWithValue(
                "@referenceType",
                string.IsNullOrWhiteSpace(
                    movement.ReferenceType)
                    ? DBNull.Value
                    : movement.ReferenceType
            );

            command.Parameters.AddWithValue(
                "@referenceId",
                movement.ReferenceId.HasValue
                    ? movement.ReferenceId.Value
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@notes",
                string.IsNullOrWhiteSpace(movement.Notes)
                    ? DBNull.Value
                    : movement.Notes
            );

            command.Parameters.AddWithValue(
                "@performedBy",
                movement.PerformedBy.HasValue
                    ? movement.PerformedBy.Value
                    : DBNull.Value
            );

            command.ExecuteNonQuery();
        }

        private static int GetQuantityChange(
            StockMovement movement)
        {
            if (movement.IsStockIncrease)
            {
                return movement.Quantity;
            }

            if (movement.IsStockDecrease)
            {
                return -movement.Quantity;
            }

            throw new InvalidOperationException(
                "Invalid stock movement type."
            );
        }

        private static StockItem MapStockItem(
            MySqlDataReader reader)
        {
            int lastUpdatedIndex =
                reader.GetOrdinal("last_updated");

            return new StockItem
            {
                StockId =
                    reader.GetInt32("stock_id"),

                ProductId =
                    reader.GetInt32("product_id"),

                Barcode =
                    reader.GetString("barcode"),

                ProductName =
                    reader.GetString("product_name"),

                CategoryName =
                    reader.GetString("category_name"),

                QuantityOnHand =
                    reader.GetInt32("quantity_on_hand"),

                ReservedQuantity =
                    reader.GetInt32("reserved_quantity"),

                MinimumStock =
                    reader.GetInt32("minimum_stock"),

                SellingPrice =
                    reader.GetDecimal("selling_price"),

                LastUpdated =
                    reader.IsDBNull(lastUpdatedIndex)
                        ? null
                        : reader.GetDateTime(
                            lastUpdatedIndex
                        )
            };
        }

        private static StockMovement MapMovement(
            MySqlDataReader reader)
        {
            int batchIdIndex =
                reader.GetOrdinal("batch_id");

            int referenceIdIndex =
                reader.GetOrdinal("reference_id");

            int performedByIndex =
                reader.GetOrdinal("performed_by");

            return new StockMovement
            {
                MovementId =
                    reader.GetInt64("movement_id"),

                ProductId =
                    reader.GetInt32("product_id"),

                ProductName =
                    reader.GetString("product_name"),

                BatchId =
                    reader.IsDBNull(batchIdIndex)
                        ? null
                        : reader.GetInt32(batchIdIndex),

                BatchNumber =
                    reader.GetString("batch_number"),

                MovementType =
                    reader.GetString("movement_type"),

                Quantity =
                    reader.GetInt32("quantity"),

                ReferenceType =
                    reader.GetString("reference_type"),

                ReferenceId =
                    reader.IsDBNull(referenceIdIndex)
                        ? null
                        : reader.GetInt64(
                            referenceIdIndex
                        ),

                Notes =
                    reader.GetString("notes"),

                PerformedBy =
                    reader.IsDBNull(performedByIndex)
                        ? null
                        : reader.GetInt32(
                            performedByIndex
                        ),

                PerformedByName =
                    reader.GetString("performed_by_name"),

                CreatedAt =
                    reader.GetDateTime("created_at")
            };
        }
    }
}