using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.InventoryManager
{
    public class StockManagementForm : Form
    {
        private readonly StockService stockService;
        private readonly ProductService productService;

        private DataGridView dgvStock = null!;
        private DataGridView dgvMovements = null!;

        private ComboBox cmbProduct = null!;
        private ComboBox cmbMovementType = null!;
        private NumericUpDown nudQuantity = null!;
        private TextBox txtReferenceType = null!;
        private TextBox txtNotes = null!;

        public StockManagementForm()
        {
            stockService = new StockService();
            productService = new ProductService();

            CreateInterface();
            LoadProducts();
            LoadStock();
            LoadMovements();
        }

        private void CreateInterface()
        {
            Text = "Stock Management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1250, 720);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Stock Management",
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
                    "View stock levels and record stock adjustments",
                Location = new Point(28, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray
            };

            Panel adjustmentPanel = new Panel
            {
                Location = new Point(25, 95),
                Size = new Size(1200, 140),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            CreateAdjustmentControls(adjustmentPanel);

            TabControl tabControl = new TabControl
            {
                Location = new Point(25, 255),
                Size = new Size(1200, 430),
                Font = new Font("Segoe UI", 10)
            };

            TabPage stockTab = new TabPage
            {
                Text = "Current Stock",
                BackColor = Color.White
            };

            TabPage historyTab = new TabPage
            {
                Text = "Movement History",
                BackColor = Color.White
            };

            dgvStock = CreateGrid();
            dgvStock.Dock = DockStyle.Fill;
            dgvStock.CellClick += DgvStock_CellClick;

            dgvMovements = CreateGrid();
            dgvMovements.Dock = DockStyle.Fill;

            CreateStockColumns();
            CreateMovementColumns();

            stockTab.Controls.Add(dgvStock);
            historyTab.Controls.Add(dgvMovements);

            tabControl.TabPages.Add(stockTab);
            tabControl.TabPages.Add(historyTab);

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(adjustmentPanel);
            Controls.Add(tabControl);
        }

        private void CreateAdjustmentControls(
            Panel panel)
        {
            AddLabel(panel, "Product", 20, 18);

            cmbProduct = new ComboBox
            {
                Location = new Point(20, 45),
                Size = new Size(250, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            AddLabel(panel, "Movement type", 290, 18);

            cmbMovementType = new ComboBox
            {
                Location = new Point(290, 45),
                Size = new Size(170, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbMovementType.Items.AddRange(
                new object[]
                {
                    "ADJUSTMENT_IN",
                    "ADJUSTMENT_OUT",
                    "RETURN_IN",
                    "RETURN_OUT",
                    "EXPIRED"
                }
            );

            AddLabel(panel, "Quantity", 480, 18);

            nudQuantity = new NumericUpDown
            {
                Location = new Point(480, 45),
                Size = new Size(130, 30),
                Minimum = 1,
                Maximum = 1000000,
                ThousandsSeparator = true
            };

            AddLabel(panel, "Reference", 630, 18);

            txtReferenceType = new TextBox
            {
                Location = new Point(630, 45),
                Size = new Size(160, 30),
                MaxLength = 40
            };

            AddLabel(panel, "Notes", 810, 18);

            txtNotes = new TextBox
            {
                Location = new Point(810, 45),
                Size = new Size(220, 30),
                MaxLength = 255
            };

            Button btnAdjust = CreateButton(
                "UPDATE STOCK",
                1045,
                42,
                Color.FromArgb(24, 90, 60)
            );

            Button btnClear = CreateButton(
                "CLEAR",
                1045,
                87,
                Color.DimGray
            );

            btnAdjust.Click += BtnAdjust_Click;
            btnClear.Click += (_, _) => ClearForm();

            panel.Controls.Add(cmbProduct);
            panel.Controls.Add(cmbMovementType);
            panel.Controls.Add(nudQuantity);
            panel.Controls.Add(txtReferenceType);
            panel.Controls.Add(txtNotes);
            panel.Controls.Add(btnAdjust);
            panel.Controls.Add(btnClear);
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

        private void LoadStock()
        {
            try
            {
                dgvStock.DataSource = null;
                dgvStock.DataSource =
                    stockService.GetAllStock();

                dgvStock.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load stock.\n\n" +
                    ex.Message
                );
            }
        }

        private void LoadMovements()
        {
            try
            {
                dgvMovements.DataSource = null;
                dgvMovements.DataSource =
                    stockService.GetStockMovements();

                dgvMovements.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load stock history.\n\n" +
                    ex.Message
                );
            }
        }

        private void BtnAdjust_Click(
            object? sender,
            EventArgs e)
        {
            if (cmbProduct.SelectedValue == null)
            {
                ShowWarning(
                    "Please select a product."
                );

                return;
            }

            if (cmbMovementType.SelectedIndex < 0)
            {
                ShowWarning(
                    "Please select a movement type."
                );

                return;
            }

            StockMovement movement = new StockMovement
            {
                ProductId =
                    Convert.ToInt32(cmbProduct.SelectedValue),

                MovementType =
                    cmbMovementType.Text,

                Quantity =
                    Convert.ToInt32(nudQuantity.Value),

                ReferenceType =
                    txtReferenceType.Text.Trim(),

                Notes =
                    txtNotes.Text.Trim(),

                BatchId = null,

                // Temporary authentication does not yet
                // provide the logged-in user ID.
                PerformedBy = null
            };

            DialogResult confirmation =
                MessageBox.Show(
                    "Do you want to update this stock quantity?",
                    "Confirm Stock Adjustment",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            OperationResult result =
                stockService.AdjustStock(movement);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadStock();
                LoadMovements();
                ClearForm();
            }
        }

        private void DgvStock_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvStock.Rows[e.RowIndex]
                .DataBoundItem is not StockItem stockItem)
            {
                return;
            }

            cmbProduct.SelectedValue =
                stockItem.ProductId;
        }

        private void ClearForm()
        {
            cmbProduct.SelectedIndex = -1;
            cmbMovementType.SelectedIndex = -1;
            nudQuantity.Value = 1;
            txtReferenceType.Clear();
            txtNotes.Clear();

            dgvStock.ClearSelection();
        }

        private void CreateStockColumns()
        {
            AddColumn(
                dgvStock,
                "Barcode",
                "Barcode",
                120
            );

            AddColumn(
                dgvStock,
                "ProductName",
                "Product",
                220
            );

            AddColumn(
                dgvStock,
                "CategoryName",
                "Category",
                160
            );

            AddColumn(
                dgvStock,
                "QuantityOnHand",
                "On Hand",
                100
            );

            AddColumn(
                dgvStock,
                "ReservedQuantity",
                "Reserved",
                100
            );

            AddColumn(
                dgvStock,
                "AvailableQuantity",
                "Available",
                100
            );

            AddColumn(
                dgvStock,
                "MinimumStock",
                "Minimum",
                100
            );

            AddColumn(
                dgvStock,
                "StockStatus",
                "Status",
                130
            );

            AddColumn(
                dgvStock,
                "LastUpdated",
                "Last Updated",
                150,
                "yyyy-MM-dd HH:mm"
            );
        }

        private void CreateMovementColumns()
        {
            AddColumn(
                dgvMovements,
                "MovementId",
                "ID",
                65
            );

            AddColumn(
                dgvMovements,
                "ProductName",
                "Product",
                200
            );

            AddColumn(
                dgvMovements,
                "BatchNumber",
                "Batch",
                110
            );

            AddColumn(
                dgvMovements,
                "MovementType",
                "Movement",
                135
            );

            AddColumn(
                dgvMovements,
                "Quantity",
                "Quantity",
                90
            );

            AddColumn(
                dgvMovements,
                "ReferenceType",
                "Reference",
                120
            );

            AddColumn(
                dgvMovements,
                "Notes",
                "Notes",
                220
            );

            AddColumn(
                dgvMovements,
                "PerformedByName",
                "Performed By",
                140
            );

            AddColumn(
                dgvMovements,
                "CreatedAt",
                "Date",
                150,
                "yyyy-MM-dd HH:mm"
            );
        }

        private static DataGridView CreateGrid()
        {
            DataGridView grid = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
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

            grid.ColumnHeadersDefaultCellStyle =
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

            grid.EnableHeadersVisualStyles = false;
            grid.RowTemplate.Height = 32;

            return grid;
        }

        private static void AddColumn(
            DataGridView grid,
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

            grid.Columns.Add(column);
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
                Size = new Size(130, 38),
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