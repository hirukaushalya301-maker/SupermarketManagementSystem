namespace SupermarketManagementSystem
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private CheckBox chkShowPassword;
        private Button btnLogin;
        private Button btnExit;

        public LoginForm()
        {
            InitializeComponent();
            CreateLoginInterface();
        }

        private void CreateLoginInterface()
        {
            // Form settings
            Text = "Supermarket Management System - Login";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(900, 550);
            BackColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            // Left-side coloured panel
            Panel leftPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(380, 550),
                BackColor = Color.FromArgb(24, 90, 60)
            };

            Label lblSystemName = new Label
            {
                Text = "SUPERMARKET MANAGEMENT\nSYSTEM",
                Location = new Point(5, 145),
                Size = new Size(370, 140),
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblDescription = new Label
            {
                Text = "Manage sales, stock and customers",
                Location = new Point(40, 310),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.WhiteSmoke,
                TextAlign = ContentAlignment.MiddleCenter
            };

            leftPanel.Controls.Add(lblSystemName);
            leftPanel.Controls.Add(lblDescription);

            // Login heading
            Label lblTitle = new Label
            {
                Text = "Welcome Back",
                Location = new Point(500, 80),
                AutoSize = true,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(24, 90, 60)
            };

            Label lblSubtitle = new Label
            {
                Text = "Sign in to continue",
                Location = new Point(505, 130),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.DimGray
            };

            // Username label
            Label lblUsername = new Label
            {
                Text = "Username",
                Location = new Point(505, 190),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            // Username textbox
            txtUsername = new TextBox
            {
                Name = "txtUsername",
                Location = new Point(505, 220),
                Size = new Size(310, 30),
                Font = new Font("Segoe UI", 12),
                MaxLength = 50
            };

            // Password label
            Label lblPassword = new Label
            {
                Text = "Password",
                Location = new Point(505, 275),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            // Password textbox
            txtPassword = new TextBox
            {
                Name = "txtPassword",
                Location = new Point(505, 305),
                Size = new Size(310, 30),
                Font = new Font("Segoe UI", 12),
                MaxLength = 100,
                UseSystemPasswordChar = true
            };

            // Show-password checkbox
            chkShowPassword = new CheckBox
            {
                Text = "Show password",
                Location = new Point(505, 350),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            chkShowPassword.CheckedChanged +=
                ChkShowPassword_CheckedChanged;

            // Login button
            btnLogin = new Button
            {
                Text = "LOGIN",
                Location = new Point(505, 395),
                Size = new Size(150, 45),
                BackColor = Color.FromArgb(24, 90, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            // Exit button
            btnExit = new Button
            {
                Text = "EXIT",
                Location = new Point(665, 395),
                Size = new Size(150, 45),
                BackColor = Color.Gainsboro,
                ForeColor = Color.DarkRed,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnExit.Click += BtnExit_Click;

            // Add all controls to the form
            Controls.Add(leftPanel);
            Controls.Add(lblTitle);
            Controls.Add(lblSubtitle);
            Controls.Add(lblUsername);
            Controls.Add(txtUsername);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(chkShowPassword);
            Controls.Add(btnLogin);
            Controls.Add(btnExit);

            // Keyboard shortcuts
            AcceptButton = btnLogin;
            CancelButton = btnExit;

            // Start typing in the username field
            ActiveControl = txtUsername;
        }

        private void ChkShowPassword_CheckedChanged(
            object? sender,
            EventArgs e)
        {
            txtPassword.UseSystemPasswordChar =
                !chkShowPassword.Checked;
        }

        private void BtnLogin_Click(
            object? sender,
            EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(
                    "Please enter your username.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Please enter your password.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtPassword.Focus();
                return;
            }

            // Database login will be added in the next step.
            MessageBox.Show(
                "Login form validation is working.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void BtnExit_Click(
            object? sender,
            EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to exit?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}