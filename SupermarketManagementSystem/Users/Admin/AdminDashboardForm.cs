namespace SupermarketManagementSystem.Users.Admin
{
    public class AdminDashboardForm : Form
    {
        private readonly Panel contentPanel;

        public AdminDashboardForm()
        {
            Text = "Supermarket Management System - Admin";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1200, 700);
            MinimumSize = new Size(1100, 650);
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
        private void OpenUserManagement()
        {
            using UserManagementForm form =
                new UserManagementForm();

            form.ShowDialog(this);
        }

        private void OpenOrganizationProfile()
        {
            using OrganizationProfileForm form =
                new OrganizationProfileForm();

            form.ShowDialog(this);
        }

        private Panel CreateSidebar()
        {
            Panel sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = Color.FromArgb(24, 90, 60)
            };

            Label logo = new Label
            {
                Text = "SUPERMARKET\nADMIN PANEL",
                Dock = DockStyle.Top,
                Height = 110,
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI",
                    15,
                    FontStyle.Bold
                ),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Button btnDashboard = CreateMenuButton(
                "Dashboard",
                ShowDashboardHome
            );

            Button btnOrganization = CreateMenuButton(
             "Organization Profile",
             OpenOrganizationProfile
            );

            Button btnManagers = CreateMenuButton(
                "Manage Managers",
                () => ShowComingSoon("Manager Management")
            );

            Button btnUsers = CreateMenuButton(
            "Manage Users",
            OpenUserManagement
            );

            Button btnExpenses = CreateMenuButton(
                "View Expenses",
                () => ShowComingSoon("Expense Management")
            );

            Button btnFeedback = CreateMenuButton(
                "View Feedback",
                () => ShowComingSoon("Feedback Management")
            );

            Button btnLogout = CreateMenuButton(
                "Logout",
                Logout
            );

            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.BackColor = Color.FromArgb(160, 50, 50);

            sidebar.Controls.Add(btnFeedback);
            sidebar.Controls.Add(btnExpenses);
            sidebar.Controls.Add(btnUsers);
            sidebar.Controls.Add(btnManagers);
            sidebar.Controls.Add(btnOrganization);
            sidebar.Controls.Add(btnDashboard);
            sidebar.Controls.Add(logo);
            sidebar.Controls.Add(btnLogout);

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
                Text = "Admin Dashboard",
                Dock = DockStyle.Left,
                Width = 350,
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
                Text = "System Administrator  |  ADMIN",
                Dock = DockStyle.Right,
                Width = 330,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray,
                TextAlign = ContentAlignment.MiddleRight
            };

            header.Controls.Add(title);
            header.Controls.Add(userDetails);

            return header;
        }

        private Button CreateMenuButton(
            string text,
            Action clickAction)
        {
            Button button = new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 55,
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

            Label welcome = new Label
            {
                Text = "Welcome, System Administrator",
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
                Text = "System overview and quick information",
                Location = new Point(24, 65),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.DimGray
            };

            Panel usersCard = CreateSummaryCard(
                "Total Users",
                "0",
                Color.FromArgb(52, 152, 219),
                new Point(20, 120)
            );

            Panel managersCard = CreateSummaryCard(
                "Managers",
                "0",
                Color.FromArgb(46, 160, 110),
                new Point(280, 120)
            );

            Panel feedbackCard = CreateSummaryCard(
                "New Feedback",
                "0",
                Color.FromArgb(241, 160, 45),
                new Point(540, 120)
            );

            contentPanel.Controls.Add(welcome);
            contentPanel.Controls.Add(subtitle);
            contentPanel.Controls.Add(usersCard);
            contentPanel.Controls.Add(managersCard);
            contentPanel.Controls.Add(feedbackCard);
        }

        private Panel CreateSummaryCard(
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

        private void ShowComingSoon(string featureName)
        {
            MessageBox.Show(
                featureName + " will be implemented next.",
                "Module Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
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