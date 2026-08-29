using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class InventoryNotificationRepository
    {
        public List<InventoryNotification>
            GetAllNotifications()
        {
            List<InventoryNotification> notifications =
                new();

            const string query = """
                SELECT
                    n.notification_id,
                    n.product_id,
                    COALESCE(p.product_name, '')
                        AS product_name,
                    COALESCE(p.barcode, '') AS barcode,
                    n.notification_type,
                    n.message,
                    n.notification_status,
                    n.created_at,
                    n.resolved_at
                FROM inventory_notifications n
                LEFT JOIN products p
                    ON n.product_id = p.product_id
                ORDER BY
                    CASE n.notification_status
                        WHEN 'UNREAD' THEN 1
                        WHEN 'READ' THEN 2
                        ELSE 3
                    END,
                    n.created_at DESC;
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
                notifications.Add(
                    MapNotification(reader)
                );
            }

            return notifications;
        }

        public int GenerateAutomaticNotifications()
        {
            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            using MySqlTransaction transaction =
                connection.BeginTransaction();

            try
            {
                int generated = 0;

                generated += ExecuteGenerationQuery(
                    connection,
                    transaction,
                    CreateOutOfStockQuery()
                );

                generated += ExecuteGenerationQuery(
                    connection,
                    transaction,
                    CreateLowStockQuery()
                );

                generated += ExecuteGenerationQuery(
                    connection,
                    transaction,
                    CreateExpiringSoonQuery()
                );

                generated += ExecuteGenerationQuery(
                    connection,
                    transaction,
                    CreateExpiredQuery()
                );

                transaction.Commit();
                return generated;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool MarkAsRead(long notificationId)
        {
            const string query = """
                UPDATE inventory_notifications
                SET notification_status = 'READ'
                WHERE notification_id = @notificationId
                  AND notification_status = 'UNREAD';
                """;

            return ExecuteStatusCommand(
                query,
                notificationId
            );
        }

        public bool Resolve(long notificationId)
        {
            const string query = """
                UPDATE inventory_notifications
                SET
                    notification_status = 'RESOLVED',
                    resolved_at = NOW()
                WHERE notification_id = @notificationId
                  AND notification_status <> 'RESOLVED';
                """;

            return ExecuteStatusCommand(
                query,
                notificationId
            );
        }

        private static bool ExecuteStatusCommand(
            string query,
            long notificationId)
        {
            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@notificationId",
                notificationId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        private static int ExecuteGenerationQuery(
            MySqlConnection connection,
            MySqlTransaction transaction,
            string query)
        {
            using MySqlCommand command =
                new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

            return command.ExecuteNonQuery();
        }

        private static string CreateOutOfStockQuery()
        {
            return """
                INSERT INTO inventory_notifications
                (
                    product_id,
                    notification_type,
                    message,
                    notification_status
                )
                SELECT
                    p.product_id,
                    'OUT_OF_STOCK',
                    CONCAT(
                        p.product_name,
                        ' is out of stock.'
                    ),
                    'UNREAD'
                FROM products p
                LEFT JOIN stock s
                    ON p.product_id = s.product_id
                WHERE p.product_status = 'ACTIVE'
                  AND COALESCE(
                      s.quantity_on_hand,
                      0
                  ) = 0
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM inventory_notifications n
                      WHERE n.product_id = p.product_id
                        AND n.notification_type =
                            'OUT_OF_STOCK'
                        AND n.notification_status <>
                            'RESOLVED'
                  );
                """;
        }

        private static string CreateLowStockQuery()
        {
            return """
                INSERT INTO inventory_notifications
                (
                    product_id,
                    notification_type,
                    message,
                    notification_status
                )
                SELECT
                    p.product_id,
                    'LOW_STOCK',
                    CONCAT(
                        p.product_name,
                        ' has low stock. Current quantity: ',
                        s.quantity_on_hand,
                        '.'
                    ),
                    'UNREAD'
                FROM products p
                INNER JOIN stock s
                    ON p.product_id = s.product_id
                WHERE p.product_status = 'ACTIVE'
                  AND s.quantity_on_hand > 0
                  AND s.quantity_on_hand <=
                      p.minimum_stock
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM inventory_notifications n
                      WHERE n.product_id = p.product_id
                        AND n.notification_type =
                            'LOW_STOCK'
                        AND n.notification_status <>
                            'RESOLVED'
                  );
                """;
        }

        private static string CreateExpiringSoonQuery()
        {
            return """
                INSERT INTO inventory_notifications
                (
                    product_id,
                    notification_type,
                    message,
                    notification_status
                )
                SELECT
                    b.product_id,
                    'EXPIRING_SOON',
                    CONCAT(
                        p.product_name,
                        ' batch ',
                        b.batch_number,
                        ' will expire on ',
                        DATE_FORMAT(
                            b.expiry_date,
                            '%Y-%m-%d'
                        ),
                        '.'
                    ),
                    'UNREAD'
                FROM product_batches b
                INNER JOIN products p
                    ON b.product_id = p.product_id
                WHERE b.batch_status = 'ACTIVE'
                  AND b.expiry_date BETWEEN
                      CURDATE()
                      AND DATE_ADD(
                          CURDATE(),
                          INTERVAL 30 DAY
                      )
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM inventory_notifications n
                      WHERE n.product_id = b.product_id
                        AND n.notification_type =
                            'EXPIRING_SOON'
                        AND n.message = CONCAT(
                            p.product_name,
                            ' batch ',
                            b.batch_number,
                            ' will expire on ',
                            DATE_FORMAT(
                                b.expiry_date,
                                '%Y-%m-%d'
                            ),
                            '.'
                        )
                        AND n.notification_status <>
                            'RESOLVED'
                  );
                """;
        }

        private static string CreateExpiredQuery()
        {
            return """
                INSERT INTO inventory_notifications
                (
                    product_id,
                    notification_type,
                    message,
                    notification_status
                )
                SELECT
                    b.product_id,
                    'EXPIRED',
                    CONCAT(
                        p.product_name,
                        ' batch ',
                        b.batch_number,
                        ' has expired.'
                    ),
                    'UNREAD'
                FROM product_batches b
                INNER JOIN products p
                    ON b.product_id = p.product_id
                WHERE b.expiry_date < CURDATE()
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM inventory_notifications n
                      WHERE n.product_id = b.product_id
                        AND n.notification_type =
                            'EXPIRED'
                        AND n.message = CONCAT(
                            p.product_name,
                            ' batch ',
                            b.batch_number,
                            ' has expired.'
                        )
                        AND n.notification_status <>
                            'RESOLVED'
                  );
                """;
        }

        private static InventoryNotification
            MapNotification(MySqlDataReader reader)
        {
            int productIdIndex =
                reader.GetOrdinal("product_id");

            int resolvedAtIndex =
                reader.GetOrdinal("resolved_at");

            return new InventoryNotification
            {
                NotificationId =
                    reader.GetInt64("notification_id"),

                ProductId =
                    reader.IsDBNull(productIdIndex)
                        ? null
                        : reader.GetInt32(productIdIndex),

                ProductName =
                    reader.GetString("product_name"),

                Barcode =
                    reader.GetString("barcode"),

                NotificationType =
                    reader.GetString(
                        "notification_type"
                    ),

                Message =
                    reader.GetString("message"),

                NotificationStatus =
                    reader.GetString(
                        "notification_status"
                    ),

                CreatedAt =
                    reader.GetDateTime("created_at"),

                ResolvedAt =
                    reader.IsDBNull(resolvedAtIndex)
                        ? null
                        : reader.GetDateTime(
                            resolvedAtIndex
                        )
            };
        }
    }
}