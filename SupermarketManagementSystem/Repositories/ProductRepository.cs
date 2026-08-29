using MySql.Data.MySqlClient;
using SupermarketManagementSystem.Data;
using SupermarketManagementSystem.Models;

namespace SupermarketManagementSystem.Repositories
{
    public class ProductRepository
    {
        public List<Product> GetAll()
        {
            List<Product> products = new();

            const string query = """
                SELECT
                    p.product_id,
                    p.category_id,
                    c.category_name,
                    p.primary_supplier_id,
                    COALESCE(s.supplier_name, '') AS supplier_name,
                    p.barcode,
                    p.product_name,
                    COALESCE(p.description, '') AS description,
                    p.unit_of_measure,
                    p.cost_price,
                    p.selling_price,
                    p.tax_rate,
                    p.minimum_stock,
                    p.product_status,
                    p.created_at,
                    p.updated_at
                FROM products p
                INNER JOIN categories c
                    ON p.category_id = c.category_id
                LEFT JOIN suppliers s
                    ON p.primary_supplier_id = s.supplier_id
                ORDER BY p.product_name;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }

        public Product? GetById(int productId)
        {
            const string query = """
                SELECT
                    p.product_id,
                    p.category_id,
                    c.category_name,
                    p.primary_supplier_id,
                    COALESCE(s.supplier_name, '') AS supplier_name,
                    p.barcode,
                    p.product_name,
                    COALESCE(p.description, '') AS description,
                    p.unit_of_measure,
                    p.cost_price,
                    p.selling_price,
                    p.tax_rate,
                    p.minimum_stock,
                    p.product_status,
                    p.created_at,
                    p.updated_at
                FROM products p
                INNER JOIN categories c
                    ON p.category_id = c.category_id
                LEFT JOIN suppliers s
                    ON p.primary_supplier_id = s.supplier_id
                WHERE p.product_id = @productId;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@productId",
                productId
            );

            connection.Open();

            using MySqlDataReader reader = command.ExecuteReader();

            return reader.Read()
                ? MapProduct(reader)
                : null;
        }

        public bool BarcodeExists(
            string barcode,
            int? excludedProductId = null)
        {
            const string query = """
                SELECT COUNT(*)
                FROM products
                WHERE barcode = @barcode
                  AND (@excludedProductId IS NULL
                       OR product_id <> @excludedProductId);
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@barcode",
                barcode
            );

            command.Parameters.AddWithValue(
                "@excludedProductId",
                excludedProductId.HasValue
                    ? excludedProductId.Value
                    : DBNull.Value
            );

            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public int Add(Product product)
        {
            const string query = """
                INSERT INTO products
                (
                    category_id,
                    primary_supplier_id,
                    barcode,
                    product_name,
                    description,
                    unit_of_measure,
                    cost_price,
                    selling_price,
                    tax_rate,
                    minimum_stock,
                    product_status
                )
                VALUES
                (
                    @categoryId,
                    @primarySupplierId,
                    @barcode,
                    @productName,
                    @description,
                    @unitOfMeasure,
                    @costPrice,
                    @sellingPrice,
                    @taxRate,
                    @minimumStock,
                    @productStatus
                );

                SELECT LAST_INSERT_ID();
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, product);

            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar());
        }

        public bool Update(Product product)
        {
            const string query = """
                UPDATE products
                SET
                    category_id = @categoryId,
                    primary_supplier_id = @primarySupplierId,
                    barcode = @barcode,
                    product_name = @productName,
                    description = @description,
                    unit_of_measure = @unitOfMeasure,
                    cost_price = @costPrice,
                    selling_price = @sellingPrice,
                    tax_rate = @taxRate,
                    minimum_stock = @minimumStock,
                    product_status = @productStatus
                WHERE product_id = @productId;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            AddParameters(command, product);

            command.Parameters.AddWithValue(
                "@productId",
                product.ProductId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool Delete(int productId)
        {
            const string query = """
                UPDATE products
                SET product_status = 'DISCONTINUED'
                WHERE product_id = @productId;
                """;

            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@productId",
                productId
            );

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        private static void AddParameters(
            MySqlCommand command,
            Product product)
        {
            command.Parameters.AddWithValue(
                "@categoryId",
                product.CategoryId
            );

            command.Parameters.AddWithValue(
                "@primarySupplierId",
                product.PrimarySupplierId.HasValue
                    ? product.PrimarySupplierId.Value
                    : DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@barcode",
                product.Barcode
            );

            command.Parameters.AddWithValue(
                "@productName",
                product.ProductName
            );

            command.Parameters.AddWithValue(
                "@description",
                string.IsNullOrWhiteSpace(product.Description)
                    ? DBNull.Value
                    : product.Description
            );

            command.Parameters.AddWithValue(
                "@unitOfMeasure",
                product.UnitOfMeasure
            );

            command.Parameters.AddWithValue(
                "@costPrice",
                product.CostPrice
            );

            command.Parameters.AddWithValue(
                "@sellingPrice",
                product.SellingPrice
            );

            command.Parameters.AddWithValue(
                "@taxRate",
                product.TaxRate
            );

            command.Parameters.AddWithValue(
                "@minimumStock",
                product.MinimumStock
            );

            command.Parameters.AddWithValue(
                "@productStatus",
                product.ProductStatus
            );
        }

        private static Product MapProduct(
            MySqlDataReader reader)
        {
            return new Product
            {
                ProductId =
                    reader.GetInt32("product_id"),

                CategoryId =
                    reader.GetInt32("category_id"),

                CategoryName =
                    reader.GetString("category_name"),

                PrimarySupplierId =
                    reader.IsDBNull(
                        reader.GetOrdinal("primary_supplier_id"))
                        ? null
                        : reader.GetInt32("primary_supplier_id"),

                SupplierName =
                    reader.GetString("supplier_name"),

                Barcode =
                    reader.GetString("barcode"),

                ProductName =
                    reader.GetString("product_name"),

                Description =
                    reader.GetString("description"),

                UnitOfMeasure =
                    reader.GetString("unit_of_measure"),

                CostPrice =
                    reader.GetDecimal("cost_price"),

                SellingPrice =
                    reader.GetDecimal("selling_price"),

                TaxRate =
                    reader.GetDecimal("tax_rate"),

                MinimumStock =
                    reader.GetInt32("minimum_stock"),

                ProductStatus =
                    reader.GetString("product_status"),

                CreatedAt =
                    reader.GetDateTime("created_at"),

                UpdatedAt =
                    reader.IsDBNull(
                        reader.GetOrdinal("updated_at"))
                        ? null
                        : reader.GetDateTime("updated_at")
            };
        }
    }
}