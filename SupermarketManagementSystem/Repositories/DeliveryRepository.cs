using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class DeliveryRepository
    {
        public List<Delivery> GetAllDeliveries()
        {
            List<Delivery> deliveries = new();

            const string query = """
                SELECT
                    d.delivery_id,
                    d.purchase_order_id,
                    po.order_number,
                    s.supplier_name,
                    d.delivery_reference,
                    d.delivery_date,
                    d.delivery_status,
                    d.received_by,
                    COALESCE(u.full_name, '')
                        AS received_by_name,
                    COALESCE(d.notes, '') AS notes,
                    d.created_at
                FROM deliveries d
                INNER JOIN purchase_orders po
                    ON d.purchase_order_id =
                       po.purchase_order_id
                INNER JOIN suppliers s
                    ON po.supplier_id = s.supplier_id
                LEFT JOIN users u
                    ON d.received_by = u.user_id
                ORDER BY d.created_at DESC;
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
                deliveries.Add(MapDelivery(reader));
            }

            return deliveries;
        }

        public bool DeliveryReferenceExists(
            string deliveryReference,
            int? excludedDeliveryId = null)
        {
            const string query = """
                SELECT COUNT(*)
                FROM deliveries
                WHERE delivery_reference =
                    @deliveryReference
                  AND (
                      @excludedDeliveryId IS NULL
                      OR delivery_id <> @excludedDeliveryId
                  );
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@deliveryReference",
                deliveryReference
            );

            command.Parameters.AddWithValue(
                "@excludedDeliveryId",
                excludedDeliveryId.HasValue
                    ? excludedDeliveryId.Value
                    : DBNull.Value
            );

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            ) > 0;
        }

        public int CreateDelivery(Delivery delivery)
        {
            const string query = """
                INSERT INTO deliveries
                (
                    purchase_order_id,
                    delivery_reference,
                    delivery_date,
                    delivery_status,
                    received_by,
                    notes
                )
                VALUES
                (
                    @purchaseOrderId,
                    @deliveryReference,
                    @deliveryDate,
                    @deliveryStatus,
                    @receivedBy,
                    @notes
                );

                SELECT LAST_INSERT_ID();
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, delivery);

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            );
        }

        public bool UpdateDelivery(Delivery delivery)
        {
            const string query = """
                UPDATE deliveries
                SET
                    purchase_order_id =
                        @purchaseOrderId,
                    delivery_reference =
                        @deliveryReference,
                    delivery_date = @deliveryDate,
                    delivery_status = @deliveryStatus,
                    received_by = @receivedBy,
                    notes = @notes
                WHERE delivery_id = @deliveryId;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, delivery);

            command.Parameters.AddWithValue(
                "@deliveryId",
                delivery.DeliveryId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool CancelDelivery(int deliveryId)
        {
            const string query = """
                UPDATE deliveries
                SET delivery_status = 'CANCELLED'
                WHERE delivery_id = @deliveryId
                  AND delivery_status <> 'DELIVERED';
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@deliveryId",
                deliveryId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        private static void AddParameters(
            MySqlCommand command,
            Delivery delivery)
        {
            command.Parameters.AddWithValue(
                "@purchaseOrderId",
                delivery.PurchaseOrderId
            );

            command.Parameters.AddWithValue(
                "@deliveryReference",
                delivery.DeliveryReference
            );

            command.Parameters.AddWithValue(
                "@deliveryDate",
                delivery.DeliveryDate.HasValue
                    ? delivery.DeliveryDate.Value.Date
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@deliveryStatus",
                delivery.DeliveryStatus
            );

            command.Parameters.AddWithValue(
                "@receivedBy",
                delivery.ReceivedBy.HasValue
                    ? delivery.ReceivedBy.Value
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@notes",
                string.IsNullOrWhiteSpace(delivery.Notes)
                    ? DBNull.Value
                    : delivery.Notes
            );
        }

        private static Delivery MapDelivery(
            MySqlDataReader reader)
        {
            int deliveryDateIndex =
                reader.GetOrdinal("delivery_date");

            int receivedByIndex =
                reader.GetOrdinal("received_by");

            return new Delivery
            {
                DeliveryId =
                    reader.GetInt32("delivery_id"),

                PurchaseOrderId =
                    reader.GetInt32(
                        "purchase_order_id"
                    ),

                OrderNumber =
                    reader.GetString("order_number"),

                SupplierName =
                    reader.GetString("supplier_name"),

                DeliveryReference =
                    reader.GetString(
                        "delivery_reference"
                    ),

                DeliveryDate =
                    reader.IsDBNull(deliveryDateIndex)
                        ? null
                        : reader.GetDateTime(
                            deliveryDateIndex
                        ),

                DeliveryStatus =
                    reader.GetString(
                        "delivery_status"
                    ),

                ReceivedBy =
                    reader.IsDBNull(receivedByIndex)
                        ? null
                        : reader.GetInt32(
                            receivedByIndex
                        ),

                ReceivedByName =
                    reader.GetString(
                        "received_by_name"
                    ),

                Notes =
                    reader.GetString("notes"),

                CreatedAt =
                    reader.GetDateTime("created_at")
            };
        }
    }
}