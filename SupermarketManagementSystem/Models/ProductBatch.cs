namespace SupermarketManagementSystem.Models
{
    public class ProductBatch
    {
        public int BatchId { get; set; }

        public int ProductId { get; set; }

        // Obtained by joining the products table.
        public string ProductName { get; set; } =
            string.Empty;

        public string Barcode { get; set; } =
            string.Empty;

        public string BatchNumber { get; set; } =
            string.Empty;

        public DateTime? ManufacturedDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int ReceivedQuantity { get; set; }

        public int AvailableQuantity { get; set; }

        public decimal CostPrice { get; set; }

        public string BatchStatus { get; set; } =
            "ACTIVE";

        public DateTime CreatedAt { get; set; }

        public bool IsExpired
        {
            get
            {
                return ExpiryDate.HasValue &&
                       ExpiryDate.Value.Date <
                       DateTime.Today;
            }
        }
    }
}