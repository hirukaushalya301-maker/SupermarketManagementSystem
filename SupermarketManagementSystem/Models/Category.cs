namespace SupermarketManagementSystem.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } =
            string.Empty;

        public string Description { get; set; } =
            string.Empty;

        public string CategoryStatus { get; set; } =
            "ACTIVE";

        public DateTime CreatedAt { get; set; }
    }
}