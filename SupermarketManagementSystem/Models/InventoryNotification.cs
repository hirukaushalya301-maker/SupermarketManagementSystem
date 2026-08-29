namespace SupermarketManagementSystem.Models
{
    public class InventoryNotification
    {
        public long NotificationId { get; set; }

        public int? ProductId { get; set; }

        public string ProductName { get; set; } =
            string.Empty;

        public string Barcode { get; set; } =
            string.Empty;

        public string NotificationType { get; set; } =
            string.Empty;

        public string Message { get; set; } =
            string.Empty;

        public string NotificationStatus { get; set; } =
            "UNREAD";

        public DateTime CreatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }
}