namespace SupermarketManagementSystem.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        public int? UserId { get; set; }

        public string EmployeeCode { get; set; } =
            string.Empty;

        public string FirstName { get; set; } =
            string.Empty;

        public string LastName { get; set; } =
            string.Empty;

        public string FullName =>
            $"{FirstName} {LastName}".Trim();

        public string Nic { get; set; } =
            string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; } =
            string.Empty;

        public string Address { get; set; } =
            string.Empty;

        public string Phone { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public string JobTitle { get; set; } =
            string.Empty;

        public DateTime HireDate { get; set; } =
            DateTime.Today;

        public decimal BasicSalary { get; set; }

        public string EmploymentStatus { get; set; } =
            "ACTIVE";

        public DateTime CreatedAt { get; set; }
    }
}