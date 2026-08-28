using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class LeaveRequestService
    {
        private readonly LeaveRequestRepository repository;

        public LeaveRequestService()
        {
            repository = new LeaveRequestRepository();
        }

        public List<LeaveRequest> GetAllLeaveRequests()
        {
            return repository.GetAllLeaveRequests();
        }

        public OperationResult CreateLeaveRequest(
            LeaveRequest request)
        {
            PrepareRequest(request);

            OperationResult validation =
                ValidateRequest(request);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            try
            {
                request.LeaveRequestId =
                    repository.CreateLeaveRequest(request);

                return Success(
                    "Leave request created successfully."
                );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to create leave request: " +
                    ex.Message
                );
            }
        }

        public OperationResult UpdateLeaveRequest(
            LeaveRequest request)
        {
            if (request.LeaveRequestId <= 0)
            {
                return Failure(
                    "Please select a leave request."
                );
            }

            if (request.RequestStatus != "PENDING")
            {
                return Failure(
                    "Only pending leave requests can be edited."
                );
            }

            PrepareRequest(request);

            OperationResult validation =
                ValidateRequest(request);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            try
            {
                bool updated =
                    repository.UpdateLeaveRequest(request);

                return updated
                    ? Success(
                        "Leave request updated successfully."
                    )
                    : Failure(
                        "Only pending requests can be updated."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update leave request: " +
                    ex.Message
                );
            }
        }

        public OperationResult ReviewLeaveRequest(
            int leaveRequestId,
            string requestStatus,
            int? reviewedBy,
            string reviewNote)
        {
            if (leaveRequestId <= 0)
            {
                return Failure(
                    "Please select a leave request."
                );
            }

            if (requestStatus != "APPROVED" &&
                requestStatus != "REJECTED")
            {
                return Failure(
                    "Review status must be APPROVED " +
                    "or REJECTED."
                );
            }

            reviewNote = reviewNote.Trim();

            if (reviewNote.Length > 500)
            {
                return Failure(
                    "Review note cannot exceed " +
                    "500 characters."
                );
            }

            try
            {
                bool reviewed =
                    repository.ReviewLeaveRequest(
                        leaveRequestId,
                        requestStatus,
                        reviewedBy,
                        reviewNote
                    );

                return reviewed
                    ? Success(
                        "Leave request " +
                        requestStatus.ToLower() +
                        " successfully."
                    )
                    : Failure(
                        "Only pending requests can be reviewed."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to review leave request: " +
                    ex.Message
                );
            }
        }

        public OperationResult CancelLeaveRequest(
            int leaveRequestId)
        {
            if (leaveRequestId <= 0)
            {
                return Failure(
                    "Please select a leave request."
                );
            }

            try
            {
                bool cancelled =
                    repository.CancelLeaveRequest(
                        leaveRequestId
                    );

                return cancelled
                    ? Success(
                        "Leave request cancelled successfully."
                    )
                    : Failure(
                        "Only pending requests can be cancelled."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to cancel leave request: " +
                    ex.Message
                );
            }
        }

        private static void PrepareRequest(
            LeaveRequest request)
        {
            request.LeaveType =
                request.LeaveType.Trim();

            request.Reason =
                request.Reason.Trim();
        }

        private static OperationResult ValidateRequest(
            LeaveRequest request)
        {
            if (request.EmployeeId <= 0)
            {
                return Failure(
                    "Please select an employee."
                );
            }

            if (!IsValidLeaveType(request.LeaveType))
            {
                return Failure(
                    "Invalid leave type."
                );
            }

            if (request.StartDate.Date < DateTime.Today)
            {
                return Failure(
                    "Leave start date cannot be in the past."
                );
            }

            if (request.EndDate.Date <
                request.StartDate.Date)
            {
                return Failure(
                    "Leave end date cannot be earlier " +
                    "than the start date."
                );
            }

            if (request.Reason.Length > 500)
            {
                return Failure(
                    "Reason cannot exceed 500 characters."
                );
            }

            return Success("Valid");
        }

        private static bool IsValidLeaveType(
            string leaveType)
        {
            return leaveType == "ANNUAL" ||
                   leaveType == "SICK" ||
                   leaveType == "CASUAL" ||
                   leaveType == "UNPAID" ||
                   leaveType == "OTHER";
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