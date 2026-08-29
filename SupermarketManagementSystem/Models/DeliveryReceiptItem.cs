namespace SupermarketManagementSystem.Models
{
    public class DeliveryReceiptItem
    {
        public int PurchaseOrderItemId { get; set; }

        public int PurchaseOrderId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } =
            string.Empty;

        public string Barcode { get; set; } =
            string.Empty;

        public int OrderedQuantity { get; set; }

        public int PreviouslyReceivedQuantity { get; set; }

        public int ReceivingQuantity { get; set; }

        public decimal UnitCost { get; set; }

        public string BatchNumber { get; set; } =
            string.Empty;

        public DateTime? ManufacturedDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int RemainingBeforeReceipt
        {
            get
            {
                return OrderedQuantity -
                       PreviouslyReceivedQuantity;
            }
        }

        public int RemainingAfterReceipt
        {
            get
            {
                return RemainingBeforeReceipt -
                       ReceivingQuantity;
            }
        }
    }
}