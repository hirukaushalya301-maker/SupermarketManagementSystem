namespace SupermarketManagementSystem.Models
{
    public class WorkRecord
    {
        public long WorkRecordId { get; set; }

        public int EmployeeId { get; set; }

        public string EmployeeCode { get; set; } =
            string.Empty;

        public string EmployeeName { get; set; } =
            string.Empty;

        public DateTime WorkDate { get; set; } =
            DateTime.Today;

        public string TaskTitle { get; set; } =
            string.Empty;

        public string Description { get; set; } =
            string.Empty;

        public string WorkStatus { get; set; } =
            "ASSIGNED";

        public int? AssignedBy { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}