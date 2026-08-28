using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.HRManager
{
    public class PayrollManagementForm : Form
    {
        private readonly PayrollService payrollService;
        private readonly EmployeeService employeeService;

        private readonly ComboBox cmbEmployee;
        private readonly NumericUpDown nudYear;
        private readonly ComboBox cmbMonth;
        private readonly NumericUpDown nudBasicSalary;
        private readonly NumericUpDown nudAllowances;
        private readonly NumericUpDown nudDeductions;
        private readonly Label lblNetSalary;
        private readonly DataGridView dgvPayroll;

        private int selectedPayrollId;
        private string selectedPaymentStatus = "PENDING";

        public PayrollManagementForm()
        {
            payrollService = new PayrollService();
            employeeService = new EmployeeService();

            Text = "Payroll Management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1200, 700);
            MinimumSize = new Size(1100, 650);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Payroll Management",
                Location = new Point(30, 20),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold
                ),
                ForeColor = Color.FromArgb(24, 90, 60)
            };

            Panel inputPanel = new Panel
            {
                Location = new Point(30, 75),
                Size = new Size(1140, 230),
                BackColor = Color.White
            };

            AddLabel(inputPanel, "Employee", 20, 20);

            cmbEmployee = CreateComboBox(
                20,
                48,
                260
            );

            inputPanel.Controls.Add(cmbEmployee);

            AddLabel(inputPanel, "Year", 300, 20);

            nudYear = new NumericUpDown
            {
                Location = new Point(300, 48),
                Size = new Size(110, 30),
                Font = new Font("Segoe UI", 10),
                Minimum = 2020,
                Maximum = DateTime.Today.Year + 1,
                Value = DateTime.Today.Year
            };

            inputPanel.Controls.Add(nudYear);

            AddLabel(inputPanel, "Month", 430, 20);

            cmbMonth = CreateComboBox(
                430,
                48,
                160
            );

            cmbMonth.Items.AddRange(
                new object[]
                {
                    "January",
                    "February",
                    "March",
                    "April",
                    "May",
                    "June",
                    "July",
                    "August",
                    "September",
                    "October",
                    "November",
                    "December"
                }
            );

            cmbMonth.SelectedIndex =
                DateTime.Today.Month - 1;

            inputPanel.Controls.Add(cmbMonth);

            AddLabel(
                inputPanel,
                "Basic salary",
                610,
                20
            );

            nudBasicSalary = CreateMoneyInput(
                610,
                48
            );

            inputPanel.Controls.Add(nudBasicSalary);

            AddLabel(
                inputPanel,
                "Allowances",
                790,
                20
            );

            nudAllowances = CreateMoneyInput(
                790,
                48
            );

            inputPanel.Controls.Add(nudAllowances);

            AddLabel(
                inputPanel,
                "Deductions",
                970,
                20
            );

            nudDeductions = CreateMoneyInput(
                970,
                48
            );

            inputPanel.Controls.Add(nudDeductions);

            Label netTitle = new Label
            {
                Text = "Net Salary",
                Location = new Point(20, 105),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold
                )
            };

            lblNetSalary = new Label
            {
                Text = "Rs. 0.00",
                Location = new Point(20, 135),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold
                ),
                ForeColor = Color.FromArgb(24, 90, 60)
            };

            inputPanel.Controls.Add(netTitle);
            inputPanel.Controls.Add(lblNetSalary);

            Button btnAdd = CreateButton(
                "ADD PAYROLL",
                300,
                130,
                Color.FromArgb(24, 90, 60)
            );

            Button btnUpdate = CreateButton(
                "UPDATE",
                460,
                130,
                Color.FromArgb(52, 120, 190)
            );

            Button btnPaid = CreateButton(
                "MARK AS PAID",
                620,
                130,
                Color.FromArgb(45, 145, 80)
            );

            Button btnDelete = CreateButton(
                "DELETE",
                780,
                130,
                Color.FromArgb(180, 60, 60)
            );

            Button btnClear = CreateButton(
                "CLEAR",
                940,
                130,
                Color.DimGray
            );

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnPaid.Click += BtnPaid_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += (_, _) => ClearForm();

            inputPanel.Controls.Add(btnAdd);
            inputPanel.Controls.Add(btnUpdate);
            inputPanel.Controls.Add(btnPaid);
            inputPanel.Controls.Add(btnDelete);
            inputPanel.Controls.Add(btnClear);

            dgvPayroll = CreatePayrollGrid();

            dgvPayroll.Location =
                new Point(30, 325);

            dgvPayroll.Size =
                new Size(1140, 330);

            dgvPayroll.CellClick +=
                DgvPayroll_CellClick;

            cmbEmployee.SelectedIndexChanged +=
                CmbEmployee_SelectedIndexChanged;

            nudBasicSalary.ValueChanged +=
                (_, _) => UpdateNetSalary();

            nudAllowances.ValueChanged +=
                (_, _) => UpdateNetSalary();

            nudDeductions.ValueChanged +=
                (_, _) => UpdateNetSalary();

            Controls.Add(title);
            Controls.Add(inputPanel);
            Controls.Add(dgvPayroll);

            Load += PayrollManagementForm_Load;
        }

        private void PayrollManagementForm_Load(
            object? sender,
            EventArgs e)
        {
            LoadEmployees();
            LoadPayrolls();
            UpdateNetSalary();
        }

        private void LoadEmployees()
        {
            try
            {
                List<Employee> employees =
                    employeeService.GetAllEmployees()
                        .Where(e =>
                            e.EmploymentStatus == "ACTIVE")
                        .ToList();

                cmbEmployee.DataSource = employees;
                cmbEmployee.DisplayMember = "FullName";
                cmbEmployee.ValueMember = "EmployeeId";
                cmbEmployee.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load employees:\n" +
                    ex.Message
                );
            }
        }

        private void LoadPayrolls()
        {
            try
            {
                dgvPayroll.DataSource = null;
                dgvPayroll.DataSource =
                    payrollService.GetAllPayrolls();

                dgvPayroll.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load payrolls:\n" +
                    ex.Message
                );
            }
        }

        private void CmbEmployee_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            if (cmbEmployee.SelectedItem
                is Employee employee)
            {
                nudBasicSalary.Value = Math.Min(
                    nudBasicSalary.Maximum,
                    employee.BasicSalary
                );
            }
        }

        private void UpdateNetSalary()
        {
            decimal netSalary =
                nudBasicSalary.Value +
                nudAllowances.Value -
                nudDeductions.Value;

            lblNetSalary.Text =
                $"Rs. {netSalary:N2}";

            lblNetSalary.ForeColor =
                netSalary >= 0
                    ? Color.FromArgb(24, 90, 60)
                    : Color.DarkRed;
        }

        private void BtnAdd_Click(
            object? sender,
            EventArgs e)
        {
            Payroll? payroll = ReadPayrollForm();

            if (payroll == null)
            {
                return;
            }

            OperationResult result =
                payrollService.CreatePayroll(payroll);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadPayrolls();
            }
        }

        private void BtnUpdate_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedPayrollId <= 0)
            {
                ShowWarning(
                    "Please select a payroll record."
                );
                return;
            }

            Payroll? payroll = ReadPayrollForm();

            if (payroll == null)
            {
                return;
            }

            payroll.PayrollId = selectedPayrollId;

            payroll.PaymentStatus =
                selectedPaymentStatus;

            OperationResult result =
                payrollService.UpdatePayroll(payroll);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadPayrolls();
            }
        }

        private void BtnPaid_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedPayrollId <= 0)
            {
                ShowWarning(
                    "Please select a payroll record."
                );
                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    "Mark this payroll as paid?",
                    "Confirm Payment",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            OperationResult result =
                payrollService.MarkAsPaid(
                    selectedPayrollId
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadPayrolls();
            }
        }

        private void BtnDelete_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedPayrollId <= 0)
            {
                ShowWarning(
                    "Please select a payroll record."
                );
                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    "Delete this payroll record?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            OperationResult result =
                payrollService.DeletePayroll(
                    selectedPayrollId
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadPayrolls();
            }
        }

        private Payroll? ReadPayrollForm()
        {
            if (cmbEmployee.SelectedValue == null)
            {
                ShowWarning("Please select an employee.");
                return null;
            }

            return new Payroll
            {
                EmployeeId = Convert.ToInt32(
                    cmbEmployee.SelectedValue
                ),

                PayYear = Convert.ToInt32(
                    nudYear.Value
                ),

                PayMonth = cmbMonth.SelectedIndex + 1,

                BasicSalary =
                    nudBasicSalary.Value,

                Allowances =
                    nudAllowances.Value,

                Deductions =
                    nudDeductions.Value,

                PaymentStatus = "PENDING"
            };
        }

        private void DgvPayroll_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvPayroll.Rows[e.RowIndex]
                .DataBoundItem is not Payroll payroll)
            {
                return;
            }

            selectedPayrollId = payroll.PayrollId;

            selectedPaymentStatus =
                payroll.PaymentStatus;

            cmbEmployee.SelectedValue =
                payroll.EmployeeId;

            nudYear.Value = payroll.PayYear;
            cmbMonth.SelectedIndex =
                payroll.PayMonth - 1;

            nudBasicSalary.Value =
                payroll.BasicSalary;

            nudAllowances.Value =
                payroll.Allowances;

            nudDeductions.Value =
                payroll.Deductions;

            UpdateNetSalary();
        }

        private void ClearForm()
        {
            selectedPayrollId = 0;
            selectedPaymentStatus = "PENDING";

            cmbEmployee.SelectedIndex = -1;

            nudYear.Value =
                DateTime.Today.Year;

            cmbMonth.SelectedIndex =
                DateTime.Today.Month - 1;

            nudBasicSalary.Value = 0;
            nudAllowances.Value = 0;
            nudDeductions.Value = 0;

            dgvPayroll.ClearSelection();
            UpdateNetSalary();
        }

        private static DataGridView CreatePayrollGrid()
        {
            DataGridView grid = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                MultiSelect = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill
            };

            grid.EnableHeadersVisualStyles = false;
            grid.RowTemplate.Height = 35;

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

            AddColumn(grid, "PayrollId", "ID");
            AddColumn(
                grid,
                "EmployeeName",
                "Employee"
            );
            AddColumn(grid, "PayPeriod", "Period");
            AddColumn(
                grid,
                "BasicSalary",
                "Basic Salary"
            );
            AddColumn(
                grid,
                "Allowances",
                "Allowances"
            );
            AddColumn(
                grid,
                "Deductions",
                "Deductions"
            );
            AddColumn(
                grid,
                "NetSalary",
                "Net Salary"
            );
            AddColumn(
                grid,
                "PaymentStatus",
                "Status"
            );
            AddColumn(grid, "PaidAt", "Paid At");

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
                    Name = property,
                    DataPropertyName = property,
                    HeaderText = heading
                }
            );
        }

        private static NumericUpDown CreateMoneyInput(
            int x,
            int y)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(160, 30),
                Font = new Font("Segoe UI", 10),
                DecimalPlaces = 2,
                Maximum = 10000000,
                ThousandsSeparator = true
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
                    8,
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