using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class AttendanceRepository
    {
        public List<Attendance> GetAllAttendance()
        {
            const string query = @"
                SELECT
                    a.attendance_id,
                    a.employee_id,
                    e.employee_code,
                    CONCAT(
                        e.first_name,
                        ' ',
                        e.last_name
                    ) AS employee_name,
                    a.attendance_date,
                    a.clock_in,
                    a.clock_out,
                    a.attendance_status,
                    a.notes,
                    a.recorded_by,
                    a.created_at
                FROM attendance a
                INNER JOIN employees e
                    ON a.employee_id = e.employee_id
                ORDER BY
                    a.attendance_date DESC,
                    e.first_name;";

            List<Attendance> records =
                new List<Attendance>();

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                records.Add(MapAttendance(reader));
            }

            return records;
        }

        public bool AttendanceExists(
            int employeeId,
            DateTime attendanceDate,
            long? excludedAttendanceId = null)
        {
            const string query = @"
                SELECT COUNT(*)
                FROM attendance
                WHERE employee_id = @employeeId
                AND attendance_date = @attendanceDate
                AND (
                    @excludedAttendanceId IS NULL
                    OR attendance_id <>
                        @excludedAttendanceId
                );";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@employeeId",
                employeeId
            );

            command.Parameters.AddWithValue(
                "@attendanceDate",
                attendanceDate.Date
            );

            command.Parameters.AddWithValue(
                "@excludedAttendanceId",
                excludedAttendanceId.HasValue
                    ? excludedAttendanceId.Value
                    : DBNull.Value
            );

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            ) > 0;
        }

        public long CreateAttendance(
            Attendance attendance)
        {
            const string query = @"
                INSERT INTO attendance (
                    employee_id,
                    attendance_date,
                    clock_in,
                    clock_out,
                    attendance_status,
                    notes,
                    recorded_by
                )
                VALUES (
                    @employeeId,
                    @attendanceDate,
                    @clockIn,
                    @clockOut,
                    @attendanceStatus,
                    @notes,
                    @recordedBy
                );

                SELECT LAST_INSERT_ID();";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, attendance);

            connection.Open();

            return Convert.ToInt64(
                command.ExecuteScalar()
            );
        }

        public bool UpdateAttendance(
            Attendance attendance)
        {
            const string query = @"
                UPDATE attendance
                SET
                    employee_id = @employeeId,
                    attendance_date = @attendanceDate,
                    clock_in = @clockIn,
                    clock_out = @clockOut,
                    attendance_status =
                        @attendanceStatus,
                    notes = @notes
                WHERE attendance_id =
                    @attendanceId;";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, attendance);

            command.Parameters.AddWithValue(
                "@attendanceId",
                attendance.AttendanceId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool DeleteAttendance(long attendanceId)
        {
            const string query = @"
                DELETE FROM attendance
                WHERE attendance_id = @attendanceId;";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@attendanceId",
                attendanceId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        private static void AddParameters(
            MySqlCommand command,
            Attendance attendance)
        {
            command.Parameters.AddWithValue(
                "@employeeId",
                attendance.EmployeeId
            );

            command.Parameters.AddWithValue(
                "@attendanceDate",
                attendance.AttendanceDate.Date
            );

            command.Parameters.AddWithValue(
                "@clockIn",
                attendance.ClockIn.HasValue
                    ? attendance.ClockIn.Value
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@clockOut",
                attendance.ClockOut.HasValue
                    ? attendance.ClockOut.Value
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@attendanceStatus",
                attendance.AttendanceStatus
            );

            command.Parameters.AddWithValue(
                "@notes",
                string.IsNullOrWhiteSpace(attendance.Notes)
                    ? DBNull.Value
                    : attendance.Notes.Trim()
            );

            command.Parameters.AddWithValue(
                "@recordedBy",
                attendance.RecordedBy.HasValue
                    ? attendance.RecordedBy.Value
                    : DBNull.Value
            );
        }

        private static Attendance MapAttendance(
            MySqlDataReader reader)
        {
            return new Attendance
            {
                AttendanceId =
                    reader.GetInt64("attendance_id"),

                EmployeeId =
                    reader.GetInt32("employee_id"),

                EmployeeCode =
                    reader.GetString("employee_code"),

                EmployeeName =
                    reader.GetString("employee_name"),

                AttendanceDate =
                    reader.GetDateTime("attendance_date"),

                ClockIn = GetNullableTime(
                    reader,
                    "clock_in"
                ),

                ClockOut = GetNullableTime(
                    reader,
                    "clock_out"
                ),

                AttendanceStatus =
                    reader.GetString(
                        "attendance_status"
                    ),

                Notes = GetNullableString(
                    reader,
                    "notes"
                ),

                RecordedBy = GetNullableInt(
                    reader,
                    "recorded_by"
                ),

                CreatedAt =
                    reader.GetDateTime("created_at")
            };
        }

        private static TimeSpan? GetNullableTime(
            MySqlDataReader reader,
            string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            return reader.IsDBNull(index)
                ? null
                : reader.GetTimeSpan(index);
        }

        private static int? GetNullableInt(
            MySqlDataReader reader,
            string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            return reader.IsDBNull(index)
                ? null
                : reader.GetInt32(index);
        }

        private static string GetNullableString(
            MySqlDataReader reader,
            string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            return reader.IsDBNull(index)
                ? string.Empty
                : reader.GetString(index);
        }
    }
}