using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class DeliveryService
    {
        private readonly DeliveryRepository repository;

        public DeliveryService()
        {
            repository = new DeliveryRepository();
        }

        public List<Delivery> GetAllDeliveries()
        {
            return repository.GetAllDeliveries();
        }

        public string GenerateDeliveryReference()
        {
            return "DEL-" +
                   DateTime.Now.ToString(
                       "yyyyMMdd-HHmmss"
                   );
        }

        public OperationResult CreateDelivery(
            Delivery delivery)
        {
            PrepareDelivery(delivery);

            OperationResult validation =
                ValidateDelivery(delivery);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.DeliveryReferenceExists(
                delivery.DeliveryReference))
            {
                return Failure(
                    "This delivery reference already exists."
                );
            }

            try
            {
                delivery.DeliveryId =
                    repository.CreateDelivery(delivery);

                return delivery.DeliveryId > 0
                    ? Success(
                        "Delivery created successfully."
                    )
                    : Failure(
                        "The delivery could not be created."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to create delivery: " +
                    ex.Message
                );
            }
        }

        public OperationResult UpdateDelivery(
            Delivery delivery)
        {
            if (delivery.DeliveryId <= 0)
            {
                return Failure(
                    "Please select a delivery."
                );
            }

            PrepareDelivery(delivery);

            OperationResult validation =
                ValidateDelivery(delivery);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.DeliveryReferenceExists(
                delivery.DeliveryReference,
                delivery.DeliveryId))
            {
                return Failure(
                    "This delivery reference already exists."
                );
            }

            try
            {
                bool updated =
                    repository.UpdateDelivery(delivery);

                return updated
                    ? Success(
                        "Delivery updated successfully."
                    )
                    : Failure(
                        "The delivery was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update delivery: " +
                    ex.Message
                );
            }
        }

        public OperationResult CancelDelivery(
            int deliveryId)
        {
            if (deliveryId <= 0)
            {
                return Failure(
                    "Please select a delivery."
                );
            }

            try
            {
                bool cancelled =
                    repository.CancelDelivery(deliveryId);

                return cancelled
                    ? Success(
                        "Delivery cancelled successfully."
                    )
                    : Failure(
                        "A delivered or missing delivery " +
                        "cannot be cancelled."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to cancel delivery: " +
                    ex.Message
                );
            }
        }

        private static void PrepareDelivery(
            Delivery delivery)
        {
            delivery.DeliveryReference =
                delivery.DeliveryReference
                    .Trim()
                    .ToUpperInvariant();

            delivery.DeliveryStatus =
                delivery.DeliveryStatus
                    .Trim()
                    .ToUpperInvariant();

            delivery.Notes =
                delivery.Notes.Trim();

            if (delivery.DeliveryStatus == "DELIVERED" &&
                !delivery.DeliveryDate.HasValue)
            {
                delivery.DeliveryDate =
                    DateTime.Today;
            }
        }

        private static OperationResult ValidateDelivery(
            Delivery delivery)
        {
            if (delivery.PurchaseOrderId <= 0)
            {
                return Failure(
                    "Please select a purchase order."
                );
            }

            if (string.IsNullOrWhiteSpace(
                delivery.DeliveryReference))
            {
                return Failure(
                    "Delivery reference is required."
                );
            }

            if (delivery.DeliveryReference.Length > 60)
            {
                return Failure(
                    "Delivery reference cannot exceed " +
                    "60 characters."
                );
            }

            string[] validStatuses =
            {
                "SCHEDULED",
                "DISPATCHED",
                "IN_TRANSIT",
                "DELIVERED",
                "REJECTED",
                "CANCELLED"
            };

            if (!validStatuses.Contains(
                delivery.DeliveryStatus))
            {
                return Failure(
                    "Please select a valid delivery status."
                );
            }

            if (delivery.Notes.Length > 500)
            {
                return Failure(
                    "Notes cannot exceed 500 characters."
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