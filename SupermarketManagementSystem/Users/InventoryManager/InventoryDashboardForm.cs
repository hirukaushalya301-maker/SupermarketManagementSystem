using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.InventoryManager
{
    public class InventoryDashboardForm : Form
    {
        private readonly Panel contentPanel;

        private readonly InventoryDashboardService
            dashboardService;

        public InventoryDashboardForm()
        {
            dashboardService =
                new InventoryDashboardService();

            Text = "Supermarket Management System - Inventory";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1250, 750);
            MinimumSize = new Size(1150, 700);
            BackColor = Color.FromArgb(245, 247, 250);

            Panel sidebar = CreateSidebar();
            Panel header = CreateHeader();

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(30)
            };

            Controls.Add(contentPanel);
            Controls.Add(header);
            Controls.Add(sidebar);

            ShowDashboardHome();
        }

        private Panel CreateSidebar()
        {
            Panel sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(24, 90, 60),
                AutoScroll = true
            };

            Label logo = new Label
            {
                Text = "SUPERMARKET\nINVENTORY PANEL",
                Dock = DockStyle.Top,
                Height = 100,
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI",
                    14,
                    FontStyle.Bold
                ),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Button btnDashboard = CreateMenuButton(
                "Dashboard",
                ShowDashboardHome
            );

            Button btnCategories = CreateMenuButton(
                "Categories",
                OpenCategoryManagement
            );

            Button btnProducts = CreateMenuButton(
                "Products",
                OpenProductManagement
            );

            Button btnBatches = CreateMenuButton(
                "Product Batches",
                OpenProductBatchManagement
            );

            Button btnStock = CreateMenuButton(
                "Stock",
                OpenStockManagement
            );

            Button btnSuppliers = CreateMenuButton(
                "Suppliers",
                OpenSupplierManagement
            );

            Button btnSupplies = CreateMenuButton(
                "Supplies",
                OpenSupplyManagement
            );

            Button btnOrders = CreateMenuButton(
                "Purchase Orders",
                OpenPurchaseOrderManagement
            );

            Button btnDeliveries = CreateMenuButton(
                "Deliveries",
                OpenDeliveryManagement
            );

            Button btnGoodsReceipt = CreateMenuButton(
                "Receive Delivery",
                 OpenGoodsReceipt
            );

            Button btnNotifications = CreateMenuButton(
                "Notifications",
                OpenInventoryNotifications
            );

            Button btnStockHistory = CreateMenuButton(
                "Stock History",
                OpenStockHistory
            );

            Button btnLogout = CreateMenuButton(
                "Logout",
                Logout
            );

            btnLogout.BackColor =
                Color.FromArgb(160, 50, 50);

            sidebar.Controls.Add(btnLogout);
            sidebar.Controls.Add(btnStockHistory);
            sidebar.Controls.Add(btnNotifications);
            sidebar.Controls.Add(btnGoodsReceipt);
            sidebar.Controls.Add(btnDeliveries);
            sidebar.Controls.Add(btnOrders);
            sidebar.Controls.Add(btnSupplies);
            sidebar.Controls.Add(btnSuppliers);
            sidebar.Controls.Add(btnStock);
            sidebar.Controls.Add(btnBatches);
            sidebar.Controls.Add(btnProducts);
            sidebar.Controls.Add(btnCategories);
            sidebar.Controls.Add(btnDashboard);
            sidebar.Controls.Add(logo);


            return sidebar;
        }

        private Panel CreateHeader()
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(25, 0, 25, 0)
            };

            Label title = new Label
            {
                Text = "Inventory Manager Dashboard",
                Dock = DockStyle.Left,
                Width = 450,
                Font = new Font(
                    "Segoe UI",
                    20,
                    FontStyle.Bold
                ),
                ForeColor = Color.FromArgb(24, 90, 60),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label userDetails = new Label
            {
                Text =
                    "Inventory Manager | INVENTORY_MANAGER",

                Dock = DockStyle.Right,
                Width = 400,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray,
                TextAlign = ContentAlignment.MiddleRight
            };

            header.Controls.Add(title);
            header.Controls.Add(userDetails);

            return header;
        }

        private static Button CreateMenuButton(
            string text,
            Action clickAction)
        {
            Button button = new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(24, 90, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(25, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;

            button.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(35, 115, 78);

            button.Click += (_, _) => clickAction();

            return button;
        }

        private void ShowDashboardHome()
        {
            contentPanel.Controls.Clear();

            InventoryDashboardSummary summary =
                dashboardService.GetSummary();

            Label welcome = new Label
            {
                Text = "Inventory Overview",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold
                ),
                ForeColor = Color.FromArgb(35, 45, 55)
            };

            Label subtitle = new Label
            {
                Text =
                    "Monitor products, stock and purchase orders",

                Location = new Point(24, 65),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.DimGray
            };

            Panel productsCard = CreateSummaryCard(
                "Total Products",
                summary.TotalProducts.ToString(),
                Color.FromArgb(52, 152, 219),
                new Point(20, 120)
            );

            Panel lowStockCard = CreateSummaryCard(
                "Low Stock",
                summary.LowStockProducts.ToString(),
                Color.FromArgb(230, 120, 50),
                new Point(280, 120)
            );

            Panel ordersCard = CreateSummaryCard(
                "Pending Orders",
                summary.PendingPurchaseOrders.ToString(),
                Color.FromArgb(140, 90, 180),
                new Point(540, 120)
            );

            Panel suppliersCard = CreateSummaryCard(
                "Active Suppliers",
                summary.ActiveSuppliers.ToString(),
                Color.FromArgb(46, 160, 110),
                new Point(20, 280)
            );

            Panel notificationsCard = CreateSummaryCard(
                "Unread Notifications",
                summary.UnreadNotifications.ToString(),
                Color.FromArgb(200, 70, 70),
                new Point(280, 280)
            );

            Panel expiringCard = CreateSummaryCard(
                "Expiring Batches",
                summary.ExpiringBatches.ToString(),
                Color.FromArgb(230, 160, 40),
                new Point(540, 280)
            );

            contentPanel.Controls.Add(welcome);
            contentPanel.Controls.Add(subtitle);
            contentPanel.Controls.Add(productsCard);
            contentPanel.Controls.Add(lowStockCard);
            contentPanel.Controls.Add(ordersCard);
            contentPanel.Controls.Add(suppliersCard);
            contentPanel.Controls.Add(notificationsCard);
            contentPanel.Controls.Add(expiringCard);
        }

        private static Panel CreateSummaryCard(
            string title,
            string value,
            Color colour,
            Point location)
        {
            Panel card = new Panel
            {
                Location = location,
                Size = new Size(230, 130),
                BackColor = Color.White
            };

            Panel colourBar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 7,
                BackColor = colour
            };

            Label valueLabel = new Label
            {
                Text = value,
                Location = new Point(25, 25),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    25,
                    FontStyle.Bold
                ),
                ForeColor = colour
            };

            Label titleLabel = new Label
            {
                Text = title,
                Location = new Point(28, 80),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.DimGray
            };

            card.Controls.Add(colourBar);
            card.Controls.Add(valueLabel);
            card.Controls.Add(titleLabel);

            return card;
        }

        private void OpenCategoryManagement()
        {
            using CategoryManagementForm form =
                new CategoryManagementForm();

            form.ShowDialog(this);
            ShowDashboardHome();
        }

        private void OpenProductManagement()
        {
            using ProductManagementForm form =
                new ProductManagementForm();

            form.ShowDialog(this);
            ShowDashboardHome();
        }

        private void OpenProductBatchManagement()
        {
            using ProductBatchManagementForm form =
                new ProductBatchManagementForm();

            form.ShowDialog(this);
            ShowDashboardHome();
        }

        private void OpenStockManagement()
        {
            using StockManagementForm form =
                new StockManagementForm();

            form.ShowDialog(this);
            ShowDashboardHome();
        }

        private void OpenSupplierManagement()
        {
            using SupplierManagementForm form =
                new SupplierManagementForm();

            form.ShowDialog(this);
            ShowDashboardHome();
        }

        private void OpenSupplyManagement()
        {
            using SupplyManagementForm form =
                new SupplyManagementForm();

            form.ShowDialog(this);
            ShowDashboardHome();
        }

        private void OpenPurchaseOrderManagement()
        {
            using PurchaseOrderManagementForm form =
                new PurchaseOrderManagementForm();

            form.ShowDialog(this);
            ShowDashboardHome();
        }

        private void OpenDeliveryManagement()
        {
            using DeliveryManagementForm form =
                new DeliveryManagementForm();

            form.ShowDialog(this);
            ShowDashboardHome();
        }
        private void OpenGoodsReceipt()
        {
            using GoodsReceiptForm form =
                new GoodsReceiptForm();

            form.ShowDialog(this);
            ShowDashboardHome();
        }

        private void OpenInventoryNotifications()
        {
            using InventoryNotificationForm form =
                new InventoryNotificationForm();

            form.ShowDialog(this);
            ShowDashboardHome();
        }

        private void OpenStockHistory()
        {
            using StockHistoryForm form =
                new StockHistoryForm();

            form.ShowDialog(this);
            ShowDashboardHome();
        }

        private void Logout()
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to log out?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                Close();
            }
        }
    }
}