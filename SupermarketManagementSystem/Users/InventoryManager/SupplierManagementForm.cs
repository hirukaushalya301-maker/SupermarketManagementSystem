using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.InventoryManager
{
    public class SupplierManagementForm : Form
    {
        private readonly SupplierService supplierService;

        private DataGridView dgvSuppliers = null!;
        private TextBox txtSupplierCode = null!;
        private TextBox txtSupplierName = null!;
        private TextBox txtContactPerson = null!;
        private TextBox txtPhone = null!;
        private TextBox txtEmail = null!;
        private TextBox txtAddress = null!;
        private ComboBox cmbStatus = null!;

        private int selectedSupplierId;
        private int? selectedUserId;

        public SupplierManagementForm()
        {
            supplierService = new SupplierService();

            CreateInterface();
            LoadSuppliers();
            ClearForm();
        }

        private void CreateInterface()
        {
            Text = "Supplier Management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1250, 700);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Supplier Management",
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
                    "Register and manage supermarket suppliers",
                Location = new Point(28, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray
            };

            Panel inputPanel = new Panel
            {
                Location = new Point(25, 95),
                Size = new Size(1200, 220),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            CreateInputControls(inputPanel);

            dgvSuppliers = new DataGridView
            {
                Location = new Point(25, 335),
                Size = new Size(1200, 335),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
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

            dgvSuppliers.ColumnHeadersDefaultCellStyle =
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

            dgvSuppliers.EnableHeadersVisualStyles = false;
            dgvSuppliers.RowTemplate.Height = 32;
            dgvSuppliers.CellClick +=
                DgvSuppliers_CellClick;

            CreateGridColumns();

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(inputPanel);
            Controls.Add(dgvSuppliers);
        }

        private void CreateInputControls(Panel panel)
        {
            AddLabel(panel, "Supplier code", 20, 18);

            txtSupplierCode = new TextBox
            {
                Location = new Point(20, 43),
                Size = new Size(170, 30),
                MaxLength = 30
            };

            AddLabel(panel, "Supplier name", 210, 18);

            txtSupplierName = new TextBox
            {
                Location = new Point(210, 43),
                Size = new Size(250, 30),
                MaxLength = 150
            };

            AddLabel(panel, "Contact person", 480, 18);

            txtContactPerson = new TextBox
            {
                Location = new Point(480, 43),
                Size = new Size(210, 30),
                MaxLength = 100
            };

            AddLabel(panel, "Phone", 710, 18);

            txtPhone = new TextBox
            {
                Location = new Point(710, 43),
                Size = new Size(160, 30),
                MaxLength = 20
            };

            AddLabel(panel, "Status", 890, 18);

            cmbStatus = new ComboBox
            {
                Location = new Point(890, 43),
                Size = new Size(160, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbStatus.Items.AddRange(
                new object[]
                {
                    "ACTIVE",
                    "INACTIVE",
                    "BLOCKED"
                }
            );

            AddLabel(panel, "Email", 20, 88);

            txtEmail = new TextBox
            {
                Location = new Point(20, 113),
                Size = new Size(280, 30),
                MaxLength = 120
            };

            AddLabel(panel, "Address", 320, 88);

            txtAddress = new TextBox
            {
                Location = new Point(320, 113),
                Size = new Size(440, 30),
                MaxLength = 255
            };

            Button btnAdd = CreateButton(
                "ADD",
                20,
                165,
                Color.FromArgb(24, 90, 60)
            );

            Button btnUpdate = CreateButton(
                "UPDATE",
                165,
                165,
                Color.FromArgb(30, 100, 180)
            );

            Button btnBlock = CreateButton(
                "BLOCK",
                310,
                165,
                Color.FromArgb(180, 50, 50)
            );

            Button btnClear = CreateButton(
                "CLEAR",
                455,
                165,
                Color.DimGray
            );

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnBlock.Click += BtnBlock_Click;
            btnClear.Click += (_, _) => ClearForm();

            panel.Controls.Add(txtSupplierCode);
            panel.Controls.Add(txtSupplierName);
            panel.Controls.Add(txtContactPerson);
            panel.Controls.Add(txtPhone);
            panel.Controls.Add(cmbStatus);
            panel.Controls.Add(txtEmail);
            panel.Controls.Add(txtAddress);
            panel.Controls.Add(btnAdd);
            panel.Controls.Add(btnUpdate);
            panel.Controls.Add(btnBlock);
            panel.Controls.Add(btnClear);
        }

        private void LoadSuppliers()
        {
            try
            {
                dgvSuppliers.DataSource = null;
                dgvSuppliers.DataSource =
                    supplierService.GetAllSuppliers();

                dgvSuppliers.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load suppliers.\n\n" +
                    ex.Message
                );
            }
        }

        private void BtnAdd_Click(
            object? sender,
            EventArgs e)
        {
            Supplier supplier =
                ReadSupplierFromForm();

            OperationResult result =
                supplierService.CreateSupplier(supplier);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadSuppliers();
                ClearForm();
            }
        }

        private void BtnUpdate_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedSupplierId <= 0)
            {
                ShowWarning(
                    "Please select a supplier."
                );

                return;
            }

            Supplier supplier =
                ReadSupplierFromForm();

            supplier.SupplierId =
                selectedSupplierId;

            supplier.UserId =
                selectedUserId;

            OperationResult result =
                supplierService.UpdateSupplier(supplier);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadSuppliers();
                ClearForm();
            }
        }

        private void BtnBlock_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedSupplierId <= 0)
            {
                ShowWarning(
                    "Please select a supplier."
                );

                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    "Do you want to block this supplier?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            OperationResult result =
                supplierService.BlockSupplier(
                    selectedSupplierId
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                LoadSuppliers();
                ClearForm();
            }
        }

        private void DgvSuppliers_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvSuppliers.Rows[e.RowIndex]
                .DataBoundItem is not Supplier supplier)
            {
                return;
            }

            selectedSupplierId =
                supplier.SupplierId;

            selectedUserId =
                supplier.UserId;

            txtSupplierCode.Text =
                supplier.SupplierCode;

            txtSupplierName.Text =
                supplier.SupplierName;

            txtContactPerson.Text =
                supplier.ContactPerson;

            txtPhone.Text =
                supplier.Phone;

            txtEmail.Text =
                supplier.Email;

            txtAddress.Text =
                supplier.Address;

            cmbStatus.Text =
                supplier.SupplierStatus;
        }

        private Supplier ReadSupplierFromForm()
        {
            return new Supplier
            {
                SupplierId =
                    selectedSupplierId,

                UserId =
                    selectedUserId,

                SupplierCode =
                    txtSupplierCode.Text.Trim(),

                SupplierName =
                    txtSupplierName.Text.Trim(),

                ContactPerson =
                    txtContactPerson.Text.Trim(),

                Phone =
                    txtPhone.Text.Trim(),

                Email =
                    txtEmail.Text.Trim(),

                Address =
                    txtAddress.Text.Trim(),

                SupplierStatus =
                    cmbStatus.Text
            };
        }

        private void ClearForm()
        {
            selectedSupplierId = 0;
            selectedUserId = null;

            txtSupplierCode.Clear();
            txtSupplierName.Clear();
            txtContactPerson.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();

            cmbStatus.SelectedItem = "ACTIVE";

            dgvSuppliers.ClearSelection();
            txtSupplierCode.Focus();
        }

        private void CreateGridColumns()
        {
            AddColumn(
                "SupplierId",
                "ID",
                55
            );

            AddColumn(
                "SupplierCode",
                "Supplier Code",
                125
            );

            AddColumn(
                "SupplierName",
                "Supplier Name",
                190
            );

            AddColumn(
                "ContactPerson",
                "Contact Person",
                160
            );

            AddColumn(
                "Phone",
                "Phone",
                125
            );

            AddColumn(
                "Email",
                "Email",
                190
            );

            AddColumn(
                "Address",
                "Address",
                220
            );

            AddColumn(
                "SupplierStatus",
                "Status",
                100
            );
        }

        private void AddColumn(
            string propertyName,
            string header,
            int width)
        {
            dgvSuppliers.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = propertyName,
                    HeaderText = header,
                    Width = width
                }
            );
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