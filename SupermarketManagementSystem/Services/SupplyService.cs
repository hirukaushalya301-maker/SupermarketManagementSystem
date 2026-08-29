using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class SupplyService
    {
        private readonly SupplyRepository repository;

        public SupplyService()
        {
            repository = new SupplyRepository();
        }

        public List<Supply> GetAllSupplies()
        {
            return repository.GetAllSupplies();
        }

        public OperationResult CreateSupply(
            Supply supply)
        {
            PrepareSupply(supply);

            OperationResult validation =
                ValidateSupply(supply);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.SupplierProductExists(
                supply.SupplierId,
                supply.ProductId))
            {
                return Failure(
                    "This supplier is already assigned " +
                    "to the selected product."
                );
            }

            try
            {
                supply.SupplyId =
                    repository.CreateSupply(supply);

                return supply.SupplyId > 0
                    ? Success(
                        "Supply relationship created successfully."
                    )
                    : Failure(
                        "The supply relationship could not be created."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to create supply relationship: " +
                    ex.Message
                );
            }
        }

        public OperationResult UpdateSupply(
            Supply supply)
        {
            if (supply.SupplyId <= 0)
            {
                return Failure(
                    "Please select a supply record."
                );
            }

            PrepareSupply(supply);

            OperationResult validation =
                ValidateSupply(supply);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.SupplierProductExists(
                supply.SupplierId,
                supply.ProductId,
                supply.SupplyId))
            {
                return Failure(
                    "This supplier is already assigned " +
                    "to the selected product."
                );
            }

            try
            {
                bool updated =
                    repository.UpdateSupply(supply);

                return updated
                    ? Success(
                        "Supply relationship updated successfully."
                    )
                    : Failure(
                        "The supply record was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update supply relationship: " +
                    ex.Message
                );
            }
        }

        public OperationResult DeactivateSupply(
            int supplyId)
        {
            if (supplyId <= 0)
            {
                return Failure(
                    "Please select a supply record."
                );
            }

            try
            {
                bool deactivated =
                    repository.DeactivateSupply(supplyId);

                return deactivated
                    ? Success(
                        "Supply relationship deactivated successfully."
                    )
                    : Failure(
                        "The supply record was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to deactivate supply relationship: " +
                    ex.Message
                );
            }
        }

        private static void PrepareSupply(
            Supply supply)
        {
            supply.SupplierProductCode =
                supply.SupplierProductCode
                    .Trim()
                    .ToUpperInvariant();
        }

        private static OperationResult ValidateSupply(
            Supply supply)
        {
            if (supply.SupplierId <= 0)
            {
                return Failure(
                    "Please select a supplier."
                );
            }

            if (supply.ProductId <= 0)
            {
                return Failure(
                    "Please select a product."
                );
            }

            if (supply.SupplierProductCode.Length > 60)
            {
                return Failure(
                    "Supplier product code cannot exceed " +
                    "60 characters."
                );
            }

            if (supply.SupplierPrice < 0)
            {
                return Failure(
                    "Supplier price cannot be negative."
                );
            }

            if (supply.LeadTimeDays < 0)
            {
                return Failure(
                    "Lead time cannot be negative."
                );
            }

            string[] validStatuses =
            {
                "ACTIVE",
                "INACTIVE"
            };

            if (!validStatuses.Contains(
                supply.SupplyStatus))
            {
                return Failure(
                    "Please select a valid supply status."
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