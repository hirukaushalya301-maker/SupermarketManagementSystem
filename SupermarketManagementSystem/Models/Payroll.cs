namespace SupermarketManagementSystem.Models
{
    public class Payroll
    {
        public int PayrollId { get; set; }

        public int EmployeeId { get; set; }

        public string EmployeeCode { get; set; } =
            string.Empty;

        public string EmployeeName { get; set; } =
            string.Empty;

        public int PayYear { get; set; } =
            DateTime.Today.Year;

        public int PayMonth { get; set; } =
            DateTime.Today.Month;

        public string PayPeriod =>
            new DateTime(PayYear, PayMonth, 1)
                .ToString("MMMM yyyy");

        public decimal BasicSalary { get; set; }

        public decimal Allowances { get; set; }

        public decimal Deductions { get; set; }

        public decimal NetSalary =>
            BasicSalary + Allowances - Deductions;

        public string PaymentStatus { get; set; } =
            "PENDING";

        public DateTime? PaidAt { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}