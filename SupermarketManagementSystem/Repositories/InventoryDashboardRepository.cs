using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class InventoryDashboardRepository
    {
        public InventoryDashboardSummary GetSummary()
        {
            const string query = """
                SELECT
                    (
                        SELECT COUNT(*)
                        FROM products
                        WHERE product_status = 'ACTIVE'
                    ) AS total_products,

                    (
                        SELECT COUNT(*)
                        FROM products p
                        LEFT JOIN stock s
                            ON p.product_id = s.product_id
                        WHERE p.product_status = 'ACTIVE'
                          AND COALESCE(
                              s.quantity_on_hand,
                              0
                          ) <= p.minimum_stock
                    ) AS low_stock_products,

                    (
                        SELECT COUNT(*)
                        FROM purchase_orders
                        WHERE order_status IN
                        (
                            'DRAFT',
                            'SENT',
                            'CONFIRMED',
                            'PROCESSING',
                            'PARTIALLY_DELIVERED'
                        )
                    ) AS pending_purchase_orders,

                    (
                        SELECT COUNT(*)
                        FROM suppliers
                        WHERE supplier_status = 'ACTIVE'
                    ) AS active_suppliers,

                    (
                        SELECT COUNT(*)
                        FROM inventory_notifications
                        WHERE notification_status = 'UNREAD'
                    ) AS unread_notifications,

                    (
                        SELECT COUNT(*)
                        FROM product_batches
                        WHERE batch_status = 'ACTIVE'
                          AND expiry_date BETWEEN
                              CURDATE()
                              AND DATE_ADD(
                                  CURDATE(),
                                  INTERVAL 30 DAY
                              )
                    ) AS expiring_batches;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            if (!reader.Read())
            {
                return new InventoryDashboardSummary();
            }

            return new InventoryDashboardSummary
            {
                TotalProducts =
                    reader.GetInt32("total_products"),

                LowStockProducts =
                    reader.GetInt32(
                        "low_stock_products"
                    ),

                PendingPurchaseOrders =
                    reader.GetInt32(
                        "pending_purchase_orders"
                    ),

                ActiveSuppliers =
                    reader.GetInt32(
                        "active_suppliers"
                    ),

                UnreadNotifications =
                    reader.GetInt32(
                        "unread_notifications"
                    ),

                ExpiringBatches =
                    reader.GetInt32(
                        "expiring_batches"
                    )
            };
        }
    }
}