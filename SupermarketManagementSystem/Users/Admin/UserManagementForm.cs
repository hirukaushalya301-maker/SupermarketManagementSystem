using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.Admin
{
    public class UserManagementForm : Form
    {
        private readonly UserService userService;

        private readonly TextBox txtUsername;
        private readonly TextBox txtPassword;
        private readonly TextBox txtFullName;
        private readonly ComboBox cmbRole;
        private readonly ComboBox cmbStatus;
        private readonly DataGridView dgvUsers;

        private int selectedUserId;

        public UserManagementForm()
        {
            userService = new UserService();

            Text = "User Management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1050, 650);
            MinimumSize = new Size(1000, 600);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "User Management",
                Location = new Point(30, 20),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold
                ),
                ForeColor = Color.FromArgb(24, 90, 60)
            };

            Panel formPanel = new Panel
            {
                Location = new Point(30, 80),
                Size = new Size(990, 190),
                BackColor = Color.White
            };

            AddLabel(formPanel, "Username", 20, 20);

            txtUsername = CreateTextBox(20, 48, 210);
            formPanel.Controls.Add(txtUsername);

            AddLabel(formPanel, "Full name", 250, 20);

            txtFullName = CreateTextBox(250, 48, 250);
            formPanel.Controls.Add(txtFullName);

            AddLabel(formPanel, "Password", 520, 20);

            txtPassword = CreateTextBox(520, 48, 210);
            txtPassword.UseSystemPasswordChar = true;
            formPanel.Controls.Add(txtPassword);

            AddLabel(formPanel, "Role", 750, 20);

            cmbRole = CreateComboBox(750, 48, 210);
            formPanel.Controls.Add(cmbRole);

            AddLabel(formPanel, "Account status", 20, 95);

            cmbStatus = CreateComboBox(20, 123, 210);
            cmbStatus.Items.AddRange(
                new object[]
                {
                    "ACTIVE",
                    "INACTIVE",
                    "BLOCKED"
                }
            );
            cmbStatus.SelectedIndex = 0;
            formPanel.Controls.Add(cmbStatus);

            Button btnAdd = CreateButton(
                "ADD USER",
                250,
                120,
                Color.FromArgb(24, 90, 60)
            );

            Button btnUpdate = CreateButton(
                "UPDATE",
                410,
                120,
                Color.FromArgb(52, 120, 190)
            );

            Button btnClear = CreateButton(
                "CLEAR",
                570,
                120,
                Color.DimGray
            );

            Button btnClose = CreateButton(
                "CLOSE",
                730,
                120,
                Color.FromArgb(160, 50, 50)
            );

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnClear.Click += (_, _) => ClearForm();
            btnClose.Click += (_, _) => Close();

            formPanel.Controls.Add(btnAdd);
            formPanel.Controls.Add(btnUpdate);
            formPanel.Controls.Add(btnClear);
            formPanel.Controls.Add(btnClose);

            dgvUsers = CreateUserGrid();
            dgvUsers.Location = new Point(30, 290);
            dgvUsers.Size = new Size(990, 320);
            dgvUsers.CellClick += DgvUsers_CellClick;

            Controls.Add(title);
            Controls.Add(formPanel);
            Controls.Add(dgvUsers);

            Load += UserManagementForm_Load;
        }

        private void UserManagementForm_Load(
            object? sender,
            EventArgs e)
        {
            LoadRoles();
            LoadUsers();
        }

        private void LoadRoles()
        {
            try
            {
                List<Role> roles =
                    userService.GetAllRoles();

                cmbRole.DataSource = roles;
                cmbRole.DisplayMember = "RoleName";
                cmbRole.ValueMember = "RoleId";
                cmbRole.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load roles:\n" + ex.Message
                );
            }
        }

        private void LoadUsers()
        {
            try
            {
                dgvUsers.DataSource = null;
                dgvUsers.DataSource =
                    userService.GetAllUsers();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load users:\n" + ex.Message
                );
            }
        }

        private void BtnAdd_Click(
            object? sender,
            EventArgs e)
        {
            if (cmbRole.SelectedValue == null)
            {
                ShowWarning("Please select a role.");
                return;
            }

            int roleId =
                Convert.ToInt32(cmbRole.SelectedValue);

            OperationResult result =
                userService.CreateUser(
                    txtUsername.Text,
                    txtPassword.Text,
                    txtFullName.Text,
                    roleId
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadUsers();
            }
        }

        private void BtnUpdate_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedUserId <= 0)
            {
                ShowWarning(
                    "Please select a user from the table."
                );
                return;
            }

            if (cmbRole.SelectedValue == null)
            {
                ShowWarning("Please select a role.");
                return;
            }

            int roleId =
                Convert.ToInt32(cmbRole.SelectedValue);

            string accountStatus =
                cmbStatus.SelectedItem?.ToString()
                ?? "ACTIVE";

            OperationResult result =
                userService.UpdateUser(
                    selectedUserId,
                    txtUsername.Text,
                    txtFullName.Text,
                    roleId,
                    accountStatus
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadUsers();
            }
        }

        private void DgvUsers_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvUsers.Rows[e.RowIndex]
                .DataBoundItem is not User user)
            {
                return;
            }

            selectedUserId = user.UserId;
            txtUsername.Text = user.Username;
            txtFullName.Text = user.FullName;
            txtPassword.Clear();

            cmbRole.SelectedValue = user.RoleId;
            cmbStatus.SelectedItem =
                user.AccountStatus;
        }

        private void ClearForm()
        {
            selectedUserId = 0;

            txtUsername.Clear();
            txtPassword.Clear();
            txtFullName.Clear();

            cmbRole.SelectedIndex = -1;
            cmbStatus.SelectedIndex = 0;

            dgvUsers.ClearSelection();
            txtUsername.Focus();
        }

        private static DataGridView CreateUserGrid()
        {
            DataGridView grid = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = false,
                MultiSelect = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill
            };

            grid.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(24, 90, 60),
                    ForeColor = Color.White,
                    Font = new Font(
                        "Segoe UI",
                        10,
                        FontStyle.Bold
                    ),
                    Alignment =
                        DataGridViewContentAlignment.MiddleLeft
                };

            grid.EnableHeadersVisualStyles = false;
            grid.RowTemplate.Height = 35;

            grid.Columns.Add(CreateColumn(
                "UserId",
                "ID",
                "UserId"
            ));

            grid.Columns.Add(CreateColumn(
                "Username",
                "Username",
                "Username"
            ));

            grid.Columns.Add(CreateColumn(
                "FullName",
                "Full Name",
                "FullName"
            ));

            grid.Columns.Add(CreateColumn(
                "RoleName",
                "Role",
                "RoleName"
            ));

            grid.Columns.Add(CreateColumn(
                "AccountStatus",
                "Status",
                "AccountStatus"
            ));

            grid.Columns.Add(CreateColumn(
                "CreatedAt",
                "Created At",
                "CreatedAt"
            ));

            return grid;
        }

        private static DataGridViewTextBoxColumn CreateColumn(
            string name,
            string header,
            string property)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                DataPropertyName = property
            };
        }

        private static void AddLabel(
            Control parent,
            string text,
            int x,
            int y)
        {
            Label label = new Label
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

        private static TextBox CreateTextBox(
            int x,
            int y,
            int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 30),
                Font = new Font("Segoe UI", 11)
            };
        }

        private static ComboBox CreateComboBox(
            int x,
            int y,
            int width)
        {
            return new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle =
                    ComboBoxStyle.DropDownList
            };
        }

        private static Button CreateButton(
            string text,
            int x,
            int y,
            Color colour)
        {
            Button button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(140, 40),
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
                    : "Operation Failed",
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