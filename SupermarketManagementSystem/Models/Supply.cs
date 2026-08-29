namespace SupermarketManagementSystem.Models
{
    public class Supply
    {
        public int SupplyId { get; set; }

        public int SupplierId { get; set; }

        public string SupplierName { get; set; } =
            string.Empty;

        public int ProductId { get; set; }

        public string ProductName { get; set; } =
            string.Empty;

        public string Barcode { get; set; } =
            string.Empty;

        public string SupplierProductCode { get; set; } =
            string.Empty;

        public decimal SupplierPrice { get; set; }

        public int LeadTimeDays { get; set; }

        public string SupplyStatus { get; set; } =
            "ACTIVE";
    }
}