using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.InventoryManager
{
    public class PurchaseOrderManagementForm : Form
    {
        private readonly PurchaseOrderService orderService;
        private readonly SupplierService supplierService;
        private readonly SupplyService supplyService;

        private readonly List<PurchaseOrderItem> currentItems =
            new();

        private DataGridView dgvOrders = null!;
        private DataGridView dgvItems = null!;

        private TextBox txtOrderNumber = null!;
        private ComboBox cmbSupplier = null!;
        private DateTimePicker dtpOrderDate = null!;
        private DateTimePicker dtpExpectedDate = null!;
        private NumericUpDown nudTaxAmount = null!;
        private Label lblSubtotal = null!;
        private Label lblTotal = null!;

        private ComboBox cmbProduct = null!;
        private NumericUpDown nudQuantity = null!;
        private NumericUpDown nudUnitCost = null!;
        private ComboBox cmbNewStatus = null!;

        private int selectedOrderId;
        private string selectedOrderStatus = string.Empty;

        public PurchaseOrderManagementForm()
        {
            orderService = new PurchaseOrderService();
            supplierService = new SupplierService();
            supplyService = new SupplyService();

            CreateInterface();
            LoadSuppliers();
            LoadOrders();
            ClearOrder();
        }

        private void CreateInterface()
        {
            Text = "Purchase Order Management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1350, 760);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Purchase Order Management",
                Location = new Point(20, 15),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    21,
                    FontStyle.Bold
                ),
                ForeColor = Color.FromArgb(24, 90, 60)
            };

            Panel orderPanel = new Panel
            {
                Location = new Point(20, 65),
                Size = new Size(1310, 125),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            CreateOrderControls(orderPanel);

            Panel itemPanel = new Panel
            {
                Location = new Point(20, 205),
                Size = new Size(1310, 115),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            CreateItemControls(itemPanel);

            SplitContainer splitContainer = new SplitContainer
            {
                Location = new Point(20, 335),
                Size = new Size(1310, 390),
                Orientation = Orientation.Vertical,
                SplitterDistance = 680
            };

            dgvOrders = CreateGrid();
            dgvOrders.Dock = DockStyle.Fill;
            dgvOrders.CellClick += DgvOrders_CellClick;

            dgvItems = CreateGrid();
            dgvItems.Dock = DockStyle.Fill;

            CreateOrderColumns();
            CreateItemColumns();

            splitContainer.Panel1.Controls.Add(dgvOrders);
            splitContainer.Panel2.Controls.Add(dgvItems);

            Controls.Add(title);
            Controls.Add(orderPanel);
            Controls.Add(itemPanel);
            Controls.Add(splitContainer);
        }

        private void CreateOrderControls(Panel panel)
        {
            AddLabel(panel, "Order number", 15, 12);

            txtOrderNumber = new TextBox
            {
                Location = new Point(15, 37),
                Size = new Size(180, 30),
                MaxLength = 40
            };

            AddLabel(panel, "Supplier", 210, 12);

            cmbSupplier = new ComboBox
            {
                Location = new Point(210, 37),
                Size = new Size(210, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbSupplier.SelectedIndexChanged +=
                CmbSupplier_SelectedIndexChanged;

            AddLabel(panel, "Order date", 435, 12);

            dtpOrderDate = new DateTimePicker
            {
                Location = new Point(435, 37),
                Size = new Size(140, 30),
                Format = DateTimePickerFormat.Short
            };

            AddLabel(panel, "Expected date", 590, 12);

            dtpExpectedDate = new DateTimePicker
            {
                Location = new Point(590, 37),
                Size = new Size(150, 30),
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true
            };

            AddLabel(panel, "Tax amount", 755, 12);

            nudTaxAmount = new NumericUpDown
            {
                Location = new Point(755, 37),
                Size = new Size(130, 30),
                DecimalPlaces = 2,
                Maximum = 10000000
            };

            nudTaxAmount.ValueChanged +=
                (_, _) => UpdateTotals();

            lblSubtotal = new Label
            {
                Location = new Point(15, 82),
                Size = new Size(220, 25),
                Font = new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold
                )
            };

            lblTotal = new Label
            {
                Location = new Point(245, 82),
                Size = new Size(220, 25),
                Font = new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold
                ),
                ForeColor = Color.FromArgb(24, 90, 60)
            };

            Button btnSave = CreateButton(
                "SAVE DRAFT",
                900,
                28,
                Color.FromArgb(24, 90, 60)
            );

            Button btnUpdate = CreateButton(
                "UPDATE",
                1030,
                28,
                Color.FromArgb(30, 100, 180)
            );

            Button btnClear = CreateButton(
                "NEW",
                1160,
                28,
                Color.DimGray
            );

            btnSave.Click += BtnSave_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnClear.Click += (_, _) => ClearOrder();

            cmbNewStatus = new ComboBox
            {
                Location = new Point(900, 78),
                Size = new Size(170, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbNewStatus.Items.AddRange(
                new object[]
                {
                    "SENT",
                    "CONFIRMED",
                    "DECLINED",
                    "PROCESSING",
                    "PARTIALLY_DELIVERED",
                    "DELIVERED",
                    "CANCELLED"
                }
            );

            Button btnStatus = CreateButton(
                "CHANGE STATUS",
                1085,
                75,
                Color.FromArgb(140, 90, 180)
            );

            btnStatus.Click += BtnStatus_Click;

            panel.Controls.Add(txtOrderNumber);
            panel.Controls.Add(cmbSupplier);
            panel.Controls.Add(dtpOrderDate);
            panel.Controls.Add(dtpExpectedDate);
            panel.Controls.Add(nudTaxAmount);
            panel.Controls.Add(lblSubtotal);
            panel.Controls.Add(lblTotal);
            panel.Controls.Add(btnSave);
            panel.Controls.Add(btnUpdate);
            panel.Controls.Add(btnClear);
            panel.Controls.Add(cmbNewStatus);
            panel.Controls.Add(btnStatus);
        }

        private void CreateItemControls(Panel panel)
        {
            AddLabel(panel, "Supplier product", 15, 12);

            cmbProduct = new ComboBox
            {
                Location = new Point(15, 37),
                Size = new Size(260, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbProduct.SelectedIndexChanged +=
                CmbProduct_SelectedIndexChanged;

            AddLabel(panel, "Quantity", 290, 12);

            nudQuantity = new NumericUpDown
            {
                Location = new Point(290, 37),
                Size = new Size(120, 30),
                Minimum = 1,
                Maximum = 1000000
            };

            AddLabel(panel, "Unit cost", 425, 12);

            nudUnitCost = new NumericUpDown
            {
                Location = new Point(425, 37),
                Size = new Size(140, 30),
                DecimalPlaces = 2,
                Maximum = 10000000
            };

            Button btnAddItem = CreateButton(
                "ADD ITEM",
                590,
                33,
                Color.FromArgb(24, 90, 60)
            );

            Button btnRemoveItem = CreateButton(
                "REMOVE ITEM",
                720,
                33,
                Color.FromArgb(180, 50, 50)
            );

            btnAddItem.Click += BtnAddItem_Click;
            btnRemoveItem.Click += BtnRemoveItem_Click;

            Label information = new Label
            {
                Text =
                    "Select an order item from the right grid before removing it.",
                Location = new Point(590, 78),
                AutoSize = true,
                ForeColor = Color.DimGray
            };

            panel.Controls.Add(cmbProduct);
            panel.Controls.Add(nudQuantity);
            panel.Controls.Add(nudUnitCost);
            panel.Controls.Add(btnAddItem);
            panel.Controls.Add(btnRemoveItem);
            panel.Controls.Add(information);
        }

        private void LoadSuppliers()
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

        private void CmbSupplier_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            cmbProduct.DataSource = null;

            if (cmbSupplier.SelectedValue == null)
            {
                return;
            }

            if (cmbSupplier.SelectedValue is not int supplierId)
            {
                return;
            }

            List<Supply> supplies =
                supplyService
                    .GetAllSupplies()
                    .Where(supply =>
                        supply.SupplierId == supplierId &&
                        supply.SupplyStatus == "ACTIVE")
                    .ToList();

            cmbProduct.DataSource = supplies;
            cmbProduct.DisplayMember = "ProductName";
            cmbProduct.ValueMember = "ProductId";
            cmbProduct.SelectedIndex = -1;
        }

        private void CmbProduct_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            if (cmbProduct.SelectedItem is Supply supply)
            {
                nudUnitCost.Value =
                    supply.SupplierPrice;
            }
        }

        private void BtnAddItem_Click(
            object? sender,
            EventArgs e)
        {
            if (cmbProduct.SelectedItem is not Supply supply)
            {
                ShowWarning("Please select a product.");
                return;
            }

            if (currentItems.Any(item =>
                item.ProductId == supply.ProductId))
            {
                ShowWarning(
                    "This product is already in the order."
                );

                return;
            }

            currentItems.Add(
                new PurchaseOrderItem
                {
                    ProductId = supply.ProductId,
                    ProductName = supply.ProductName,
                    Barcode = supply.Barcode,
                    OrderedQuantity =
                        Convert.ToInt32(nudQuantity.Value),
                    ReceivedQuantity = 0,
                    UnitCost = nudUnitCost.Value
                }
            );

            RefreshItemsGrid();
        }

        private void BtnRemoveItem_Click(
            object? sender,
            EventArgs e)
        {
            if (dgvItems.CurrentRow?.DataBoundItem
                is not PurchaseOrderItem item)
            {
                ShowWarning(
                    "Please select an order item."
                );

                return;
            }

            currentItems.Remove(item);
            RefreshItemsGrid();
        }

        private void BtnSave_Click(
            object? sender,
            EventArgs e)
        {
            PurchaseOrder? order = ReadOrder();

            if (order == null)
            {
                return;
            }

            OperationResult result =
                orderService.CreateOrder(order);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadOrders();
                ClearOrder();
            }
        }

        private void BtnUpdate_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedOrderId <= 0)
            {
                ShowWarning(
                    "Please select a purchase order."
                );

                return;
            }

            PurchaseOrder? order = ReadOrder();

            if (order == null)
            {
                return;
            }

            order.PurchaseOrderId = selectedOrderId;

            OperationResult result =
                orderService.UpdateDraftOrder(order);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadOrders();
                ClearOrder();
            }
        }

        private void BtnStatus_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedOrderId <= 0 ||
                cmbNewStatus.SelectedIndex < 0)
            {
                ShowWarning(
                    "Select an order and a new status."
                );

                return;
            }

            OperationResult result =
                orderService.ChangeOrderStatus(
                    selectedOrderId,
                    cmbNewStatus.Text
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadOrders();
                ClearOrder();
            }
        }

        private PurchaseOrder? ReadOrder()
        {
            if (cmbSupplier.SelectedValue is not int supplierId)
            {
                ShowWarning("Please select a supplier.");
                return null;
            }

            return new PurchaseOrder
            {
                OrderNumber = txtOrderNumber.Text.Trim(),
                SupplierId = supplierId,
                OrderDate = dtpOrderDate.Value.Date,
                ExpectedDeliveryDate =
                    dtpExpectedDate.Checked
                        ? dtpExpectedDate.Value.Date
                        : null,
                TaxAmount = nudTaxAmount.Value,
                CreatedBy = null,
                Items = currentItems.ToList()
            };
        }

        private void LoadOrders()
        {
            try
            {
                dgvOrders.DataSource = null;
                dgvOrders.DataSource =
                    orderService.GetAllOrders();

                dgvOrders.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load purchase orders.\n\n" +
                    ex.Message
                );
            }
        }

        private void DgvOrders_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                dgvOrders.Rows[e.RowIndex].DataBoundItem
                is not PurchaseOrder order)
            {
                return;
            }

            selectedOrderId = order.PurchaseOrderId;
            selectedOrderStatus = order.OrderStatus;

            txtOrderNumber.Text = order.OrderNumber;
            cmbSupplier.SelectedValue = order.SupplierId;
            dtpOrderDate.Value = order.OrderDate;

            if (order.ExpectedDeliveryDate.HasValue)
            {
                dtpExpectedDate.Value =
                    order.ExpectedDeliveryDate.Value;

                dtpExpectedDate.Checked = true;
            }
            else
            {
                dtpExpectedDate.Checked = false;
            }

            nudTaxAmount.Value = order.TaxAmount;

            currentItems.Clear();
            currentItems.AddRange(
                orderService.GetOrderItems(
                    order.PurchaseOrderId
                )
            );

            RefreshItemsGrid();
        }

        private void RefreshItemsGrid()
        {
            dgvItems.DataSource = null;
            dgvItems.DataSource = currentItems.ToList();
            dgvItems.ClearSelection();

            UpdateTotals();
        }

        private void UpdateTotals()
        {
            decimal subtotal =
                currentItems.Sum(item => item.LineTotal);

            decimal total =
                subtotal + nudTaxAmount.Value;

            lblSubtotal.Text =
                $"Subtotal: {subtotal:N2}";

            lblTotal.Text =
                $"Total: {total:N2}";
        }

        private void ClearOrder()
        {
            selectedOrderId = 0;
            selectedOrderStatus = string.Empty;

            txtOrderNumber.Text =
                orderService.GenerateOrderNumber();

            cmbSupplier.SelectedIndex = -1;
            cmbProduct.DataSource = null;
            dtpOrderDate.Value = DateTime.Today;
            dtpExpectedDate.Value = DateTime.Today;
            dtpExpectedDate.Checked = false;
            nudTaxAmount.Value = 0;
            nudQuantity.Value = 1;
            nudUnitCost.Value = 0;
            cmbNewStatus.SelectedIndex = -1;

            currentItems.Clear();
            RefreshItemsGrid();
            dgvOrders.ClearSelection();
        }

        private void CreateOrderColumns()
        {
            AddColumn(
                dgvOrders,
                "OrderNumber",
                "Order Number",
                135
            );

            AddColumn(
                dgvOrders,
                "SupplierName",
                "Supplier",
                170
            );

            AddColumn(
                dgvOrders,
                "OrderDate",
                "Order Date",
                100,
                "yyyy-MM-dd"
            );

            AddColumn(
                dgvOrders,
                "OrderStatus",
                "Status",
                125
            );

            AddColumn(
                dgvOrders,
                "TotalAmount",
                "Total",
                110,
                "N2"
            );
        }

        private void CreateItemColumns()
        {
            AddColumn(
                dgvItems,
                "ProductName",
                "Product",
                180
            );

            AddColumn(
                dgvItems,
                "Barcode",
                "Barcode",
                100
            );

            AddColumn(
                dgvItems,
                "OrderedQuantity",
                "Quantity",
                85
            );

            AddColumn(
                dgvItems,
                "UnitCost",
                "Unit Cost",
                100,
                "N2"
            );

            AddColumn(
                dgvItems,
                "LineTotal",
                "Line Total",
                110,
                "N2"
            );
        }

        private static DataGridView CreateGrid()
        {
            DataGridView grid = new DataGridView
            {
                BackgroundColor = Color.White,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                MultiSelect = false,
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
                    )
                };

            grid.EnableHeadersVisualStyles = false;
            grid.RowTemplate.Height = 30;

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
                Size = new Size(120, 36),
                BackColor = colour,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(
                    "Segoe UI",
                    8,
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