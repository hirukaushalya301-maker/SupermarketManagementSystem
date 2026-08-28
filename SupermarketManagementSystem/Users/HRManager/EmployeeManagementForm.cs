using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.HRManager
{
    public class EmployeeManagementForm : Form
    {
        private readonly EmployeeService employeeService;

        private readonly TextBox txtEmployeeCode;
        private readonly TextBox txtFirstName;
        private readonly TextBox txtLastName;
        private readonly TextBox txtNic;
        private readonly TextBox txtPhone;
        private readonly TextBox txtEmail;
        private readonly TextBox txtJobTitle;
        private readonly DateTimePicker dtpHireDate;
        private readonly NumericUpDown nudBasicSalary;
        private readonly ComboBox cmbStatus;
        private readonly DataGridView dgvEmployees;

        private int selectedEmployeeId;

        public EmployeeManagementForm()
        {
            employeeService = new EmployeeService();

            Text = "Employee Management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1200, 700);
            MinimumSize = new Size(1100, 650);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Employee Management",
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
                Location = new Point(30, 75),
                Size = new Size(1140, 245),
                BackColor = Color.White
            };

            AddLabel(formPanel, "Employee code", 20, 20);
            txtEmployeeCode = CreateTextBox(20, 48, 190);
            formPanel.Controls.Add(txtEmployeeCode);

            AddLabel(formPanel, "First name", 230, 20);
            txtFirstName = CreateTextBox(230, 48, 210);
            formPanel.Controls.Add(txtFirstName);

            AddLabel(formPanel, "Last name", 460, 20);
            txtLastName = CreateTextBox(460, 48, 210);
            formPanel.Controls.Add(txtLastName);

            AddLabel(formPanel, "NIC", 690, 20);
            txtNic = CreateTextBox(690, 48, 190);
            formPanel.Controls.Add(txtNic);

            AddLabel(formPanel, "Phone", 900, 20);
            txtPhone = CreateTextBox(900, 48, 210);
            formPanel.Controls.Add(txtPhone);

            AddLabel(formPanel, "Email", 20, 95);
            txtEmail = CreateTextBox(20, 123, 270);
            formPanel.Controls.Add(txtEmail);

            AddLabel(formPanel, "Job title", 310, 95);
            txtJobTitle = CreateTextBox(310, 123, 220);
            formPanel.Controls.Add(txtJobTitle);

            AddLabel(formPanel, "Hire date", 550, 95);

            dtpHireDate = new DateTimePicker
            {
                Location = new Point(550, 123),
                Size = new Size(180, 30),
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short,
                MaxDate = DateTime.Today
            };

            formPanel.Controls.Add(dtpHireDate);

            AddLabel(formPanel, "Basic salary", 750, 95);

            nudBasicSalary = new NumericUpDown
            {
                Location = new Point(750, 123),
                Size = new Size(160, 30),
                Font = new Font("Segoe UI", 10),
                DecimalPlaces = 2,
                Maximum = 10000000,
                ThousandsSeparator = true
            };

            formPanel.Controls.Add(nudBasicSalary);

            AddLabel(formPanel, "Status", 930, 95);

            cmbStatus = new ComboBox
            {
                Location = new Point(930, 123),
                Size = new Size(180, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbStatus.Items.AddRange(
                new object[]
                {
                    "ACTIVE",
                    "INACTIVE",
                    "TERMINATED"
                }
            );

            cmbStatus.SelectedIndex = 0;
            formPanel.Controls.Add(cmbStatus);

            Button btnAdd = CreateButton(
                "ADD EMPLOYEE",
                250,
                180,
                Color.FromArgb(24, 90, 60)
            );

            Button btnUpdate = CreateButton(
                "UPDATE",
                430,
                180,
                Color.FromArgb(52, 120, 190)
            );

            Button btnClear = CreateButton(
                "CLEAR",
                610,
                180,
                Color.DimGray
            );

            Button btnClose = CreateButton(
                "CLOSE",
                790,
                180,
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

            dgvEmployees = CreateEmployeeGrid();
            dgvEmployees.Location = new Point(30, 340);
            dgvEmployees.Size = new Size(1140, 320);

            dgvEmployees.CellClick +=
                DgvEmployees_CellClick;

            Controls.Add(title);
            Controls.Add(formPanel);
            Controls.Add(dgvEmployees);

            Load += EmployeeManagementForm_Load;
        }

        private void EmployeeManagementForm_Load(
            object? sender,
            EventArgs e)
        {
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                dgvEmployees.DataSource = null;
                dgvEmployees.DataSource =
                    employeeService.GetAllEmployees();

                dgvEmployees.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load employees:\n" +
                    ex.Message
                );
            }
        }

        private void BtnAdd_Click(
            object? sender,
            EventArgs e)
        {
            Employee employee = ReadEmployeeForm();

            OperationResult result =
                employeeService.CreateEmployee(employee);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadEmployees();
            }
        }

        private void BtnUpdate_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedEmployeeId <= 0)
            {
                ShowWarning(
                    "Please select an employee."
                );
                return;
            }

            Employee employee = ReadEmployeeForm();
            employee.EmployeeId = selectedEmployeeId;

            OperationResult result =
                employeeService.UpdateEmployee(employee);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadEmployees();
            }
        }

        private Employee ReadEmployeeForm()
        {
            return new Employee
            {
                EmployeeCode = txtEmployeeCode.Text,
                FirstName = txtFirstName.Text,
                LastName = txtLastName.Text,
                Nic = txtNic.Text,
                Phone = txtPhone.Text,
                Email = txtEmail.Text,
                JobTitle = txtJobTitle.Text,
                HireDate = dtpHireDate.Value.Date,
                BasicSalary = nudBasicSalary.Value,

                EmploymentStatus =
                    cmbStatus.SelectedItem?.ToString()
                    ?? "ACTIVE"
            };
        }

        private void DgvEmployees_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvEmployees.Rows[e.RowIndex]
                .DataBoundItem is not Employee employee)
            {
                return;
            }

            selectedEmployeeId = employee.EmployeeId;

            txtEmployeeCode.Text =
                employee.EmployeeCode;

            txtFirstName.Text = employee.FirstName;
            txtLastName.Text = employee.LastName;
            txtNic.Text = employee.Nic;
            txtPhone.Text = employee.Phone;
            txtEmail.Text = employee.Email;
            txtJobTitle.Text = employee.JobTitle;

            dtpHireDate.Value =
                employee.HireDate.Date;

            nudBasicSalary.Value =
                Math.Min(
                    nudBasicSalary.Maximum,
                    employee.BasicSalary
                );

            cmbStatus.SelectedItem =
                employee.EmploymentStatus;
        }

        private void ClearForm()
        {
            selectedEmployeeId = 0;

            txtEmployeeCode.Clear();
            txtFirstName.Clear();
            txtLastName.Clear();
            txtNic.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtJobTitle.Clear();

            dtpHireDate.Value = DateTime.Today;
            nudBasicSalary.Value = 0;
            cmbStatus.SelectedIndex = 0;

            dgvEmployees.ClearSelection();
            txtEmployeeCode.Focus();
        }

        private static DataGridView CreateEmployeeGrid()
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

            grid.EnableHeadersVisualStyles = false;

            grid.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(24, 90, 60),
                    ForeColor = Color.White,
                    Font = new Font(
                        "Segoe UI",
                        10,
                        FontStyle.Bold
                    )
                };

            grid.RowTemplate.Height = 35;

            AddColumn(grid, "EmployeeId", "ID");
            AddColumn(
                grid,
                "EmployeeCode",
                "Employee Code"
            );
            AddColumn(grid, "FullName", "Full Name");
            AddColumn(grid, "Nic", "NIC");
            AddColumn(grid, "Phone", "Phone");
            AddColumn(grid, "JobTitle", "Job Title");
            AddColumn(
                grid,
                "BasicSalary",
                "Basic Salary"
            );
            AddColumn(
                grid,
                "EmploymentStatus",
                "Status"
            );

            return grid;
        }

        private static void AddColumn(
            DataGridView grid,
            string property,
            string heading)
        {
            grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = property,
                    HeaderText = heading,
                    Name = property
                }
            );
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
                Font = new Font("Segoe UI", 10)
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
                Size = new Size(160, 42),
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