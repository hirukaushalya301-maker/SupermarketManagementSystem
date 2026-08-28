using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class LeaveRequestRepository
    {
        public List<LeaveRequest> GetAllLeaveRequests()
        {
            const string query = @"
                SELECT
                    l.leave_request_id,
                    l.employee_id,
                    e.employee_code,
                    CONCAT(
                        e.first_name,
                        ' ',
                        e.last_name
                    ) AS employee_name,
                    l.leave_type,
                    l.start_date,
                    l.end_date,
                    l.reason,
                    l.request_status,
                    l.reviewed_by,
                    l.reviewed_at,
                    l.review_note,
                    l.created_at
                FROM leave_requests l
                INNER JOIN employees e
                    ON l.employee_id = e.employee_id
                ORDER BY
                    l.created_at DESC;";

            List<LeaveRequest> requests =
                new List<LeaveRequest>();

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                requests.Add(MapLeaveRequest(reader));
            }

            return requests;
        }

        public int CreateLeaveRequest(
            LeaveRequest request)
        {
            const string query = @"
                INSERT INTO leave_requests (
                    employee_id,
                    leave_type,
                    start_date,
                    end_date,
                    reason,
                    request_status
                )
                VALUES (
                    @employeeId,
                    @leaveType,
                    @startDate,
                    @endDate,
                    @reason,
                    'PENDING'
                );

                SELECT LAST_INSERT_ID();";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddRequestParameters(command, request);

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            );
        }

        public bool UpdateLeaveRequest(
            LeaveRequest request)
        {
            const string query = @"
                UPDATE leave_requests
                SET
                    employee_id = @employeeId,
                    leave_type = @leaveType,
                    start_date = @startDate,
                    end_date = @endDate,
                    reason = @reason
                WHERE leave_request_id =
                    @leaveRequestId
                AND request_status = 'PENDING';";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddRequestParameters(command, request);

            command.Parameters.AddWithValue(
                "@leaveRequestId",
                request.LeaveRequestId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool ReviewLeaveRequest(
            int leaveRequestId,
            string requestStatus,
            int? reviewedBy,
            string reviewNote)
        {
            const string query = @"
                UPDATE leave_requests
                SET
                    request_status = @requestStatus,
                    reviewed_by = @reviewedBy,
                    reviewed_at = CURRENT_TIMESTAMP,
                    review_note = @reviewNote
                WHERE leave_request_id =
                    @leaveRequestId
                AND request_status = 'PENDING';";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@requestStatus",
                requestStatus
            );

            command.Parameters.AddWithValue(
                "@reviewedBy",
                reviewedBy.HasValue
                    ? reviewedBy.Value
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@reviewNote",
                string.IsNullOrWhiteSpace(reviewNote)
                    ? DBNull.Value
                    : reviewNote.Trim()
            );

            command.Parameters.AddWithValue(
                "@leaveRequestId",
                leaveRequestId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool CancelLeaveRequest(
            int leaveRequestId)
        {
            const string query = @"
                UPDATE leave_requests
                SET request_status = 'CANCELLED'
                WHERE leave_request_id =
                    @leaveRequestId
                AND request_status = 'PENDING';";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@leaveRequestId",
                leaveRequestId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        private static void AddRequestParameters(
            MySqlCommand command,
            LeaveRequest request)
        {
            command.Parameters.AddWithValue(
                "@employeeId",
                request.EmployeeId
            );

            command.Parameters.AddWithValue(
                "@leaveType",
                request.LeaveType
            );

            command.Parameters.AddWithValue(
                "@startDate",
                request.StartDate.Date
            );

            command.Parameters.AddWithValue(
                "@endDate",
                request.EndDate.Date
            );

            command.Parameters.AddWithValue(
                "@reason",
                string.IsNullOrWhiteSpace(request.Reason)
                    ? DBNull.Value
                    : request.Reason.Trim()
            );
        }

        private static LeaveRequest MapLeaveRequest(
            MySqlDataReader reader)
        {
            return new LeaveRequest
            {
                LeaveRequestId =
                    reader.GetInt32(
                        "leave_request_id"
                    ),

                EmployeeId =
                    reader.GetInt32("employee_id"),

                EmployeeCode =
                    reader.GetString("employee_code"),

                EmployeeName =
                    reader.GetString("employee_name"),

                LeaveType =
                    reader.GetString("leave_type"),

                StartDate =
                    reader.GetDateTime("start_date"),

                EndDate =
                    reader.GetDateTime("end_date"),

                Reason = GetNullableString(
                    reader,
                    "reason"
                ),

                RequestStatus =
                    reader.GetString(
                        "request_status"
                    ),

                ReviewedBy = GetNullableInt(
                    reader,
                    "reviewed_by"
                ),

                ReviewedAt = GetNullableDateTime(
                    reader,
                    "reviewed_at"
                ),

                ReviewNote = GetNullableString(
                    reader,
                    "review_note"
                ),

                CreatedAt =
                    reader.GetDateTime("created_at")
            };
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

        private static int? GetNullableInt(
            MySqlDataReader reader,
            string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            return reader.IsDBNull(index)
                ? null
                : reader.GetInt32(index);
        }

        private static DateTime? GetNullableDateTime(
            MySqlDataReader reader,
            string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            return reader.IsDBNull(index)
                ? null
                : reader.GetDateTime(index);
        }
    }
}