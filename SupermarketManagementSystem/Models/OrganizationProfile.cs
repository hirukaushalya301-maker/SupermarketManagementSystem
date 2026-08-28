namespace SupermarketManagementSystem.Models
{
    public class OrganizationProfile
    {
        public int OrganizationId { get; set; }

        public string OrganizationName { get; set; } =
            string.Empty;

        public string Address { get; set; } =
            string.Empty;

        public string Phone { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public string OpeningHours { get; set; } =
            string.Empty;

        public string TaxNumber { get; set; } =
            string.Empty;

        public string LogoPath { get; set; } =
            string.Empty;

        public DateTime UpdatedAt { get; set; }
    }
}