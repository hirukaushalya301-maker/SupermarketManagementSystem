using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository repository;

        public CategoryService()
        {
            repository = new CategoryRepository();
        }

        public List<Category> GetAllCategories()
        {
            return repository.GetAllCategories();
        }

        public OperationResult CreateCategory(
            Category category)
        {
            PrepareCategory(category);

            OperationResult validation =
                ValidateCategory(category);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.CategoryNameExists(
                category.CategoryName))
            {
                return Failure(
                    "This category name already exists."
                );
            }

            try
            {
                category.CategoryId =
                    repository.CreateCategory(category);

                return Success(
                    "Category created successfully."
                );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to create category: " +
                    ex.Message
                );
            }
        }

        public OperationResult UpdateCategory(
            Category category)
        {
            if (category.CategoryId <= 0)
            {
                return Failure(
                    "Please select a category."
                );
            }

            PrepareCategory(category);

            OperationResult validation =
                ValidateCategory(category);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.CategoryNameExists(
                category.CategoryName,
                category.CategoryId))
            {
                return Failure(
                    "This category name already exists."
                );
            }

            try
            {
                bool updated =
                    repository.UpdateCategory(category);

                return updated
                    ? Success(
                        "Category updated successfully."
                    )
                    : Failure(
                        "Category was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update category: " +
                    ex.Message
                );
            }
        }

        public OperationResult DeleteCategory(
            int categoryId)
        {
            if (categoryId <= 0)
            {
                return Failure(
                    "Please select a category."
                );
            }

            try
            {
                bool deleted =
                    repository.DeleteCategory(categoryId);

                return deleted
                    ? Success(
                        "Category deleted successfully."
                    )
                    : Failure(
                        "This category cannot be deleted " +
                        "because it is assigned to products."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to delete category: " +
                    ex.Message
                );
            }
        }

        private static void PrepareCategory(
            Category category)
        {
            category.CategoryName =
                category.CategoryName.Trim();

            category.Description =
                category.Description.Trim();
        }

        private static OperationResult ValidateCategory(
            Category category)
        {
            if (string.IsNullOrWhiteSpace(
                category.CategoryName))
            {
                return Failure(
                    "Category name is required."
                );
            }

            if (category.CategoryName.Length > 100)
            {
                return Failure(
                    "Category name cannot exceed " +
                    "100 characters."
                );
            }

            if (category.Description.Length > 255)
            {
                return Failure(
                    "Description cannot exceed " +
                    "255 characters."
                );
            }

            if (category.CategoryStatus != "ACTIVE" &&
                category.CategoryStatus != "INACTIVE")
            {
                return Failure(
                    "Invalid category status."
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