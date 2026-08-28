using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Services;

namespace SupermarketManagementSystem.Users.Admin
{
    public class OrganizationProfileForm : Form
    {
        private readonly OrganizationService service;

        private readonly TextBox txtOrganizationName;
        private readonly TextBox txtAddress;
        private readonly TextBox txtPhone;
        private readonly TextBox txtEmail;
        private readonly TextBox txtOpeningHours;
        private readonly TextBox txtTaxNumber;

        private int organizationId;
        private string logoPath = string.Empty;

        public OrganizationProfileForm()
        {
            service = new OrganizationService();

            Text = "Organization Profile";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(800, 620);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);

            Label title = new Label
            {
                Text = "Organization Profile",
                Location = new Point(40, 25),
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
                Text = "Manage supermarket business information",
                Location = new Point(43, 70),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray
            };

            Panel formPanel = new Panel
            {
                Location = new Point(40, 110),
                Size = new Size(720, 440),
                BackColor = Color.White
            };

            AddLabel(
                formPanel,
                "Organization name",
                30,
                25
            );

            txtOrganizationName =
                CreateTextBox(30, 55, 660);

            formPanel.Controls.Add(txtOrganizationName);

            AddLabel(formPanel, "Address", 30, 100);

            txtAddress = CreateTextBox(
                30,
                130,
                660
            );

            txtAddress.Multiline = true;
            txtAddress.Size = new Size(660, 65);

            formPanel.Controls.Add(txtAddress);

            AddLabel(formPanel, "Phone", 30, 215);

            txtPhone = CreateTextBox(
                30,
                245,
                310
            );

            formPanel.Controls.Add(txtPhone);

            AddLabel(formPanel, "Email", 380, 215);

            txtEmail = CreateTextBox(
                380,
                245,
                310
            );

            formPanel.Controls.Add(txtEmail);

            AddLabel(
                formPanel,
                "Opening hours",
                30,
                295
            );

            txtOpeningHours = CreateTextBox(
                30,
                325,
                310
            );

            formPanel.Controls.Add(txtOpeningHours);

            AddLabel(
                formPanel,
                "Tax number",
                380,
                295
            );

            txtTaxNumber = CreateTextBox(
                380,
                325,
                310
            );

            formPanel.Controls.Add(txtTaxNumber);

            Button btnSave = CreateButton(
                "SAVE CHANGES",
                350,
                380,
                Color.FromArgb(24, 90, 60)
            );

            Button btnClose = CreateButton(
                "CLOSE",
                530,
                380,
                Color.FromArgb(160, 50, 50)
            );

            btnSave.Click += BtnSave_Click;
            btnClose.Click += (_, _) => Close();

            formPanel.Controls.Add(btnSave);
            formPanel.Controls.Add(btnClose);

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(formPanel);

            Load += OrganizationProfileForm_Load;
        }

        private void OrganizationProfileForm_Load(
            object? sender,
            EventArgs e)
        {
            LoadProfile();
        }

        private void LoadProfile()
        {
            try
            {
                OrganizationProfile? profile =
                    service.GetProfile();

                if (profile == null)
                {
                    return;
                }

                organizationId =
                    profile.OrganizationId;

                logoPath = profile.LogoPath;

                txtOrganizationName.Text =
                    profile.OrganizationName;

                txtAddress.Text = profile.Address;
                txtPhone.Text = profile.Phone;
                txtEmail.Text = profile.Email;

                txtOpeningHours.Text =
                    profile.OpeningHours;

                txtTaxNumber.Text =
                    profile.TaxNumber;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to load profile:\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void BtnSave_Click(
            object? sender,
            EventArgs e)
        {
            OrganizationProfile profile =
                new OrganizationProfile
                {
                    OrganizationId = organizationId,
                    OrganizationName =
                        txtOrganizationName.Text,

                    Address = txtAddress.Text,
                    Phone = txtPhone.Text,
                    Email = txtEmail.Text,

                    OpeningHours =
                        txtOpeningHours.Text,

                    TaxNumber = txtTaxNumber.Text,
                    LogoPath = logoPath
                };

            OperationResult result =
                service.SaveProfile(profile);

            MessageBox.Show(
                result.Message,
                result.IsSuccessful
                    ? "Success"
                    : "Validation",
                MessageBoxButtons.OK,
                result.IsSuccessful
                    ? MessageBoxIcon.Information
                    : MessageBoxIcon.Warning
            );

            if (result.IsSuccessful)
            {
                organizationId =
                    profile.OrganizationId;
            }
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

        private static TextBox CreateTextBox(
            int x,
            int y,
            int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 32),
                Font = new Font("Segoe UI", 11)
            };
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
                Size = new Size(150, 42),
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
    }
}