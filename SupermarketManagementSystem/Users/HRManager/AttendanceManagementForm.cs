using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.HRManager
{
    public class AttendanceManagementForm : Form
    {
        private readonly AttendanceService attendanceService;
        private readonly EmployeeService employeeService;

        private readonly ComboBox cmbEmployee;
        private readonly DateTimePicker dtpDate;
        private readonly DateTimePicker dtpClockIn;
        private readonly DateTimePicker dtpClockOut;
        private readonly ComboBox cmbStatus;
        private readonly TextBox txtNotes;
        private readonly DataGridView dgvAttendance;

        private long selectedAttendanceId;

        public AttendanceManagementForm()
        {
            attendanceService = new AttendanceService();
            employeeService = new EmployeeService();

            Text = "Attendance Management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1150, 680);
            MinimumSize = new Size(1050, 620);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Attendance Management",
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
                Size = new Size(1090, 210),
                BackColor = Color.White
            };

            AddLabel(inputPanel, "Employee", 20, 20);

            cmbEmployee = CreateComboBox(
                20,
                48,
                280
            );

            inputPanel.Controls.Add(cmbEmployee);

            AddLabel(inputPanel, "Date", 320, 20);

            dtpDate = new DateTimePicker
            {
                Location = new Point(320, 48),
                Size = new Size(160, 30),
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short,
                MaxDate = DateTime.Today
            };

            inputPanel.Controls.Add(dtpDate);

            AddLabel(inputPanel, "Clock in", 500, 20);

            dtpClockIn = CreateTimePicker(
                500,
                48
            );

            inputPanel.Controls.Add(dtpClockIn);

            AddLabel(inputPanel, "Clock out", 680, 20);

            dtpClockOut = CreateTimePicker(
                680,
                48
            );

            inputPanel.Controls.Add(dtpClockOut);

            AddLabel(inputPanel, "Status", 860, 20);

            cmbStatus = CreateComboBox(
                860,
                48,
                200
            );

            cmbStatus.Items.AddRange(
                new object[]
                {
                    "PRESENT",
                    "ABSENT",
                    "LATE",
                    "LEAVE",
                    "HALF_DAY"
                }
            );

            cmbStatus.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbStatus);

            AddLabel(inputPanel, "Notes", 20, 95);

            txtNotes = new TextBox
            {
                Location = new Point(20, 123),
                Size = new Size(460, 32),
                Font = new Font("Segoe UI", 10),
                MaxLength = 255
            };

            inputPanel.Controls.Add(txtNotes);

            Button btnAdd = CreateButton(
                "ADD",
                510,
                120,
                Color.FromArgb(24, 90, 60)
            );

            Button btnUpdate = CreateButton(
                "UPDATE",
                650,
                120,
                Color.FromArgb(52, 120, 190)
            );

            Button btnDelete = CreateButton(
                "DELETE",
                790,
                120,
                Color.FromArgb(180, 60, 60)
            );

            Button btnClear = CreateButton(
                "CLEAR",
                930,
                120,
                Color.DimGray
            );

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += (_, _) => ClearForm();

            inputPanel.Controls.Add(btnAdd);
            inputPanel.Controls.Add(btnUpdate);
            inputPanel.Controls.Add(btnDelete);
            inputPanel.Controls.Add(btnClear);

            dgvAttendance = CreateAttendanceGrid();

            dgvAttendance.Location =
                new Point(30, 305);

            dgvAttendance.Size =
                new Size(1090, 330);

            dgvAttendance.CellClick +=
                DgvAttendance_CellClick;

            Controls.Add(title);
            Controls.Add(inputPanel);
            Controls.Add(dgvAttendance);

            Load += AttendanceManagementForm_Load;
        }

        private void AttendanceManagementForm_Load(
            object? sender,
            EventArgs e)
        {
            LoadEmployees();
            LoadAttendance();
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

        private void LoadAttendance()
        {
            try
            {
                dgvAttendance.DataSource = null;
                dgvAttendance.DataSource =
                    attendanceService.GetAllAttendance();

                dgvAttendance.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load attendance:\n" +
                    ex.Message
                );
            }
        }

        private void BtnAdd_Click(
            object? sender,
            EventArgs e)
        {
            Attendance? attendance =
                ReadAttendanceForm();

            if (attendance == null)
            {
                return;
            }

            OperationResult result =
                attendanceService.CreateAttendance(
                    attendance
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadAttendance();
            }
        }

        private void BtnUpdate_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedAttendanceId <= 0)
            {
                ShowWarning(
                    "Please select an attendance record."
                );
                return;
            }

            Attendance? attendance =
                ReadAttendanceForm();

            if (attendance == null)
            {
                return;
            }

            attendance.AttendanceId =
                selectedAttendanceId;

            OperationResult result =
                attendanceService.UpdateAttendance(
                    attendance
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadAttendance();
            }
        }

        private void BtnDelete_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedAttendanceId <= 0)
            {
                ShowWarning(
                    "Please select an attendance record."
                );
                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    "Delete the selected attendance record?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            OperationResult result =
                attendanceService.DeleteAttendance(
                    selectedAttendanceId
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadAttendance();
            }
        }

        private Attendance? ReadAttendanceForm()
        {
            if (cmbEmployee.SelectedValue == null)
            {
                ShowWarning("Please select an employee.");
                return null;
            }

            return new Attendance
            {
                EmployeeId = Convert.ToInt32(
                    cmbEmployee.SelectedValue
                ),

                AttendanceDate = dtpDate.Value.Date,

                ClockIn = dtpClockIn.Checked
                    ? dtpClockIn.Value.TimeOfDay
                    : null,

                ClockOut = dtpClockOut.Checked
                    ? dtpClockOut.Value.TimeOfDay
                    : null,

                AttendanceStatus =
                    cmbStatus.SelectedItem?.ToString()
                    ?? "PRESENT",

                Notes = txtNotes.Text
            };
        }

        private void DgvAttendance_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvAttendance.Rows[e.RowIndex]
                .DataBoundItem is not Attendance attendance)
            {
                return;
            }

            selectedAttendanceId =
                attendance.AttendanceId;

            cmbEmployee.SelectedValue =
                attendance.EmployeeId;

            dtpDate.Value =
                attendance.AttendanceDate.Date;

            SetTimePicker(
                dtpClockIn,
                attendance.ClockIn
            );

            SetTimePicker(
                dtpClockOut,
                attendance.ClockOut
            );

            cmbStatus.SelectedItem =
                attendance.AttendanceStatus;

            txtNotes.Text = attendance.Notes;
        }

        private void ClearForm()
        {
            selectedAttendanceId = 0;
            cmbEmployee.SelectedIndex = -1;
            dtpDate.Value = DateTime.Today;

            dtpClockIn.Checked = false;
            dtpClockOut.Checked = false;

            cmbStatus.SelectedIndex = 0;
            txtNotes.Clear();

            dgvAttendance.ClearSelection();
            cmbEmployee.Focus();
        }

        private static void SetTimePicker(
            DateTimePicker picker,
            TimeSpan? time)
        {
            if (!time.HasValue)
            {
                picker.Checked = false;
                return;
            }

            picker.Checked = true;
            picker.Value = DateTime.Today.Add(
                time.Value
            );
        }

        private static DateTimePicker CreateTimePicker(
            int x,
            int y)
        {
            return new DateTimePicker
            {
                Location = new Point(x, y),
                Size = new Size(160, 30),
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                ShowCheckBox = true,
                Checked = false
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

        private static DataGridView CreateAttendanceGrid()
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

            AddColumn(grid, "AttendanceId", "ID");
            AddColumn(
                grid,
                "AttendanceDate",
                "Date"
            );
            AddColumn(
                grid,
                "EmployeeCode",
                "Employee Code"
            );
            AddColumn(
                grid,
                "EmployeeName",
                "Employee"
            );
            AddColumn(grid, "ClockIn", "Clock In");
            AddColumn(grid, "ClockOut", "Clock Out");
            AddColumn(
                grid,
                "AttendanceStatus",
                "Status"
            );
            AddColumn(grid, "Notes", "Notes");

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
                Size = new Size(120, 40),
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