namespace SupermarketManagementSystem.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public string AccountStatus { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}