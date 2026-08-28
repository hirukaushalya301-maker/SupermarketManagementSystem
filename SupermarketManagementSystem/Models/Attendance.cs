namespace SupermarketManagementSystem.Models
{
    public class Attendance
    {
        public long AttendanceId { get; set; }

        public int EmployeeId { get; set; }

        public string EmployeeCode { get; set; } =
            string.Empty;

        public string EmployeeName { get; set; } =
            string.Empty;

        public DateTime AttendanceDate { get; set; } =
            DateTime.Today;

        public TimeSpan? ClockIn { get; set; }

        public TimeSpan? ClockOut { get; set; }

        public string AttendanceStatus { get; set; } =
            "PRESENT";

        public string Notes { get; set; } =
            string.Empty;

        public int? RecordedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}