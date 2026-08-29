using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.InventoryManager
{
    public class StockHistoryForm : Form
    {
        private readonly StockService stockService;
        private readonly ProductService productService;

        private DataGridView dgvHistory = null!;
        private ComboBox cmbProduct = null!;
        private ComboBox cmbMovementType = null!;

        private List<StockMovement> allMovements = new();

        public StockHistoryForm()
        {
            stockService = new StockService();
            productService = new ProductService();

            CreateInterface();
            LoadProducts();
            LoadHistory();
        }

        private void CreateInterface()
        {
            Text = "Stock Movement History";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1200, 650);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Stock Movement History",
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
                    "Audit all stock increases and decreases",
                Location = new Point(28, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray
            };

            Panel filterPanel = new Panel
            {
                Location = new Point(25, 95),
                Size = new Size(1150, 80),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            AddLabel(filterPanel, "Product", 20, 12);

            cmbProduct = new ComboBox
            {
                Location = new Point(20, 37),
                Size = new Size(250, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            AddLabel(
                filterPanel,
                "Movement type",
                290,
                12
            );

            cmbMovementType = new ComboBox
            {
                Location = new Point(290, 37),
                Size = new Size(180, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbMovementType.Items.AddRange(
                new object[]
                {
                    "ALL",
                    "PURCHASE",
                    "SALE",
                    "RETURN_IN",
                    "RETURN_OUT",
                    "ADJUSTMENT_IN",
                    "ADJUSTMENT_OUT",
                    "EXPIRED"
                }
            );

            cmbMovementType.SelectedItem = "ALL";

            Button btnFilter = CreateButton(
                "FILTER",
                500,
                28,
                Color.FromArgb(24, 90, 60)
            );

            Button btnClear = CreateButton(
                "CLEAR FILTER",
                640,
                28,
                Color.DimGray
            );

            Button btnRefresh = CreateButton(
                "REFRESH",
                780,
                28,
                Color.FromArgb(30, 100, 180)
            );

            btnFilter.Click += (_, _) => ApplyFilter();
            btnClear.Click += BtnClearFilter_Click;
            btnRefresh.Click += (_, _) => LoadHistory();

            filterPanel.Controls.Add(cmbProduct);
            filterPanel.Controls.Add(cmbMovementType);
            filterPanel.Controls.Add(btnFilter);
            filterPanel.Controls.Add(btnClear);
            filterPanel.Controls.Add(btnRefresh);

            dgvHistory = new DataGridView
            {
                Location = new Point(25, 195),
                Size = new Size(1150, 420),
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

            dgvHistory.ColumnHeadersDefaultCellStyle =
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

            dgvHistory.EnableHeadersVisualStyles = false;
            dgvHistory.RowTemplate.Height = 32;

            CreateGridColumns();

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(filterPanel);
            Controls.Add(dgvHistory);
        }

        private void LoadProducts()
        {
            try
            {
                List<Product> products =
                    productService.GetAllProducts();

                Product allProducts = new Product
                {
                    ProductId = 0,
                    ProductName = "ALL PRODUCTS"
                };

                products.Insert(0, allProducts);

                cmbProduct.DataSource = products;
                cmbProduct.DisplayMember = "ProductName";
                cmbProduct.ValueMember = "ProductId";
                cmbProduct.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load products.\n\n" +
                    ex.Message
                );
            }
        }

        private void LoadHistory()
        {
            try
            {
                allMovements =
                    stockService.GetStockMovements();

                ApplyFilter();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load stock history.\n\n" +
                    ex.Message
                );
            }
        }

        private void ApplyFilter()
        {
            IEnumerable<StockMovement> filtered =
                allMovements;

            if (cmbProduct.SelectedValue != null)
            {
                int productId = Convert.ToInt32(
                    cmbProduct.SelectedValue
                );

                if (productId > 0)
                {
                    filtered = filtered.Where(
                        movement =>
                            movement.ProductId ==
                            productId
                    );
                }
            }

            if (!string.IsNullOrWhiteSpace(
                    cmbMovementType.Text) &&
                cmbMovementType.Text != "ALL")
            {
                filtered = filtered.Where(
                    movement =>
                        movement.MovementType ==
                        cmbMovementType.Text
                );
            }

            dgvHistory.DataSource = null;
            dgvHistory.DataSource = filtered.ToList();
            dgvHistory.ClearSelection();
        }

        private void BtnClearFilter_Click(
            object? sender,
            EventArgs e)
        {
            cmbProduct.SelectedIndex = 0;
            cmbMovementType.SelectedItem = "ALL";
            ApplyFilter();
        }

        private void CreateGridColumns()
        {
            AddColumn("MovementId", "ID", 65);
            AddColumn("ProductName", "Product", 190);
            AddColumn("BatchNumber", "Batch", 110);
            AddColumn("MovementType", "Movement", 140);
            AddColumn("Quantity", "Quantity", 90);
            AddColumn("ReferenceType", "Reference", 120);
            AddColumn("Notes", "Notes", 230);
            AddColumn(
                "PerformedByName",
                "Performed By",
                140
            );

            AddColumn(
                "CreatedAt",
                "Date",
                150,
                "yyyy-MM-dd HH:mm"
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

            dgvHistory.Columns.Add(column);
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