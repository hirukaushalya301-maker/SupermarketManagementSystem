using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.InventoryManager
{
    public class SupplyManagementForm : Form
    {
        private readonly SupplyService supplyService;
        private readonly SupplierService supplierService;
        private readonly ProductService productService;

        private DataGridView dgvSupplies = null!;
        private ComboBox cmbSupplier = null!;
        private ComboBox cmbProduct = null!;
        private TextBox txtSupplierProductCode = null!;
        private NumericUpDown nudSupplierPrice = null!;
        private NumericUpDown nudLeadTimeDays = null!;
        private ComboBox cmbStatus = null!;

        private int selectedSupplyId;

        public SupplyManagementForm()
        {
            supplyService = new SupplyService();
            supplierService = new SupplierService();
            productService = new ProductService();

            CreateInterface();
            LoadSuppliers();
            LoadProducts();
            LoadSupplies();
            ClearForm();
        }

        private void CreateInterface()
        {
            Text = "Supply Management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1200, 680);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Supply Management",
                Location = new Point(25, 18),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold
                ),
                ForeColor = Color.FromArgb(24, 90, 60)
            };

            Label subtitle = new Label
            {
                Text =
                    "Assign products to suppliers and record supplier prices",
                Location = new Point(28, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray
            };

            Panel inputPanel = new Panel
            {
                Location = new Point(25, 95),
                Size = new Size(1150, 190),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            CreateInputControls(inputPanel);

            dgvSupplies = new DataGridView
            {
                Location = new Point(25, 305),
                Size = new Size(1150, 340),
                BackgroundColor = Color.White,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect
            };

            dgvSupplies.ColumnHeadersDefaultCellStyle =
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

            dgvSupplies.EnableHeadersVisualStyles = false;
            dgvSupplies.RowTemplate.Height = 32;
            dgvSupplies.CellClick += DgvSupplies_CellClick;

            CreateGridColumns();

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(inputPanel);
            Controls.Add(dgvSupplies);
        }

        private void CreateInputControls(Panel panel)
        {
            AddLabel(panel, "Supplier", 20, 18);

            cmbSupplier = new ComboBox
            {
                Location = new Point(20, 43),
                Size = new Size(240, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            AddLabel(panel, "Product", 280, 18);

            cmbProduct = new ComboBox
            {
                Location = new Point(280, 43),
                Size = new Size(240, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            AddLabel(panel, "Supplier product code", 540, 18);

            txtSupplierProductCode = new TextBox
            {
                Location = new Point(540, 43),
                Size = new Size(190, 30),
                MaxLength = 60
            };

            AddLabel(panel, "Supplier price", 750, 18);

            nudSupplierPrice = new NumericUpDown
            {
                Location = new Point(750, 43),
                Size = new Size(150, 30),
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 10000000,
                ThousandsSeparator = true
            };

            AddLabel(panel, "Lead time (days)", 920, 18);

            nudLeadTimeDays = new NumericUpDown
            {
                Location = new Point(920, 43),
                Size = new Size(140, 30),
                Minimum = 0,
                Maximum = 365
            };

            AddLabel(panel, "Status", 20, 88);

            cmbStatus = new ComboBox
            {
                Location = new Point(20, 113),
                Size = new Size(160, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbStatus.Items.AddRange(
                new object[]
                {
                    "ACTIVE",
                    "INACTIVE"
                }
            );

            Button btnAdd = CreateButton(
                "ADD",
                220,
                105,
                Color.FromArgb(24, 90, 60)
            );

            Button btnUpdate = CreateButton(
                "UPDATE",
                360,
                105,
                Color.FromArgb(30, 100, 180)
            );

            Button btnDeactivate = CreateButton(
                "DEACTIVATE",
                500,
                105,
                Color.FromArgb(180, 50, 50)
            );

            Button btnClear = CreateButton(
                "CLEAR",
                640,
                105,
                Color.DimGray
            );

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDeactivate.Click += BtnDeactivate_Click;
            btnClear.Click += (_, _) => ClearForm();

            panel.Controls.Add(cmbSupplier);
            panel.Controls.Add(cmbProduct);
            panel.Controls.Add(txtSupplierProductCode);
            panel.Controls.Add(nudSupplierPrice);
            panel.Controls.Add(nudLeadTimeDays);
            panel.Controls.Add(cmbStatus);
            panel.Controls.Add(btnAdd);
            panel.Controls.Add(btnUpdate);
            panel.Controls.Add(btnDeactivate);
            panel.Controls.Add(btnClear);
        }

        private void LoadSuppliers()
        {
            try
            {
                List<Supplier> suppliers =
                    supplierService
                        .GetAllSuppliers()
                        .Where(supplier =>
                            supplier.SupplierStatus == "ACTIVE")
                        .ToList();

                cmbSupplier.DataSource = suppliers;
                cmbSupplier.DisplayMember = "SupplierName";
                cmbSupplier.ValueMember = "SupplierId";
                cmbSupplier.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load suppliers.\n\n" +
                    ex.Message
                );
            }
        }

        private void LoadProducts()
        {
            try
            {
                List<Product> products =
                    productService
                        .GetAllProducts()
                        .Where(product =>
                            product.ProductStatus == "ACTIVE")
                        .ToList();

                cmbProduct.DataSource = products;
                cmbProduct.DisplayMember = "ProductName";
                cmbProduct.ValueMember = "ProductId";
                cmbProduct.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load products.\n\n" +
                    ex.Message
                );
            }
        }

        private void LoadSupplies()
        {
            try
            {
                dgvSupplies.DataSource = null;
                dgvSupplies.DataSource =
                    supplyService.GetAllSupplies();

                dgvSupplies.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load supplies.\n\n" +
                    ex.Message
                );
            }
        }

        private void BtnAdd_Click(
            object? sender,
            EventArgs e)
        {
            Supply? supply = ReadSupplyFromForm();

            if (supply == null)
            {
                return;
            }

            OperationResult result =
                supplyService.CreateSupply(supply);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadSupplies();
                ClearForm();
            }
        }

        private void BtnUpdate_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedSupplyId <= 0)
            {
                ShowWarning(
                    "Please select a supply record."
                );

                return;
            }

            Supply? supply = ReadSupplyFromForm();

            if (supply == null)
            {
                return;
            }

            supply.SupplyId = selectedSupplyId;

            OperationResult result =
                supplyService.UpdateSupply(supply);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadSupplies();
                ClearForm();
            }
        }

        private void BtnDeactivate_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedSupplyId <= 0)
            {
                ShowWarning(
                    "Please select a supply record."
                );

                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    "Do you want to deactivate this supply record?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            OperationResult result =
                supplyService.DeactivateSupply(
                    selectedSupplyId
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadSupplies();
                ClearForm();
            }
        }

        private void DgvSupplies_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvSupplies.Rows[e.RowIndex]
                .DataBoundItem is not Supply supply)
            {
                return;
            }

            selectedSupplyId = supply.SupplyId;
            cmbSupplier.SelectedValue = supply.SupplierId;
            cmbProduct.SelectedValue = supply.ProductId;

            txtSupplierProductCode.Text =
                supply.SupplierProductCode;

            nudSupplierPrice.Value =
                supply.SupplierPrice;

            nudLeadTimeDays.Value =
                supply.LeadTimeDays;

            cmbStatus.Text =
                supply.SupplyStatus;
        }

        private Supply? ReadSupplyFromForm()
        {
            if (cmbSupplier.SelectedValue == null)
            {
                ShowWarning(
                    "Please select a supplier."
                );

                return null;
            }

            if (cmbProduct.SelectedValue == null)
            {
                ShowWarning(
                    "Please select a product."
                );

                return null;
            }

            return new Supply
            {
                SupplierId =
                    Convert.ToInt32(
                        cmbSupplier.SelectedValue
                    ),

                ProductId =
                    Convert.ToInt32(
                        cmbProduct.SelectedValue
                    ),

                SupplierProductCode =
                    txtSupplierProductCode.Text.Trim(),

                SupplierPrice =
                    nudSupplierPrice.Value,

                LeadTimeDays =
                    Convert.ToInt32(
                        nudLeadTimeDays.Value
                    ),

                SupplyStatus =
                    cmbStatus.Text
            };
        }

        private void ClearForm()
        {
            selectedSupplyId = 0;

            cmbSupplier.SelectedIndex = -1;
            cmbProduct.SelectedIndex = -1;
            txtSupplierProductCode.Clear();
            nudSupplierPrice.Value = 0;
            nudLeadTimeDays.Value = 0;
            cmbStatus.SelectedItem = "ACTIVE";

            dgvSupplies.ClearSelection();
        }

        private void CreateGridColumns()
        {
            AddColumn("SupplyId", "ID", 55);
            AddColumn("SupplierName", "Supplier", 200);
            AddColumn("ProductName", "Product", 200);
            AddColumn("Barcode", "Barcode", 120);

            AddColumn(
                "SupplierProductCode",
                "Supplier Product Code",
                160
            );

            AddColumn(
                "SupplierPrice",
                "Supplier Price",
                120,
                "N2"
            );

            AddColumn(
                "LeadTimeDays",
                "Lead Time",
                100
            );

            AddColumn(
                "SupplyStatus",
                "Status",
                100
            );
        }

        private void AddColumn(
            string propertyName,
            string header,
            int width,
            string? format = null)
        {
            DataGridViewTextBoxColumn column = new()
            {
                DataPropertyName = propertyName,
                HeaderText = header,
                Width = width
            };

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.DefaultCellStyle.Format = format;
            }

            dgvSupplies.Columns.Add(column);
        }

        private static void AddLabel(
            Control parent,
            string text,
            int x,
            int y)
        {
            parent.Controls.Add(
                new Label
                {
                    Text = text,
                    Location = new Point(x, y),
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9,
                        FontStyle.Bold
                    )
                }
            );
        }

        private static Button CreateButton(
            string text,
            int x,
            int y,
            Color colour)
        {
            Button button = new()
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(125, 38),
                BackColor = colour,
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