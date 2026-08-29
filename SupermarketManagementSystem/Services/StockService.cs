using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class StockService
    {
        private readonly StockRepository repository;

        public StockService()
        {
            repository = new StockRepository();
        }

        public List<StockItem> GetAllStock()
        {
            return repository.GetAllStock();
        }

        public List<StockMovement> GetStockMovements(
            int? productId = null)
        {
            return repository.GetMovements(productId);
        }

        public OperationResult AdjustStock(
            StockMovement movement)
        {
            PrepareMovement(movement);

            OperationResult validation =
                ValidateMovement(movement);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            try
            {
                bool adjusted =
                    repository.AdjustStock(movement);

                return adjusted
                    ? Success(
                        "Stock updated successfully."
                    )
                    : Failure(
                        "Stock could not be updated."
                    );
            }
            catch (InvalidOperationException ex)
            {
                return Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update stock: " +
                    ex.Message
                );
            }
        }

        private static void PrepareMovement(
            StockMovement movement)
        {
            movement.MovementType =
                movement.MovementType
                    .Trim()
                    .ToUpperInvariant();

            movement.ReferenceType =
                movement.ReferenceType.Trim();

            movement.Notes =
                movement.Notes.Trim();
        }

        private static OperationResult ValidateMovement(
            StockMovement movement)
        {
            if (movement.ProductId <= 0)
            {
                return Failure(
                    "Please select a product."
                );
            }

            if (movement.Quantity <= 0)
            {
                return Failure(
                    "Quantity must be greater than zero."
                );
            }

            string[] validMovementTypes =
            {
                "PURCHASE",
                "SALE",
                "RETURN_IN",
                "RETURN_OUT",
                "ADJUSTMENT_IN",
                "ADJUSTMENT_OUT",
                "EXPIRED"
            };

            if (!validMovementTypes.Contains(
                movement.MovementType))
            {
                return Failure(
                    "Please select a valid movement type."
                );
            }

            if (movement.ReferenceType.Length > 40)
            {
                return Failure(
                    "Reference type cannot exceed " +
                    "40 characters."
                );
            }

            if (movement.Notes.Length > 255)
            {
                return Failure(
                    "Notes cannot exceed 255 characters."
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