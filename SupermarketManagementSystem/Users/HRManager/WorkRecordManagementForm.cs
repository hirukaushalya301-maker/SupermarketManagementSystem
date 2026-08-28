using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.HRManager
{
    public class WorkRecordManagementForm : Form
    {
        private readonly WorkRecordService workService;
        private readonly EmployeeService employeeService;

        private readonly ComboBox cmbEmployee;
        private readonly DateTimePicker dtpWorkDate;
        private readonly TextBox txtTaskTitle;
        private readonly TextBox txtDescription;
        private readonly ComboBox cmbStatus;
        private readonly DataGridView dgvWorkRecords;

        private long selectedWorkRecordId;

        public WorkRecordManagementForm()
        {
            workService = new WorkRecordService();
            employeeService = new EmployeeService();

            Text = "Work Record Management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1150, 680);
            MinimumSize = new Size(1050, 620);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Employee Work Records",
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
                Size = new Size(1090, 230),
                BackColor = Color.White
            };

            AddLabel(inputPanel, "Employee", 20, 20);

            cmbEmployee = CreateComboBox(
                20,
                48,
                260
            );

            inputPanel.Controls.Add(cmbEmployee);

            AddLabel(inputPanel, "Work date", 300, 20);

            dtpWorkDate = new DateTimePicker
            {
                Location = new Point(300, 48),
                Size = new Size(160, 30),
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short
            };

            inputPanel.Controls.Add(dtpWorkDate);

            AddLabel(inputPanel, "Task title", 480, 20);

            txtTaskTitle = new TextBox
            {
                Location = new Point(480, 48),
                Size = new Size(350, 30),
                Font = new Font("Segoe UI", 10),
                MaxLength = 150
            };

            inputPanel.Controls.Add(txtTaskTitle);

            AddLabel(inputPanel, "Status", 850, 20);

            cmbStatus = CreateComboBox(
                850,
                48,
                210
            );

            cmbStatus.Items.AddRange(
                new object[]
                {
                    "ASSIGNED",
                    "IN_PROGRESS",
                    "COMPLETED",
                    "CANCELLED"
                }
            );

            cmbStatus.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbStatus);

            AddLabel(inputPanel, "Description", 20, 95);

            txtDescription = new TextBox
            {
                Location = new Point(20, 123),
                Size = new Size(600, 60),
                Multiline = true,
                Font = new Font("Segoe UI", 10),
                MaxLength = 500
            };

            inputPanel.Controls.Add(txtDescription);

            Button btnAdd = CreateButton(
                "ADD TASK",
                650,
                125,
                Color.FromArgb(24, 90, 60)
            );

            Button btnUpdate = CreateButton(
                "UPDATE",
                790,
                125,
                Color.FromArgb(52, 120, 190)
            );

            Button btnDelete = CreateButton(
                "DELETE",
                930,
                125,
                Color.FromArgb(180, 60, 60)
            );

            Button btnClear = CreateButton(
                "CLEAR",
                790,
                175,
                Color.DimGray
            );

            Button btnClose = CreateButton(
                "CLOSE",
                930,
                175,
                Color.FromArgb(90, 90, 90)
            );

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += (_, _) => ClearForm();
            btnClose.Click += (_, _) => Close();

            inputPanel.Controls.Add(btnAdd);
            inputPanel.Controls.Add(btnUpdate);
            inputPanel.Controls.Add(btnDelete);
            inputPanel.Controls.Add(btnClear);
            inputPanel.Controls.Add(btnClose);

            dgvWorkRecords = CreateWorkRecordGrid();

            dgvWorkRecords.Location =
                new Point(30, 325);

            dgvWorkRecords.Size =
                new Size(1090, 320);

            dgvWorkRecords.CellClick +=
                DgvWorkRecords_CellClick;

            Controls.Add(title);
            Controls.Add(inputPanel);
            Controls.Add(dgvWorkRecords);

            Load += WorkRecordManagementForm_Load;
        }

        private void WorkRecordManagementForm_Load(
            object? sender,
            EventArgs e)
        {
            LoadEmployees();
            LoadWorkRecords();
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

        private void LoadWorkRecords()
        {
            try
            {
                dgvWorkRecords.DataSource = null;
                dgvWorkRecords.DataSource =
                    workService.GetAllWorkRecords();

                dgvWorkRecords.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load work records:\n" +
                    ex.Message
                );
            }
        }

        private void BtnAdd_Click(
            object? sender,
            EventArgs e)
        {
            WorkRecord? record = ReadForm();

            if (record == null)
            {
                return;
            }

            OperationResult result =
                workService.CreateWorkRecord(record);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadWorkRecords();
            }
        }

        private void BtnUpdate_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedWorkRecordId <= 0)
            {
                ShowWarning(
                    "Please select a work record."
                );
                return;
            }

            WorkRecord? record = ReadForm();

            if (record == null)
            {
                return;
            }

            record.WorkRecordId =
                selectedWorkRecordId;

            OperationResult result =
                workService.UpdateWorkRecord(record);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadWorkRecords();
            }
        }

        private void BtnDelete_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedWorkRecordId <= 0)
            {
                ShowWarning(
                    "Please select a work record."
                );
                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    "Delete this work record?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            OperationResult result =
                workService.DeleteWorkRecord(
                    selectedWorkRecordId
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadWorkRecords();
            }
        }

        private WorkRecord? ReadForm()
        {
            if (cmbEmployee.SelectedValue == null)
            {
                ShowWarning("Please select an employee.");
                return null;
            }

            return new WorkRecord
            {
                EmployeeId = Convert.ToInt32(
                    cmbEmployee.SelectedValue
                ),

                WorkDate = dtpWorkDate.Value.Date,
                TaskTitle = txtTaskTitle.Text,
                Description = txtDescription.Text,

                WorkStatus =
                    cmbStatus.SelectedItem?.ToString()
                    ?? "ASSIGNED",

                // Replace with logged-in HR user ID later.
                AssignedBy = null
            };
        }

        private void DgvWorkRecords_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvWorkRecords.Rows[e.RowIndex]
                .DataBoundItem is not WorkRecord record)
            {
                return;
            }

            selectedWorkRecordId =
                record.WorkRecordId;

            cmbEmployee.SelectedValue =
                record.EmployeeId;

            dtpWorkDate.Value = record.WorkDate;
            txtTaskTitle.Text = record.TaskTitle;
            txtDescription.Text = record.Description;
            cmbStatus.SelectedItem = record.WorkStatus;
        }

        private void ClearForm()
        {
            selectedWorkRecordId = 0;

            cmbEmployee.SelectedIndex = -1;
            dtpWorkDate.Value = DateTime.Today;
            txtTaskTitle.Clear();
            txtDescription.Clear();
            cmbStatus.SelectedIndex = 0;

            dgvWorkRecords.ClearSelection();
            cmbEmployee.Focus();
        }

        private static DataGridView CreateWorkRecordGrid()
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

            AddColumn(grid, "WorkRecordId", "ID");
            AddColumn(
                grid,
                "EmployeeName",
                "Employee"
            );
            AddColumn(grid, "WorkDate", "Work Date");
            AddColumn(grid, "TaskTitle", "Task");
            AddColumn(
                grid,
                "Description",
                "Description"
            );
            AddColumn(grid, "WorkStatus", "Status");
            AddColumn(
                grid,
                "CompletedAt",
                "Completed At"
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
                    Name = property,
                    DataPropertyName = property,
                    HeaderText = heading
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
                Size = new Size(120, 38),
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