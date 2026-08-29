namespace SupermarketManagementSystem.Models
{
    public class StockItem
    {
        public int StockId { get; set; }

        public int ProductId { get; set; }

        public string Barcode { get; set; } =
            string.Empty;

        public string ProductName { get; set; } =
            string.Empty;

        public string CategoryName { get; set; } =
            string.Empty;

        public int QuantityOnHand { get; set; }

        public int ReservedQuantity { get; set; }

        public int MinimumStock { get; set; }

        public decimal SellingPrice { get; set; }

        public DateTime? LastUpdated { get; set; }

        public int AvailableQuantity
        {
            get
            {
                return QuantityOnHand -
                       ReservedQuantity;
            }
        }

        public string StockStatus
        {
            get
            {
                if (QuantityOnHand == 0)
                {
                    return "OUT_OF_STOCK";
                }

                if (QuantityOnHand <= MinimumStock)
                {
                    return "LOW_STOCK";
                }

                return "AVAILABLE";
            }
        }
    }
}