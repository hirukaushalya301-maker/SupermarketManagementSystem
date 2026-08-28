using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class OrganizationRepository
    {
        public OrganizationProfile? GetProfile()
        {
            const string query = @"
                SELECT
                    organization_id,
                    organization_name,
                    address,
                    phone,
                    email,
                    opening_hours,
                    tax_number,
                    logo_path,
                    updated_at
                FROM organization_profile
                ORDER BY organization_id
                LIMIT 1;";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return new OrganizationProfile
            {
                OrganizationId =
                    reader.GetInt32("organization_id"),

                OrganizationName =
                    reader.GetString("organization_name"),

                Address = GetNullableString(
                    reader,
                    "address"
                ),

                Phone = GetNullableString(
                    reader,
                    "phone"
                ),

                Email = GetNullableString(
                    reader,
                    "email"
                ),

                OpeningHours = GetNullableString(
                    reader,
                    "opening_hours"
                ),

                TaxNumber = GetNullableString(
                    reader,
                    "tax_number"
                ),

                LogoPath = GetNullableString(
                    reader,
                    "logo_path"
                ),

                UpdatedAt =
                    reader.GetDateTime("updated_at")
            };
        }

        public int CreateProfile(
            OrganizationProfile profile)
        {
            const string query = @"
                INSERT INTO organization_profile (
                    organization_name,
                    address,
                    phone,
                    email,
                    opening_hours,
                    tax_number,
                    logo_path
                )
                VALUES (
                    @organizationName,
                    @address,
                    @phone,
                    @email,
                    @openingHours,
                    @taxNumber,
                    @logoPath
                );

                SELECT LAST_INSERT_ID();";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, profile);

            connection.Open();

            return Convert.ToInt32(
                command.ExecuteScalar()
            );
        }

        public bool UpdateProfile(
            OrganizationProfile profile)
        {
            const string query = @"
                UPDATE organization_profile
                SET
                    organization_name =
                        @organizationName,
                    address = @address,
                    phone = @phone,
                    email = @email,
                    opening_hours = @openingHours,
                    tax_number = @taxNumber,
                    logo_path = @logoPath
                WHERE organization_id =
                    @organizationId;";

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, profile);

            command.Parameters.AddWithValue(
                "@organizationId",
                profile.OrganizationId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        private static void AddParameters(
            MySqlCommand command,
            OrganizationProfile profile)
        {
            command.Parameters.AddWithValue(
                "@organizationName",
                profile.OrganizationName
            );

            command.Parameters.AddWithValue(
                "@address",
                NullIfEmpty(profile.Address)
            );

            command.Parameters.AddWithValue(
                "@phone",
                NullIfEmpty(profile.Phone)
            );

            command.Parameters.AddWithValue(
                "@email",
                NullIfEmpty(profile.Email)
            );

            command.Parameters.AddWithValue(
                "@openingHours",
                NullIfEmpty(profile.OpeningHours)
            );

            command.Parameters.AddWithValue(
                "@taxNumber",
                NullIfEmpty(profile.TaxNumber)
            );

            command.Parameters.AddWithValue(
                "@logoPath",
                NullIfEmpty(profile.LogoPath)
            );
        }

        private static string GetNullableString(
            MySqlDataReader reader,
            string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            return reader.IsDBNull(index)
                ? string.Empty
                : reader.GetString(index);
        }

        private static object NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? DBNull.Value
                : value.Trim();
        }
    }
}