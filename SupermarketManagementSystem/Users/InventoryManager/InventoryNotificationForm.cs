using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.InventoryManager
{
    public class InventoryNotificationForm : Form
    {
        private readonly InventoryNotificationService
            notificationService;

        private DataGridView dgvNotifications = null!;
        private ComboBox cmbFilter = null!;

        private List<InventoryNotification>
            allNotifications = new();

        private long selectedNotificationId;

        public InventoryNotificationForm()
        {
            notificationService =
                new InventoryNotificationService();

            CreateInterface();
            LoadNotifications();
        }

        private void CreateInterface()
        {
            Text = "Inventory Notifications";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1200, 650);
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Inventory Notifications",
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
                    "Monitor low stock, unavailable products and expiring batches",
                Location = new Point(28, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray
            };

            Panel actionPanel = new Panel
            {
                Location = new Point(25, 95),
                Size = new Size(1150, 80),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            AddLabel(actionPanel, "Filter", 20, 12);

            cmbFilter = new ComboBox
            {
                Location = new Point(20, 37),
                Size = new Size(160, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbFilter.Items.AddRange(
                new object[]
                {
                    "ALL",
                    "UNREAD",
                    "READ",
                    "RESOLVED"
                }
            );

            cmbFilter.SelectedItem = "ALL";

            cmbFilter.SelectedIndexChanged +=
                (_, _) => ApplyFilter();

            Button btnGenerate = CreateButton(
                "GENERATE",
                220,
                28,
                Color.FromArgb(24, 90, 60)
            );

            Button btnRead = CreateButton(
                "MARK READ",
                360,
                28,
                Color.FromArgb(30, 100, 180)
            );

            Button btnResolve = CreateButton(
                "RESOLVE",
                500,
                28,
                Color.FromArgb(140, 90, 180)
            );

            Button btnRefresh = CreateButton(
                "REFRESH",
                640,
                28,
                Color.DimGray
            );

            btnGenerate.Click += BtnGenerate_Click;
            btnRead.Click += BtnRead_Click;
            btnResolve.Click += BtnResolve_Click;
            btnRefresh.Click += (_, _) =>
                LoadNotifications();

            actionPanel.Controls.Add(cmbFilter);
            actionPanel.Controls.Add(btnGenerate);
            actionPanel.Controls.Add(btnRead);
            actionPanel.Controls.Add(btnResolve);
            actionPanel.Controls.Add(btnRefresh);

            dgvNotifications = new DataGridView
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

            dgvNotifications.ColumnHeadersDefaultCellStyle =
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

            dgvNotifications.EnableHeadersVisualStyles =
                false;

            dgvNotifications.RowTemplate.Height = 34;

            dgvNotifications.CellClick +=
                DgvNotifications_CellClick;

            dgvNotifications.CellFormatting +=
                DgvNotifications_CellFormatting;

            CreateGridColumns();

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(actionPanel);
            Controls.Add(dgvNotifications);
        }

        private void LoadNotifications()
        {
            try
            {
                allNotifications =
                    notificationService
                        .GetAllNotifications();

                selectedNotificationId = 0;
                ApplyFilter();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Unable to load notifications.\n\n" +
                    ex.Message
                );
            }
        }

        private void ApplyFilter()
        {
            string selectedFilter =
                cmbFilter.Text;

            List<InventoryNotification> filtered =
                selectedFilter == "ALL"
                    ? allNotifications
                    : allNotifications
                        .Where(notification =>
                            notification
                                .NotificationStatus ==
                            selectedFilter)
                        .ToList();

            dgvNotifications.DataSource = null;
            dgvNotifications.DataSource = filtered;
            dgvNotifications.ClearSelection();
        }

        private void BtnGenerate_Click(
            object? sender,
            EventArgs e)
        {
            OperationResult result =
                notificationService
                    .GenerateAutomaticNotifications();

            ShowResult(result);
            LoadNotifications();
        }

        private void BtnRead_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedNotificationId <= 0)
            {
                ShowWarning(
                    "Please select a notification."
                );

                return;
            }

            OperationResult result =
                notificationService.MarkAsRead(
                    selectedNotificationId
                );

            ShowResult(result);
            LoadNotifications();
        }

        private void BtnResolve_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedNotificationId <= 0)
            {
                ShowWarning(
                    "Please select a notification."
                );

                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    "Do you want to resolve this notification?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            OperationResult result =
                notificationService.Resolve(
                    selectedNotificationId
                );

            ShowResult(result);
            LoadNotifications();
        }

        private void DgvNotifications_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                dgvNotifications.Rows[e.RowIndex]
                    .DataBoundItem
                is not InventoryNotification notification)
            {
                return;
            }

            selectedNotificationId =
                notification.NotificationId;
        }

        private void DgvNotifications_CellFormatting(
     object? sender,
     DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvNotifications.Rows[e.RowIndex]
                .DataBoundItem
                is not InventoryNotification notification)
            {
                return;
            }

            DataGridViewCellStyle? cellStyle =
                e.CellStyle;

            if (cellStyle == null)
            {
                return;
            }

            if (notification.NotificationStatus == "UNREAD")
            {
                cellStyle.BackColor =
                    Color.FromArgb(255, 245, 215);

                cellStyle.Font = new Font(
                    dgvNotifications.Font,
                    FontStyle.Bold
                );
            }
            else if (
                notification.NotificationStatus == "RESOLVED")
            {
                cellStyle.ForeColor = Color.Gray;
            }
        }

        private void CreateGridColumns()
        {
            AddColumn(
                "NotificationId",
                "ID",
                60
            );

            AddColumn(
                "NotificationType",
                "Type",
                145
            );

            AddColumn(
                "ProductName",
                "Product",
                180
            );

            AddColumn(
                "Barcode",
                "Barcode",
                110
            );

            AddColumn(
                "Message",
                "Message",
                350
            );

            AddColumn(
                "NotificationStatus",
                "Status",
                100
            );

            AddColumn(
                "CreatedAt",
                "Created",
                145,
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

            dgvNotifications.Columns.Add(column);
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

        private static void ShowResult(
            OperationResult result)
        {
            MessageBox.Show(
                result.Message,
                result.IsSuccessful ? "Success" : "Error",
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