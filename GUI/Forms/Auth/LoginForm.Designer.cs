namespace GUI.Auth
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtPassword = new ReaLTaiizor.Controls.MaterialTextBoxEdit();
            txtUsername = new ReaLTaiizor.Controls.MaterialTextBoxEdit();
            btnLogin = new ReaLTaiizor.Controls.HopeRoundButton();
            label1 = new Label();
            lklblFogotPw = new ReaLTaiizor.Controls.FoxLinkLabel();
            lklblSignup = new ReaLTaiizor.Controls.FoxLinkLabel();
            SuspendLayout();
            // 
            // txtPassword
            // 
            txtPassword.AnimateReadOnly = false;
            txtPassword.AutoCompleteMode = AutoCompleteMode.None;
            txtPassword.AutoCompleteSource = AutoCompleteSource.None;
            txtPassword.BackgroundImageLayout = ImageLayout.None;
            txtPassword.CharacterCasing = CharacterCasing.Normal;
            txtPassword.Depth = 0;
            txtPassword.Font = new Font("Microsoft Sans Serif", 20F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtPassword.HideSelection = true;
            txtPassword.Hint = "Mật khẩu";
            txtPassword.LeadingIcon = null;
            txtPassword.Location = new Point(249, 222);
            txtPassword.MaxLength = 32767;
            txtPassword.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.OUT;
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '\0';
            txtPassword.PrefixSuffixText = null;
            txtPassword.ReadOnly = false;
            txtPassword.RightToLeft = RightToLeft.No;
            txtPassword.SelectedText = "";
            txtPassword.SelectionLength = 0;
            txtPassword.SelectionStart = 0;
            txtPassword.ShortcutsEnabled = true;
            txtPassword.Size = new Size(312, 60);
            txtPassword.TabIndex = 1;
            txtPassword.TabStop = false;
            txtPassword.TextAlign = HorizontalAlignment.Left;
            txtPassword.TrailingIcon = null;
            txtPassword.UseSystemPasswordChar = false;
            // 
            // txtUsername
            // 
            txtUsername.AnimateReadOnly = false;
            txtUsername.AutoCompleteMode = AutoCompleteMode.None;
            txtUsername.AutoCompleteSource = AutoCompleteSource.None;
            txtUsername.BackgroundImageLayout = ImageLayout.None;
            txtUsername.CharacterCasing = CharacterCasing.Normal;
            txtUsername.Depth = 0;
            txtUsername.Font = new Font("Microsoft Sans Serif", 20F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtUsername.HideSelection = true;
            txtUsername.Hint = "Tên đăng nhập";
            txtUsername.LeadingIcon = null;
            txtUsername.Location = new Point(249, 139);
            txtUsername.MaxLength = 32767;
            txtUsername.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.OUT;
            txtUsername.Name = "txtUsername";
            txtUsername.PasswordChar = '\0';
            txtUsername.PrefixSuffixText = null;
            txtUsername.ReadOnly = false;
            txtUsername.RightToLeft = RightToLeft.No;
            txtUsername.SelectedText = "";
            txtUsername.SelectionLength = 0;
            txtUsername.SelectionStart = 0;
            txtUsername.ShortcutsEnabled = true;
            txtUsername.Size = new Size(312, 60);
            txtUsername.TabIndex = 2;
            txtUsername.TabStop = false;
            txtUsername.TextAlign = HorizontalAlignment.Left;
            txtUsername.TrailingIcon = null;
            txtUsername.UseSystemPasswordChar = false;
            // 
            // btnLogin
            // 
            btnLogin.BorderColor = Color.FromArgb(220, 223, 230);
            btnLogin.ButtonType = ReaLTaiizor.Util.HopeButtonType.Primary;
            btnLogin.DangerColor = Color.FromArgb(245, 108, 108);
            btnLogin.DefaultColor = Color.FromArgb(255, 255, 255);
            btnLogin.Font = new Font("Segoe UI", 12F);
            btnLogin.HoverTextColor = Color.FromArgb(48, 49, 51);
            btnLogin.InfoColor = Color.FromArgb(144, 147, 153);
            btnLogin.Location = new Point(312, 337);
            btnLogin.Name = "btnLogin";
            btnLogin.PrimaryColor = Color.FromArgb(64, 158, 255);
            btnLogin.Size = new Size(164, 50);
            btnLogin.SuccessColor = Color.FromArgb(103, 194, 58);
            btnLogin.TabIndex = 3;
            btnLogin.Text = "Đăng nhập";
            btnLogin.TextColor = Color.White;
            btnLogin.WarningColor = Color.FromArgb(230, 162, 60);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 19.8000011F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(224, 58);
            label1.Name = "label1";
            label1.Size = new Size(353, 46);
            label1.TabIndex = 4;
            label1.Text = "TRANG ĐĂNG NHẬP";
            // 
            // lklblFogotPw
            // 
            lklblFogotPw.BackColor = Color.Transparent;
            lklblFogotPw.DownColor = Color.FromArgb(255, 149, 0);
            lklblFogotPw.EnabledCalc = true;
            lklblFogotPw.Font = new Font("Segoe UI", 10F);
            lklblFogotPw.ForeColor = Color.FromArgb(0, 149, 221);
            lklblFogotPw.Location = new Point(249, 288);
            lklblFogotPw.Name = "lklblFogotPw";
            lklblFogotPw.OverColor = Color.FromArgb(23, 140, 229);
            lklblFogotPw.Size = new Size(129, 27);
            lklblFogotPw.TabIndex = 6;
            lklblFogotPw.Text = "Quên mật khẩu";
            // 
            // lklblSignup
            // 
            lklblSignup.BackColor = Color.Transparent;
            lklblSignup.DownColor = Color.FromArgb(255, 149, 0);
            lklblSignup.EnabledCalc = true;
            lklblSignup.Font = new Font("Segoe UI", 10F);
            lklblSignup.ForeColor = Color.FromArgb(0, 149, 221);
            lklblSignup.Location = new Point(488, 288);
            lklblSignup.Name = "lklblSignup";
            lklblSignup.OverColor = Color.FromArgb(23, 140, 229);
            lklblSignup.Size = new Size(73, 27);
            lklblSignup.TabIndex = 7;
            lklblSignup.Text = "Đăng ký";
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lklblSignup);
            Controls.Add(lklblFogotPw);
            Controls.Add(label1);
            Controls.Add(btnLogin);
            Controls.Add(txtUsername);
            Controls.Add(txtPassword);
            Name = "LoginForm";
            Text = "Đăng nhập hệ thống";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ReaLTaiizor.Controls.MaterialTextBoxEdit txtPassword;
        private ReaLTaiizor.Controls.MaterialTextBoxEdit txtUsername;
        private ReaLTaiizor.Controls.HopeRoundButton btnLogin;
        private Label label1;
        private ReaLTaiizor.Controls.FoxLinkLabel lklblFogotPw;
        private ReaLTaiizor.Controls.FoxLinkLabel lklblSignup;
    }
}