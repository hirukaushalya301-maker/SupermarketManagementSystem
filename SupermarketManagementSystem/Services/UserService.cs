using System.Text.RegularExpressions;
using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class UserService
    {
        private readonly UserRepository userRepository;

        public UserService()
        {
            userRepository = new UserRepository();
        }

        public List<User> GetAllUsers()
        {
            return userRepository.GetAllUsers();
        }

        public List<Role> GetAllRoles()
        {
            return userRepository.GetAllRoles();
        }

        public OperationResult CreateUser(
            string username,
            string password,
            string fullName,
            int roleId)
        {
            username = username.Trim();
            fullName = fullName.Trim();

            OperationResult validation =
                ValidateUserDetails(
                    username,
                    fullName,
                    roleId
                );

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (string.IsNullOrWhiteSpace(password) ||
                password.Length < 6)
            {
                return Failure(
                    "Password must contain at least 6 characters."
                );
            }

            if (userRepository.UsernameExists(username))
            {
                return Failure(
                    "This username is already being used."
                );
            }

            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(password);

            User user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                FullName = fullName,
                RoleId = roleId,
                AccountStatus = "ACTIVE"
            };

            try
            {
                userRepository.CreateUser(user);

                return Success(
                    "User account created successfully."
                );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to create user: " + ex.Message
                );
            }
        }

        public OperationResult UpdateUser(
            int userId,
            string username,
            string fullName,
            int roleId,
            string accountStatus)
        {
            username = username.Trim();
            fullName = fullName.Trim();

            if (userId <= 0)
            {
                return Failure(
                    "Please select a user to update."
                );
            }

            OperationResult validation =
                ValidateUserDetails(
                    username,
                    fullName,
                    roleId
                );

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (!IsValidStatus(accountStatus))
            {
                return Failure(
                    "Invalid account status."
                );
            }

            if (userRepository.UsernameExists(
                username,
                userId))
            {
                return Failure(
                    "This username is already being used."
                );
            }

            User user = new User
            {
                UserId = userId,
                Username = username,
                FullName = fullName,
                RoleId = roleId,
                AccountStatus = accountStatus
            };

            try
            {
                bool updated =
                    userRepository.UpdateUser(user);

                return updated
                    ? Success("User updated successfully.")
                    : Failure("User record was not found.");
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update user: " + ex.Message
                );
            }
        }

        public OperationResult UpdateAccountStatus(
            int userId,
            string accountStatus)
        {
            if (userId <= 0)
            {
                return Failure(
                    "Please select a user."
                );
            }

            if (!IsValidStatus(accountStatus))
            {
                return Failure(
                    "Invalid account status."
                );
            }

            try
            {
                bool updated =
                    userRepository.UpdateAccountStatus(
                        userId,
                        accountStatus
                    );

                return updated
                    ? Success(
                        "Account status updated successfully."
                    )
                    : Failure("User record was not found.");
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update status: " + ex.Message
                );
            }
        }

        private static OperationResult ValidateUserDetails(
            string username,
            string fullName,
            int roleId)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Failure(
                    "Username is required."
                );
            }

            if (!Regex.IsMatch(
                username,
                @"^[a-zA-Z0-9_]{3,50}$"))
            {
                return Failure(
                    "Username must contain 3-50 letters, " +
                    "numbers or underscores."
                );
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                return Failure(
                    "Full name is required."
                );
            }

            if (fullName.Length > 100)
            {
                return Failure(
                    "Full name cannot exceed 100 characters."
                );
            }

            if (roleId <= 0)
            {
                return Failure(
                    "Please select a user role."
                );
            }

            return Success("Valid");
        }

        private static bool IsValidStatus(
            string accountStatus)
        {
            return accountStatus == "ACTIVE" ||
                   accountStatus == "INACTIVE" ||
                   accountStatus == "BLOCKED";
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