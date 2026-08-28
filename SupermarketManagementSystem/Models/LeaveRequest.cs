namespace SupermarketManagementSystem.Models
{
    public class LeaveRequest
    {
        public int LeaveRequestId { get; set; }

        public int EmployeeId { get; set; }

        public string EmployeeCode { get; set; } =
            string.Empty;

        public string EmployeeName { get; set; } =
            string.Empty;

        public string LeaveType { get; set; } =
            "ANNUAL";

        public DateTime StartDate { get; set; } =
            DateTime.Today;

        public DateTime EndDate { get; set; } =
            DateTime.Today;

        public int NumberOfDays =>
            (EndDate.Date - StartDate.Date).Days + 1;

        public string Reason { get; set; } =
            string.Empty;

        public string RequestStatus { get; set; } =
            "PENDING";

        public int? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string ReviewNote { get; set; } =
            string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}