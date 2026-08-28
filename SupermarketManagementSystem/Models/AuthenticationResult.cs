namespace SupermarketManagementSystem.Models
{
    public class AuthenticationResult
    {
        public bool IsSuccessful { get; set; }

        public string Message { get; set; } = string.Empty;

        public User? User { get; set; }
    }
}