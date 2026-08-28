using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class WorkRecordService
    {
        private readonly WorkRecordRepository repository;

        public WorkRecordService()
        {
            repository = new WorkRecordRepository();
        }

        public List<WorkRecord> GetAllWorkRecords()
        {
            return repository.GetAllWorkRecords();
        }

        public OperationResult CreateWorkRecord(
            WorkRecord record)
        {
            PrepareRecord(record);

            OperationResult validation =
                ValidateRecord(record);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            try
            {
                record.WorkRecordId =
                    repository.CreateWorkRecord(record);

                return Success(
                    "Work record created successfully."
                );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to create work record: " +
                    ex.Message
                );
            }
        }

        public OperationResult UpdateWorkRecord(
            WorkRecord record)
        {
            if (record.WorkRecordId <= 0)
            {
                return Failure(
                    "Please select a work record."
                );
            }

            PrepareRecord(record);

            OperationResult validation =
                ValidateRecord(record);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            try
            {
                bool updated =
                    repository.UpdateWorkRecord(record);

                return updated
                    ? Success(
                        "Work record updated successfully."
                    )
                    : Failure(
                        "Work record was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update work record: " +
                    ex.Message
                );
            }
        }

        public OperationResult UpdateWorkStatus(
            long workRecordId,
            string workStatus)
        {
            if (workRecordId <= 0)
            {
                return Failure(
                    "Please select a work record."
                );
            }

            if (!IsValidStatus(workStatus))
            {
                return Failure(
                    "Invalid work status."
                );
            }

            try
            {
                bool updated =
                    repository.UpdateWorkStatus(
                        workRecordId,
                        workStatus
                    );

                return updated
                    ? Success(
                        "Work status updated successfully."
                    )
                    : Failure(
                        "Work record was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update work status: " +
                    ex.Message
                );
            }
        }

        public OperationResult DeleteWorkRecord(
            long workRecordId)
        {
            if (workRecordId <= 0)
            {
                return Failure(
                    "Please select a work record."
                );
            }

            try
            {
                bool deleted =
                    repository.DeleteWorkRecord(
                        workRecordId
                    );

                return deleted
                    ? Success(
                        "Work record deleted successfully."
                    )
                    : Failure(
                        "Completed work records cannot be deleted."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to delete work record: " +
                    ex.Message
                );
            }
        }

        private static void PrepareRecord(
            WorkRecord record)
        {
            record.TaskTitle =
                record.TaskTitle.Trim();

            record.Description =
                record.Description.Trim();
        }

        private static OperationResult ValidateRecord(
            WorkRecord record)
        {
            if (record.EmployeeId <= 0)
            {
                return Failure(
                    "Please select an employee."
                );
            }

            if (string.IsNullOrWhiteSpace(
                record.TaskTitle))
            {
                return Failure(
                    "Task title is required."
                );
            }

            if (record.TaskTitle.Length > 150)
            {
                return Failure(
                    "Task title cannot exceed 150 characters."
                );
            }

            if (record.Description.Length > 500)
            {
                return Failure(
                    "Description cannot exceed 500 characters."
                );
            }

            if (!IsValidStatus(record.WorkStatus))
            {
                return Failure(
                    "Invalid work status."
                );
            }

            return Success("Valid");
        }

        private static bool IsValidStatus(string status)
        {
            return status == "ASSIGNED" ||
                   status == "IN_PROGRESS" ||
                   status == "COMPLETED" ||
                   status == "CANCELLED";
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