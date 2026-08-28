using System.Net.Mail;
using System.Text.RegularExpressions;
using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class EmployeeService
    {
        private readonly EmployeeRepository repository;

        public EmployeeService()
        {
            repository = new EmployeeRepository();
        }

        public List<Employee> GetAllEmployees()
        {
            return repository.GetAllEmployees();
        }

        public Employee? GetEmployeeById(int employeeId)
        {
            return repository.GetEmployeeById(employeeId);
        }

        public OperationResult CreateEmployee(
            Employee employee)
        {
            PrepareEmployee(employee);

            OperationResult validation =
                ValidateEmployee(employee);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.EmployeeCodeExists(
                employee.EmployeeCode))
            {
                return Failure(
                    "This employee code already exists."
                );
            }

            try
            {
                employee.EmployeeId =
                    repository.CreateEmployee(employee);

                return Success(
                    "Employee added successfully."
                );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to add employee: " +
                    ex.Message
                );
            }
        }

        public OperationResult UpdateEmployee(
            Employee employee)
        {
            if (employee.EmployeeId <= 0)
            {
                return Failure(
                    "Please select an employee to update."
                );
            }

            PrepareEmployee(employee);

            OperationResult validation =
                ValidateEmployee(employee);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.EmployeeCodeExists(
                employee.EmployeeCode,
                employee.EmployeeId))
            {
                return Failure(
                    "This employee code already exists."
                );
            }

            try
            {
                bool updated =
                    repository.UpdateEmployee(employee);

                return updated
                    ? Success(
                        "Employee updated successfully."
                    )
                    : Failure(
                        "Employee record was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update employee: " +
                    ex.Message
                );
            }
        }

        public OperationResult UpdateEmployeeStatus(
            int employeeId,
            string employmentStatus)
        {
            if (employeeId <= 0)
            {
                return Failure(
                    "Please select an employee."
                );
            }

            if (!IsValidStatus(employmentStatus))
            {
                return Failure(
                    "Invalid employee status."
                );
            }

            try
            {
                bool updated =
                    repository.UpdateEmployeeStatus(
                        employeeId,
                        employmentStatus
                    );

                return updated
                    ? Success(
                        "Employee status updated successfully."
                    )
                    : Failure(
                        "Employee record was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update employee status: " +
                    ex.Message
                );
            }
        }

        private static void PrepareEmployee(
            Employee employee)
        {
            employee.EmployeeCode =
                employee.EmployeeCode.Trim();

            employee.FirstName =
                employee.FirstName.Trim();

            employee.LastName =
                employee.LastName.Trim();

            employee.Nic = employee.Nic.Trim();
            employee.Gender = employee.Gender.Trim();
            employee.Address = employee.Address.Trim();
            employee.Phone = employee.Phone.Trim();
            employee.Email = employee.Email.Trim();

            employee.JobTitle =
                employee.JobTitle.Trim();
        }

        private static OperationResult ValidateEmployee(
            Employee employee)
        {
            if (!Regex.IsMatch(
                employee.EmployeeCode,
                @"^[a-zA-Z0-9\-]{2,30}$"))
            {
                return Failure(
                    "Employee code must contain 2-30 " +
                    "letters, numbers or hyphens."
                );
            }

            if (string.IsNullOrWhiteSpace(
                employee.FirstName))
            {
                return Failure(
                    "First name is required."
                );
            }

            if (string.IsNullOrWhiteSpace(
                employee.LastName))
            {
                return Failure(
                    "Last name is required."
                );
            }

            if (employee.FirstName.Length > 60 ||
                employee.LastName.Length > 60)
            {
                return Failure(
                    "First and last names cannot exceed " +
                    "60 characters."
                );
            }

            if (string.IsNullOrWhiteSpace(
                employee.JobTitle))
            {
                return Failure(
                    "Job title is required."
                );
            }

            if (employee.JobTitle.Length > 80)
            {
                return Failure(
                    "Job title cannot exceed 80 characters."
                );
            }

            if (employee.DateOfBirth.HasValue &&
                employee.DateOfBirth.Value.Date >=
                DateTime.Today)
            {
                return Failure(
                    "Enter a valid date of birth."
                );
            }

            if (employee.HireDate.Date > DateTime.Today)
            {
                return Failure(
                    "Hire date cannot be in the future."
                );
            }

            if (employee.BasicSalary < 0)
            {
                return Failure(
                    "Basic salary cannot be negative."
                );
            }

            if (!string.IsNullOrWhiteSpace(employee.Phone) &&
                !Regex.IsMatch(
                    employee.Phone,
                    @"^[0-9+\-\s]{7,20}$"))
            {
                return Failure(
                    "Enter a valid phone number."
                );
            }

            if (!string.IsNullOrWhiteSpace(employee.Email) &&
                !IsValidEmail(employee.Email))
            {
                return Failure(
                    "Enter a valid email address."
                );
            }

            if (!IsValidStatus(
                employee.EmploymentStatus))
            {
                return Failure(
                    "Invalid employment status."
                );
            }

            return Success("Valid");
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                MailAddress address =
                    new MailAddress(email);

                return address.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidStatus(string status)
        {
            return status == "ACTIVE" ||
                   status == "INACTIVE" ||
                   status == "TERMINATED";
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