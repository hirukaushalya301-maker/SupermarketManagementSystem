using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.InventoryManager
{
    public class ProductManagementForm : Form
    {
        private readonly ProductService productService;
        private readonly CategoryService categoryService;

        private DataGridView dgvProducts = null!;
        private ComboBox cmbCategory = null!;
        private TextBox txtBarcode = null!;
        private TextBox txtProductName = null!;
        private TextBox txtDescription = null!;
        private ComboBox cmbUnit = null!;
        private NumericUpDown nudCostPrice = null!;
        private NumericUpDown nudSellingPrice = null!;
        private NumericUpDown nudTaxRate = null!;
        private NumericUpDown nudMinimumStock = null!;
        private ComboBox cmbStatus = null!;

        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDiscontinue = null!;
        private Button btnClear = null!;

        private int selectedProductId;

        public ProductManagementForm()
        {
            productService = new ProductService();
            categoryService = new CategoryService();

            CreateInterface();
            LoadCategories();
            LoadProducts();
        }

        private void CreateInterface()
        {
            Text = "Product Management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1250, 720);
            BackColor = Color.FromArgb(245, 247, 250);
            MinimumSize = new Size(1100, 700);

            Label lblTitle = new Label
            {
                Text = "Product Management",
                Location = new Point(25, 20),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold
                ),
                ForeColor = Color.FromArgb(24, 90, 60)
            };

            Label lblSubtitle = new Label
            {
                Text = "Add, update and manage supermarket products",
                Location = new Point(28, 62),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray
            };

            Panel inputPanel = new Panel
            {
                Location = new Point(25, 100),
                Size = new Size(1200, 235),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            CreateInputControls(inputPanel);

            dgvProducts = new DataGridView
            {
                Location = new Point(25, 355),
                Size = new Size(1200, 330),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = false,
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect
            };

            dgvProducts.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(24, 90, 60),
                    ForeColor = Color.White,
                    Font = new Font(
                        "Segoe UI",
                        9,
                        FontStyle.Bold
                    ),
                    Alignment =
                        DataGridViewContentAlignment.MiddleCenter
                };

            dgvProducts.EnableHeadersVisualStyles = false;
            dgvProducts.DefaultCellStyle.Font =
                new Font("Segoe UI", 9);

            dgvProducts.RowTemplate.Height = 32;
            dgvProducts.CellClick += DgvProducts_CellClick;

            CreateProductGridColumns();

            Controls.Add(lblTitle);
            Controls.Add(lblSubtitle);
            Controls.Add(inputPanel);
            Controls.Add(dgvProducts);
        }

        private void CreateInputControls(Panel panel)
        {
            AddLabel(panel, "Category", 20, 20);

            cmbCategory = new ComboBox
            {
                Location = new Point(20, 45),
                Size = new Size(220, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            AddLabel(panel, "Barcode", 260, 20);

            txtBarcode = new TextBox
            {
                Location = new Point(260, 45),
                Size = new Size(200, 30),
                MaxLength = 80
            };

            AddLabel(panel, "Product name", 480, 20);

            txtProductName = new TextBox
            {
                Location = new Point(480, 45),
                Size = new Size(260, 30),
                MaxLength = 150
            };

            AddLabel(panel, "Unit", 760, 20);

            cmbUnit = new ComboBox
            {
                Location = new Point(760, 45),
                Size = new Size(150, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbUnit.Items.AddRange(
                new object[]
                {
                    "Unit",
                    "Kilogram",
                    "Gram",
                    "Litre",
                    "Millilitre",
                    "Packet",
                    "Box",
                    "Bottle"
                }
            );

            AddLabel(panel, "Status", 930, 20);

            cmbStatus = new ComboBox
            {
                Location = new Point(930, 45),
                Size = new Size(160, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbStatus.Items.AddRange(
                new object[]
                {
                    "ACTIVE",
                    "INACTIVE",
                    "DISCONTINUED"
                }
            );

            AddLabel(panel, "Description", 20, 90);

            txtDescription = new TextBox
            {
                Location = new Point(20, 115),
                Size = new Size(300, 30),
                MaxLength = 500
            };

            AddLabel(panel, "Cost price", 340, 90);

            nudCostPrice = CreateMoneyInput(340, 115);

            AddLabel(panel, "Selling price", 500, 90);

            nudSellingPrice = CreateMoneyInput(500, 115);

            AddLabel(panel, "Tax rate (%)", 660, 90);

            nudTaxRate = new NumericUpDown
            {
                Location = new Point(660, 115),
                Size = new Size(130, 30),
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 100
            };

            AddLabel(panel, "Minimum stock", 810, 90);

            nudMinimumStock = new NumericUpDown
            {
                Location = new Point(810, 115),
                Size = new Size(130, 30),
                Minimum = 0,
                Maximum = 100000
            };

            btnAdd = CreateButton(
                "ADD",
                20,
                170,
                Color.FromArgb(24, 90, 60)
            );

            btnUpdate = CreateButton(
                "UPDATE",
                165,
                170,
                Color.FromArgb(30, 100, 180)
            );

            btnDiscontinue = CreateButton(
                "DISCONTINUE",
                310,
                170,
                Color.FromArgb(180, 50, 50)
            );

            btnClear = CreateButton(
                "CLEAR",
                455,
                170,
                Color.DimGray
            );

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDiscontinue.Click += BtnDiscontinue_Click;
            btnClear.Click += BtnClear_Click;

            panel.Controls.Add(cmbCategory);
            panel.Controls.Add(txtBarcode);
            panel.Controls.Add(txtProductName);
            panel.Controls.Add(cmbUnit);
            panel.Controls.Add(cmbStatus);
            panel.Controls.Add(txtDescription);
            panel.Controls.Add(nudCostPrice);
            panel.Controls.Add(nudSellingPrice);
            panel.Controls.Add(nudTaxRate);
            panel.Controls.Add(nudMinimumStock);
            panel.Controls.Add(btnAdd);
            panel.Controls.Add(btnUpdate);
            panel.Controls.Add(btnDiscontinue);
            panel.Controls.Add(btnClear);
        }

        private void LoadCategories()
        {
            try
            {
                List<Category> categories =
                    categoryService
                        .GetAllCategories()
                        .Where(category =>
                            category.CategoryStatus == "ACTIVE")
                        .ToList();

                cmbCategory.DataSource = categories;
                cmbCategory.DisplayMember = "CategoryName";
                cmbCategory.ValueMember = "CategoryId";
                cmbCategory.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load categories.\n\n" +
                    ex.Message
                );
            }
        }

        private void LoadProducts()
        {
            try
            {
                dgvProducts.DataSource = null;
                dgvProducts.DataSource =
                    productService.GetAllProducts();

                dgvProducts.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load products.\n\n" +
                    ex.Message
                );
            }
        }

        private void BtnAdd_Click(
            object? sender,
            EventArgs e)
        {
            Product? product = ReadProductFromForm();

            if (product == null)
            {
                return;
            }

            OperationResult result =
                productService.AddProduct(product);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadProducts();
                ClearForm();
            }
        }

        private void BtnUpdate_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedProductId <= 0)
            {
                ShowWarning(
                    "Please select a product to update."
                );

                return;
            }

            Product? product = ReadProductFromForm();

            if (product == null)
            {
                return;
            }

            product.ProductId = selectedProductId;

            OperationResult result =
                productService.UpdateProduct(product);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadProducts();
                ClearForm();
            }
        }

        private void BtnDiscontinue_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedProductId <= 0)
            {
                ShowWarning(
                    "Please select a product."
                );

                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    "Do you want to discontinue this product?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            OperationResult result =
                productService.DiscontinueProduct(
                    selectedProductId
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadProducts();
                ClearForm();
            }
        }

        private void BtnClear_Click(
            object? sender,
            EventArgs e)
        {
            ClearForm();
        }

        private void DgvProducts_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvProducts.Rows[e.RowIndex]
                .DataBoundItem is not Product product)
            {
                return;
            }

            selectedProductId = product.ProductId;

            cmbCategory.SelectedValue = product.CategoryId;
            txtBarcode.Text = product.Barcode;
            txtProductName.Text = product.ProductName;
            txtDescription.Text = product.Description;
            cmbUnit.Text = product.UnitOfMeasure;
            nudCostPrice.Value = product.CostPrice;
            nudSellingPrice.Value = product.SellingPrice;
            nudTaxRate.Value = product.TaxRate;
            nudMinimumStock.Value = product.MinimumStock;
            cmbStatus.Text = product.ProductStatus;
        }

        private Product? ReadProductFromForm()
        {
            if (cmbCategory.SelectedValue == null)
            {
                ShowWarning(
                    "Please select a category."
                );

                cmbCategory.Focus();
                return null;
            }

            return new Product
            {
                CategoryId =
                    Convert.ToInt32(cmbCategory.SelectedValue),

                PrimarySupplierId = null,

                Barcode =
                    txtBarcode.Text.Trim(),

                ProductName =
                    txtProductName.Text.Trim(),

                Description =
                    txtDescription.Text.Trim(),

                UnitOfMeasure =
                    cmbUnit.Text,

                CostPrice =
                    nudCostPrice.Value,

                SellingPrice =
                    nudSellingPrice.Value,

                TaxRate =
                    nudTaxRate.Value,

                MinimumStock =
                    Convert.ToInt32(
                        nudMinimumStock.Value
                    ),

                ProductStatus =
                    cmbStatus.Text
            };
        }

        private void ClearForm()
        {
            selectedProductId = 0;

            cmbCategory.SelectedIndex = -1;
            txtBarcode.Clear();
            txtProductName.Clear();
            txtDescription.Clear();
            cmbUnit.SelectedItem = "Unit";
            nudCostPrice.Value = 0;
            nudSellingPrice.Value = 0;
            nudTaxRate.Value = 0;
            nudMinimumStock.Value = 0;
            cmbStatus.SelectedItem = "ACTIVE";

            dgvProducts.ClearSelection();
            txtBarcode.Focus();
        }

        private void CreateProductGridColumns()
        {
            AddColumn(
                "ProductId",
                "ID",
                55
            );

            AddColumn(
                "Barcode",
                "Barcode",
                110
            );

            AddColumn(
                "ProductName",
                "Product Name",
                180
            );

            AddColumn(
                "CategoryName",
                "Category",
                130
            );

            AddColumn(
                "UnitOfMeasure",
                "Unit",
                90
            );

            AddColumn(
                "CostPrice",
                "Cost Price",
                100,
                "N2"
            );

            AddColumn(
                "SellingPrice",
                "Selling Price",
                110,
                "N2"
            );

            AddColumn(
                "TaxRate",
                "Tax %",
                75,
                "N2"
            );

            AddColumn(
                "MinimumStock",
                "Minimum Stock",
                110
            );

            AddColumn(
                "ProductStatus",
                "Status",
                110
            );
        }

        private void AddColumn(
            string propertyName,
            string headerText,
            int width,
            string? format = null)
        {
            DataGridViewTextBoxColumn column = new()
            {
                DataPropertyName = propertyName,
                HeaderText = headerText,
                Width = width,
                SortMode =
                    DataGridViewColumnSortMode.Automatic
            };

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.DefaultCellStyle.Format = format;
            }

            dgvProducts.Columns.Add(column);
        }

        private static void AddLabel(
            Control parent,
            string text,
            int x,
            int y)
        {
            Label label = new()
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold
                )
            };

            parent.Controls.Add(label);
        }

        private static NumericUpDown CreateMoneyInput(
            int x,
            int y)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(140, 30),
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 10000000,
                ThousandsSeparator = true
            };
        }

        private static Button CreateButton(
            string text,
            int x,
            int y,
            Color backColor)
        {
            Button button = new()
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(130, 40),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold
                ),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;

            return button;
        }

        private static void ShowResult(
            OperationResult result)
        {
            MessageBox.Show(
                result.Message,
                result.IsSuccessful
                    ? "Success"
                    : "Error",
                MessageBoxButtons.OK,
                result.IsSuccessful
                    ? MessageBoxIcon.Information
                    : MessageBoxIcon.Warning
            );
        }

        private static void ShowWarning(string message)
        {
            MessageBox.Show(
                message,
                "Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        private static void ShowError(string message)
        {
            MessageBox.Show(
                message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}