using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.InventoryManager
{
    public class GoodsReceiptForm : Form
    {
        private readonly GoodsReceiptService receiptService;
        private readonly DeliveryService deliveryService;

        private ComboBox cmbDelivery = null!;
        private DataGridView dgvItems = null!;

        private Label lblSelectedProduct = null!;
        private Label lblRemainingQuantity = null!;
        private NumericUpDown nudReceivingQuantity = null!;
        private TextBox txtBatchNumber = null!;
        private DateTimePicker dtpManufactured = null!;
        private DateTimePicker dtpExpiry = null!;

        private readonly List<DeliveryReceiptItem>
            receiptItems = new();

        private DeliveryReceiptItem? selectedItem;

        public GoodsReceiptForm()
        {
            receiptService = new GoodsReceiptService();
            deliveryService = new DeliveryService();

            CreateInterface();
            LoadDeliveries();
        }

        private void CreateInterface()
        {
            Text = "Receive Delivery";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1200, 680);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Receive Delivery",
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
                    "Receive ordered products and update inventory stock",
                Location = new Point(28, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray
            };

            Panel deliveryPanel = new Panel
            {
                Location = new Point(25, 95),
                Size = new Size(1150, 75),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            AddLabel(
                deliveryPanel,
                "Select delivery",
                20,
                10
            );

            cmbDelivery = new ComboBox
            {
                Location = new Point(20, 35),
                Size = new Size(350, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbDelivery.SelectedIndexChanged +=
                CmbDelivery_SelectedIndexChanged;

            Button btnRefresh = CreateButton(
                "REFRESH",
                390,
                29,
                Color.DimGray
            );

            btnRefresh.Click += (_, _) =>
                LoadDeliveries();

            deliveryPanel.Controls.Add(cmbDelivery);
            deliveryPanel.Controls.Add(btnRefresh);

            Panel itemPanel = new Panel
            {
                Location = new Point(25, 185),
                Size = new Size(1150, 145),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            CreateReceiptItemControls(itemPanel);

            dgvItems = new DataGridView
            {
                Location = new Point(25, 350),
                Size = new Size(1150, 270),
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

            dgvItems.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(24, 90, 60),
                    ForeColor = Color.White,
                    Font = new Font(
                        "Segoe UI",
                        9,
                        FontStyle.Bold
                    )
                };

            dgvItems.EnableHeadersVisualStyles = false;
            dgvItems.RowTemplate.Height = 32;
            dgvItems.CellClick += DgvItems_CellClick;

            CreateGridColumns();

            Button btnReceive = CreateButton(
                "RECEIVE DELIVERY",
                995,
                630,
                Color.FromArgb(24, 90, 60)
            );

            btnReceive.Size = new Size(180, 38);
            btnReceive.Click += BtnReceive_Click;

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(deliveryPanel);
            Controls.Add(itemPanel);
            Controls.Add(dgvItems);
            Controls.Add(btnReceive);
        }

        private void CreateReceiptItemControls(
            Panel panel)
        {
            AddLabel(panel, "Selected product", 20, 12);

            lblSelectedProduct = new Label
            {
                Text = "No product selected",
                Location = new Point(20, 38),
                Size = new Size(230, 25),
                ForeColor = Color.FromArgb(24, 90, 60),
                Font = new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold
                )
            };

            AddLabel(panel, "Remaining", 270, 12);

            lblRemainingQuantity = new Label
            {
                Text = "0",
                Location = new Point(270, 38),
                Size = new Size(100, 25),
                Font = new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold
                )
            };

            AddLabel(panel, "Receive quantity", 380, 12);

            nudReceivingQuantity = new NumericUpDown
            {
                Location = new Point(380, 37),
                Size = new Size(130, 30),
                Minimum = 0,
                Maximum = 1000000
            };

            AddLabel(panel, "Batch number", 530, 12);

            txtBatchNumber = new TextBox
            {
                Location = new Point(530, 37),
                Size = new Size(180, 30),
                MaxLength = 60
            };

            AddLabel(panel, "Manufactured", 730, 12);

            dtpManufactured =
                CreateOptionalDatePicker(730, 37);

            AddLabel(panel, "Expiry", 900, 12);

            dtpExpiry =
                CreateOptionalDatePicker(900, 37);

            Button btnApply = CreateButton(
                "APPLY ITEM",
                20,
                88,
                Color.FromArgb(30, 100, 180)
            );

            Button btnSkip = CreateButton(
                "SKIP ITEM",
                160,
                88,
                Color.DimGray
            );

            btnApply.Click += BtnApplyItem_Click;
            btnSkip.Click += BtnSkipItem_Click;

            panel.Controls.Add(lblSelectedProduct);
            panel.Controls.Add(lblRemainingQuantity);
            panel.Controls.Add(nudReceivingQuantity);
            panel.Controls.Add(txtBatchNumber);
            panel.Controls.Add(dtpManufactured);
            panel.Controls.Add(dtpExpiry);
            panel.Controls.Add(btnApply);
            panel.Controls.Add(btnSkip);
        }

        private void LoadDeliveries()
        {
            try
            {
                List<Delivery> deliveries =
                    deliveryService
                        .GetAllDeliveries()
                        .Where(delivery =>
                            delivery.DeliveryStatus !=
                                "DELIVERED" &&
                            delivery.DeliveryStatus !=
                                "CANCELLED" &&
                            delivery.DeliveryStatus !=
                                "REJECTED")
                        .ToList();

                cmbDelivery.DataSource = null;
                cmbDelivery.DataSource = deliveries;
                cmbDelivery.DisplayMember =
                    "DeliveryReference";
                cmbDelivery.ValueMember = "DeliveryId";
                cmbDelivery.SelectedIndex = -1;

                receiptItems.Clear();
                RefreshGrid();
                ClearSelectedItem();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load deliveries.\n\n" +
                    ex.Message
                );
            }
        }

        private void CmbDelivery_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            if (cmbDelivery.SelectedItem
                is not Delivery delivery)
            {
                return;
            }

            try
            {
                List<DeliveryReceiptItem> loadedItems =
                    receiptService.GetReceiptItems(
                        delivery.PurchaseOrderId
                    );

                receiptItems.Clear();

                foreach (DeliveryReceiptItem item
                    in loadedItems)
                {
                    if (item.RemainingBeforeReceipt <= 0)
                    {
                        continue;
                    }

                    item.ReceivingQuantity =
                        item.RemainingBeforeReceipt;

                    item.BatchNumber =
                        CreateDefaultBatchNumber(
                            delivery.DeliveryReference,
                            item.ProductId
                        );

                    receiptItems.Add(item);
                }

                RefreshGrid();
                ClearSelectedItem();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load delivery items.\n\n" +
                    ex.Message
                );
            }
        }

        private void DgvItems_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                dgvItems.Rows[e.RowIndex].DataBoundItem
                is not DeliveryReceiptItem item)
            {
                return;
            }

            selectedItem = item;

            lblSelectedProduct.Text =
                item.ProductName;

            lblRemainingQuantity.Text =
                item.RemainingBeforeReceipt.ToString();

            nudReceivingQuantity.Maximum =
                item.RemainingBeforeReceipt;

            nudReceivingQuantity.Value =
                item.ReceivingQuantity;

            txtBatchNumber.Text =
                item.BatchNumber;

            SetDatePicker(
                dtpManufactured,
                item.ManufacturedDate
            );

            SetDatePicker(
                dtpExpiry,
                item.ExpiryDate
            );
        }

        private void BtnApplyItem_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedItem == null)
            {
                ShowWarning(
                    "Please select a product from the grid."
                );

                return;
            }

            selectedItem.ReceivingQuantity =
                Convert.ToInt32(
                    nudReceivingQuantity.Value
                );

            selectedItem.BatchNumber =
                txtBatchNumber.Text.Trim();

            selectedItem.ManufacturedDate =
                dtpManufactured.Checked
                    ? dtpManufactured.Value.Date
                    : null;

            selectedItem.ExpiryDate =
                dtpExpiry.Checked
                    ? dtpExpiry.Value.Date
                    : null;

            RefreshGrid();
            ClearSelectedItem();
        }

        private void BtnSkipItem_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedItem == null)
            {
                ShowWarning(
                    "Please select a product from the grid."
                );

                return;
            }

            selectedItem.ReceivingQuantity = 0;
            selectedItem.BatchNumber = string.Empty;
            selectedItem.ManufacturedDate = null;
            selectedItem.ExpiryDate = null;

            RefreshGrid();
            ClearSelectedItem();
        }

        private void BtnReceive_Click(
            object? sender,
            EventArgs e)
        {
            if (cmbDelivery.SelectedItem
                is not Delivery delivery)
            {
                ShowWarning(
                    "Please select a delivery."
                );

                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    "Receive the entered quantities and " +
                    "update stock?",
                    "Confirm Goods Receipt",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            OperationResult result =
                receiptService.ReceiveDelivery(
                    delivery.DeliveryId,
                    delivery.PurchaseOrderId,
                    receiptItems,
                    null
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadDeliveries();
            }
        }

        private void RefreshGrid()
        {
            dgvItems.DataSource = null;
            dgvItems.DataSource =
                receiptItems.ToList();

            dgvItems.ClearSelection();
        }

        private void ClearSelectedItem()
        {
            selectedItem = null;

            lblSelectedProduct.Text =
                "No product selected";

            lblRemainingQuantity.Text = "0";
            nudReceivingQuantity.Maximum = 1000000;
            nudReceivingQuantity.Value = 0;
            txtBatchNumber.Clear();
            dtpManufactured.Checked = false;
            dtpExpiry.Checked = false;
        }

        private void CreateGridColumns()
        {
            AddColumn("ProductName", "Product", 190);
            AddColumn("Barcode", "Barcode", 110);

            AddColumn(
                "OrderedQuantity",
                "Ordered",
                90
            );

            AddColumn(
                "PreviouslyReceivedQuantity",
                "Previously Received",
                135
            );

            AddColumn(
                "RemainingBeforeReceipt",
                "Remaining",
                100
            );

            AddColumn(
                "ReceivingQuantity",
                "Receiving",
                95
            );

            AddColumn(
                "BatchNumber",
                "Batch Number",
                145
            );

            AddColumn(
                "ExpiryDate",
                "Expiry",
                110,
                "yyyy-MM-dd"
            );

            AddColumn(
                "UnitCost",
                "Unit Cost",
                100,
                "N2"
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

            dgvItems.Columns.Add(column);
        }

        private static string CreateDefaultBatchNumber(
            string deliveryReference,
            int productId)
        {
            string batchNumber =
                deliveryReference + "-P" + productId;

            return batchNumber.Length <= 60
                ? batchNumber
                : batchNumber[..60];
        }

        private static DateTimePicker
            CreateOptionalDatePicker(int x, int y)
        {
            return new DateTimePicker
            {
                Location = new Point(x, y),
                Size = new Size(150, 30),
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };
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
                )
            };

            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static void ShowResult(
            OperationResult result)
        {
            MessageBox.Show(
                result.Message,
                result.IsSuccessful ? "Success" : "Error",
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