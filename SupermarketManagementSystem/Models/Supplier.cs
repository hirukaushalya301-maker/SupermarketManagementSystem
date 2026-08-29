namespace SupermarketManagementSystem.Models
{
    public class Supplier
    {
        public int SupplierId { get; set; }

        public int? UserId { get; set; }

        public string SupplierCode { get; set; } =
            string.Empty;

        public string SupplierName { get; set; } =
            string.Empty;

        public string ContactPerson { get; set; } =
            string.Empty;

        public string Phone { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public string Address { get; set; } =
            string.Empty;

        public string SupplierStatus { get; set; } =
            "ACTIVE";

        public DateTime CreatedAt { get; set; }
    }
}