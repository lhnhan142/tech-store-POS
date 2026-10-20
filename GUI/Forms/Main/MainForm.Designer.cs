#nullable disable
namespace GUI.Forms.Main
{
    partial class MainForm
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
            menuStrip = new MenuStrip();
            mnuSystem = new ToolStripMenuItem();
            mnuLogout = new ToolStripMenuItem();
            mnuSeparator = new ToolStripSeparator();
            mnuExit = new ToolStripMenuItem();
            pnlHeader = new Panel();
            lblTitle = new Label();
            btnLogout = new ReaLTaiizor.Controls.HopeRoundButton();
            pnlSidebar = new Panel();
            pnlNav = new Panel();
            lblBrand = new Label();
            pnlContent = new Panel();
            menuStrip.SuspendLayout();
            pnlHeader.SuspendLayout();
            pnlSidebar.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { mnuSystem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1280, 24);
            menuStrip.TabIndex = 3;
            // 
            // mnuSystem
            // 
            mnuSystem.DropDownItems.AddRange(new ToolStripItem[] { mnuLogout, mnuSeparator, mnuExit });
            mnuSystem.Name = "mnuSystem";
            mnuSystem.Text = "Hệ thống";
            // 
            // mnuLogout
            // 
            mnuLogout.Name = "mnuLogout";
            mnuLogout.Text = "Đăng xuất";
            mnuLogout.Click += mnuLogout_Click;
            // 
            // mnuSeparator
            // 
            mnuSeparator.Name = "mnuSeparator";
            // 
            // mnuExit
            // 
            mnuExit.Name = "mnuExit";
            mnuExit.Text = "Thoát";
            mnuExit.Click += mnuExit_Click;
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(btnLogout);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 24);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(20, 10, 16, 10);
            pnlHeader.Size = new Size(1280, 60);
            pnlHeader.TabIndex = 2;
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.ForeColor = Color.FromArgb(15, 15, 60);
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(1124, 40);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Tiêu đề trang";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnLogout
            // 
            btnLogout.ButtonType = ReaLTaiizor.Util.HopeButtonType.Primary;
            btnLogout.Dock = DockStyle.Right;
            btnLogout.Location = new Point(1144, 10);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(120, 40);
            btnLogout.TabIndex = 1;
            btnLogout.Text = "Đăng xuất";
            btnLogout.Click += btnLogout_Click;
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.FromArgb(15, 15, 60);
            pnlSidebar.Controls.Add(pnlNav);
            pnlSidebar.Controls.Add(lblBrand);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Location = new Point(0, 84);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(230, 676);
            pnlSidebar.TabIndex = 1;
            // 
            // pnlNav
            // 
            pnlNav.BackColor = Color.FromArgb(15, 15, 60);
            pnlNav.Dock = DockStyle.Fill;
            pnlNav.Location = new Point(0, 64);
            pnlNav.Name = "pnlNav";
            pnlNav.Size = new Size(230, 612);
            pnlNav.TabIndex = 1;
            // 
            // lblBrand
            // 
            lblBrand.Dock = DockStyle.Top;
            lblBrand.Font = new Font("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Point);
            lblBrand.ForeColor = Color.White;
            lblBrand.Location = new Point(0, 0);
            lblBrand.Name = "lblBrand";
            lblBrand.Size = new Size(230, 64);
            lblBrand.TabIndex = 0;
            lblBrand.Text = "TECH STORE POS";
            lblBrand.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(245, 245, 245);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(230, 84);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(1050, 676);
            pnlContent.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 245, 245);
            ClientSize = new Size(1280, 760);
            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);
            Controls.Add(pnlHeader);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            MinimumSize = new Size(1000, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tech Store POS";
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            pnlHeader.ResumeLayout(false);
            pnlSidebar.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip;
        private ToolStripMenuItem mnuSystem;
        private ToolStripMenuItem mnuLogout;
        private ToolStripSeparator mnuSeparator;
        private ToolStripMenuItem mnuExit;
        private Panel pnlHeader;
        private Label lblTitle;
        private ReaLTaiizor.Controls.HopeRoundButton btnLogout;
        private Panel pnlSidebar;
        private Panel pnlNav;
        private Label lblBrand;
        private Panel pnlContent;
    }
}