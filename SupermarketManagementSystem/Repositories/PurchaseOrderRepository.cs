using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class PurchaseOrderRepository
    {
        public List<PurchaseOrder> GetAllOrders()
        {
            List<PurchaseOrder> orders = new();

            const string query = """
                SELECT
                    po.purchase_order_id,
                    po.order_number,
                    po.supplier_id,
                    s.supplier_name,
                    po.order_date,
                    po.expected_delivery_date,
                    po.order_status,
                    COALESCE(po.supplier_response_note, '')
                        AS supplier_response_note,
                    po.subtotal,
                    po.tax_amount,
                    po.total_amount,
                    po.created_by,
                    COALESCE(u.full_name, '')
                        AS created_by_name,
                    po.created_at
                FROM purchase_orders po
                INNER JOIN suppliers s
                    ON po.supplier_id = s.supplier_id
                LEFT JOIN users u
                    ON po.created_by = u.user_id
                ORDER BY po.created_at DESC;
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
                orders.Add(MapOrder(reader));
            }

            return orders;
        }

        public List<PurchaseOrderItem> GetOrderItems(
            int purchaseOrderId)
        {
            List<PurchaseOrderItem> items = new();

            const string query = """
                SELECT
                    poi.purchase_order_item_id,
                    poi.purchase_order_id,
                    poi.product_id,
                    p.product_name,
                    p.barcode,
                    poi.ordered_quantity,
                    poi.received_quantity,
                    poi.unit_cost,
                    poi.line_total
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
                items.Add(MapOrderItem(reader));
            }

            return items;
        }

        public bool OrderNumberExists(
            string orderNumber,
            int? excludedOrderId = null)
        {
            const string query = """
                SELECT COUNT(*)
                FROM purchase_orders
                WHERE order_number = @orderNumber
                  AND (
                      @excludedOrderId IS NULL
                      OR purchase_order_id <> @excludedOrderId
                  );
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@orderNumber",
                orderNumber
            );

            command.Parameters.AddWithValue(
                "@excludedOrderId",
                excludedOrderId.HasValue
                    ? excludedOrderId.Value
                    : DBNull.Value
            );

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            ) > 0;
        }

        public int CreateOrder(PurchaseOrder order)
        {
            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            using MySqlTransaction transaction =
                connection.BeginTransaction();

            try
            {
                int orderId = InsertOrder(
                    connection,
                    transaction,
                    order
                );

                foreach (PurchaseOrderItem item in order.Items)
                {
                    InsertOrderItem(
                        connection,
                        transaction,
                        orderId,
                        item
                    );
                }

                transaction.Commit();
                return orderId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool UpdateDraftOrder(PurchaseOrder order)
        {
            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            using MySqlTransaction transaction =
                connection.BeginTransaction();

            try
            {
                const string updateQuery = """
                    UPDATE purchase_orders
                    SET
                        order_number = @orderNumber,
                        supplier_id = @supplierId,
                        order_date = @orderDate,
                        expected_delivery_date =
                            @expectedDeliveryDate,
                        subtotal = @subtotal,
                        tax_amount = @taxAmount,
                        total_amount = @totalAmount
                    WHERE purchase_order_id =
                        @purchaseOrderId
                      AND order_status = 'DRAFT';
                    """;

                using MySqlCommand updateCommand =
                    new MySqlCommand(
                        updateQuery,
                        connection,
                        transaction
                    );

                AddOrderParameters(
                    updateCommand,
                    order
                );

                updateCommand.Parameters.AddWithValue(
                    "@purchaseOrderId",
                    order.PurchaseOrderId
                );

                int updatedRows =
                    updateCommand.ExecuteNonQuery();

                if (updatedRows == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                const string deleteItemsQuery = """
                    DELETE FROM purchase_order_items
                    WHERE purchase_order_id =
                        @purchaseOrderId;
                    """;

                using MySqlCommand deleteCommand =
                    new MySqlCommand(
                        deleteItemsQuery,
                        connection,
                        transaction
                    );

                deleteCommand.Parameters.AddWithValue(
                    "@purchaseOrderId",
                    order.PurchaseOrderId
                );

                deleteCommand.ExecuteNonQuery();

                foreach (PurchaseOrderItem item in order.Items)
                {
                    InsertOrderItem(
                        connection,
                        transaction,
                        order.PurchaseOrderId,
                        item
                    );
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool UpdateOrderStatus(
            int purchaseOrderId,
            string status)
        {
            const string query = """
                UPDATE purchase_orders
                SET order_status = @status
                WHERE purchase_order_id =
                    @purchaseOrderId;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@status",
                status
            );

            command.Parameters.AddWithValue(
                "@purchaseOrderId",
                purchaseOrderId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        private static int InsertOrder(
            MySqlConnection connection,
            MySqlTransaction transaction,
            PurchaseOrder order)
        {
            const string query = """
                INSERT INTO purchase_orders
                (
                    order_number,
                    supplier_id,
                    order_date,
                    expected_delivery_date,
                    order_status,
                    supplier_response_note,
                    subtotal,
                    tax_amount,
                    total_amount,
                    created_by
                )
                VALUES
                (
                    @orderNumber,
                    @supplierId,
                    @orderDate,
                    @expectedDeliveryDate,
                    @orderStatus,
                    @supplierResponseNote,
                    @subtotal,
                    @taxAmount,
                    @totalAmount,
                    @createdBy
                );

                SELECT LAST_INSERT_ID();
                """;

            using MySqlCommand command =
                new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

            AddOrderParameters(command, order);

            command.Parameters.AddWithValue(
                "@orderStatus",
                order.OrderStatus
            );

            command.Parameters.AddWithValue(
                "@supplierResponseNote",
                string.IsNullOrWhiteSpace(
                    order.SupplierResponseNote)
                    ? DBNull.Value
                    : order.SupplierResponseNote
            );

            command.Parameters.AddWithValue(
                "@createdBy",
                order.CreatedBy.HasValue
                    ? order.CreatedBy.Value
                    : DBNull.Value
            );

            return Convert.ToInt32(
                command.ExecuteScalar()
            );
        }

        private static void InsertOrderItem(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int orderId,
            PurchaseOrderItem item)
        {
            const string query = """
                INSERT INTO purchase_order_items
                (
                    purchase_order_id,
                    product_id,
                    ordered_quantity,
                    received_quantity,
                    unit_cost,
                    line_total
                )
                VALUES
                (
                    @purchaseOrderId,
                    @productId,
                    @orderedQuantity,
                    @receivedQuantity,
                    @unitCost,
                    @lineTotal
                );
                """;

            using MySqlCommand command =
                new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

            command.Parameters.AddWithValue(
                "@purchaseOrderId",
                orderId
            );

            command.Parameters.AddWithValue(
                "@productId",
                item.ProductId
            );

            command.Parameters.AddWithValue(
                "@orderedQuantity",
                item.OrderedQuantity
            );

            command.Parameters.AddWithValue(
                "@receivedQuantity",
                item.ReceivedQuantity
            );

            command.Parameters.AddWithValue(
                "@unitCost",
                item.UnitCost
            );

            command.Parameters.AddWithValue(
                "@lineTotal",
                item.LineTotal
            );

            command.ExecuteNonQuery();
        }

        private static void AddOrderParameters(
            MySqlCommand command,
            PurchaseOrder order)
        {
            command.Parameters.AddWithValue(
                "@orderNumber",
                order.OrderNumber
            );

            command.Parameters.AddWithValue(
                "@supplierId",
                order.SupplierId
            );

            command.Parameters.AddWithValue(
                "@orderDate",
                order.OrderDate.Date
            );

            command.Parameters.AddWithValue(
                "@expectedDeliveryDate",
                order.ExpectedDeliveryDate.HasValue
                    ? order.ExpectedDeliveryDate.Value.Date
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@subtotal",
                order.Subtotal
            );

            command.Parameters.AddWithValue(
                "@taxAmount",
                order.TaxAmount
            );

            command.Parameters.AddWithValue(
                "@totalAmount",
                order.TotalAmount
            );
        }

        private static PurchaseOrder MapOrder(
            MySqlDataReader reader)
        {
            int expectedDateIndex =
                reader.GetOrdinal(
                    "expected_delivery_date"
                );

            int createdByIndex =
                reader.GetOrdinal("created_by");

            return new PurchaseOrder
            {
                PurchaseOrderId =
                    reader.GetInt32(
                        "purchase_order_id"
                    ),

                OrderNumber =
                    reader.GetString("order_number"),

                SupplierId =
                    reader.GetInt32("supplier_id"),

                SupplierName =
                    reader.GetString("supplier_name"),

                OrderDate =
                    reader.GetDateTime("order_date"),

                ExpectedDeliveryDate =
                    reader.IsDBNull(expectedDateIndex)
                        ? null
                        : reader.GetDateTime(
                            expectedDateIndex
                        ),

                OrderStatus =
                    reader.GetString("order_status"),

                SupplierResponseNote =
                    reader.GetString(
                        "supplier_response_note"
                    ),

                Subtotal =
                    reader.GetDecimal("subtotal"),

                TaxAmount =
                    reader.GetDecimal("tax_amount"),

                TotalAmount =
                    reader.GetDecimal("total_amount"),

                CreatedBy =
                    reader.IsDBNull(createdByIndex)
                        ? null
                        : reader.GetInt32(createdByIndex),

                CreatedByName =
                    reader.GetString("created_by_name"),

                CreatedAt =
                    reader.GetDateTime("created_at")
            };
        }

        private static PurchaseOrderItem MapOrderItem(
            MySqlDataReader reader)
        {
            return new PurchaseOrderItem
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
                    reader.GetString("product_name"),

                Barcode =
                    reader.GetString("barcode"),

                OrderedQuantity =
                    reader.GetInt32(
                        "ordered_quantity"
                    ),

                ReceivedQuantity =
                    reader.GetInt32(
                        "received_quantity"
                    ),

                UnitCost =
                    reader.GetDecimal("unit_cost")
            };
        }
    }
}