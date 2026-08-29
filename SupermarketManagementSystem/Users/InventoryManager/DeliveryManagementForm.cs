using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.InventoryManager
{
    public class DeliveryManagementForm : Form
    {
        private readonly DeliveryService deliveryService;
        private readonly PurchaseOrderService orderService;

        private DataGridView dgvDeliveries = null!;
        private ComboBox cmbPurchaseOrder = null!;
        private TextBox txtDeliveryReference = null!;
        private DateTimePicker dtpDeliveryDate = null!;
        private ComboBox cmbStatus = null!;
        private TextBox txtNotes = null!;

        private int selectedDeliveryId;
        private int? selectedReceivedBy;

        public DeliveryManagementForm()
        {
            deliveryService = new DeliveryService();
            orderService = new PurchaseOrderService();

            CreateInterface();
            LoadPurchaseOrders();
            LoadDeliveries();
            ClearForm();
        }

        private void CreateInterface()
        {
            Text = "Delivery Management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1200, 680);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Delivery Management",
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
                    "Schedule and track purchase-order deliveries",
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

            dgvDeliveries = new DataGridView
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

            dgvDeliveries.ColumnHeadersDefaultCellStyle =
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

            dgvDeliveries.EnableHeadersVisualStyles = false;
            dgvDeliveries.RowTemplate.Height = 32;
            dgvDeliveries.CellClick +=
                DgvDeliveries_CellClick;

            CreateGridColumns();

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(inputPanel);
            Controls.Add(dgvDeliveries);
        }

        private void CreateInputControls(Panel panel)
        {
            AddLabel(panel, "Purchase order", 20, 18);

            cmbPurchaseOrder = new ComboBox
            {
                Location = new Point(20, 43),
                Size = new Size(260, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            AddLabel(panel, "Delivery reference", 300, 18);

            txtDeliveryReference = new TextBox
            {
                Location = new Point(300, 43),
                Size = new Size(200, 30),
                MaxLength = 60
            };

            AddLabel(panel, "Delivery date", 520, 18);

            dtpDeliveryDate = new DateTimePicker
            {
                Location = new Point(520, 43),
                Size = new Size(160, 30),
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };

            AddLabel(panel, "Status", 700, 18);

            cmbStatus = new ComboBox
            {
                Location = new Point(700, 43),
                Size = new Size(170, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbStatus.Items.AddRange(
                new object[]
                {
                    "SCHEDULED",
                    "DISPATCHED",
                    "IN_TRANSIT",
                    "REJECTED",
                    "CANCELLED"
                }
            );

            AddLabel(panel, "Notes", 20, 88);

            txtNotes = new TextBox
            {
                Location = new Point(20, 113),
                Size = new Size(600, 30),
                MaxLength = 500
            };

            Button btnAdd = CreateButton(
                "ADD",
                650,
                105,
                Color.FromArgb(24, 90, 60)
            );

            Button btnUpdate = CreateButton(
                "UPDATE",
                790,
                105,
                Color.FromArgb(30, 100, 180)
            );

            Button btnCancel = CreateButton(
                "CANCEL",
                930,
                105,
                Color.FromArgb(180, 50, 50)
            );

            Button btnClear = CreateButton(
                "CLEAR",
                930,
                45,
                Color.DimGray
            );

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnCancel.Click += BtnCancel_Click;
            btnClear.Click += (_, _) => ClearForm();

            panel.Controls.Add(cmbPurchaseOrder);
            panel.Controls.Add(txtDeliveryReference);
            panel.Controls.Add(dtpDeliveryDate);
            panel.Controls.Add(cmbStatus);
            panel.Controls.Add(txtNotes);
            panel.Controls.Add(btnAdd);
            panel.Controls.Add(btnUpdate);
            panel.Controls.Add(btnCancel);
            panel.Controls.Add(btnClear);
        }

        private void LoadPurchaseOrders()
        {
            try
            {
                List<PurchaseOrder> orders =
                    orderService
                        .GetAllOrders()
                        .Where(order =>
                            order.OrderStatus != "DRAFT" &&
                            order.OrderStatus != "DECLINED" &&
                            order.OrderStatus != "CANCELLED" &&
                            order.OrderStatus != "DELIVERED")
                        .ToList();

                cmbPurchaseOrder.DataSource = orders;
                cmbPurchaseOrder.DisplayMember = "OrderNumber";
                cmbPurchaseOrder.ValueMember =
                    "PurchaseOrderId";

                cmbPurchaseOrder.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load purchase orders.\n\n" +
                    ex.Message
                );
            }
        }

        private void LoadDeliveries()
        {
            try
            {
                dgvDeliveries.DataSource = null;
                dgvDeliveries.DataSource =
                    deliveryService.GetAllDeliveries();

                dgvDeliveries.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load deliveries.\n\n" +
                    ex.Message
                );
            }
        }

        private void BtnAdd_Click(
            object? sender,
            EventArgs e)
        {
            Delivery? delivery =
                ReadDeliveryFromForm();

            if (delivery == null)
            {
                return;
            }

            OperationResult result =
                deliveryService.CreateDelivery(
                    delivery
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadDeliveries();
                ClearForm();
            }
        }

        private void BtnUpdate_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedDeliveryId <= 0)
            {
                ShowWarning(
                    "Please select a delivery."
                );

                return;
            }

            Delivery? delivery =
                ReadDeliveryFromForm();

            if (delivery == null)
            {
                return;
            }

            delivery.DeliveryId =
                selectedDeliveryId;

            delivery.ReceivedBy =
                selectedReceivedBy;

            OperationResult result =
                deliveryService.UpdateDelivery(
                    delivery
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadPurchaseOrders();
                LoadDeliveries();
                ClearForm();
            }
        }

        private void BtnCancel_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedDeliveryId <= 0)
            {
                ShowWarning(
                    "Please select a delivery."
                );

                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    "Do you want to cancel this delivery?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            OperationResult result =
                deliveryService.CancelDelivery(
                    selectedDeliveryId
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadDeliveries();
                ClearForm();
            }
        }

        private void DgvDeliveries_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                dgvDeliveries.Rows[e.RowIndex]
                    .DataBoundItem is not Delivery delivery)
            {
                return;
            }

            selectedDeliveryId =
                delivery.DeliveryId;

            selectedReceivedBy =
                delivery.ReceivedBy;

            cmbPurchaseOrder.SelectedValue =
                delivery.PurchaseOrderId;

            txtDeliveryReference.Text =
                delivery.DeliveryReference;

            if (delivery.DeliveryDate.HasValue)
            {
                dtpDeliveryDate.Value =
                    delivery.DeliveryDate.Value;

                dtpDeliveryDate.Checked = true;
            }
            else
            {
                dtpDeliveryDate.Checked = false;
            }

            cmbStatus.Text =
                delivery.DeliveryStatus;

            txtNotes.Text =
                delivery.Notes;
        }

        private Delivery? ReadDeliveryFromForm()
        {
            if (cmbPurchaseOrder.SelectedValue == null)
            {
                ShowWarning(
                    "Please select a purchase order."
                );

                return null;
            }

            return new Delivery
            {
                PurchaseOrderId =
                    Convert.ToInt32(
                        cmbPurchaseOrder.SelectedValue
                    ),

                DeliveryReference =
                    txtDeliveryReference.Text.Trim(),

                DeliveryDate =
                    dtpDeliveryDate.Checked
                        ? dtpDeliveryDate.Value.Date
                        : null,

                DeliveryStatus =
                    cmbStatus.Text,

                ReceivedBy =
                    selectedReceivedBy,

                Notes =
                    txtNotes.Text.Trim()
            };
        }

        private void ClearForm()
        {
            selectedDeliveryId = 0;
            selectedReceivedBy = null;

            cmbPurchaseOrder.SelectedIndex = -1;

            txtDeliveryReference.Text =
                deliveryService
                    .GenerateDeliveryReference();

            dtpDeliveryDate.Value = DateTime.Today;
            dtpDeliveryDate.Checked = false;
            cmbStatus.SelectedItem = "SCHEDULED";
            txtNotes.Clear();

            dgvDeliveries.ClearSelection();
        }

        private void CreateGridColumns()
        {
            AddColumn(
                "DeliveryReference",
                "Reference",
                145
            );

            AddColumn(
                "OrderNumber",
                "Order Number",
                135
            );

            AddColumn(
                "SupplierName",
                "Supplier",
                190
            );

            AddColumn(
                "DeliveryDate",
                "Delivery Date",
                120,
                "yyyy-MM-dd"
            );

            AddColumn(
                "DeliveryStatus",
                "Status",
                120
            );

            AddColumn(
                "ReceivedByName",
                "Received By",
                150
            );

            AddColumn(
                "Notes",
                "Notes",
                250
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

            dgvDeliveries.Columns.Add(column);
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