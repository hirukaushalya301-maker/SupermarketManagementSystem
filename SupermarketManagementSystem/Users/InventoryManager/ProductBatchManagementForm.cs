using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.InventoryManager
{
    public class ProductBatchManagementForm : Form
    {
        private readonly ProductBatchService batchService;
        private readonly ProductService productService;

        private DataGridView dgvBatches = null!;
        private ComboBox cmbProduct = null!;
        private TextBox txtBatchNumber = null!;
        private DateTimePicker dtpManufactured = null!;
        private DateTimePicker dtpExpiry = null!;
        private NumericUpDown nudReceivedQuantity = null!;
        private NumericUpDown nudAvailableQuantity = null!;
        private NumericUpDown nudCostPrice = null!;
        private ComboBox cmbStatus = null!;

        private int selectedBatchId;

        public ProductBatchManagementForm()
        {
            batchService = new ProductBatchService();
            productService = new ProductService();

            CreateInterface();
            LoadProducts();
            LoadBatches();
            ClearForm();
        }

        private void CreateInterface()
        {
            Text = "Product Batch Management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1250, 700);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Product Batch Management",
                Location = new Point(25, 20),
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
                    "Manage received quantities, expiry dates and batch availability",
                Location = new Point(28, 62),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray
            };

            Panel inputPanel = new Panel
            {
                Location = new Point(25, 100),
                Size = new Size(1200, 220),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            CreateInputControls(inputPanel);

            dgvBatches = new DataGridView
            {
                Location = new Point(25, 340),
                Size = new Size(1200, 330),
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

            dgvBatches.ColumnHeadersDefaultCellStyle =
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

            dgvBatches.EnableHeadersVisualStyles = false;
            dgvBatches.RowTemplate.Height = 32;
            dgvBatches.CellClick += DgvBatches_CellClick;

            CreateGridColumns();

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(inputPanel);
            Controls.Add(dgvBatches);
        }

        private void CreateInputControls(Panel panel)
        {
            AddLabel(panel, "Product", 20, 20);

            cmbProduct = new ComboBox
            {
                Location = new Point(20, 45),
                Size = new Size(260, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            AddLabel(panel, "Batch number", 300, 20);

            txtBatchNumber = new TextBox
            {
                Location = new Point(300, 45),
                Size = new Size(180, 30),
                MaxLength = 60
            };

            AddLabel(panel, "Manufactured date", 500, 20);

            dtpManufactured = CreateOptionalDatePicker(
                500,
                45
            );

            AddLabel(panel, "Expiry date", 700, 20);

            dtpExpiry = CreateOptionalDatePicker(
                700,
                45
            );

            AddLabel(panel, "Status", 900, 20);

            cmbStatus = new ComboBox
            {
                Location = new Point(900, 45),
                Size = new Size(160, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbStatus.Items.AddRange(
                new object[]
                {
                    "ACTIVE",
                    "EXPIRED",
                    "DELETED",
                    "BLOCKED"
                }
            );

            AddLabel(panel, "Received quantity", 20, 90);

            nudReceivedQuantity = CreateQuantityInput(
                20,
                115
            );

            AddLabel(panel, "Available quantity", 200, 90);

            nudAvailableQuantity = CreateQuantityInput(
                200,
                115
            );

            AddLabel(panel, "Batch cost price", 380, 90);

            nudCostPrice = new NumericUpDown
            {
                Location = new Point(380, 115),
                Size = new Size(160, 30),
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 10000000,
                ThousandsSeparator = true
            };

            Button btnAdd = CreateButton(
                "ADD",
                590,
                105,
                Color.FromArgb(24, 90, 60)
            );

            Button btnUpdate = CreateButton(
                "UPDATE",
                730,
                105,
                Color.FromArgb(30, 100, 180)
            );

            Button btnBlock = CreateButton(
                "BLOCK",
                870,
                105,
                Color.FromArgb(180, 50, 50)
            );

            Button btnClear = CreateButton(
                "CLEAR",
                1010,
                105,
                Color.DimGray
            );

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnBlock.Click += BtnBlock_Click;
            btnClear.Click += (_, _) => ClearForm();

            panel.Controls.Add(cmbProduct);
            panel.Controls.Add(txtBatchNumber);
            panel.Controls.Add(dtpManufactured);
            panel.Controls.Add(dtpExpiry);
            panel.Controls.Add(cmbStatus);
            panel.Controls.Add(nudReceivedQuantity);
            panel.Controls.Add(nudAvailableQuantity);
            panel.Controls.Add(nudCostPrice);
            panel.Controls.Add(btnAdd);
            panel.Controls.Add(btnUpdate);
            panel.Controls.Add(btnBlock);
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

        private void LoadBatches()
        {
            try
            {
                dgvBatches.DataSource = null;
                dgvBatches.DataSource =
                    batchService.GetAllBatches();

                dgvBatches.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load product batches.\n\n" +
                    ex.Message
                );
            }
        }

        private void BtnAdd_Click(
            object? sender,
            EventArgs e)
        {
            ProductBatch? batch =
                ReadBatchFromForm();

            if (batch == null)
            {
                return;
            }

            OperationResult result =
                batchService.CreateBatch(batch);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadBatches();
                ClearForm();
            }
        }

        private void BtnUpdate_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedBatchId <= 0)
            {
                ShowWarning(
                    "Please select a product batch."
                );

                return;
            }

            ProductBatch? batch =
                ReadBatchFromForm();

            if (batch == null)
            {
                return;
            }

            batch.BatchId = selectedBatchId;

            OperationResult result =
                batchService.UpdateBatch(batch);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadBatches();
                ClearForm();
            }
        }

        private void BtnBlock_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedBatchId <= 0)
            {
                ShowWarning(
                    "Please select a product batch."
                );

                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    "Do you want to block this product batch?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            OperationResult result =
                batchService.BlockBatch(selectedBatchId);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadBatches();
                ClearForm();
            }
        }

        private void DgvBatches_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvBatches.Rows[e.RowIndex]
                .DataBoundItem is not ProductBatch batch)
            {
                return;
            }

            selectedBatchId = batch.BatchId;

            cmbProduct.SelectedValue = batch.ProductId;
            txtBatchNumber.Text = batch.BatchNumber;

            SetDatePicker(
                dtpManufactured,
                batch.ManufacturedDate
            );

            SetDatePicker(
                dtpExpiry,
                batch.ExpiryDate
            );

            nudReceivedQuantity.Value =
                batch.ReceivedQuantity;

            nudAvailableQuantity.Value =
                batch.AvailableQuantity;

            nudAvailableQuantity.Enabled = true;

            nudCostPrice.Value = batch.CostPrice;
            cmbStatus.Text = batch.BatchStatus;
        }

        private ProductBatch? ReadBatchFromForm()
        {
            if (cmbProduct.SelectedValue == null)
            {
                ShowWarning(
                    "Please select a product."
                );

                return null;
            }

            return new ProductBatch
            {
                ProductId =
                    Convert.ToInt32(cmbProduct.SelectedValue),

                BatchNumber =
                    txtBatchNumber.Text.Trim(),

                ManufacturedDate =
                    dtpManufactured.Checked
                        ? dtpManufactured.Value.Date
                        : null,

                ExpiryDate =
                    dtpExpiry.Checked
                        ? dtpExpiry.Value.Date
                        : null,

                ReceivedQuantity =
                    Convert.ToInt32(
                        nudReceivedQuantity.Value
                    ),

                AvailableQuantity =
                    Convert.ToInt32(
                        nudAvailableQuantity.Value
                    ),

                CostPrice =
                    nudCostPrice.Value,

                BatchStatus =
                    cmbStatus.Text
            };
        }

        private void ClearForm()
        {
            selectedBatchId = 0;

            cmbProduct.SelectedIndex = -1;
            txtBatchNumber.Clear();

            dtpManufactured.Checked = false;
            dtpExpiry.Checked = false;

            nudReceivedQuantity.Value = 0;
            nudAvailableQuantity.Value = 0;
            nudAvailableQuantity.Enabled = false;
            nudCostPrice.Value = 0;

            cmbStatus.SelectedItem = "ACTIVE";

            dgvBatches.ClearSelection();
            cmbProduct.Focus();
        }

        private void CreateGridColumns()
        {
            AddColumn("BatchId", "ID", 50);
            AddColumn("ProductName", "Product", 180);
            AddColumn("Barcode", "Barcode", 110);
            AddColumn("BatchNumber", "Batch Number", 120);

            AddColumn(
                "ManufacturedDate",
                "Manufactured",
                110,
                "yyyy-MM-dd"
            );

            AddColumn(
                "ExpiryDate",
                "Expiry",
                110,
                "yyyy-MM-dd"
            );

            AddColumn(
                "ReceivedQuantity",
                "Received",
                90
            );

            AddColumn(
                "AvailableQuantity",
                "Available",
                90
            );

            AddColumn(
                "CostPrice",
                "Cost Price",
                100,
                "N2"
            );

            AddColumn(
                "BatchStatus",
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

            dgvBatches.Columns.Add(column);
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

        private static DateTimePicker CreateOptionalDatePicker(
            int x,
            int y)
        {
            return new DateTimePicker
            {
                Location = new Point(x, y),
                Size = new Size(180, 30),
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };
        }

        private static NumericUpDown CreateQuantityInput(
            int x,
            int y)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(150, 30),
                Minimum = 0,
                Maximum = 1000000,
                ThousandsSeparator = true
            };
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
                Size = new Size(125, 40),
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

        private static void SetDatePicker(
            DateTimePicker picker,
            DateTime? value)
        {
            if (value.HasValue)
            {
                picker.Value = value.Value;
                picker.Checked = true;
            }
            else
            {
                picker.Value = DateTime.Today;
                picker.Checked = false;
            }
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