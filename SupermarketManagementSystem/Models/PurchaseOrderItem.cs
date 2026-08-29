namespace SupermarketManagementSystem.Models
{
    public class PurchaseOrderItem
    {
        public int PurchaseOrderItemId { get; set; }

        public int PurchaseOrderId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } =
            string.Empty;

        public string Barcode { get; set; } =
            string.Empty;

        public int OrderedQuantity { get; set; }

        public int ReceivedQuantity { get; set; }

        public decimal UnitCost { get; set; }

        public decimal LineTotal
        {
            get
            {
                return OrderedQuantity * UnitCost;
            }
        }

        public int RemainingQuantity
        {
            get
            {
                return OrderedQuantity -
                       ReceivedQuantity;
            }
        }
    }
}