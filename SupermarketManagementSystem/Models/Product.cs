namespace SupermarketManagementSystem.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public int? PrimarySupplierId { get; set; }

        public string SupplierName { get; set; } = string.Empty;

        public string Barcode { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string UnitOfMeasure { get; set; } = "Unit";

        public decimal CostPrice { get; set; }

        public decimal SellingPrice { get; set; }

        public decimal TaxRate { get; set; }

        public int MinimumStock { get; set; }

        public string ProductStatus { get; set; } = "ACTIVE";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}