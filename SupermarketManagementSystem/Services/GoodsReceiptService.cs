using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class GoodsReceiptService
    {
        private readonly GoodsReceiptRepository repository;

        public GoodsReceiptService()
        {
            repository = new GoodsReceiptRepository();
        }

        public List<DeliveryReceiptItem> GetReceiptItems(
            int purchaseOrderId)
        {
            if (purchaseOrderId <= 0)
            {
                return new List<DeliveryReceiptItem>();
            }

            return repository.GetReceiptItems(
                purchaseOrderId
            );
        }

        public OperationResult ReceiveDelivery(
            int deliveryId,
            int purchaseOrderId,
            List<DeliveryReceiptItem> items,
            int? receivedBy)
        {
            if (deliveryId <= 0)
            {
                return Failure(
                    "Please select a delivery."
                );
            }

            if (purchaseOrderId <= 0)
            {
                return Failure(
                    "The delivery does not have a valid " +
                    "purchase order."
                );
            }

            List<DeliveryReceiptItem> receivingItems =
                items
                    .Where(item =>
                        item.ReceivingQuantity > 0)
                    .ToList();

            if (receivingItems.Count == 0)
            {
                return Failure(
                    "Enter a receiving quantity for at least " +
                    "one product."
                );
            }

            foreach (DeliveryReceiptItem item
                in receivingItems)
            {
                PrepareItem(item);

                OperationResult validation =
                    ValidateItem(item);

                if (!validation.IsSuccessful)
                {
                    return validation;
                }
            }

            try
            {
                bool received =
                    repository.ReceiveDelivery(
                        deliveryId,
                        purchaseOrderId,
                        receivingItems,
                        receivedBy
                    );

                return received
                    ? Success(
                        "Delivery received and stock " +
                        "updated successfully."
                    )
                    : Failure(
                        "The delivery could not be received."
                    );
            }
            catch (InvalidOperationException ex)
            {
                return Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to receive delivery: " +
                    ex.Message
                );
            }
        }

        private static void PrepareItem(
            DeliveryReceiptItem item)
        {
            item.BatchNumber =
                item.BatchNumber
                    .Trim()
                    .ToUpperInvariant();
        }

        private static OperationResult ValidateItem(
            DeliveryReceiptItem item)
        {
            if (item.ProductId <= 0)
            {
                return Failure(
                    "A receipt item has an invalid product."
                );
            }

            if (item.ReceivingQuantity <= 0)
            {
                return Failure(
                    $"Enter a valid receiving quantity for " +
                    $"{item.ProductName}."
                );
            }

            if (item.ReceivingQuantity >
                item.RemainingBeforeReceipt)
            {
                return Failure(
                    $"Receiving quantity for " +
                    $"{item.ProductName} cannot exceed " +
                    $"{item.RemainingBeforeReceipt}."
                );
            }

            if (string.IsNullOrWhiteSpace(
                item.BatchNumber))
            {
                return Failure(
                    $"Enter a batch number for " +
                    $"{item.ProductName}."
                );
            }

            if (item.BatchNumber.Length > 60)
            {
                return Failure(
                    $"Batch number for {item.ProductName} " +
                    "cannot exceed 60 characters."
                );
            }

            if (item.ManufacturedDate.HasValue &&
                item.ManufacturedDate.Value.Date >
                DateTime.Today)
            {
                return Failure(
                    $"Manufactured date for " +
                    $"{item.ProductName} cannot be " +
                    "in the future."
                );
            }

            if (item.ManufacturedDate.HasValue &&
                item.ExpiryDate.HasValue &&
                item.ExpiryDate.Value.Date <
                item.ManufacturedDate.Value.Date)
            {
                return Failure(
                    $"Expiry date for {item.ProductName} " +
                    "cannot be before its manufactured date."
                );
            }

            if (item.UnitCost < 0)
            {
                return Failure(
                    $"Unit cost for {item.ProductName} " +
                    "cannot be negative."
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