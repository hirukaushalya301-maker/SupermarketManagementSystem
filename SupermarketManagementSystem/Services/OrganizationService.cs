using System.Net.Mail;
using System.Text.RegularExpressions;
using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class OrganizationService
    {
        private readonly OrganizationRepository repository;

        public OrganizationService()
        {
            repository = new OrganizationRepository();
        }

        public OrganizationProfile? GetProfile()
        {
            return repository.GetProfile();
        }

        public OperationResult SaveProfile(
            OrganizationProfile profile)
        {
            OperationResult validation =
                ValidateProfile(profile);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            try
            {
                if (profile.OrganizationId <= 0)
                {
                    profile.OrganizationId =
                        repository.CreateProfile(profile);

                    return Success(
                        "Organization profile created successfully."
                    );
                }

                bool updated =
                    repository.UpdateProfile(profile);

                return updated
                    ? Success(
                        "Organization profile updated successfully."
                    )
                    : Failure(
                        "Organization profile was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to save organization profile: " +
                    ex.Message
                );
            }
        }

        private static OperationResult ValidateProfile(
            OrganizationProfile profile)
        {
            profile.OrganizationName =
                profile.OrganizationName.Trim();

            profile.Address = profile.Address.Trim();
            profile.Phone = profile.Phone.Trim();
            profile.Email = profile.Email.Trim();
            profile.OpeningHours =
                profile.OpeningHours.Trim();
            profile.TaxNumber = profile.TaxNumber.Trim();

            if (string.IsNullOrWhiteSpace(
                profile.OrganizationName))
            {
                return Failure(
                    "Organization name is required."
                );
            }

            if (profile.OrganizationName.Length > 150)
            {
                return Failure(
                    "Organization name cannot exceed " +
                    "150 characters."
                );
            }

            if (profile.Address.Length > 255)
            {
                return Failure(
                    "Address cannot exceed 255 characters."
                );
            }

            if (!string.IsNullOrWhiteSpace(profile.Phone) &&
                !Regex.IsMatch(
                    profile.Phone,
                    @"^[0-9+\-\s]{7,20}$"))
            {
                return Failure(
                    "Enter a valid phone number."
                );
            }

            if (!string.IsNullOrWhiteSpace(profile.Email) &&
                !IsValidEmail(profile.Email))
            {
                return Failure(
                    "Enter a valid email address."
                );
            }

            if (profile.OpeningHours.Length > 150)
            {
                return Failure(
                    "Opening hours cannot exceed " +
                    "150 characters."
                );
            }

            if (profile.TaxNumber.Length > 50)
            {
                return Failure(
                    "Tax number cannot exceed 50 characters."
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