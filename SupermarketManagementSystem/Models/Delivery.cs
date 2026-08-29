namespace SupermarketManagementSystem.Models
{
    public class Delivery
    {
        public int DeliveryId { get; set; }

        public int PurchaseOrderId { get; set; }

        public string OrderNumber { get; set; } =
            string.Empty;

        public string SupplierName { get; set; } =
            string.Empty;

        public string DeliveryReference { get; set; } =
            string.Empty;

        public DateTime? DeliveryDate { get; set; }

        public string DeliveryStatus { get; set; } =
            "SCHEDULED";

        public int? ReceivedBy { get; set; }

        public string ReceivedByName { get; set; } =
            string.Empty;

        public string Notes { get; set; } =
            string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}