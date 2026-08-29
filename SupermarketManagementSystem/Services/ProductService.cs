using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class ProductService
    {
        private readonly ProductRepository productRepository;

        public ProductService()
        {
            productRepository = new ProductRepository();
        }

        public List<Product> GetAllProducts()
        {
            return productRepository.GetAll();
        }

        public Product? GetProductById(int productId)
        {
            if (productId <= 0)
            {
                return null;
            }

            return productRepository.GetById(productId);
        }

        public OperationResult AddProduct(Product product)
        {
            OperationResult validationResult =
                ValidateProduct(product);

            if (!validationResult.IsSuccessful)
            {
                return validationResult;
            }

            if (productRepository.BarcodeExists(product.Barcode))
            {
                return new OperationResult
                {
                    IsSuccessful = false,
                    Message = "This barcode is already used by another product."
                };
            }

            try
            {
                int productId =
                    productRepository.Add(product);

                return new OperationResult
                {
                    IsSuccessful = productId > 0,
                    Message = productId > 0
                        ? "Product added successfully."
                        : "The product could not be added."
                };
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    IsSuccessful = false,
                    Message = "Unable to add product.\n\n" +
                              ex.Message
                };
            }
        }

        public OperationResult UpdateProduct(Product product)
        {
            if (product.ProductId <= 0)
            {
                return new OperationResult
                {
                    IsSuccessful = false,
                    Message = "Please select a product to update."
                };
            }

            OperationResult validationResult =
                ValidateProduct(product);

            if (!validationResult.IsSuccessful)
            {
                return validationResult;
            }

            if (productRepository.BarcodeExists(
                product.Barcode,
                product.ProductId))
            {
                return new OperationResult
                {
                    IsSuccessful = false,
                    Message = "This barcode is already used by another product."
                };
            }

            try
            {
                bool updated =
                    productRepository.Update(product);

                return new OperationResult
                {
                    IsSuccessful = updated,
                    Message = updated
                        ? "Product updated successfully."
                        : "No product changes were saved."
                };
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    IsSuccessful = false,
                    Message = "Unable to update product.\n\n" +
                              ex.Message
                };
            }
        }

        public OperationResult DiscontinueProduct(int productId)
        {
            if (productId <= 0)
            {
                return new OperationResult
                {
                    IsSuccessful = false,
                    Message = "Please select a product."
                };
            }

            try
            {
                bool discontinued =
                    productRepository.Delete(productId);

                return new OperationResult
                {
                    IsSuccessful = discontinued,
                    Message = discontinued
                        ? "Product discontinued successfully."
                        : "The product could not be discontinued."
                };
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    IsSuccessful = false,
                    Message = "Unable to discontinue product.\n\n" +
                              ex.Message
                };
            }
        }

        private static OperationResult ValidateProduct(
            Product product)
        {
            if (product.CategoryId <= 0)
            {
                return Invalid(
                    "Please select a product category."
                );
            }

            if (string.IsNullOrWhiteSpace(product.Barcode))
            {
                return Invalid(
                    "Please enter the product barcode."
                );
            }

            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                return Invalid(
                    "Please enter the product name."
                );
            }

            if (string.IsNullOrWhiteSpace(product.UnitOfMeasure))
            {
                return Invalid(
                    "Please enter the unit of measure."
                );
            }

            if (product.CostPrice < 0)
            {
                return Invalid(
                    "Cost price cannot be negative."
                );
            }

            if (product.SellingPrice <= 0)
            {
                return Invalid(
                    "Selling price must be greater than zero."
                );
            }

            if (product.TaxRate < 0 ||
                product.TaxRate > 100)
            {
                return Invalid(
                    "Tax rate must be between 0 and 100."
                );
            }

            if (product.MinimumStock < 0)
            {
                return Invalid(
                    "Minimum stock cannot be negative."
                );
            }

            string[] validStatuses =
            {
                "ACTIVE",
                "INACTIVE",
                "DISCONTINUED"
            };

            if (!validStatuses.Contains(product.ProductStatus))
            {
                return Invalid(
                    "Please select a valid product status."
                );
            }

            return new OperationResult
            {
                IsSuccessful = true,
                Message = "Product information is valid."
            };
        }

        private static OperationResult Invalid(string message)
        {
            return new OperationResult
            {
                IsSuccessful = false,
                Message = message
            };
        }
    }
}