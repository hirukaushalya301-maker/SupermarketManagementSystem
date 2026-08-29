namespace SupermarketManagementSystem.Models
{
    public class PurchaseOrder
    {
        public int PurchaseOrderId { get; set; }

        public string OrderNumber { get; set; } =
            string.Empty;

        public int SupplierId { get; set; }

        public string SupplierName { get; set; } =
            string.Empty;

        public DateTime OrderDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public string OrderStatus { get; set; } =
            "DRAFT";

        public string SupplierResponseNote { get; set; } =
            string.Empty;

        public decimal Subtotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public int? CreatedBy { get; set; }

        public string CreatedByName { get; set; } =
            string.Empty;

        public DateTime CreatedAt { get; set; }

        public List<PurchaseOrderItem> Items { get; set; } =
            new();
    }
}