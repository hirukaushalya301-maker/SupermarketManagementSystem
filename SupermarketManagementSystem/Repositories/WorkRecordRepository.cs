using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class WorkRecordRepository
    {
        public List<WorkRecord> GetAllWorkRecords()
        {
            const string query = @"
                SELECT
                    w.work_record_id,
                    w.employee_id,
                    e.employee_code,
                    CONCAT(
                        e.first_name,
                        ' ',
                        e.last_name
                    ) AS employee_name,
                    w.work_date,
                    w.task_title,
                    w.description,
                    w.work_status,
                    w.assigned_by,
                    w.completed_at,
                    w.created_at
                FROM work_records w
                INNER JOIN employees e
                    ON w.employee_id = e.employee_id
                ORDER BY
                    w.work_date DESC,
                    w.created_at DESC;";

            List<WorkRecord> records =
                new List<WorkRecord>();

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                records.Add(MapWorkRecord(reader));
            }

            return records;
        }

        public long CreateWorkRecord(WorkRecord record)
        {
            const string query = @"
                INSERT INTO work_records (
                    employee_id,
                    work_date,
                    task_title,
                    description,
                    work_status,
                    assigned_by
                )
                VALUES (
                    @employeeId,
                    @workDate,
                    @taskTitle,
                    @description,
                    @workStatus,
                    @assignedBy
                );

                SELECT LAST_INSERT_ID();";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, record);
            connection.Open();

            return Convert.ToInt64(
                command.ExecuteScalar()
            );
        }

        public bool UpdateWorkRecord(WorkRecord record)
        {
            const string query = @"
                UPDATE work_records
                SET
                    employee_id = @employeeId,
                    work_date = @workDate,
                    task_title = @taskTitle,
                    description = @description,
                    work_status = @workStatus,
                    completed_at =
                        CASE
                            WHEN @workStatus = 'COMPLETED'
                                THEN COALESCE(
                                    completed_at,
                                    CURRENT_TIMESTAMP
                                )
                            ELSE NULL
                        END
                WHERE work_record_id = @workRecordId;";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, record);

            command.Parameters.AddWithValue(
                "@workRecordId",
                record.WorkRecordId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool UpdateWorkStatus(
            long workRecordId,
            string workStatus)
        {
            const string query = @"
                UPDATE work_records
                SET
                    work_status = @workStatus,
                    completed_at =
                        CASE
                            WHEN @workStatus = 'COMPLETED'
                                THEN CURRENT_TIMESTAMP
                            ELSE NULL
                        END
                WHERE work_record_id = @workRecordId;";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@workStatus",
                workStatus
            );

            command.Parameters.AddWithValue(
                "@workRecordId",
                workRecordId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool DeleteWorkRecord(long workRecordId)
        {
            const string query = @"
                DELETE FROM work_records
                WHERE work_record_id = @workRecordId
                AND work_status <> 'COMPLETED';";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@workRecordId",
                workRecordId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        private static void AddParameters(
            MySqlCommand command,
            WorkRecord record)
        {
            command.Parameters.AddWithValue(
                "@employeeId",
                record.EmployeeId
            );

            command.Parameters.AddWithValue(
                "@workDate",
                record.WorkDate.Date
            );

            command.Parameters.AddWithValue(
                "@taskTitle",
                record.TaskTitle.Trim()
            );

            command.Parameters.AddWithValue(
                "@description",
                string.IsNullOrWhiteSpace(
                    record.Description)
                    ? DBNull.Value
                    : record.Description.Trim()
            );

            command.Parameters.AddWithValue(
                "@workStatus",
                record.WorkStatus
            );

            command.Parameters.AddWithValue(
                "@assignedBy",
                record.AssignedBy.HasValue
                    ? record.AssignedBy.Value
                    : DBNull.Value
            );
        }

        private static WorkRecord MapWorkRecord(
            MySqlDataReader reader)
        {
            return new WorkRecord
            {
                WorkRecordId =
                    reader.GetInt64("work_record_id"),

                EmployeeId =
                    reader.GetInt32("employee_id"),

                EmployeeCode =
                    reader.GetString("employee_code"),

                EmployeeName =
                    reader.GetString("employee_name"),

                WorkDate =
                    reader.GetDateTime("work_date"),

                TaskTitle =
                    reader.GetString("task_title"),

                Description = GetNullableString(
                    reader,
                    "description"
                ),

                WorkStatus =
                    reader.GetString("work_status"),

                AssignedBy = GetNullableInt(
                    reader,
                    "assigned_by"
                ),

                CompletedAt = GetNullableDateTime(
                    reader,
                    "completed_at"
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