using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class GoodsReceiptRepository
    {
        public List<DeliveryReceiptItem> GetReceiptItems(
            int purchaseOrderId)
        {
            List<DeliveryReceiptItem> items = new();

            const string query = """
                SELECT
                    poi.purchase_order_item_id,
                    poi.purchase_order_id,
                    poi.product_id,
                    p.product_name,
                    p.barcode,
                    poi.ordered_quantity,
                    poi.received_quantity,
                    poi.unit_cost
                FROM purchase_order_items poi
                INNER JOIN products p
                    ON poi.product_id = p.product_id
                WHERE poi.purchase_order_id =
                    @purchaseOrderId
                ORDER BY p.product_name;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@purchaseOrderId",
                purchaseOrderId
            );

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                items.Add(
                    new DeliveryReceiptItem
                    {
                        PurchaseOrderItemId =
                            reader.GetInt32(
                                "purchase_order_item_id"
                            ),

                        PurchaseOrderId =
                            reader.GetInt32(
                                "purchase_order_id"
                            ),

                        ProductId =
                            reader.GetInt32("product_id"),

                        ProductName =
                            reader.GetString(
                                "product_name"
                            ),

                        Barcode =
                            reader.GetString("barcode"),

                        OrderedQuantity =
                            reader.GetInt32(
                                "ordered_quantity"
                            ),

                        PreviouslyReceivedQuantity =
                            reader.GetInt32(
                                "received_quantity"
                            ),

                        ReceivingQuantity = 0,

                        UnitCost =
                            reader.GetDecimal("unit_cost")
                    }
                );
            }

            return items;
        }

        public bool ReceiveDelivery(
            int deliveryId,
            int purchaseOrderId,
            List<DeliveryReceiptItem> receiptItems,
            int? receivedBy)
        {
            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            using MySqlTransaction transaction =
                connection.BeginTransaction();

            try
            {
                LockAndValidateDelivery(
                    connection,
                    transaction,
                    deliveryId,
                    purchaseOrderId
                );

                foreach (DeliveryReceiptItem item
                    in receiptItems)
                {
                    if (item.ReceivingQuantity <= 0)
                    {
                        continue;
                    }

                    ValidateOrderItemQuantity(
                        connection,
                        transaction,
                        purchaseOrderId,
                        item
                    );

                    int batchId = CreateOrUpdateBatch(
                        connection,
                        transaction,
                        item
                    );

                    IncreaseStock(
                        connection,
                        transaction,
                        item.ProductId,
                        item.ReceivingQuantity
                    );

                    InsertStockMovement(
                        connection,
                        transaction,
                        deliveryId,
                        batchId,
                        item,
                        receivedBy
                    );

                    UpdateReceivedQuantity(
                        connection,
                        transaction,
                        item
                    );
                }

                string orderStatus =
                    DetermineOrderStatus(
                        connection,
                        transaction,
                        purchaseOrderId
                    );

                UpdatePurchaseOrderStatus(
                    connection,
                    transaction,
                    purchaseOrderId,
                    orderStatus
                );

                MarkDeliveryAsDelivered(
                    connection,
                    transaction,
                    deliveryId,
                    receivedBy
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

        private static void LockAndValidateDelivery(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int deliveryId,
            int purchaseOrderId)
        {
            const string query = """
                SELECT delivery_status
                FROM deliveries
                WHERE delivery_id = @deliveryId
                  AND purchase_order_id =
                      @purchaseOrderId
                FOR UPDATE;
                """;

            using MySqlCommand command =
                new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

            command.Parameters.AddWithValue(
                "@deliveryId",
                deliveryId
            );

            command.Parameters.AddWithValue(
                "@purchaseOrderId",
                purchaseOrderId
            );

            object? result = command.ExecuteScalar();

            if (result == null)
            {
                throw new InvalidOperationException(
                    "The selected delivery was not found."
                );
            }

            string status =
                Convert.ToString(result) ??
                string.Empty;

            if (status == "DELIVERED")
            {
                throw new InvalidOperationException(
                    "This delivery has already been received."
                );
            }

            if (status == "CANCELLED" ||
                status == "REJECTED")
            {
                throw new InvalidOperationException(
                    "A cancelled or rejected delivery " +
                    "cannot be received."
                );
            }
        }

        private static void ValidateOrderItemQuantity(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int purchaseOrderId,
            DeliveryReceiptItem item)
        {
            const string query = """
                SELECT
                    ordered_quantity,
                    received_quantity
                FROM purchase_order_items
                WHERE purchase_order_item_id =
                    @purchaseOrderItemId
                  AND purchase_order_id =
                    @purchaseOrderId
                  AND product_id = @productId
                FOR UPDATE;
                """;

            using MySqlCommand command =
                new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

            command.Parameters.AddWithValue(
                "@purchaseOrderItemId",
                item.PurchaseOrderItemId
            );

            command.Parameters.AddWithValue(
                "@purchaseOrderId",
                purchaseOrderId
            );

            command.Parameters.AddWithValue(
                "@productId",
                item.ProductId
            );

            using MySqlDataReader reader =
                command.ExecuteReader();

            if (!reader.Read())
            {
                throw new InvalidOperationException(
                    "A purchase-order item was not found."
                );
            }

            int ordered =
                reader.GetInt32("ordered_quantity");

            int received =
                reader.GetInt32("received_quantity");

            reader.Close();

            int remaining = ordered - received;

            if (item.ReceivingQuantity > remaining)
            {
                throw new InvalidOperationException(
                    $"The receiving quantity for " +
                    $"{item.ProductName} exceeds the " +
                    "remaining order quantity."
                );
            }
        }

        private static int CreateOrUpdateBatch(
            MySqlConnection connection,
            MySqlTransaction transaction,
            DeliveryReceiptItem item)
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
                    @quantity,
                    @quantity,
                    @unitCost,
                    'ACTIVE'
                )
                ON DUPLICATE KEY UPDATE
                    batch_id = LAST_INSERT_ID(batch_id),
                    received_quantity =
                        received_quantity + @quantity,
                    available_quantity =
                        available_quantity + @quantity,
                    cost_price = @unitCost;

                SELECT LAST_INSERT_ID();
                """;

            using MySqlCommand command =
                new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

            command.Parameters.AddWithValue(
                "@productId",
                item.ProductId
            );

            command.Parameters.AddWithValue(
                "@batchNumber",
                item.BatchNumber
            );

            command.Parameters.AddWithValue(
                "@manufacturedDate",
                item.ManufacturedDate.HasValue
                    ? item.ManufacturedDate.Value.Date
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@expiryDate",
                item.ExpiryDate.HasValue
                    ? item.ExpiryDate.Value.Date
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@quantity",
                item.ReceivingQuantity
            );

            command.Parameters.AddWithValue(
                "@unitCost",
                item.UnitCost
            );

            return Convert.ToInt32(
                command.ExecuteScalar()
            );
        }

        private static void IncreaseStock(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int productId,
            int quantity)
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
                    @quantity,
                    0
                )
                ON DUPLICATE KEY UPDATE
                    quantity_on_hand =
                        quantity_on_hand + @quantity;
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

            command.Parameters.AddWithValue(
                "@quantity",
                quantity
            );

            command.ExecuteNonQuery();
        }

        private static void InsertStockMovement(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int deliveryId,
            int batchId,
            DeliveryReceiptItem item,
            int? receivedBy)
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
                    'PURCHASE',
                    @quantity,
                    'DELIVERY',
                    @deliveryId,
                    @notes,
                    @receivedBy
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
                item.ProductId
            );

            command.Parameters.AddWithValue(
                "@batchId",
                batchId
            );

            command.Parameters.AddWithValue(
                "@quantity",
                item.ReceivingQuantity
            );

            command.Parameters.AddWithValue(
                "@deliveryId",
                deliveryId
            );

            command.Parameters.AddWithValue(
                "@notes",
                "Stock received for batch " +
                item.BatchNumber
            );

            command.Parameters.AddWithValue(
                "@receivedBy",
                receivedBy.HasValue
                    ? receivedBy.Value
                    : DBNull.Value
            );

            command.ExecuteNonQuery();
        }

        private static void UpdateReceivedQuantity(
            MySqlConnection connection,
            MySqlTransaction transaction,
            DeliveryReceiptItem item)
        {
            const string query = """
                UPDATE purchase_order_items
                SET received_quantity =
                    received_quantity + @quantity
                WHERE purchase_order_item_id =
                    @purchaseOrderItemId;
                """;

            using MySqlCommand command =
                new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

            command.Parameters.AddWithValue(
                "@quantity",
                item.ReceivingQuantity
            );

            command.Parameters.AddWithValue(
                "@purchaseOrderItemId",
                item.PurchaseOrderItemId
            );

            command.ExecuteNonQuery();
        }

        private static string DetermineOrderStatus(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int purchaseOrderId)
        {
            const string query = """
                SELECT
                    SUM(received_quantity) AS received,
                    SUM(ordered_quantity) AS ordered
                FROM purchase_order_items
                WHERE purchase_order_id =
                    @purchaseOrderId;
                """;

            using MySqlCommand command =
                new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

            command.Parameters.AddWithValue(
                "@purchaseOrderId",
                purchaseOrderId
            );

            using MySqlDataReader reader =
                command.ExecuteReader();

            reader.Read();

            int received =
                Convert.ToInt32(reader["received"]);

            int ordered =
                Convert.ToInt32(reader["ordered"]);

            return received >= ordered
                ? "DELIVERED"
                : "PARTIALLY_DELIVERED";
        }

        private static void UpdatePurchaseOrderStatus(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int purchaseOrderId,
            string status)
        {
            const string query = """
                UPDATE purchase_orders
                SET order_status = @status
                WHERE purchase_order_id =
                    @purchaseOrderId;
                """;

            using MySqlCommand command =
                new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

            command.Parameters.AddWithValue(
                "@status",
                status
            );

            command.Parameters.AddWithValue(
                "@purchaseOrderId",
                purchaseOrderId
            );

            command.ExecuteNonQuery();
        }

        private static void MarkDeliveryAsDelivered(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int deliveryId,
            int? receivedBy)
        {
            const string query = """
                UPDATE deliveries
                SET
                    delivery_status = 'DELIVERED',
                    delivery_date = CURDATE(),
                    received_by = @receivedBy
                WHERE delivery_id = @deliveryId;
                """;

            using MySqlCommand command =
                new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

            command.Parameters.AddWithValue(
                "@receivedBy",
                receivedBy.HasValue
                    ? receivedBy.Value
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@deliveryId",
                deliveryId
            );

            command.ExecuteNonQuery();
        }
    }
}