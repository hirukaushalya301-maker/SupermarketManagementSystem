using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class PayrollService
    {
        private readonly PayrollRepository repository;

        public PayrollService()
        {
            repository = new PayrollRepository();
        }

        public List<Payroll> GetAllPayrolls()
        {
            return repository.GetAllPayrolls();
        }

        public OperationResult CreatePayroll(
            Payroll payroll)
        {
            OperationResult validation =
                ValidatePayroll(payroll);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.PayrollExists(
                payroll.EmployeeId,
                payroll.PayYear,
                payroll.PayMonth))
            {
                return Failure(
                    "A payroll record already exists for " +
                    "this employee and pay period."
                );
            }

            try
            {
                payroll.PaymentStatus = "PENDING";

                payroll.PayrollId =
                    repository.CreatePayroll(payroll);

                return Success(
                    "Payroll record created successfully."
                );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to create payroll: " +
                    ex.Message
                );
            }
        }

        public OperationResult UpdatePayroll(
            Payroll payroll)
        {
            if (payroll.PayrollId <= 0)
            {
                return Failure(
                    "Please select a payroll record."
                );
            }

            if (payroll.PaymentStatus == "PAID")
            {
                return Failure(
                    "Paid payroll records cannot be edited."
                );
            }

            OperationResult validation =
                ValidatePayroll(payroll);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.PayrollExists(
                payroll.EmployeeId,
                payroll.PayYear,
                payroll.PayMonth,
                payroll.PayrollId))
            {
                return Failure(
                    "A payroll record already exists for " +
                    "this employee and pay period."
                );
            }

            try
            {
                bool updated =
                    repository.UpdatePayroll(payroll);

                return updated
                    ? Success(
                        "Payroll updated successfully."
                    )
                    : Failure(
                        "Payroll record was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update payroll: " +
                    ex.Message
                );
            }
        }

        public OperationResult MarkAsPaid(int payrollId)
        {
            if (payrollId <= 0)
            {
                return Failure(
                    "Please select a payroll record."
                );
            }

            try
            {
                bool updated =
                    repository.MarkPayrollAsPaid(
                        payrollId
                    );

                return updated
                    ? Success(
                        "Payroll marked as paid successfully."
                    )
                    : Failure(
                        "Only pending payroll can be paid."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update payroll payment: " +
                    ex.Message
                );
            }
        }

        public OperationResult DeletePayroll(int payrollId)
        {
            if (payrollId <= 0)
            {
                return Failure(
                    "Please select a payroll record."
                );
            }

            try
            {
                bool deleted =
                    repository.DeletePayroll(payrollId);

                return deleted
                    ? Success(
                        "Payroll deleted successfully."
                    )
                    : Failure(
                        "Paid payroll cannot be deleted."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to delete payroll: " +
                    ex.Message
                );
            }
        }

        private static OperationResult ValidatePayroll(
            Payroll payroll)
        {
            if (payroll.EmployeeId <= 0)
            {
                return Failure(
                    "Please select an employee."
                );
            }

            if (payroll.PayYear < 2020 ||
                payroll.PayYear >
                DateTime.Today.Year + 1)
            {
                return Failure(
                    "Enter a valid payroll year."
                );
            }

            if (payroll.PayMonth < 1 ||
                payroll.PayMonth > 12)
            {
                return Failure(
                    "Select a valid payroll month."
                );
            }

            if (payroll.BasicSalary < 0)
            {
                return Failure(
                    "Basic salary cannot be negative."
                );
            }

            if (payroll.Allowances < 0)
            {
                return Failure(
                    "Allowances cannot be negative."
                );
            }

            if (payroll.Deductions < 0)
            {
                return Failure(
                    "Deductions cannot be negative."
                );
            }

            if (payroll.NetSalary < 0)
            {
                return Failure(
                    "Deductions cannot exceed salary " +
                    "and allowances."
                );
            }

            return Success("Valid");
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