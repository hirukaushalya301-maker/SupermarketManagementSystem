using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.HRManager
{
    public class LeaveRequestManagementForm : Form
    {
        private readonly LeaveRequestService leaveService;
        private readonly EmployeeService employeeService;

        private readonly ComboBox cmbEmployee;
        private readonly ComboBox cmbLeaveType;
        private readonly DateTimePicker dtpStartDate;
        private readonly DateTimePicker dtpEndDate;
        private readonly TextBox txtReason;
        private readonly TextBox txtReviewNote;
        private readonly DataGridView dgvLeaveRequests;

        private int selectedLeaveRequestId;
        private string selectedRequestStatus = string.Empty;

        public LeaveRequestManagementForm()
        {
            leaveService = new LeaveRequestService();
            employeeService = new EmployeeService();

            Text = "Leave Request Management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1200, 700);
            MinimumSize = new Size(1100, 650);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Leave Request Management",
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
                Size = new Size(1140, 240),
                BackColor = Color.White
            };

            AddLabel(inputPanel, "Employee", 20, 20);

            cmbEmployee = CreateComboBox(
                20,
                48,
                270
            );

            inputPanel.Controls.Add(cmbEmployee);

            AddLabel(inputPanel, "Leave type", 310, 20);

            cmbLeaveType = CreateComboBox(
                310,
                48,
                180
            );

            cmbLeaveType.Items.AddRange(
                new object[]
                {
                    "ANNUAL",
                    "SICK",
                    "CASUAL",
                    "UNPAID",
                    "OTHER"
                }
            );

            cmbLeaveType.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbLeaveType);

            AddLabel(inputPanel, "Start date", 510, 20);

            dtpStartDate = CreateDatePicker(
                510,
                48
            );

            inputPanel.Controls.Add(dtpStartDate);

            AddLabel(inputPanel, "End date", 690, 20);

            dtpEndDate = CreateDatePicker(
                690,
                48
            );

            inputPanel.Controls.Add(dtpEndDate);

            AddLabel(inputPanel, "Reason", 20, 95);

            txtReason = new TextBox
            {
                Location = new Point(20, 123),
                Size = new Size(470, 60),
                Multiline = true,
                Font = new Font("Segoe UI", 10),
                MaxLength = 500
            };

            inputPanel.Controls.Add(txtReason);

            AddLabel(inputPanel, "Review note", 510, 95);

            txtReviewNote = new TextBox
            {
                Location = new Point(510, 123),
                Size = new Size(360, 60),
                Multiline = true,
                Font = new Font("Segoe UI", 10),
                MaxLength = 500
            };

            inputPanel.Controls.Add(txtReviewNote);

            Button btnAdd = CreateButton(
                "ADD",
                20,
                195,
                Color.FromArgb(24, 90, 60)
            );

            Button btnUpdate = CreateButton(
                "UPDATE",
                155,
                195,
                Color.FromArgb(52, 120, 190)
            );

            Button btnApprove = CreateButton(
                "APPROVE",
                290,
                195,
                Color.FromArgb(45, 145, 80)
            );

            Button btnReject = CreateButton(
                "REJECT",
                425,
                195,
                Color.FromArgb(190, 70, 60)
            );

            Button btnCancel = CreateButton(
                "CANCEL REQUEST",
                560,
                195,
                Color.FromArgb(220, 140, 40)
            );

            Button btnClear = CreateButton(
                "CLEAR",
                735,
                195,
                Color.DimGray
            );

            Button btnClose = CreateButton(
                "CLOSE",
                870,
                195,
                Color.FromArgb(90, 90, 90)
            );

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;

            btnApprove.Click += (_, _) =>
                ReviewSelectedRequest("APPROVED");

            btnReject.Click += (_, _) =>
                ReviewSelectedRequest("REJECTED");

            btnCancel.Click += BtnCancel_Click;
            btnClear.Click += (_, _) => ClearForm();
            btnClose.Click += (_, _) => Close();

            inputPanel.Controls.Add(btnAdd);
            inputPanel.Controls.Add(btnUpdate);
            inputPanel.Controls.Add(btnApprove);
            inputPanel.Controls.Add(btnReject);
            inputPanel.Controls.Add(btnCancel);
            inputPanel.Controls.Add(btnClear);
            inputPanel.Controls.Add(btnClose);

            dgvLeaveRequests = CreateLeaveGrid();

            dgvLeaveRequests.Location =
                new Point(30, 335);

            dgvLeaveRequests.Size =
                new Size(1140, 320);

            dgvLeaveRequests.CellClick +=
                DgvLeaveRequests_CellClick;

            Controls.Add(title);
            Controls.Add(inputPanel);
            Controls.Add(dgvLeaveRequests);

            Load += LeaveRequestManagementForm_Load;
        }

        private void LeaveRequestManagementForm_Load(
            object? sender,
            EventArgs e)
        {
            LoadEmployees();
            LoadLeaveRequests();
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

        private void LoadLeaveRequests()
        {
            try
            {
                dgvLeaveRequests.DataSource = null;
                dgvLeaveRequests.DataSource =
                    leaveService.GetAllLeaveRequests();

                dgvLeaveRequests.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load leave requests:\n" +
                    ex.Message
                );
            }
        }

        private void BtnAdd_Click(
            object? sender,
            EventArgs e)
        {
            LeaveRequest? request =
                ReadLeaveRequestForm();

            if (request == null)
            {
                return;
            }

            OperationResult result =
                leaveService.CreateLeaveRequest(
                    request
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadLeaveRequests();
            }
        }

        private void BtnUpdate_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedLeaveRequestId <= 0)
            {
                ShowWarning(
                    "Please select a leave request."
                );
                return;
            }

            LeaveRequest? request =
                ReadLeaveRequestForm();

            if (request == null)
            {
                return;
            }

            request.LeaveRequestId =
                selectedLeaveRequestId;

            request.RequestStatus =
                selectedRequestStatus;

            OperationResult result =
                leaveService.UpdateLeaveRequest(
                    request
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadLeaveRequests();
            }
        }

        private void ReviewSelectedRequest(
            string reviewStatus)
        {
            if (selectedLeaveRequestId <= 0)
            {
                ShowWarning(
                    "Please select a leave request."
                );
                return;
            }

            OperationResult result =
                leaveService.ReviewLeaveRequest(
                    selectedLeaveRequestId,
                    reviewStatus,

                    // Replace with logged-in HR user ID later.
                    null,

                    txtReviewNote.Text
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadLeaveRequests();
            }
        }

        private void BtnCancel_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedLeaveRequestId <= 0)
            {
                ShowWarning(
                    "Please select a leave request."
                );
                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    "Cancel the selected leave request?",
                    "Confirm Cancellation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            OperationResult result =
                leaveService.CancelLeaveRequest(
                    selectedLeaveRequestId
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadLeaveRequests();
            }
        }

        private LeaveRequest? ReadLeaveRequestForm()
        {
            if (cmbEmployee.SelectedValue == null)
            {
                ShowWarning("Please select an employee.");
                return null;
            }

            return new LeaveRequest
            {
                EmployeeId = Convert.ToInt32(
                    cmbEmployee.SelectedValue
                ),

                LeaveType =
                    cmbLeaveType.SelectedItem?.ToString()
                    ?? "ANNUAL",

                StartDate = dtpStartDate.Value.Date,
                EndDate = dtpEndDate.Value.Date,
                Reason = txtReason.Text
            };
        }

        private void DgvLeaveRequests_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvLeaveRequests.Rows[e.RowIndex]
                .DataBoundItem is not LeaveRequest request)
            {
                return;
            }

            selectedLeaveRequestId =
                request.LeaveRequestId;

            selectedRequestStatus =
                request.RequestStatus;

            cmbEmployee.SelectedValue =
                request.EmployeeId;

            cmbLeaveType.SelectedItem =
                request.LeaveType;

            dtpStartDate.Value =
                request.StartDate.Date;

            dtpEndDate.Value =
                request.EndDate.Date;

            txtReason.Text = request.Reason;
            txtReviewNote.Text = request.ReviewNote;
        }

        private void ClearForm()
        {
            selectedLeaveRequestId = 0;
            selectedRequestStatus = string.Empty;

            cmbEmployee.SelectedIndex = -1;
            cmbLeaveType.SelectedIndex = 0;

            dtpStartDate.Value = DateTime.Today;
            dtpEndDate.Value = DateTime.Today;

            txtReason.Clear();
            txtReviewNote.Clear();

            dgvLeaveRequests.ClearSelection();
            cmbEmployee.Focus();
        }

        private static DataGridView CreateLeaveGrid()
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

            AddColumn(grid, "LeaveRequestId", "ID");
            AddColumn(
                grid,
                "EmployeeName",
                "Employee"
            );
            AddColumn(grid, "LeaveType", "Leave Type");
            AddColumn(grid, "StartDate", "Start Date");
            AddColumn(grid, "EndDate", "End Date");
            AddColumn(
                grid,
                "NumberOfDays",
                "Days"
            );
            AddColumn(grid, "Reason", "Reason");
            AddColumn(
                grid,
                "RequestStatus",
                "Status"
            );
            AddColumn(
                grid,
                "ReviewNote",
                "Review Note"
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

        private static DateTimePicker CreateDatePicker(
            int x,
            int y)
        {
            return new DateTimePicker
            {
                Location = new Point(x, y),
                Size = new Size(160, 30),
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short
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
                Size = new Size(
                    text == "CANCEL REQUEST"
                        ? 160
                        : 120,
                    35
                ),
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