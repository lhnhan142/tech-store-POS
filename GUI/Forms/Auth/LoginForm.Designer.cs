namespace GUI.Forms.Auth
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            SiticoneBorderlessForm1 = new Siticone.Desktop.UI.WinForms.SiticoneBorderlessForm(components);
            SiticoneControlBox1 = new Siticone.Desktop.UI.WinForms.SiticoneControlBox();
            txtPassword = new Siticone.Desktop.UI.WinForms.SiticoneTextBox();
            txtUsername = new Siticone.Desktop.UI.WinForms.SiticoneTextBox();
            chkRememberMe = new Siticone.Desktop.UI.WinForms.SiticoneToggleSwitch();
            lblRemenberMe = new Label();
            btnLogin = new Siticone.Desktop.UI.WinForms.SiticoneButton();
            SiticoneHtmlLabel1 = new Siticone.Desktop.UI.WinForms.SiticoneHtmlLabel();
            SuspendLayout();
            // 
            // SiticoneBorderlessForm1
            // 
            SiticoneBorderlessForm1.BorderRadius = 15;
            SiticoneBorderlessForm1.ContainerControl = this;
            SiticoneBorderlessForm1.DockIndicatorTransparencyValue = 0.6D;
            SiticoneBorderlessForm1.TransparentWhileDrag = true;
            // 
            // SiticoneControlBox1
            // 
            SiticoneControlBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SiticoneControlBox1.FillColor = Color.FromArgb(139, 152, 166);
            SiticoneControlBox1.IconColor = Color.White;
            SiticoneControlBox1.Location = new Point(743, -2);
            SiticoneControlBox1.Name = "SiticoneControlBox1";
            SiticoneControlBox1.Size = new Size(56, 36);
            SiticoneControlBox1.TabIndex = 0;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.Transparent;
            txtPassword.BorderRadius = 8;
            txtPassword.DefaultText = "";
            txtPassword.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtPassword.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtPassword.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtPassword.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtPassword.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            txtPassword.Font = new Font("Segoe UI", 9F);
            txtPassword.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtPassword.Location = new Point(261, 228);
            txtPassword.Margin = new Padding(3, 4, 3, 4);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.PlaceholderText = "Mật khẩu";
            txtPassword.SelectedText = "";
            txtPassword.Size = new Size(286, 60);
            txtPassword.TabIndex = 1;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.Transparent;
            txtUsername.BorderRadius = 8;
            txtUsername.DefaultText = "";
            txtUsername.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtUsername.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtUsername.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtUsername.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtUsername.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            txtUsername.Font = new Font("Segoe UI", 9F);
            txtUsername.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtUsername.Location = new Point(261, 137);
            txtUsername.Margin = new Padding(3, 4, 3, 4);
            txtUsername.Name = "txtUsername";
            txtUsername.PasswordChar = '\0';
            txtUsername.PlaceholderText = "Tên Đăng Nhập";
            txtUsername.SelectedText = "";
            txtUsername.Size = new Size(286, 60);
            txtUsername.TabIndex = 2;
            // 
            // chkRememberMe
            // 
            chkRememberMe.BackColor = Color.Transparent;
            chkRememberMe.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            chkRememberMe.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            chkRememberMe.CheckedState.InnerBorderColor = Color.White;
            chkRememberMe.CheckedState.InnerColor = Color.White;
            chkRememberMe.Location = new Point(451, 307);
            chkRememberMe.Name = "chkRememberMe";
            chkRememberMe.Size = new Size(44, 25);
            chkRememberMe.TabIndex = 3;
            chkRememberMe.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            chkRememberMe.UncheckedState.FillColor = Color.FromArgb(125, 137, 149);
            chkRememberMe.UncheckedState.InnerBorderColor = Color.White;
            chkRememberMe.UncheckedState.InnerColor = Color.White;
            // 
            // lblRemenberMe
            // 
            lblRemenberMe.AutoSize = true;
            lblRemenberMe.BackColor = Color.Transparent;
            lblRemenberMe.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblRemenberMe.Location = new Point(261, 304);
            lblRemenberMe.Name = "lblRemenberMe";
            lblRemenberMe.Size = new Size(184, 28);
            lblRemenberMe.TabIndex = 4;
            lblRemenberMe.Text = "Ghi nhớ đăng nhập:";
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.Transparent;
            btnLogin.BorderRadius = 8;
            btnLogin.DisabledState.BorderColor = Color.DarkGray;
            btnLogin.DisabledState.CustomBorderColor = Color.DarkGray;
            btnLogin.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnLogin.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnLogin.Font = new Font("Segoe UI", 9F);
            btnLogin.ForeColor = Color.White;
            btnLogin.HoverState.FillColor = SystemColors.ActiveCaption;
            btnLogin.Location = new Point(296, 354);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(225, 56);
            btnLogin.TabIndex = 5;
            btnLogin.Text = "ĐĂNG NHẬP";
            btnLogin.Click += btnLogin_Click;
            // 
            // SiticoneHtmlLabel1
            // 
            SiticoneHtmlLabel1.BackColor = Color.Transparent;
            SiticoneHtmlLabel1.Font = new Font("Segoe UI", 19.8000011F, FontStyle.Bold, GraphicsUnit.Point, 0);
            SiticoneHtmlLabel1.Location = new Point(207, 52);
            SiticoneHtmlLabel1.Name = "SiticoneHtmlLabel1";
            SiticoneHtmlLabel1.Size = new Size(381, 47);
            SiticoneHtmlLabel1.TabIndex = 6;
            SiticoneHtmlLabel1.Text = "ĐĂNG NHẬP HỆ THỐNG";
            SiticoneHtmlLabel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.dried_leaf_frame_botanical_background;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(800, 450);
            Controls.Add(SiticoneHtmlLabel1);
            Controls.Add(btnLogin);
            Controls.Add(lblRemenberMe);
            Controls.Add(chkRememberMe);
            Controls.Add(txtUsername);
            Controls.Add(txtPassword);
            Controls.Add(SiticoneControlBox1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "LoginForm";
            Text = "LoginForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Siticone.Desktop.UI.WinForms.SiticoneBorderlessForm SiticoneBorderlessForm1;
        private Siticone.Desktop.UI.WinForms.SiticoneTextBox txtUsername;
        private Siticone.Desktop.UI.WinForms.SiticoneTextBox txtPassword;
        private Siticone.Desktop.UI.WinForms.SiticoneControlBox SiticoneControlBox1;
        private Siticone.Desktop.UI.WinForms.SiticoneButton btnLogin;
        private System.Windows.Forms.Label lblRemenberMe;
        private Siticone.Desktop.UI.WinForms.SiticoneToggleSwitch chkRememberMe;
        private Siticone.Desktop.UI.WinForms.SiticoneHtmlLabel SiticoneHtmlLabel1;
    }
}