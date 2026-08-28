using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class EmployeeRepository
    {
        public List<Employee> GetAllEmployees()
        {
            const string query = @"
                SELECT
                    employee_id,
                    user_id,
                    employee_code,
                    first_name,
                    last_name,
                    nic,
                    date_of_birth,
                    gender,
                    address,
                    phone,
                    email,
                    job_title,
                    hire_date,
                    basic_salary,
                    employment_status,
                    created_at
                FROM employees
                ORDER BY employee_id DESC;";

            List<Employee> employees =
                new List<Employee>();

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                employees.Add(MapEmployee(reader));
            }

            return employees;
        }

        public Employee? GetEmployeeById(int employeeId)
        {
            const string query = @"
                SELECT
                    employee_id,
                    user_id,
                    employee_code,
                    first_name,
                    last_name,
                    nic,
                    date_of_birth,
                    gender,
                    address,
                    phone,
                    email,
                    job_title,
                    hire_date,
                    basic_salary,
                    employment_status,
                    created_at
                FROM employees
                WHERE employee_id = @employeeId
                LIMIT 1;";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@employeeId",
                employeeId
            );

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            return reader.Read()
                ? MapEmployee(reader)
                : null;
        }

        public bool EmployeeCodeExists(
            string employeeCode,
            int? excludedEmployeeId = null)
        {
            const string query = @"
                SELECT COUNT(*)
                FROM employees
                WHERE employee_code = @employeeCode
                AND (
                    @excludedEmployeeId IS NULL
                    OR employee_id <> @excludedEmployeeId
                );";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@employeeCode",
                employeeCode
            );

            command.Parameters.AddWithValue(
                "@excludedEmployeeId",
                excludedEmployeeId.HasValue
                    ? excludedEmployeeId.Value
                    : DBNull.Value
            );

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            ) > 0;
        }

        public int CreateEmployee(Employee employee)
        {
            const string query = @"
                INSERT INTO employees (
                    user_id,
                    employee_code,
                    first_name,
                    last_name,
                    nic,
                    date_of_birth,
                    gender,
                    address,
                    phone,
                    email,
                    job_title,
                    hire_date,
                    basic_salary,
                    employment_status
                )
                VALUES (
                    @userId,
                    @employeeCode,
                    @firstName,
                    @lastName,
                    @nic,
                    @dateOfBirth,
                    @gender,
                    @address,
                    @phone,
                    @email,
                    @jobTitle,
                    @hireDate,
                    @basicSalary,
                    @employmentStatus
                );

                SELECT LAST_INSERT_ID();";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, employee);

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            );
        }

        public bool UpdateEmployee(Employee employee)
        {
            const string query = @"
                UPDATE employees
                SET
                    user_id = @userId,
                    employee_code = @employeeCode,
                    first_name = @firstName,
                    last_name = @lastName,
                    nic = @nic,
                    date_of_birth = @dateOfBirth,
                    gender = @gender,
                    address = @address,
                    phone = @phone,
                    email = @email,
                    job_title = @jobTitle,
                    hire_date = @hireDate,
                    basic_salary = @basicSalary,
                    employment_status =
                        @employmentStatus
                WHERE employee_id = @employeeId;";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, employee);

            command.Parameters.AddWithValue(
                "@employeeId",
                employee.EmployeeId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool UpdateEmployeeStatus(
            int employeeId,
            string employmentStatus)
        {
            const string query = @"
                UPDATE employees
                SET employment_status =
                    @employmentStatus
                WHERE employee_id = @employeeId;";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@employmentStatus",
                employmentStatus
            );

            command.Parameters.AddWithValue(
                "@employeeId",
                employeeId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        private static void AddParameters(
            MySqlCommand command,
            Employee employee)
        {
            command.Parameters.AddWithValue(
                "@userId",
                employee.UserId.HasValue
                    ? employee.UserId.Value
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@employeeCode",
                employee.EmployeeCode.Trim()
            );

            command.Parameters.AddWithValue(
                "@firstName",
                employee.FirstName.Trim()
            );

            command.Parameters.AddWithValue(
                "@lastName",
                employee.LastName.Trim()
            );

            command.Parameters.AddWithValue(
                "@nic",
                NullIfEmpty(employee.Nic)
            );

            command.Parameters.AddWithValue(
                "@dateOfBirth",
                employee.DateOfBirth.HasValue
                    ? employee.DateOfBirth.Value.Date
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@gender",
                NullIfEmpty(employee.Gender)
            );

            command.Parameters.AddWithValue(
                "@address",
                NullIfEmpty(employee.Address)
            );

            command.Parameters.AddWithValue(
                "@phone",
                NullIfEmpty(employee.Phone)
            );

            command.Parameters.AddWithValue(
                "@email",
                NullIfEmpty(employee.Email)
            );

            command.Parameters.AddWithValue(
                "@jobTitle",
                employee.JobTitle.Trim()
            );

            command.Parameters.AddWithValue(
                "@hireDate",
                employee.HireDate.Date
            );

            command.Parameters.AddWithValue(
                "@basicSalary",
                employee.BasicSalary
            );

            command.Parameters.AddWithValue(
                "@employmentStatus",
                employee.EmploymentStatus
            );
        }

        private static Employee MapEmployee(
            MySqlDataReader reader)
        {
            return new Employee
            {
                EmployeeId =
                    reader.GetInt32("employee_id"),

                UserId = GetNullableInt(
                    reader,
                    "user_id"
                ),

                EmployeeCode =
                    reader.GetString("employee_code"),

                FirstName =
                    reader.GetString("first_name"),

                LastName =
                    reader.GetString("last_name"),

                Nic = GetNullableString(reader, "nic"),

                DateOfBirth = GetNullableDate(
                    reader,
                    "date_of_birth"
                ),

                Gender = GetNullableString(
                    reader,
                    "gender"
                ),

                Address = GetNullableString(
                    reader,
                    "address"
                ),

                Phone = GetNullableString(
                    reader,
                    "phone"
                ),

                Email = GetNullableString(
                    reader,
                    "email"
                ),

                JobTitle =
                    reader.GetString("job_title"),

                HireDate =
                    reader.GetDateTime("hire_date"),

                BasicSalary =
                    reader.GetDecimal("basic_salary"),

                EmploymentStatus =
                    reader.GetString(
                        "employment_status"
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

        private static DateTime? GetNullableDate(
            MySqlDataReader reader,
            string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            return reader.IsDBNull(index)
                ? null
                : reader.GetDateTime(index);
        }

        private static object NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? DBNull.Value
                : value.Trim();
        }
    }
}