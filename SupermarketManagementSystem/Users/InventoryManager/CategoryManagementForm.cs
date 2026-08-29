using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.InventoryManager
{
    public class CategoryManagementForm : Form
    {
        private readonly CategoryService categoryService;

        private readonly TextBox txtCategoryName;
        private readonly TextBox txtDescription;
        private readonly ComboBox cmbStatus;
        private readonly DataGridView dgvCategories;

        private int selectedCategoryId;

        public CategoryManagementForm()
        {
            categoryService = new CategoryService();

            Text = "Category Management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(900, 600);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Category Management",
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
                Size = new Size(840, 190),
                BackColor = Color.White
            };

            AddLabel(
                inputPanel,
                "Category name",
                20,
                20
            );

            txtCategoryName = new TextBox
            {
                Location = new Point(20, 48),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 10),
                MaxLength = 100
            };

            inputPanel.Controls.Add(txtCategoryName);

            AddLabel(inputPanel, "Description", 290, 20);

            txtDescription = new TextBox
            {
                Location = new Point(290, 48),
                Size = new Size(320, 60),
                Multiline = true,
                Font = new Font("Segoe UI", 10),
                MaxLength = 255
            };

            inputPanel.Controls.Add(txtDescription);

            AddLabel(inputPanel, "Status", 630, 20);

            cmbStatus = new ComboBox
            {
                Location = new Point(630, 48),
                Size = new Size(180, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle =
                    ComboBoxStyle.DropDownList
            };

            cmbStatus.Items.AddRange(
                new object[]
                {
                    "ACTIVE",
                    "INACTIVE"
                }
            );

            cmbStatus.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbStatus);

            Button btnAdd = CreateButton(
                "ADD",
                110,
                125,
                Color.FromArgb(24, 90, 60)
            );

            Button btnUpdate = CreateButton(
                "UPDATE",
                260,
                125,
                Color.FromArgb(52, 120, 190)
            );

            Button btnDelete = CreateButton(
                "DELETE",
                410,
                125,
                Color.FromArgb(180, 60, 60)
            );

            Button btnClear = CreateButton(
                "CLEAR",
                560,
                125,
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

            dgvCategories = CreateCategoryGrid();

            dgvCategories.Location =
                new Point(30, 285);

            dgvCategories.Size =
                new Size(840, 270);

            dgvCategories.CellClick +=
                DgvCategories_CellClick;

            Controls.Add(title);
            Controls.Add(inputPanel);
            Controls.Add(dgvCategories);

            Load += CategoryManagementForm_Load;
        }

        private void CategoryManagementForm_Load(
            object? sender,
            EventArgs e)
        {
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                dgvCategories.DataSource = null;
                dgvCategories.DataSource =
                    categoryService.GetAllCategories();

                dgvCategories.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load categories:\n" +
                    ex.Message
                );
            }
        }

        private void BtnAdd_Click(
            object? sender,
            EventArgs e)
        {
            Category category = ReadForm();

            OperationResult result =
                categoryService.CreateCategory(category);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadCategories();
            }
        }

        private void BtnUpdate_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedCategoryId <= 0)
            {
                ShowWarning(
                    "Please select a category."
                );
                return;
            }

            Category category = ReadForm();
            category.CategoryId = selectedCategoryId;

            OperationResult result =
                categoryService.UpdateCategory(category);

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadCategories();
            }
        }

        private void BtnDelete_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedCategoryId <= 0)
            {
                ShowWarning(
                    "Please select a category."
                );
                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    "Delete this category?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            OperationResult result =
                categoryService.DeleteCategory(
                    selectedCategoryId
                );

            ShowResult(result);

            if (result.IsSuccessful)
            {
                ClearForm();
                LoadCategories();
            }
        }

        private Category ReadForm()
        {
            return new Category
            {
                CategoryName = txtCategoryName.Text,
                Description = txtDescription.Text,

                CategoryStatus =
                    cmbStatus.SelectedItem?.ToString()
                    ?? "ACTIVE"
            };
        }

        private void DgvCategories_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvCategories.Rows[e.RowIndex]
                .DataBoundItem is not Category category)
            {
                return;
            }

            selectedCategoryId = category.CategoryId;

            txtCategoryName.Text =
                category.CategoryName;

            txtDescription.Text =
                category.Description;

            cmbStatus.SelectedItem =
                category.CategoryStatus;
        }

        private void ClearForm()
        {
            selectedCategoryId = 0;

            txtCategoryName.Clear();
            txtDescription.Clear();
            cmbStatus.SelectedIndex = 0;

            dgvCategories.ClearSelection();
            txtCategoryName.Focus();
        }

        private static DataGridView CreateCategoryGrid()
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

            AddColumn(grid, "CategoryId", "ID");
            AddColumn(
                grid,
                "CategoryName",
                "Category Name"
            );
            AddColumn(
                grid,
                "Description",
                "Description"
            );
            AddColumn(
                grid,
                "CategoryStatus",
                "Status"
            );
            AddColumn(
                grid,
                "CreatedAt",
                "Created At"
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