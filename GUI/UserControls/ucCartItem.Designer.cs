namespace GUI.UserControls
{
    partial class ucCartItem
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            picThumb = new PictureBox();
            pnlInfo = new Panel();
            pnlRow = new Panel();
            btnPlus = new Button();
            lblLineTotal = new Label();
            lblQty = new Label();
            btnMinus = new Button();
            pnlTop = new Panel();
            lblName = new Label();
            btnRemove = new Button();
            ((System.ComponentModel.ISupportInitialize)picThumb).BeginInit();
            pnlInfo.SuspendLayout();
            pnlRow.SuspendLayout();
            pnlTop.SuspendLayout();
            SuspendLayout();
            // 
            // picThumb
            // 
            picThumb.BackColor = Color.FromArgb(235, 235, 235);
            picThumb.Dock = DockStyle.Left;
            picThumb.Location = new Point(4, 4);
            picThumb.Name = "picThumb";
            picThumb.Size = new Size(78, 82);
            picThumb.SizeMode = PictureBoxSizeMode.Zoom;
            picThumb.TabIndex = 0;
            picThumb.TabStop = false;
            // 
            // pnlInfo
            // 
            pnlInfo.Controls.Add(pnlRow);
            pnlInfo.Controls.Add(pnlTop);
            pnlInfo.Dock = DockStyle.Fill;
            pnlInfo.Location = new Point(82, 4);
            pnlInfo.Name = "pnlInfo";
            pnlInfo.Padding = new Padding(10, 0, 0, 0);
            pnlInfo.Size = new Size(254, 82);
            pnlInfo.TabIndex = 1;
            // 
            // pnlRow
            // 
            pnlRow.Controls.Add(btnPlus);
            pnlRow.Controls.Add(lblLineTotal);
            pnlRow.Controls.Add(lblQty);
            pnlRow.Controls.Add(btnMinus);
            pnlRow.Dock = DockStyle.Fill;
            pnlRow.Location = new Point(10, 40);
            pnlRow.Name = "pnlRow";
            pnlRow.Padding = new Padding(0, 4, 0, 4);
            pnlRow.Size = new Size(244, 42);
            pnlRow.TabIndex = 2;
            // 
            // btnPlus
            // 
            btnPlus.Cursor = Cursors.Hand;
            btnPlus.Dock = DockStyle.Left;
            btnPlus.FlatAppearance.BorderColor = Color.FromArgb(217, 220, 227);
            btnPlus.FlatStyle = FlatStyle.Flat;
            btnPlus.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPlus.ForeColor = Color.FromArgb(15, 15, 60);
            btnPlus.Location = new Point(75, 4);
            btnPlus.Name = "btnPlus";
            btnPlus.Size = new Size(37, 34);
            btnPlus.TabIndex = 3;
            btnPlus.Text = "\t+";
            btnPlus.UseVisualStyleBackColor = true;
            // 
            // lblLineTotal
            // 
            lblLineTotal.Dock = DockStyle.Right;
            lblLineTotal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLineTotal.ForeColor = Color.FromArgb(0, 112, 243);
            lblLineTotal.Location = new Point(104, 4);
            lblLineTotal.Name = "lblLineTotal";
            lblLineTotal.Size = new Size(140, 34);
            lblLineTotal.TabIndex = 2;
            lblLineTotal.Text = "\t390.000đ";
            lblLineTotal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblQty
            // 
            lblQty.Dock = DockStyle.Left;
            lblQty.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblQty.Location = new Point(39, 4);
            lblQty.Name = "lblQty";
            lblQty.Size = new Size(36, 34);
            lblQty.TabIndex = 1;
            lblQty.Text = "1";
            lblQty.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnMinus
            // 
            btnMinus.Cursor = Cursors.Hand;
            btnMinus.Dock = DockStyle.Left;
            btnMinus.FlatAppearance.BorderColor = Color.FromArgb(217, 220, 227);
            btnMinus.FlatStyle = FlatStyle.Flat;
            btnMinus.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnMinus.ForeColor = Color.FromArgb(15, 15, 60);
            btnMinus.Location = new Point(0, 4);
            btnMinus.Name = "btnMinus";
            btnMinus.Size = new Size(39, 34);
            btnMinus.TabIndex = 0;
            btnMinus.Text = "\t−";
            btnMinus.UseVisualStyleBackColor = true;
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(lblName);
            pnlTop.Controls.Add(btnRemove);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(10, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(244, 40);
            pnlTop.TabIndex = 0;
            // 
            // lblName
            // 
            lblName.Dock = DockStyle.Fill;
            lblName.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblName.ForeColor = Color.FromArgb(15, 15, 60);
            lblName.Location = new Point(0, 0);
            lblName.Name = "lblName";
            lblName.Size = new Size(216, 40);
            lblName.TabIndex = 1;
            lblName.Text = "\tChuột Logitech G102";
            lblName.Click += lblName_Click;
            // 
            // btnRemove
            // 
            btnRemove.Cursor = Cursors.Hand;
            btnRemove.Dock = DockStyle.Right;
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.FlatStyle = FlatStyle.Flat;
            btnRemove.ForeColor = Color.FromArgb(160, 164, 174);
            btnRemove.Location = new Point(216, 0);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(28, 40);
            btnRemove.TabIndex = 0;
            btnRemove.Text = "✕";
            btnRemove.UseVisualStyleBackColor = true;
            // 
            // ucCartItem
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(pnlInfo);
            Controls.Add(picThumb);
            Margin = new Padding(0, 0, 0, 8);
            Name = "ucCartItem";
            Padding = new Padding(4);
            Size = new Size(340, 90);
            ((System.ComponentModel.ISupportInitialize)picThumb).EndInit();
            pnlInfo.ResumeLayout(false);
            pnlRow.ResumeLayout(false);
            pnlTop.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private PictureBox picThumb;
        private Panel pnlInfo;
        private Panel pnlTop;
        private Label lblName;
        private Button btnRemove;
        private Panel pnlRow;
        private Button btnMinus;
        private Label lblQty;
        private Label lblLineTotal;
        private Button btnPlus;
    }
}
