using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class AttendanceService
    {
        private readonly AttendanceRepository repository;

        public AttendanceService()
        {
            repository = new AttendanceRepository();
        }

        public List<Attendance> GetAllAttendance()
        {
            return repository.GetAllAttendance();
        }

        public OperationResult CreateAttendance(
            Attendance attendance)
        {
            PrepareAttendance(attendance);

            OperationResult validation =
                ValidateAttendance(attendance);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.AttendanceExists(
                attendance.EmployeeId,
                attendance.AttendanceDate))
            {
                return Failure(
                    "Attendance has already been recorded " +
                    "for this employee on this date."
                );
            }

            try
            {
                attendance.AttendanceId =
                    repository.CreateAttendance(attendance);

                return Success(
                    "Attendance recorded successfully."
                );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to record attendance: " +
                    ex.Message
                );
            }
        }

        public OperationResult UpdateAttendance(
            Attendance attendance)
        {
            if (attendance.AttendanceId <= 0)
            {
                return Failure(
                    "Please select an attendance record."
                );
            }

            PrepareAttendance(attendance);

            OperationResult validation =
                ValidateAttendance(attendance);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.AttendanceExists(
                attendance.EmployeeId,
                attendance.AttendanceDate,
                attendance.AttendanceId))
            {
                return Failure(
                    "Attendance has already been recorded " +
                    "for this employee on this date."
                );
            }

            try
            {
                bool updated =
                    repository.UpdateAttendance(attendance);

                return updated
                    ? Success(
                        "Attendance updated successfully."
                    )
                    : Failure(
                        "Attendance record was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update attendance: " +
                    ex.Message
                );
            }
        }

        public OperationResult DeleteAttendance(
            long attendanceId)
        {
            if (attendanceId <= 0)
            {
                return Failure(
                    "Please select an attendance record."
                );
            }

            try
            {
                bool deleted =
                    repository.DeleteAttendance(
                        attendanceId
                    );

                return deleted
                    ? Success(
                        "Attendance deleted successfully."
                    )
                    : Failure(
                        "Attendance record was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to delete attendance: " +
                    ex.Message
                );
            }
        }

        private static void PrepareAttendance(
            Attendance attendance)
        {
            attendance.Notes = attendance.Notes.Trim();

            // Absent and leave records should not have times.
            if (attendance.AttendanceStatus == "ABSENT" ||
                attendance.AttendanceStatus == "LEAVE")
            {
                attendance.ClockIn = null;
                attendance.ClockOut = null;
            }
        }

        private static OperationResult ValidateAttendance(
            Attendance attendance)
        {
            if (attendance.EmployeeId <= 0)
            {
                return Failure(
                    "Please select an employee."
                );
            }

            if (attendance.AttendanceDate.Date >
                DateTime.Today)
            {
                return Failure(
                    "Attendance date cannot be in the future."
                );
            }

            if (!IsValidStatus(
                attendance.AttendanceStatus))
            {
                return Failure(
                    "Invalid attendance status."
                );
            }

            if (attendance.ClockIn.HasValue &&
                attendance.ClockOut.HasValue &&
                attendance.ClockOut.Value <
                attendance.ClockIn.Value)
            {
                return Failure(
                    "Clock-out time cannot be earlier " +
                    "than clock-in time."
                );
            }

            if (attendance.Notes.Length > 255)
            {
                return Failure(
                    "Notes cannot exceed 255 characters."
                );
            }

            return Success("Valid");
        }

        private static bool IsValidStatus(string status)
        {
            return status == "PRESENT" ||
                   status == "ABSENT" ||
                   status == "LATE" ||
                   status == "LEAVE" ||
                   status == "HALF_DAY";
        }

        private static OperationResult Success(
            string message)
        {
            return new OperationResult
            {
                IsSuccessful = true,
                Message = message
            };
        }

        private static OperationResult Failure(
            string message)
        {
            return new OperationResult
            {
                IsSuccessful = false,
                Message = message
            };
        }
    }
}