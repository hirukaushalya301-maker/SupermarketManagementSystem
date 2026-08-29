using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class ProductBatchService
    {
        private readonly ProductBatchRepository repository;

        public ProductBatchService()
        {
            repository = new ProductBatchRepository();
        }

        public List<ProductBatch> GetAllBatches()
        {
            repository.UpdateExpiredBatches();

            return repository.GetAll();
        }

        public OperationResult CreateBatch(
            ProductBatch batch)
        {
            PrepareBatch(batch);

            // A new batch initially has all received stock available.
            batch.AvailableQuantity =
                batch.ReceivedQuantity;

            OperationResult validation =
                ValidateBatch(batch);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.BatchNumberExists(
                batch.ProductId,
                batch.BatchNumber))
            {
                return Failure(
                    "This batch number already exists " +
                    "for the selected product."
                );
            }

            try
            {
                batch.BatchId = repository.Add(batch);

                return batch.BatchId > 0
                    ? Success(
                        "Product batch created successfully."
                    )
                    : Failure(
                        "The product batch could not be created."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to create product batch: " +
                    ex.Message
                );
            }
        }

        public OperationResult UpdateBatch(
            ProductBatch batch)
        {
            if (batch.BatchId <= 0)
            {
                return Failure(
                    "Please select a product batch."
                );
            }

            PrepareBatch(batch);

            OperationResult validation =
                ValidateBatch(batch);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.BatchNumberExists(
                batch.ProductId,
                batch.BatchNumber,
                batch.BatchId))
            {
                return Failure(
                    "This batch number already exists " +
                    "for the selected product."
                );
            }

            try
            {
                bool updated =
                    repository.Update(batch);

                return updated
                    ? Success(
                        "Product batch updated successfully."
                    )
                    : Failure(
                        "The product batch was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update product batch: " +
                    ex.Message
                );
            }
        }

        public OperationResult BlockBatch(int batchId)
        {
            if (batchId <= 0)
            {
                return Failure(
                    "Please select a product batch."
                );
            }

            try
            {
                bool blocked =
                    repository.Block(batchId);

                return blocked
                    ? Success(
                        "Product batch blocked successfully."
                    )
                    : Failure(
                        "The product batch was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to block product batch: " +
                    ex.Message
                );
            }
        }

        private static void PrepareBatch(
            ProductBatch batch)
        {
            batch.BatchNumber =
                batch.BatchNumber.Trim();

            if (batch.ExpiryDate.HasValue &&
                batch.ExpiryDate.Value.Date <
                DateTime.Today)
            {
                batch.BatchStatus = "EXPIRED";
            }
            else if (batch.AvailableQuantity == 0 &&
                     batch.ReceivedQuantity > 0)
            {
                batch.BatchStatus = "DEPLETED";
            }
        }

        private static OperationResult ValidateBatch(
            ProductBatch batch)
        {
            if (batch.ProductId <= 0)
            {
                return Failure(
                    "Please select a product."
                );
            }

            if (string.IsNullOrWhiteSpace(
                batch.BatchNumber))
            {
                return Failure(
                    "Batch number is required."
                );
            }

            if (batch.BatchNumber.Length > 60)
            {
                return Failure(
                    "Batch number cannot exceed 60 characters."
                );
            }

            if (batch.ManufacturedDate.HasValue &&
                batch.ManufacturedDate.Value.Date >
                DateTime.Today)
            {
                return Failure(
                    "Manufactured date cannot be in the future."
                );
            }

            if (batch.ManufacturedDate.HasValue &&
                batch.ExpiryDate.HasValue &&
                batch.ExpiryDate.Value.Date <
                batch.ManufacturedDate.Value.Date)
            {
                return Failure(
                    "Expiry date cannot be before " +
                    "the manufactured date."
                );
            }

            if (batch.ReceivedQuantity < 0)
            {
                return Failure(
                    "Received quantity cannot be negative."
                );
            }

            if (batch.AvailableQuantity < 0)
            {
                return Failure(
                    "Available quantity cannot be negative."
                );
            }

            if (batch.AvailableQuantity >
                batch.ReceivedQuantity)
            {
                return Failure(
                    "Available quantity cannot exceed " +
                    "received quantity."
                );
            }

            if (batch.CostPrice < 0)
            {
                return Failure(
                    "Cost price cannot be negative."
                );
            }

            string[] validStatuses =
            {
                "ACTIVE",
                "EXPIRED",
                "DEPLETED",
                "BLOCKED"
            };

            if (!validStatuses.Contains(
                batch.BatchStatus))
            {
                return Failure(
                    "Invalid batch status."
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