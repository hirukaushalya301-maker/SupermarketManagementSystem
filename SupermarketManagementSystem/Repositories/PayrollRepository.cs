using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class PayrollRepository
    {
        public List<Payroll> GetAllPayrolls()
        {
            const string query = @"
                SELECT
                    p.payroll_id,
                    p.employee_id,
                    e.employee_code,
                    CONCAT(
                        e.first_name,
                        ' ',
                        e.last_name
                    ) AS employee_name,
                    p.pay_year,
                    p.pay_month,
                    p.basic_salary,
                    p.allowances,
                    p.deductions,
                    p.net_salary,
                    p.payment_status,
                    p.paid_at,
                    p.created_by,
                    p.created_at
                FROM payroll p
                INNER JOIN employees e
                    ON p.employee_id = e.employee_id
                ORDER BY
                    p.pay_year DESC,
                    p.pay_month DESC,
                    e.first_name;";

            List<Payroll> payrolls =
                new List<Payroll>();

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                payrolls.Add(MapPayroll(reader));
            }

            return payrolls;
        }

        public bool PayrollExists(
            int employeeId,
            int payYear,
            int payMonth,
            int? excludedPayrollId = null)
        {
            const string query = @"
                SELECT COUNT(*)
                FROM payroll
                WHERE employee_id = @employeeId
                AND pay_year = @payYear
                AND pay_month = @payMonth
                AND (
                    @excludedPayrollId IS NULL
                    OR payroll_id <> @excludedPayrollId
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
                "@payYear",
                payYear
            );

            command.Parameters.AddWithValue(
                "@payMonth",
                payMonth
            );

            command.Parameters.AddWithValue(
                "@excludedPayrollId",
                excludedPayrollId.HasValue
                    ? excludedPayrollId.Value
                    : DBNull.Value
            );

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            ) > 0;
        }

        public int CreatePayroll(Payroll payroll)
        {
            const string query = @"
                INSERT INTO payroll (
                    employee_id,
                    pay_year,
                    pay_month,
                    basic_salary,
                    allowances,
                    deductions,
                    net_salary,
                    payment_status,
                    created_by
                )
                VALUES (
                    @employeeId,
                    @payYear,
                    @payMonth,
                    @basicSalary,
                    @allowances,
                    @deductions,
                    @netSalary,
                    @paymentStatus,
                    @createdBy
                );

                SELECT LAST_INSERT_ID();";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, payroll);

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            );
        }

        public bool UpdatePayroll(Payroll payroll)
        {
            const string query = @"
                UPDATE payroll
                SET
                    employee_id = @employeeId,
                    pay_year = @payYear,
                    pay_month = @payMonth,
                    basic_salary = @basicSalary,
                    allowances = @allowances,
                    deductions = @deductions,
                    net_salary = @netSalary,
                    payment_status = @paymentStatus
                WHERE payroll_id = @payrollId;";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, payroll);

            command.Parameters.AddWithValue(
                "@payrollId",
                payroll.PayrollId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool MarkPayrollAsPaid(int payrollId)
        {
            const string query = @"
                UPDATE payroll
                SET
                    payment_status = 'PAID',
                    paid_at = CURRENT_TIMESTAMP
                WHERE payroll_id = @payrollId
                AND payment_status = 'PENDING';";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@payrollId",
                payrollId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool DeletePayroll(int payrollId)
        {
            const string query = @"
                DELETE FROM payroll
                WHERE payroll_id = @payrollId
                AND payment_status <> 'PAID';";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@payrollId",
                payrollId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        private static void AddParameters(
            MySqlCommand command,
            Payroll payroll)
        {
            command.Parameters.AddWithValue(
                "@employeeId",
                payroll.EmployeeId
            );

            command.Parameters.AddWithValue(
                "@payYear",
                payroll.PayYear
            );

            command.Parameters.AddWithValue(
                "@payMonth",
                payroll.PayMonth
            );

            command.Parameters.AddWithValue(
                "@basicSalary",
                payroll.BasicSalary
            );

            command.Parameters.AddWithValue(
                "@allowances",
                payroll.Allowances
            );

            command.Parameters.AddWithValue(
                "@deductions",
                payroll.Deductions
            );

            command.Parameters.AddWithValue(
                "@netSalary",
                payroll.NetSalary
            );

            command.Parameters.AddWithValue(
                "@paymentStatus",
                payroll.PaymentStatus
            );

            command.Parameters.AddWithValue(
                "@createdBy",
                payroll.CreatedBy.HasValue
                    ? payroll.CreatedBy.Value
                    : DBNull.Value
            );
        }

        private static Payroll MapPayroll(
            MySqlDataReader reader)
        {
            return new Payroll
            {
                PayrollId =
                    reader.GetInt32("payroll_id"),

                EmployeeId =
                    reader.GetInt32("employee_id"),

                EmployeeCode =
                    reader.GetString("employee_code"),

                EmployeeName =
                    reader.GetString("employee_name"),

                PayYear =
                    reader.GetInt32("pay_year"),

                PayMonth =
                    reader.GetInt32("pay_month"),

                BasicSalary =
                    reader.GetDecimal("basic_salary"),

                Allowances =
                    reader.GetDecimal("allowances"),

                Deductions =
                    reader.GetDecimal("deductions"),

                PaymentStatus =
                    reader.GetString("payment_status"),

                PaidAt = GetNullableDateTime(
                    reader,
                    "paid_at"
                ),

                CreatedBy = GetNullableInt(
                    reader,
                    "created_by"
                ),

                CreatedAt =
                    reader.GetDateTime("created_at")
            };
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