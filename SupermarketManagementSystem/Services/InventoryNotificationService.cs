using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class InventoryNotificationService
    {
        private readonly InventoryNotificationRepository
            repository;

        public InventoryNotificationService()
        {
            repository =
                new InventoryNotificationRepository();
        }

        public List<InventoryNotification>
            GetAllNotifications()
        {
            return repository.GetAllNotifications();
        }

        public OperationResult
            GenerateAutomaticNotifications()
        {
            try
            {
                int generated =
                    repository
                        .GenerateAutomaticNotifications();

                string message = generated > 0
                    ? generated +
                      " new inventory notification(s) generated."
                    : "No new inventory notifications were required.";

                return Success(message);
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to generate notifications: " +
                    ex.Message
                );
            }
        }

        public OperationResult MarkAsRead(
            long notificationId)
        {
            if (notificationId <= 0)
            {
                return Failure(
                    "Please select a notification."
                );
            }

            try
            {
                bool updated =
                    repository.MarkAsRead(
                        notificationId
                    );

                return updated
                    ? Success(
                        "Notification marked as read."
                    )
                    : Failure(
                        "Only an unread notification " +
                        "can be marked as read."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update notification: " +
                    ex.Message
                );
            }
        }

        public OperationResult Resolve(
            long notificationId)
        {
            if (notificationId <= 0)
            {
                return Failure(
                    "Please select a notification."
                );
            }

            try
            {
                bool resolved =
                    repository.Resolve(notificationId);

                return resolved
                    ? Success(
                        "Notification resolved successfully."
                    )
                    : Failure(
                        "The notification is already resolved " +
                        "or was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to resolve notification: " +
                    ex.Message
                );
            }
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