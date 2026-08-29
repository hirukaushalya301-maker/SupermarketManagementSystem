using System.Net.Mail;
using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class SupplierService
    {
        private readonly SupplierRepository repository;

        public SupplierService()
        {
            repository = new SupplierRepository();
        }

        public List<Supplier> GetAllSuppliers()
        {
            return repository.GetAllSuppliers();
        }

        public OperationResult CreateSupplier(
            Supplier supplier)
        {
            PrepareSupplier(supplier);

            OperationResult validation =
                ValidateSupplier(supplier);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.SupplierCodeExists(
                supplier.SupplierCode))
            {
                return Failure(
                    "This supplier code already exists."
                );
            }

            try
            {
                supplier.SupplierId =
                    repository.CreateSupplier(supplier);

                return supplier.SupplierId > 0
                    ? Success(
                        "Supplier created successfully."
                    )
                    : Failure(
                        "The supplier could not be created."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to create supplier: " +
                    ex.Message
                );
            }
        }

        public OperationResult UpdateSupplier(
            Supplier supplier)
        {
            if (supplier.SupplierId <= 0)
            {
                return Failure(
                    "Please select a supplier."
                );
            }

            PrepareSupplier(supplier);

            OperationResult validation =
                ValidateSupplier(supplier);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.SupplierCodeExists(
                supplier.SupplierCode,
                supplier.SupplierId))
            {
                return Failure(
                    "This supplier code already exists."
                );
            }

            try
            {
                bool updated =
                    repository.UpdateSupplier(supplier);

                return updated
                    ? Success(
                        "Supplier updated successfully."
                    )
                    : Failure(
                        "The supplier was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update supplier: " +
                    ex.Message
                );
            }
        }

        public OperationResult BlockSupplier(int supplierId)
        {
            if (supplierId <= 0)
            {
                return Failure(
                    "Please select a supplier."
                );
            }

            try
            {
                bool blocked =
                    repository.BlockSupplier(supplierId);

                return blocked
                    ? Success(
                        "Supplier blocked successfully."
                    )
                    : Failure(
                        "The supplier was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to block supplier: " +
                    ex.Message
                );
            }
        }

        private static void PrepareSupplier(
            Supplier supplier)
        {
            supplier.SupplierCode =
                supplier.SupplierCode
                    .Trim()
                    .ToUpperInvariant();

            supplier.SupplierName =
                supplier.SupplierName.Trim();

            supplier.ContactPerson =
                supplier.ContactPerson.Trim();

            supplier.Phone =
                supplier.Phone.Trim();

            supplier.Email =
                supplier.Email
                    .Trim()
                    .ToLowerInvariant();

            supplier.Address =
                supplier.Address.Trim();
        }

        private static OperationResult ValidateSupplier(
            Supplier supplier)
        {
            if (string.IsNullOrWhiteSpace(
                supplier.SupplierCode))
            {
                return Failure(
                    "Supplier code is required."
                );
            }

            if (supplier.SupplierCode.Length > 30)
            {
                return Failure(
                    "Supplier code cannot exceed " +
                    "30 characters."
                );
            }

            if (string.IsNullOrWhiteSpace(
                supplier.SupplierName))
            {
                return Failure(
                    "Supplier name is required."
                );
            }

            if (supplier.SupplierName.Length > 150)
            {
                return Failure(
                    "Supplier name cannot exceed " +
                    "150 characters."
                );
            }

            if (supplier.ContactPerson.Length > 100)
            {
                return Failure(
                    "Contact person cannot exceed " +
                    "100 characters."
                );
            }

            if (supplier.Phone.Length > 20)
            {
                return Failure(
                    "Phone number cannot exceed " +
                    "20 characters."
                );
            }

            if (!string.IsNullOrWhiteSpace(
                    supplier.Email) &&
                !IsValidEmail(supplier.Email))
            {
                return Failure(
                    "Please enter a valid email address."
                );
            }

            if (supplier.Email.Length > 120)
            {
                return Failure(
                    "Email cannot exceed 120 characters."
                );
            }

            if (supplier.Address.Length > 255)
            {
                return Failure(
                    "Address cannot exceed 255 characters."
                );
            }

            string[] validStatuses =
            {
                "ACTIVE",
                "INACTIVE",
                "BLOCKED"
            };

            if (!validStatuses.Contains(
                supplier.SupplierStatus))
            {
                return Failure(
                    "Please select a valid supplier status."
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